using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    public class BaseController : Controller
    {
        public static DB_IMBRINOEntities db = new DB_IMBRINOEntities();
        public string MensajeErrorCath = "Error inesperado en la aplicacion Favor contactar a un soporte";

        public enum TipoCobro
        {
            Pago = 11
           , Abono = 12
        }
        public enum TotalCuotas
        {
            pendiente = 1
          , total = 2
        }

        public enum TypeInteres
        {
            FIJO = 1,
            INSOLUTO = 2,
            SOLOINTERES = 3,
            SININTERES = 4,
            VINSOLUTO = 5,
            MANUAL = 6
        }

        public enum TypeCuota
        {
            NORMAL = 1,
            ADICIONAL = 2
        }

        public enum TypeFORMAPG
        {
            MENSUAL = 1,
            QUINCENAL = 2,
            SEMANAL = 3,
            DIARIO = 4
        }

        public enum Status
        {
            NUEVO = 5,
            NUEVO_CERRADO = 1,
            ABONO = 12,
            PAGADO = 11
        }
        public enum typeOTRO
        {
            INVERSIONES = 1,
            TRANSFERENCIA_BANCARIA = 2,
            VENTA_DOLARES = 3,
            REPOSICION_CHEQUE_DEVUELT = 4,
            VENTA_DE_ACTIVO = 5,
            REPOS_RETIRO_PROVICIONAL = 6,
            CORRECION_REGISTROS = 7,
            DOLARES = 8,
            CUENTA_POR_PAGAR = 9,
            CUENTA_POR_COBRAR = 10,
            ABONOO_CANC__CTA_X_COBRA = 11,
            ABONO_OCANCEL_CTAX_PAGAR = 12,
            PENALPOR_LLAMADA = 13,
            GASTOS_REGISTRO_HIPOTECA = 14,
            MORA = 50,
            OTROS_INGRESOS = 51,
            PENALIDAD_POR_CH_DEV = 52,
            COMISION_CECOMSA = 53,
            INTERESES_PENDIENTES = 54,
            RECUPERACION_CREDITOS_CDC = 56
        }


        public static TipoCobro ReturnTipoCobro(string tipo_Cobro)
        {
            TipoCobro returnTipo_cobro = TipoCobro.Pago;
            switch (tipo_Cobro)
            {
                case "Pago":
                case "Saldo incluye pago de mora":
                case "Saldo NO pago de la mora":
                case "Saldo cuota":
                    returnTipo_cobro = TipoCobro.Pago;
                    break;
                case "Abono":
                case "Abono incluye pago de mora":
                case "Abono NO pago de la mora":
                case "Abono a cuota":
                    returnTipo_cobro = TipoCobro.Abono;
                    break;

            }

            return returnTipo_cobro;


        }

        public static List<View_cobrosHeader> CobrosHeader(string cliente = "", bool is_Procedure = false)
        {
            var clientesAgrupados = ClientesAgrupados(cliente, false);

            List<View_cobrosHeader> listCobrosHeader = clientesAgrupados.Select(x => new View_cobrosHeader
            {
                interesTotal = x.Sum(x2 => x2.InteresTotal),
                capitalTotal = x.Sum(x2 => x2.CapitalTotal),
                montoTotal = x.Sum(x2 => x2.MONTO),
                moraTotal = x.Sum(x2 => x2.mora),
                cliente = cliente == "" ? x.Key : x.FirstOrDefault().C__FIN.ToString(),
                FinID = generateFindID(cliente == "" ? x.GroupBy(x2 => x2.C__FIN).Count() : x.Count(), cliente, x.Max(x2 => x2.CodigoCLiente)),//(cliente == "" ? x.GroupBy(x2 => x2.C__FIN).Count() : x.Count()) > 1 ? 0 : MaxFinIdByClient(x.Max(x2 => x2.CodigoCLiente)),
                ClienteId = x.Max(x2 => x2.CodigoCLiente),
                CountT = cliente == "" ? x.GroupBy(x2 => x2.C__FIN).Count() : x.Count()
            }
           ).ToList();


            //List<View_cobrosHeader> listCobrosHeader = clientesAgrupados.Select(x => new
            //{
            //    interesTotal = x.Sum(x2 => x2.InteresTotal),
            //    capitalTotal = x.Sum(x2 => x2.CapitalTotal),
            //    montoTotal = x.Sum(x2 => x2.MONTO),
            //    MoraTotal = x.Sum(x2 => x2.mora),
            //    cliente = cliente == "" ? x.Max(x2 => x2.CLIENTE) : x.Max(x2 => x2.C__FIN).ToString(),
            //    countT = cliente == "" ? x.GroupBy(x2 => x2.C__FIN).Count() : x.Count(),
            //    ClienteId = x.Max(x2 => x2.CodigoCLiente)
            //}
            //).ToList().Select(lcc => new View_cobrosHeader()
            //{
            //    cliente = lcc.cliente,
            //    capitalTotal = lcc.capitalTotal,
            //    interesTotal = lcc.interesTotal,
            //    montoTotal = lcc.montoTotal,
            //    CountT = lcc.countT,
            //    ClienteId = lcc.ClienteId,
            //    moraTotal = lcc.MoraTotal,
            //    FinID = lcc.countT > 1 ? 0 : MaxFinIdByClient(lcc.ClienteId)

            //}).ToList();


            return listCobrosHeader;
        }

        private static int generateFindID(int counter, string cliente, int codCLiente)
        {
            return counter > 1 ? 0 : MaxFinIdByClient(codCLiente);
        }

        public static List<View_cobrosHeader> CobrosHeader_SP(string cliente = "", bool is_Procedure = false, DateTime? fecha_pago = null)
        {

            var clientesAgrupados = db.sp_cuotasVencidas(fecha_pago).Where(x => x.CLIENTE == cliente).ToList().GroupBy(x => x.C__FIN.ToString());

            var listClientesCuotas = clientesAgrupados.Select(x => new
            {
                interesTotal = x.Sum(x2 => x2.InteresTotal)
               ,
                capitalTotal = x.Sum(x2 => x2.CapitalTotal)
               ,
                montoTotal = x.Sum(x2 => x2.MONTO)
               ,
                MoraTotal = x.Sum(x2 => x2.mora)
               ,
                cliente = cliente == "" ? x.Max(x2 => x2.CLIENTE) : x.Max(x2 => x2.C__FIN).ToString()
               ,
                countT = cliente == "" ? x.GroupBy(x2 => x2.C__FIN).Count() : x.Count()
               ,
                ClienteId = x.Max(x2 => x2.CodigoCLiente)


            }
                                                ).ToList();
            List<View_cobrosHeader> listCobrosHeader = new List<View_cobrosHeader>();

            foreach (var lcc in listClientesCuotas)
            {
                listCobrosHeader.Add(new View_cobrosHeader
                {
                    cliente = lcc.cliente,
                    capitalTotal = lcc.capitalTotal,
                    interesTotal = lcc.interesTotal,
                    montoTotal = lcc.montoTotal,
                    CountT = lcc.countT,
                    ClienteId = lcc.ClienteId,
                    moraTotal = lcc.MoraTotal
                    ,
                    FinID = lcc.countT > 1 ? 0 : MaxFinIdByClient(lcc.ClienteId)
                });
            }

            return listCobrosHeader;
        }


        private static IEnumerable<IGrouping<string, VW_rptCuotasVencidas>> ClientesAgrupados(string cliente = "", bool is_detail = false)
        {
            IEnumerable<IGrouping<string, VW_rptCuotasVencidas>> clientesAgrupados = null;

            if (cliente == "" && is_detail == false)
                clientesAgrupados = db.VW_rptCuotasVencidas.ToList().GroupBy(x => x.CLIENTE);
            else
                clientesAgrupados = db.VW_rptCuotasVencidas.Where(x => x.CLIENTE == cliente).ToList().GroupBy(x => x.C__FIN.ToString());
            return clientesAgrupados;
        }



        private static int MaxFinIdByClient(int clienteID)
        {
            return db.VW_rptCuotasVencidas.Where(x => x.CodigoCLiente == clienteID).FirstOrDefault().C__FIN;
        }

        public string returnDate(DateTime fecha)
        {
            string fechaReturn = string.Format("{0}-{1}-{2}", fecha.Day, returMonthName(fecha.Month), fecha.Year);

            return fechaReturn;
        }

        public bool validarCliente(CLIENTE cLIENTE)
        {
            bool value = cLIENTE.CTE_TIPO.HasValue && (!string.IsNullOrEmpty(cLIENTE.CTE_ZONA) || cLIENTE.CTE_ZONA != "0");
            return value && !string.IsNullOrEmpty(cLIENTE.CTE_NOMBRE)
                         && !string.IsNullOrEmpty(cLIENTE.CTE_APELLI)
                         && !string.IsNullOrEmpty(cLIENTE.CTE_TELEFO)
                         && !string.IsNullOrEmpty(cLIENTE.CTE_CEDULA)
                         && !string.IsNullOrEmpty(cLIENTE.CTE_DIRECC);

        }
        public bool validarFiador(FIADOR FIADOR)
        {
            return !string.IsNullOrEmpty(FIADOR.FIA_NOMBRE)
                         && !string.IsNullOrEmpty(FIADOR.FIA_TELEFO)
                         && !string.IsNullOrEmpty(FIADOR.FIA_CEDULA)
                         && !string.IsNullOrEmpty(FIADOR.FIA_DIRECC);
        }
        public int lastCodCliente()
        {
            int maxVal = db.CLIENTE.Any() 
                        ? db.CLIENTE.Max(x => x.CTE_CODIGO) 
                        : 0;
            maxVal++;
            return maxVal;
        }
        public int lastCodFiador()
        {
            int maxVal = db.FIADOR.Any() 
                        ? db.FIADOR.Max(x => x.FIA_CODIGO)
                        : 0;
            maxVal++;
            return maxVal;
        }
        public static string returMonthName(int month)
        {
            string[] str = { "ENE", "FEB", "MAR", "ABR", "MAY", "JUN", "JUL", "AGO", "SEP", "OCT", "NOV", "DIC" };
            return str[month - 1];
        }

        public static message GuardarPrestamo(View_ListFincaciamientos financiamiento, DateTime fecha)
        {
            message message = new message();
            var tran = db.Database.BeginTransaction();
            string status = ((int)Status.NUEVO).ToString();
            try
            {


                List<CUOTA> ListCuotas = new List<CUOTA>();

                var ListFin = financiamiento.ListFinanciamientos;
                int LastFinancy = 0, LastFactura = 0, clienteID = 0, fiadorID = 0, promotorID = 0;
                View_fincaciamientos firtFina = new View_fincaciamientos();
                double monto = 0, porInt = 0, interes = 0, capital = 0, montoTotal = 0, capitalTotal = 0, interesTotal = 0;

                if (ListFin.Any() == false)
                {
                    message.Message = "No existen registros para insertar";
                    message.Is_Success = false;
                    tran.Rollback();
                    return message;
                }

                double.TryParse(ListFin.Sum(x => x.Monto).ToString("n2"), out montoTotal);
                double.TryParse(ListFin.Sum(x => x.capital).ToString("n2"), out capitalTotal);
                double.TryParse(ListFin.Sum(x => x.interes).ToString("n2"), out interesTotal);



                // -------------------------- Guardar en tabla Financy --------------------------

                firtFina = ListFin.FirstOrDefault();

                var CurrentFinancy = db.FINANCY.ToList();
                var CurrentFactura = db.FACTURA.ToList();
                LastFinancy = CurrentFinancy.Any() ? CurrentFinancy.Max(x => x.FIN_NUMERO) + 1 : 1;
                LastFactura = CurrentFactura.Any() ? CurrentFactura.Max(x => x.FAC_NUMERO) + 1 : 1;
                int.TryParse(firtFina.Fiador, out fiadorID);
                int.TryParse(firtFina.Promotor, out promotorID);
                int.TryParse(firtFina.Cliente, out clienteID);


                FINANCY fINANCY = new FINANCY()
                {
                    FIN_NUMERO = LastFinancy,
                    FIN_NUMCTE = clienteID,
                    FIN_NUMFAC = LastFactura,
                    FIN_NUMFIA = fiadorID,
                    FIN_NUMVEN = promotorID,
                    FIN_STATUS = (int)Status.NUEVO
                };



                db.FINANCY.Add(fINANCY);


                // -------------------------- Guardar en tabla  FACTURA --------------------------

                double.TryParse(firtFina.PorInt.ToString(), out porInt);


                FACTURA fACTURA = new FACTURA()
                {
                    FAC_NUMERO = LastFactura,
                    FAC_NUMFIN = LastFinancy,
                    FAC_TIPINT = firtFina.TipoInteres,
                    FAC_FORMPG = firtFina.FormaPago,
                    FAC_TIPFIN = firtFina.TipoFin,
                    FAC_MONTO = montoTotal,
                    FAC_FECHA = fecha.ToString("M/dd/yyyy"),
                    FAC_NUMCUO = ListFin.Count(),
                    FAC_INTERE = porInt,
                    FAC_GARANT = firtFina.Garantia,
                    FAC_STATUS = status

                };
                db.FACTURA.Add(fACTURA);

                // -------------------------- Guardar en tabla   CUOTA --------------------------


                foreach (View_fincaciamientos item in ListFin)
                {
                    double.TryParse(item.Monto.ToString("n2"), out monto);
                    double.TryParse(item.capital.ToString("n2"), out capital);
                    double.TryParse(item.interes.ToString("n2"), out interes);

                    CUOTA cUOTA = new CUOTA()
                    {
                        CUO_NUMFAC = LastFactura,
                        CUO_NUMCUO = item.numCuota,
                        CUO_NUMREC = 0,
                        CUO_STATUS = status,
                        CUO_MONTOT = monto,
                        CUO_MONTOC = capital,
                        CUO_MONTOI = interes,
                        CUO_FECHAV = item.FechaVencimiento.ToString("M/dd/yyyy"),
                        CUO_FECHAP = null,
                        CUO_TIPO = item.TipoCuota == "NORMAL" ? "N" : "A",
                        CUO_STAANT = null


                    };
                    ListCuotas.Add(cUOTA);
                }

                db.CUOTA.AddRange(ListCuotas);

                // -------------------------- Guardar en tabla ESTCUENTA --------------------------
                ESTDCTA eSTDCTA = new ESTDCTA()
                {
                    EST_NUMFIN = LastFinancy,
                    EST_BALINI = montoTotal,
                    EST_CAPINI = capitalTotal,
                    EST_INTINI = interesTotal,
                    EST_OTRCAR = 0,
                    EST_BALACT = montoTotal,
                    EST_CAPPAG = 0,
                    EST_CAPPER = 0,
                    EST_CARNRP = 0,
                    EST_CARRYP = 0,
                    EST_INTPAG = 0,
                    EST_INTPER = 0,
                    EST_STATUS = status
                };
                db.ESTDCTA.Add(eSTDCTA);

                // -------------------------- Actualizar en tabla CARTERA --------------------------
                var cartera = db.CARTERA.FirstOrDefault();

                cartera.CAR_MONTOC += capitalTotal;
                cartera.CAR_MONTOI += interesTotal;


                db.SaveChanges();
                message.Is_Success = true;
                message.Message = "Prestamo creado correctamente";
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                message.Is_Success = false;
                message.Message = "Error inesperado : No se puede activar el prestamo";
            }
            return message;
        }
    }
}