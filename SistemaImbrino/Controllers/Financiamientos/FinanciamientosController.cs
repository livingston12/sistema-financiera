using Newtonsoft.Json;
using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    [Authorize(Roles = "financiamientos,admin")]
    public class FinanciamientosController : BaseController
    {
        public FinanciamientosController()
        {
            db = new DB_IMBRINOEntities();
        }
        private TypeFORMAPG _TypeFORMAPG { get; set; }
        private int DayToadd { get; set; }
        private decimal montoTotalPrestamo { get; set; }
        private decimal Balance_actual { get; set; }
        private bool isRecalculate { get; set; }
        
        // GET: Financiamientos
        public ActionResult Index()
        {

            var ListFac = db.FACTURA.ToList();

            ViewBag.FINACIAMIENTOLast = ListFac.Any() ? db.FACTURA.Max(x => x.FAC_NUMERO) + 1 : 1;
            return View();
        }

        public JsonResult activarPrestamo(string json,string jsonCheque, DateTime fecha, string clienteID, string fiadorID, string promotorID)
        {
            message ResultMessage = new message();
            
            try
            {
                View_ListFincaciamientos ListFinanciamiento = new View_ListFincaciamientos();
                ListFinanciamiento = JsonConvert.DeserializeObject<View_ListFincaciamientos>(json);
                OTROSCR otroCR = new OTROSCR();
                otroCR = JsonConvert.DeserializeObject<OTROSCR>(jsonCheque);

                foreach (var item in ListFinanciamiento.ListFinanciamientos)
                {
                    item.Cliente = clienteID;
                    item.Fiador = fiadorID;
                    item.Promotor = promotorID;
                }

                // validar si el financiamiento se puede generar
                if (ListFinanciamiento.ListFinanciamientos.Count() == 0)
                {
                    ResultMessage = new message()
                    {
                        Message = "Datos incorrectos favor crear amortizacion antes de enviar los datos",
                        Is_Success = false
                    };
                }
                else
                {
                    ResultMessage = FinanciamientoIsvalid(ListFinanciamiento.ListFinanciamientos.FirstOrDefault(), false);
                }

                if (ResultMessage.Is_Success == false)
                    return Json(ResultMessage);

                ResultMessage = GuardarPrestamo(ListFinanciamiento, fecha, otroCR);
            }
            catch (Exception)
            {
                ResultMessage.Is_Success = false;
                ResultMessage.Message = "Error inesperado: No se puede convertir el objeto";
            }


            return Json(ResultMessage);
        }


        public JsonResult CalcularAmortizacion(string json)
        {
            View_ListFincaciamientos lstFina = new View_ListFincaciamientos();
            lstFina.ListFinanciamientos = new List<View_fincaciamientos>();
            //List<View_fincaciamientos> ListFinanciamientos = new List<View_fincaciamientos>();
            View_fincaciamientos Financiamiento = new View_fincaciamientos();
            lstFina.message = new message() { Is_Success = true };
            bool Is_manual = false;
            try
            {

                Financiamiento = JsonConvert.DeserializeObject<View_fincaciamientos>(json);
                lstFina.message = FinanciamientoIsvalid(Financiamiento);

                if (lstFina.message.Is_Success == false)
                    return Json(lstFina, JsonRequestBehavior.AllowGet);

                montoTotalPrestamo = Financiamiento.Monto;
                Balance_actual = Financiamiento.Monto;
                _TypeFORMAPG = (TypeFORMAPG)Enum.Parse(typeof(TypeFORMAPG), Financiamiento.FormaPago.ToString());

                DayToadd = 1;
                Financiamiento.CuotasAdiccionales = Financiamiento.listCuotasAdiccionales.Count();
                if (TypeFORMAPG.MENSUAL != _TypeFORMAPG)
                {
                    var id = (int)_TypeFORMAPG;
                    var currentFormaPago = db.FORMAPG.Where(x => x.FOR_CODIGO == id).FirstOrDefault().FOR_PERIOD;
                    DayToadd = int.Parse(currentFormaPago.ToString());


                }
                /* Si existen algunas cuotas adiccionales se le resta el capital al monto total 
                 * para que el recalculo se haga en base al total sin las cuotas adiccionales */
                if (Financiamiento.CuotasAdiccionales > 0 && (TypeInteres)Financiamiento.TipoInteres != TypeInteres.MANUAL)
                    montoTotalPrestamo -= Financiamiento.listCuotasAdiccionales.Sum(x => x.capital);

                switch ((TypeInteres)Enum.Parse(typeof(TypeInteres), Financiamiento.TipoInteres.ToString()))
                {

                    case TypeInteres.FIJO:
                        lstFina.ListFinanciamientos.AddRange(calcularInteresFijo(Financiamiento));
                        break;
                    case TypeInteres.INSOLUTO:
                        lstFina.ListFinanciamientos.AddRange(calcularInteresINSOLUTO(Financiamiento));
                        break;
                    case TypeInteres.SOLOINTERES:
                        lstFina.ListFinanciamientos.AddRange(calcularInteresSOLOINTERES(Financiamiento));
                        break;
                    case TypeInteres.SININTERES:
                        lstFina.ListFinanciamientos.AddRange(calcularInteresSININTERES(Financiamiento));
                        break;
                    case TypeInteres.VINSOLUTO:
                        lstFina.ListFinanciamientos.AddRange(calcularInteresVINSOLUTO(Financiamiento));
                        break;
                    case TypeInteres.MANUAL:
                        if (Financiamiento.listCuotasAdiccionales.Count() < 1)
                        {
                            lstFina.message = new message() { Is_Success = false, Message = "Esta opcion no esta permitida por el sistema" };
                            break;
                        }
                        Balance_actual = Financiamiento.listCuotasAdiccionales.Sum(x => x.capital);
                        lstFina.ListFinanciamientos.AddRange(recalcularCuotas(Financiamiento.listCuotasAdiccionales.OrderBy(x => x.FechaVencimiento).ToList()));
                        Is_manual = true;
                        break;

                }

                // Se calcula siempre excepto cuando es un tipo de interes manual
                if (Is_manual == false)
                {
                    // Se recalculan las cuotas si existen cuotas adiccionales
                    if (Financiamiento.CuotasAdiccionales > 0)
                    {
                        Financiamiento.listCuotasAdiccionales =
                            Financiamiento.listCuotasAdiccionales
                            .Select(x =>
                                new View_fincaciamientos
                                {
                                    Cliente = Financiamiento.Cliente,
                                    Fiador = Financiamiento.Fiador,
                                    Promotor = Financiamiento.Promotor,
                                    TipoFin = Financiamiento.TipoFin,
                                    TipoInteres = Financiamiento.TipoInteres,
                                    TipoCuota = x.TipoCuota,
                                    Garantia = Financiamiento.Garantia,
                                    PorInt = Financiamiento.PorInt,
                                    FormaPago =Financiamiento.FormaPago,
                                    numCuota = x.numCuota,
                                    Balance = x.Balance,
                                    capital = x.capital,
                                    Fecha = x.Fecha,
                                    FechaVencimiento = x.FechaVencimiento,
                                    interes = x.interes,
                                    Monto = x.Monto                                    
                                })
                        .ToList();
                        lstFina.ListFinanciamientos.AddRange(Financiamiento.listCuotasAdiccionales);
                        Balance_actual = lstFina.ListFinanciamientos.Sum(x => x.capital);
                        lstFina.ListFinanciamientos = recalcularCuotas(lstFina.ListFinanciamientos.OrderBy(x => x.FechaVencimiento.Date).ThenByDescending(x => x.TipoCuota).ToList());
                    }
                    // Se redondean siempre las cuotas para que queden en el multiplo de 5 mas mayor
                    lstFina.ListFinanciamientos = redondearCuotas(lstFina.ListFinanciamientos);


                }

            }
            catch (Exception ex)
            {
                lstFina.message.Is_Success = false;
                lstFina.message.Message = "Error inesperado: No se a podido procesar la amortizacion";

            }

            if (lstFina.message.Is_Success == false)
                lstFina.ListFinanciamientos = new List<View_fincaciamientos>();


            return Json(lstFina, JsonRequestBehavior.AllowGet);

            //return Json(ListFinanciamientos, JsonRequestBehavior.AllowGet);
        }

        private message FinanciamientoIsvalid(View_fincaciamientos fincaciamientos, bool optional = true)
        {
            message message = new message() { Message = "", Is_Success = true };
            string incorrecto = " Incorrecto", incorrecta = " Incorrecta";
            if (optional == false)
            {
                if (string.IsNullOrWhiteSpace(fincaciamientos.Cliente) || fincaciamientos.Cliente == "0")
                    message.Message = "- cliente" + incorrecto;
                //if (string.IsNullOrWhiteSpace(fincaciamientos.Fiador) || fincaciamientos.Fiador == "0")
                //message.Message += Environment.NewLine + "- Fiador" + incorrecto;
                if (string.IsNullOrWhiteSpace(fincaciamientos.Promotor))
                    message.Message += Environment.NewLine + "- Promotor" + incorrecto;
            }
            if (string.IsNullOrWhiteSpace(fincaciamientos.TipoFin.ToString()) || fincaciamientos.TipoFin < 1)
                message.Message += Environment.NewLine + "- Tipo Financiamiento" + incorrecto;
            if (string.IsNullOrWhiteSpace(fincaciamientos.Monto.ToString()) || fincaciamientos.Monto < 1)
                message.Message += Environment.NewLine + "- Monto" + incorrecto;
            if (string.IsNullOrWhiteSpace(fincaciamientos.Fecha))
                message.Message += Environment.NewLine + "- Fecha" + incorrecta;
            if (string.IsNullOrWhiteSpace(fincaciamientos.FormaPago.ToString()) || fincaciamientos.FormaPago < 1)
                message.Message += Environment.NewLine + "- Forma de pago" + incorrecta;
            if (string.IsNullOrWhiteSpace(fincaciamientos.TipoInteres.ToString()) || fincaciamientos.TipoInteres < 1)
                message.Message += Environment.NewLine + "- Tipo interes" + incorrecto;
            if (string.IsNullOrWhiteSpace(fincaciamientos.PorInt.ToString()) || fincaciamientos.PorInt < 1)
                message.Message += Environment.NewLine + "- Porciento interes" + incorrecto;
            if (string.IsNullOrWhiteSpace(fincaciamientos.PorInt.ToString()) || fincaciamientos.PorInt < 1)
                message.Message += Environment.NewLine + "- Cuotas normales" + incorrecta + "s";
            if (string.IsNullOrWhiteSpace(fincaciamientos.FechaVencimiento.ToString()))
                message.Message += Environment.NewLine + "- Fecha Vencimiento 1ra cuota" + incorrecta;

            if (message.Message.Count() > 0)
                message.Is_Success = false;

            return message;

        }

        private List<View_fincaciamientos> redondearCuotas(List<View_fincaciamientos> listFinanciamientos)
        {

            int redondeado = 0;
            decimal totalResiduo = 0;
            foreach (var fin in listFinanciamientos)
            {
                if (fin.Monto % 5 == 0)
                    continue;
                else
                {
                    redondeado = decimal.ToInt32((fin.Monto / 5));
                    redondeado = (redondeado + 1) * 5;
                    totalResiduo = redondeado - fin.Monto;
                    fin.interes += totalResiduo;
                    //fin.Balance += fin.Balance > 0 ? totalResiduo : 0;
                    fin.Monto += totalResiduo;
                }


                listFinanciamientos.Where(x => x.numCuota == fin.numCuota)
                    .Select(x => new { fin }
                    );

            }
            return listFinanciamientos;


            //return listFinanciamientos;
        }

        private List<View_fincaciamientos> recalcularCuotas(List<View_fincaciamientos> listFinanciamientos)
        {
            isRecalculate = false;
            List<View_fincaciamientos> listFinanciamientos2 = new List<View_fincaciamientos>();
            View_fincaciamientos fincaciamiento = new View_fincaciamientos();
            int i = 0;
            foreach (View_fincaciamientos fincaciamiento2 in listFinanciamientos)
            {
                //DateTime fecha = ReturFechaFormadePago(_TypeFORMAPG, fincaciamiento.FechaVencimiento, i);
                fincaciamiento = fincaciamiento2;
                fincaciamiento.TipoCuota = fincaciamiento.TipoInteres == (int)TypeInteres.MANUAL ? TypeCuota.NORMAL.ToString() : fincaciamiento.TipoCuota;
                if (i > 0)
                {
                    fincaciamiento = new View_fincaciamientos()
                    {
                        capital = fincaciamiento.capital,
                        CuotasNormales = fincaciamiento.CuotasNormales,
                        PorInt = fincaciamiento.PorInt,
                        FechaVencimiento = fincaciamiento.FechaVencimiento,
                        Fecha = fincaciamiento.Fecha,
                        TipoCuota = fincaciamiento.TipoCuota,
                        interes = fincaciamiento.interes


                    };
                }
                fincaciamiento.numCuota = i + 1;


                //if (fincaciamiento.TipoCuota == TypeCuota.ADICIONAL.ToString())                
                //    isRecalculate = true;

                fincaciamiento.Monto = fincaciamiento.capital + fincaciamiento.interes;

                //if (isRecalculate)
                //    Balance_actual = Balance_actual - fincaciamiento.capital;               
                //else
                //    Balance_actual = montoTotalPrestamo - (fincaciamiento.capital * fincaciamiento.numCuota);

                Balance_actual -= fincaciamiento.capital;


                fincaciamiento.Balance = Balance_actual < 0 ? 0 : Balance_actual;
                listFinanciamientos2.Add(fincaciamiento);
                i++;
            }


            return listFinanciamientos2;
        }

        private List<View_fincaciamientos> calcularInteresVINSOLUTO(View_fincaciamientos fincaciamiento)
        {
            List<View_fincaciamientos> Listfincaciamientos = new List<View_fincaciamientos>();
            decimal totalCal = 0, porint = 0;
            decimal totalAdicionales = fincaciamiento.listCuotasAdiccionales.Sum(x => x.capital);
            montoTotalPrestamo = fincaciamiento.Monto - totalAdicionales;
            porint = decimal.Parse(fincaciamiento.PorInt.ToString());



            #region ----------------calculo 1 -----------------
            totalCal = montoTotalPrestamo / fincaciamiento.CuotasNormales;

            #endregion  // Finalizacion calulo 2


            #region ----------------calculo 2 -----------------

            for (int i = 0; i < fincaciamiento.CuotasNormales; i++)
            {
                DateTime fecha = ReturFechaFormadePago(_TypeFORMAPG, fincaciamiento.FechaVencimiento, i);

                if (i == 0)
                    fincaciamiento.interes = fincaciamiento.Monto * (porint / 100);
                else
                {
                    fincaciamiento = new View_fincaciamientos()
                    {
                        capital = fincaciamiento.capital,
                        CuotasNormales = fincaciamiento.CuotasNormales,
                        Fecha = fincaciamiento.Fecha,
                        PorInt = fincaciamiento.PorInt,
                        Monto = fincaciamiento.Monto,
                        interes = fincaciamiento.interes,
                        Balance = fincaciamiento.Balance,
                        FechaVencimiento = fincaciamiento.FechaVencimiento
                    };
                    fincaciamiento.interes = montoTotalPrestamo * porint / 100;
                }


                fincaciamiento.FechaVencimiento = fecha;
                fincaciamiento.Fecha = returnDate(fecha);
                fincaciamiento.capital = totalCal;
                fincaciamiento.Monto = fincaciamiento.capital + fincaciamiento.interes;
                fincaciamiento.numCuota = i + 1;
                Balance_actual = montoTotalPrestamo - fincaciamiento.capital;
                Balance_actual = Balance_actual < 0 ? 0 : Balance_actual;
                fincaciamiento.Balance = Balance_actual;
                montoTotalPrestamo = Balance_actual;
                fincaciamiento.TipoCuota = TypeCuota.NORMAL.ToString();
                Listfincaciamientos.Add(fincaciamiento);

            }


            #endregion  // Finalizacion calulo 3

            return Listfincaciamientos;
        }

        private List<View_fincaciamientos> calcularInteresSININTERES(View_fincaciamientos fincaciamiento)
        {
            List<View_fincaciamientos> Listfincaciamientos = new List<View_fincaciamientos>();
            decimal totalAdicionales = fincaciamiento.listCuotasAdiccionales.Sum(x => x.capital);
            montoTotalPrestamo = fincaciamiento.Monto - totalAdicionales;
            for (int i = 0; i < fincaciamiento.CuotasNormales; i++)
            {
                DateTime fecha = ReturFechaFormadePago(_TypeFORMAPG, fincaciamiento.FechaVencimiento, i);

                if (i > 0)
                {
                    fincaciamiento = new View_fincaciamientos()
                    {
                        Monto = montoTotalPrestamo,
                        Balance = fincaciamiento.Balance,
                        capital = fincaciamiento.capital,
                        interes = fincaciamiento.interes,
                        PorInt = fincaciamiento.PorInt,
                        numCuota = fincaciamiento.numCuota,
                        CuotasNormales = fincaciamiento.CuotasNormales,
                        Fecha = fincaciamiento.Fecha,
                        FechaVencimiento = fincaciamiento.FechaVencimiento
                    };
                }

                fincaciamiento.FechaVencimiento = fecha;
                fincaciamiento.Fecha = returnDate(fecha);
                fincaciamiento.interes = 0;
                fincaciamiento.capital = montoTotalPrestamo / fincaciamiento.CuotasNormales;

                fincaciamiento.Monto = fincaciamiento.interes + fincaciamiento.capital;
                fincaciamiento.numCuota = i + 1;
                fincaciamiento.Balance = montoTotalPrestamo - (fincaciamiento.capital * fincaciamiento.numCuota);

                fincaciamiento.TipoCuota = TypeCuota.NORMAL.ToString();
                Listfincaciamientos.Add(fincaciamiento);
            }


            return Listfincaciamientos;
        }

        private List<View_fincaciamientos> calcularInteresSOLOINTERES(View_fincaciamientos fincaciamiento)
        {
            List<View_fincaciamientos> Listfincaciamientos = new List<View_fincaciamientos>();
            decimal totalAdicionales = fincaciamiento.listCuotasAdiccionales.Sum(x => x.capital);
            montoTotalPrestamo = fincaciamiento.Monto - totalAdicionales;
            for (int i = 0; i < fincaciamiento.CuotasNormales; i++)
            {
                DateTime fecha = ReturFechaFormadePago(_TypeFORMAPG, fincaciamiento.FechaVencimiento, i);

                if (i > 0)
                {
                    fincaciamiento = new View_fincaciamientos()
                    {
                        Monto = montoTotalPrestamo,
                        Balance = fincaciamiento.Balance,
                        capital = fincaciamiento.capital,
                        interes = fincaciamiento.interes,
                        PorInt = fincaciamiento.PorInt,
                        numCuota = fincaciamiento.numCuota,
                        CuotasNormales = fincaciamiento.CuotasNormales,
                        Fecha = fincaciamiento.Fecha,
                        FechaVencimiento = fincaciamiento.FechaVencimiento
                    };
                }

                fincaciamiento.FechaVencimiento = fecha;
                fincaciamiento.Fecha = returnDate(fecha);
                fincaciamiento.interes = fincaciamiento.Monto * fincaciamiento.PorInt / 100;
                //fincaciamiento.interes = i == fincaciamiento.CuotasNormales - 1 ? 0 : fincaciamiento.Monto * fincaciamiento.PorInt / 100;
                fincaciamiento.capital = i == fincaciamiento.CuotasNormales - 1 ? montoTotalPrestamo : 0;

                fincaciamiento.Monto = fincaciamiento.interes + fincaciamiento.capital;
                fincaciamiento.Balance = montoTotalPrestamo - fincaciamiento.capital;
                fincaciamiento.numCuota = i + 1;
                fincaciamiento.TipoCuota = TypeCuota.NORMAL.ToString();
                Listfincaciamientos.Add(fincaciamiento);
            }


            return Listfincaciamientos;
        }

        private List<View_fincaciamientos> calcularInteresINSOLUTO(View_fincaciamientos fincaciamiento)
        {
            List<View_fincaciamientos> Listfincaciamientos = new List<View_fincaciamientos>();
            decimal _Base = 0, valor = 0, totalCal = 0, porint = 0;
            decimal totalAdicionales = fincaciamiento.listCuotasAdiccionales.Sum(x => x.capital);
            montoTotalPrestamo = fincaciamiento.Monto - totalAdicionales;
            porint = decimal.Parse(fincaciamiento.PorInt.ToString());
            _Base = (1 + porint / 100);
            #region -----------------calculo 1---------------
            for (int i = 0; i < fincaciamiento.CuotasNormales; i++)
            {
                if (i == 0)
                {
                    valor = _Base;
                    continue;
                }


                valor *= _Base;

            }

            totalCal = 1 / valor;
            #endregion // Finalizacion calulo 1

            #region ----------------calculo 2 -----------------
            totalCal = ((montoTotalPrestamo) / ((1 - totalCal) / (porint / 100)));

            #endregion  // Finalizacion calulo 2


            #region ----------------calculo 3 -----------------

            for (int i = 0; i < fincaciamiento.CuotasNormales; i++)
            {
                //var fecha = TypeFORMAPG.MENSUAL != _TypeFORMAPG ? fincaciamiento.FechaVencimiento.AddDays(DayToadd) : fincaciamiento.FechaVencimiento.AddMonths(DayToadd);
                DateTime fecha = ReturFechaFormadePago(_TypeFORMAPG, fincaciamiento.FechaVencimiento, i);

                if (i == 0)
                    fincaciamiento.interes = montoTotalPrestamo * (porint / 100);
                else
                {
                    fincaciamiento = new View_fincaciamientos()
                    {
                        capital = fincaciamiento.capital,
                        CuotasNormales = fincaciamiento.CuotasNormales,
                        Fecha = fincaciamiento.Fecha,
                        PorInt = fincaciamiento.PorInt,
                        Monto = fincaciamiento.Monto,
                        interes = fincaciamiento.interes,
                        Balance = fincaciamiento.Balance,
                        FechaVencimiento = fincaciamiento.FechaVencimiento
                    };
                    fincaciamiento.interes = montoTotalPrestamo * porint / 100;
                }

                fincaciamiento.FechaVencimiento = fecha;
                fincaciamiento.Fecha = returnDate(fecha);
                fincaciamiento.capital = totalCal - fincaciamiento.interes;

                fincaciamiento.Monto = fincaciamiento.capital + fincaciamiento.interes;
                fincaciamiento.numCuota = i + 1;
                Balance_actual = montoTotalPrestamo - fincaciamiento.capital;
                Balance_actual = Balance_actual < 0 ? 0 : Balance_actual;
                fincaciamiento.Balance = Balance_actual;
                montoTotalPrestamo = Balance_actual;
                fincaciamiento.TipoCuota = TypeCuota.NORMAL.ToString();
                Listfincaciamientos.Add(fincaciamiento);

            }


            #endregion  // Finalizacion calulo 3

            return Listfincaciamientos;
        }

        private List<View_fincaciamientos> calcularInteresFijo(View_fincaciamientos fincaciamiento)
        {



            List<View_fincaciamientos> ListFinanciamientos = new List<View_fincaciamientos>();
            for (int i = 0; i < fincaciamiento.CuotasNormales; i++)
            {

                //var fecha = TypeFORMAPG.MENSUAL != TypeFORMAPG ? fincaciamiento.FechaVencimiento.AddDays(DayToadd) : fincaciamiento.FechaVencimiento.AddMonths(DayToadd);
                DateTime fecha = ReturFechaFormadePago(_TypeFORMAPG, fincaciamiento.FechaVencimiento, i);

                if (i > 0)
                {

                    fincaciamiento = new View_fincaciamientos()
                    {
                        capital = fincaciamiento.capital,
                        CuotasNormales = fincaciamiento.CuotasNormales,
                        PorInt = fincaciamiento.PorInt,
                        FechaVencimiento = fincaciamiento.FechaVencimiento,
                        Fecha = fincaciamiento.Fecha

                    };
                }
                fincaciamiento.FechaVencimiento = fecha;
                fincaciamiento.Fecha = returnDate(fecha);
                fincaciamiento.numCuota = i + 1;
                fincaciamiento.capital = montoTotalPrestamo / fincaciamiento.CuotasNormales;
                fincaciamiento.interes = (fincaciamiento.PorInt * montoTotalPrestamo) / 100;
                fincaciamiento.Monto = fincaciamiento.capital + fincaciamiento.interes;
                Balance_actual = montoTotalPrestamo - (fincaciamiento.capital * fincaciamiento.numCuota);
                fincaciamiento.Balance = Balance_actual;
                fincaciamiento.TipoCuota = TypeCuota.NORMAL.ToString();
                ListFinanciamientos.Add(fincaciamiento);
            }
            return ListFinanciamientos;

        }

        private List<View_fincaciamientos> calcularInteresMANUAL(View_fincaciamientos fincaciamiento)
        {
            List<View_fincaciamientos> ListFinanciamientos = new List<View_fincaciamientos>();
            for (int i = 0; i < fincaciamiento.CuotasNormales; i++)
            {

                //var fecha = TypeFORMAPG.MENSUAL != TypeFORMAPG ? fincaciamiento.FechaVencimiento.AddDays(DayToadd) : fincaciamiento.FechaVencimiento.AddMonths(DayToadd);
                DateTime fecha = ReturFechaFormadePago(_TypeFORMAPG, fincaciamiento.FechaVencimiento, i);

                if (i > 0)
                {

                    fincaciamiento = new View_fincaciamientos()
                    {
                        capital = fincaciamiento.capital,
                        CuotasNormales = fincaciamiento.CuotasNormales,
                        PorInt = fincaciamiento.PorInt,
                        FechaVencimiento = fincaciamiento.FechaVencimiento,
                        Fecha = fincaciamiento.Fecha,
                        interes = fincaciamiento.interes

                    };
                }
                fincaciamiento.FechaVencimiento = fecha;
                fincaciamiento.Fecha = returnDate(fecha);
                fincaciamiento.numCuota = i + 1;

                fincaciamiento.Monto = fincaciamiento.capital + fincaciamiento.interes;
                Balance_actual = montoTotalPrestamo - (fincaciamiento.capital * fincaciamiento.numCuota);
                fincaciamiento.Balance = Balance_actual;
                fincaciamiento.TipoCuota = TypeCuota.NORMAL.ToString();
                ListFinanciamientos.Add(fincaciamiento);
            }
            return ListFinanciamientos;
        }

        private DateTime ReturFechaFormadePago(TypeFORMAPG typeFORMAPG, DateTime fechaVencimiento, int index)
        {
            DateTime fechaReturn = DateTime.Now;
            bool dayIncurrent = false;
            if (fechaVencimiento.Day == DateTime.DaysInMonth(fechaVencimiento.Year, fechaVencimiento.Month))
                dayIncurrent = true;
            var id = (int)typeFORMAPG;

            if (id != 1)
            {
                var currentFormaPago = db.FORMAPG.Where(x => x.FOR_CODIGO == id).FirstOrDefault().FOR_PERIOD;
                DayToadd = int.Parse(currentFormaPago.ToString());
            }

            switch (typeFORMAPG)
            {
                case TypeFORMAPG.MENSUAL:
                    fechaReturn = dayIncurrent ? fechaVencimiento.AddDays(1).AddMonths(1).AddDays(-1) : fechaVencimiento.AddMonths(1);
                    break;
                case TypeFORMAPG.DIARIO:
                case TypeFORMAPG.SEMANAL:
                case TypeFORMAPG.QUINCENAL:
                    fechaReturn = fechaVencimiento.AddDays(DayToadd);
                    break;

            }


            return index != 0 ? fechaReturn : fechaVencimiento;
        }

        public JsonResult FechasCuotas(int cuotasNormales, DateTime FechaVencimiento, string FormaPago)
        {

            List<string> dates = new List<string>();
            DateTime FechaVencimiento2 = new DateTime();
            _TypeFORMAPG = (TypeFORMAPG)Enum.Parse(typeof(TypeFORMAPG), FormaPago);
            for (int i = 0; i < cuotasNormales; i++)
            {

                if (i == 0)
                    FechaVencimiento2 = ReturFechaFormadePago(_TypeFORMAPG, FechaVencimiento, i);
                else
                    FechaVencimiento2 = ReturFechaFormadePago(_TypeFORMAPG, FechaVencimiento2, i);
                dates.Add(FechaVencimiento2.ToString("yyyy-MM-dd"));
            }
            return Json(dates.ToArray(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTypeINT()
        {
            db = new DB_IMBRINOEntities();
            var list = db.TIPOINT.Select(x => new { id = x.TIP_CODIGO, value = x.TIP_DESCRI }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetTypeFin()
        {
            db = new DB_IMBRINOEntities();
            var list = db.TIPOFIN.Select(x => new { id = x.TIP_CODIGO, value = x.TIP_DESCRI }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetFormaPago()
        {
            db = new DB_IMBRINOEntities();
            var list = db.FORMAPG.Select(x => new { id = x.FOR_CODIGO, value = x.FOR_DESCRI }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCLientes()
        {
            db = new DB_IMBRINOEntities();
            var list = db.CLIENTE.Select(x => new { id = x.CTE_CODIGO, value = x.CTE_NOMBRE + " " + x.CTE_APELLI }).OrderBy(x => x.value).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);

        }        
       
        public JsonResult GetFiadores()
        {
            db = new DB_IMBRINOEntities();
            var list = db.FIADOR.Select(x => new { id = x.FIA_CODIGO, value = x.FIA_NOMBRE }).OrderBy(x => x.value).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPromotores()
        {
            db = new DB_IMBRINOEntities();
            var list = db.VENDEDOR.Select(x => new { id = x.VEN_CODIGO, value = x.VEN_NOMBRE }).OrderBy(x => x.value).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);

        }
    }
}