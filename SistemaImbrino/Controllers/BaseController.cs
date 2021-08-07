using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    public class BaseController : Controller
    {
        public static DB_IMBRINOEntities db = new DB_IMBRINOEntities();
        public string MensajeErrorCath = "Error inesperado en la aplicacion Favor contactar a un soporte";
        private static string[] listaMeses = { "ENE", "FEB", "MAR", "ABR", "MAY", "JUN", "JUL", "AGO", "SEP", "OCT", "NOV", "DIC" };
        public static string formatoFecha = "MM/dd/yyyy";

      
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
        public enum typeFecha
        {
            FechaR = 10,
            FechaP = 20
        }
        public enum typeIngreso
        {
            SIN_ASIGNAR = 0,
            CUOTA = 1,
            OTROS_INGRESOS = 2
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
            PLACA = 51,
            SEGURO = 52,
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
                case "Saldo cargo adicional":
                    returnTipo_cobro = TipoCobro.Pago;
                    break;
                case "Abono":
                case "Abono incluye pago de mora":
                case "Abono NO pago de la mora":
                case "Abono a cuota":
                case "Abono cargo adicional":
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

            return listCobrosHeader;
        }

        public static bool pagarOtroCargoAdiccional(OTROCARG otroCargo, string fechaPago, TipoCobro status, int numRef)
        {
            bool actualizo = false;
            try
            {
                if (status == TipoCobro.Pago)
                {
                    otroCargo.CAR_FECHAP = fechaPago;
                }
                otroCargo.CAR_STATUS = ((int)status).ToString();
                otroCargo.CAR_NUMREC = numRef.ToString();
                db.Entry(otroCargo).State = EntityState.Modified;
                actualizo = true;
            }
            catch (Exception)
            {

            }
            return actualizo;

        }

        public static string MaxNumAbo(string NumFin)
        {
            var listAboCarg = db.ABOOCARG.Where(x => x.ABO_NUMFIN == NumFin).ToList();
            string numAbo = string.Empty;
            int numAboi = 0;
            if (listAboCarg.Any())
            {
                numAbo = listAboCarg.Max(x => x.ABO_NUMABO);
                int.TryParse(numAbo, out numAboi);
                numAboi++;
            }
            else
            {
                numAboi = 1;
            }
            return numAboi.ToString();
        }


        public static string MaxCarSecun(string NumFin)
        {
            var listAboCarg = db.OTROCARG.Where(x => x.CAR_NUMFIN == NumFin).ToList();
            string numSecun = string.Empty;
            int numSecuni = 0;
            if (listAboCarg.Any())
            {
                numSecun = listAboCarg.Max(x => x.CAR_SECU);
                int.TryParse(numSecun, out numSecuni);
                numSecuni++;
            }
            else
            {
                numSecuni = 1;
            }
            return numSecuni.ToString();
        }
        private static int generateFindID(int counter, string cliente, int codCLiente)
        {
            return counter > 1 ? 0 : MaxFinIdByClient(codCLiente);
        }

        public static List<View_cobrosHeader> CobrosHeader_SP(string cliente = "", bool is_Procedure = false, DateTime? fecha_pago = null)
        {

            var clientesAgrupados = db.sp_cuotasVencidas(fecha_pago).Where(x => x.CLIENTE == cliente).ToList().GroupBy(x => x.C__FIN.ToString());

            var listClientesCuotas =
                clientesAgrupados
                    .Select(x => new
                    {
                        interesTotal = x.Sum(x2 => x2.InteresTotal),
                        capitalTotal = x.Sum(x2 => x2.CapitalTotal),
                        montoTotal = x.Sum(x2 => x2.MONTO),
                        MoraTotal = x.Sum(x2 => x2.mora),
                        cliente = cliente == "" ? x.Max(x2 => x2.CLIENTE) : x.Max(x2 => x2.C__FIN).ToString(),
                        countT = cliente == "" ? x.GroupBy(x2 => x2.C__FIN).Count() : x.Count(),
                        ClienteId = x.Max(x2 => x2.CodigoCLiente)
                    })
                    .ToList();
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


        private static IEnumerable<IGrouping<string, sp_cuotasVencidas_Result>> ClientesAgrupados(string cliente = "", bool is_detail = false)
        {
            IEnumerable<IGrouping<string, sp_cuotasVencidas_Result>> clientesAgrupados = null;
            db = new DB_IMBRINOEntities();
            if (cliente == "" && is_detail == false)
                clientesAgrupados = db.sp_cuotasVencidas(DateTime.Now).ToList().GroupBy(x => x.CLIENTE);
            else
                clientesAgrupados = db.sp_cuotasVencidas(DateTime.Now).Where(x => x.CLIENTE == cliente).ToList().GroupBy(x => x.C__FIN.ToString());
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

        public bool validarCargo(OTROCARG Cargo)
        {
            double montoT = 0;
            double.TryParse(Cargo.CAR_MONTOT.Replace(",", ""), out montoT);
            Cargo.CAR_MONTOT = montoT.ToString();

            var fechaSpt = Cargo.CAR_FECHAR.Split('-');
            bool value = (!string.IsNullOrEmpty(Cargo.CAR_CODCAR) || Cargo.CAR_CODCAR != "0") || fechaSpt.Length == 2;
            return value && !string.IsNullOrEmpty(Cargo.CAR_FECHAR)
                         && montoT > 0
                         && !string.IsNullOrEmpty(Cargo.CAR_NUMFIN);
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
            maxVal = maxVal + 1;
            return maxVal;
        }
        public static string returMonthName(int month)
        {
            return listaMeses[month - 1];
        }

        public static string returDateFormat(DateTime? date)
        {
            bool exist = date.HasValue;
            string currentDate = string.Empty;
            if (exist)
            {
                currentDate = $"{date.Value.Day}/{returMonthName(date.Value.Month)}/{date.Value.Year}";
            }
            return currentDate;
        }

        public static DateTime? returDateFormat(string date)
        {
            DateTime? dateRetur = null;
            var DateByPart = date.Split('-');
            date = $"{returMonthNumber(DateByPart[1])}/{DateByPart[0]}/{DateByPart[2]}";
            if (DateTime.TryParse(date, out DateTime currentDate))
            {
                dateRetur = currentDate;
            }
            return dateRetur;
        }

        public static int returMonthNumber(string monthName)
        {
            int month = Array.FindIndex(listaMeses, x => x == monthName);
            return month + 1;
        }
        public static string getTipoIngreso(string ingresoId)
        {
            var ingresos = db.INGRESO.Where(x => x.ING_CODIGO == ingresoId).ToList();
            string ingreso = ingresos.Any() ? ingresos.FirstOrDefault().ING_DESCRI : string.Empty;
            return ingreso.Trim();
        }

        public static string getBanco(int id)
        {
            var list = db.BANCO.Where(x => x.BCO_CODIGO == id).ToList();
            string value = list.Any() ? list.FirstOrDefault().BCO_NOMBRE : string.Empty;
            return value.Trim();
        }

        public static string getTipoSalida(int id)
        {
            var list = db.TIPOCR2.Where(x => x.ID == id).ToList();
            string value = list.Any() ? list.FirstOrDefault().DESCRIPCION : string.Empty;
            return value.Trim();
        }

        public static string getTipoCredito(int id)
        {
            var list = db.TIPOCR1.Where(x => x.ID == id).ToList();
            string value = list.Any() ? list.FirstOrDefault().DESCRIPCION : string.Empty;
            return value.Trim();
        }
        public static string getTipoEntrada(int id)
        {
            var list = db.TIPODB1.Where(x => x.ID == id).ToList();
            string value = list.Any() ? list.FirstOrDefault().DESCRIPCION : string.Empty;
            return value.Trim();
        }
        public static string getTipoDebito(int id)
        {
            var list = db.TIPODB2.Where(x => x.ID == id).ToList();
            string value = list.Any() ? list.FirstOrDefault().DESCRIPCION : string.Empty;
            return value.Trim();
        }

        public static string getCliente(int id)
        {
            var list = db.CLIENTE.Where(x => x.CTE_CODIGO == id).ToList();
            string value = list.Any() ?
                            $"{list.FirstOrDefault().CTE_NOMBRE} {list.FirstOrDefault().CTE_APELLI}"
                            : string.Empty;
            return value.Trim();
        }

        public static string getCuentaBancaria(int id)
        {
            var list = db.CTABANCO.Where(x => x.CTA_CODIGO == id).ToList();
            string value = list.Any() ? list.FirstOrDefault().CTA_NUMERO : string.Empty;
            return value.Trim();
        }


        public static message GuardarPrestamo(View_ListFincaciamientos financiamiento, DateTime fecha, OTROSCR OtroCargo)
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
                GuardarCargo(OtroCargo, financiamiento.ListFinanciamientos.FirstOrDefault());

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

        private static bool GuardarCargo(OTROSCR otroCargo, View_fincaciamientos fincaciamientos)
        {
            bool insertado = false;
            try
            {
                otroCargo.BENEFICIARIO = fincaciamientos.Cliente;
                otroCargo.TIPO_SALIDA = 1;
                otroCargo.MONTO = fincaciamientos.Monto;
                otroCargo.FECHA = returDateFormat(fincaciamientos.Fecha);
                otroCargo.Activo = true;
                db.OTROSCR.Add(otroCargo);
                insertado = true;
            }
            catch (Exception)
            {
                insertado = false;
            }

            return insertado;
        }

        public View_CuadreCajaGeneral GetListCuadreCaja(DateTime? cierreFecha = null)
        {
            db = new DB_IMBRINOEntities();
            List<String> ListOtrosIngresosNetos = new List<string>()
            {
                "OTROS_EC","OTROS_DT"
            };

            List<String> ListTotalIngresado = new List<string>()
            {
                "CUOTAS_EC","CUOTAS_DT"
            };

            var CajaGeneral = new View_CuadreCajaGeneral()
            {
                Detalle = db.vw_CuadreCaja
                        .GroupBy(x => new
                        {
                            x.ID,
                            x.Tipo,
                            x.TextoTipo
                        })
                        .Select(x => new View_CuadreCaja
                        {
                            ID = x.Key.ID,
                            Tipo = x.Key.Tipo,
                            TipoTexto = x.Key.TextoTipo,
                            TotalTipo = x.Where(z => z.Tipo == x.Key.Tipo)
                                                        .Sum(z => z.MontoTotal),
                            TotalTipoAnterior = db.vw_CuadreCaja.Where(z => z.ID == x.Key.ID - 1)
                                                        .Sum(z => z.MontoTotal),
                            Detalle = x.Where(z => z.Tipo == x.Key.Tipo)
                                                    .Select(z => new View_CuadreCajaDetalle
                                                    {
                                                        Recibo = z.Recibo,
                                                        Cliente = z.Cliente,
                                                        Descripcion = z.Descripcion,
                                                        MontoCapital = z.MontoCapital,
                                                        MontoInteres = z.MontoInteres,
                                                        MontoTotal = z.MontoTotal,
                                                        Fecha = z.Fecha,
                                                        FechaTexto = z.FechaTexto
                                                    }),
                        })
                        .OrderBy(z => z.ID),
                Resumen = db.vw_CuadreCaja
                        .GroupBy(x => new
                        {
                            x.ID,
                            x.Tipo,
                            x.TextoTipo
                        })
                        .Select(x => new View_CuadreCajaResumen()
                        {
                            TotalCapital = db.vw_CuadreCaja.Where(z => ListTotalIngresado
                                                .Contains(z.Tipo))
                                            .Sum(z => z.MontoCapital),
                            TotalInteres = db.vw_CuadreCaja.Where(z => ListTotalIngresado
                                                .Contains(z.Tipo))
                                            .Sum(z => z.MontoInteres),
                            IngresosNoNetos = db.vw_CuadreCaja.Where(z => z.Tipo == "INGRESOS_NN")
                                            .Sum(z => z.MontoTotal),
                            OtrosIngresosNetos =
                                    db.vw_CuadreCaja.Where(z => ListOtrosIngresosNetos
                                                .Contains(z.Tipo))
                                        .Sum(z => z.MontoTotal),
                            DepositoTranferencia = db.vw_CuadreCaja.Where(z => z.Tipo == "SALIDAS")
                                            .Sum(z => z.MontoTotal),
                        })
                        .FirstOrDefault()
            };
            return CajaGeneral;
        }

        public message validarMetodoPago(string tipoPago, OTROSDB Deposito)
        {
            message message = new message();
            List<string> menssageErrors = new List<string>();

            switch (tipoPago)
            {
                case "E":
                    if (Deposito != null)
                    {
                        menssageErrors
                            .Add("El tipo de pago <b>efectivo</b> no necesita informacion de deposito");
                    }
                    break;
                case "D":
                    if (Deposito == null)
                    {
                        menssageErrors
                            .Add("El tipo de pago <b>deposito</b> necesita informacion de deposito");
                    }
                    else if(Deposito.TIPO_ENTRADA != 1)
                    {
                        menssageErrors
                             .Add("El tipo de entrada tiene que ser <b>DEPOSITO</b>");
                    }
                    break;
                case "T":
                    if (Deposito == null)
                    {
                        menssageErrors
                            .Add("El tipo de pago <b>Tarjeta</b> necesita informacion de deposito");
                    }
                    else if (Deposito.TIPO_ENTRADA != 2)
                    {
                        menssageErrors
                             .Add("El tipo de entrada tiene que ser <b>TRANFERENCIA</b>");
                    }
                    break;
                case "C":
                    if (Deposito != null)
                    {
                        menssageErrors
                            .Add("El tipo de pago <b>cheque</b> no necesita informacion de deposito");
                    }
                    break;
                default:
                    menssageErrors
                            .Add("Se requiere un metodo de pago");
                    break;
            }

            validarDeposito(Deposito, menssageErrors);

            message.Message = string.Join("<br>", menssageErrors);
            message.Is_Success = !menssageErrors.Any();
           
            return message;
        }

        private void validarDeposito(OTROSDB deposito, List<string> menssageErrors)
        {
            if (string.IsNullOrEmpty(getBanco(deposito.BANCO)))
            {
                menssageErrors
                    .Add($"El campo <b>{nameof(deposito.BANCO)}</b> no puede ser vacio");
            }
            if (string.IsNullOrEmpty(getCuentaBancaria(deposito.CUENTA_BANCARIA)))
            {
                menssageErrors
                    .Add($"El campo <b>{nameof(deposito.CUENTA_BANCARIA).Replace("_"," ")}</b> no puede ser vacio");
            }
            if (string.IsNullOrEmpty(getTipoDebito(deposito.TIPO_DEBITO)))
            {
                menssageErrors
                    .Add($"El campo <b>{nameof(deposito.TIPO_DEBITO).Replace("_", " ")}</b> no puede ser vacio");
            }

            if (string.IsNullOrEmpty(getTipoEntrada(deposito.TIPO_ENTRADA)))
            {
                menssageErrors
                    .Add($"El campo <b>{nameof(deposito.TIPO_ENTRADA).Replace("_", " ")}</b> no puede ser vacio");
            }
            if (deposito.MONTO < 1)
            {
                menssageErrors
                    .Add($"El campo <b>{nameof(deposito.MONTO)}</b> tiene que ser mayor que cero");
            }
            if (deposito.FECHA == null)
            {
                menssageErrors
                    .Add($"El campo <b>{nameof(deposito.FECHA)}</b> tiene un formato incorrecto");
            }
        }
    }
}