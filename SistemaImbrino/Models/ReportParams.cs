using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using SistemaImbrino.App_Start;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SistemaImbrino.Models
{
    public class ReportParams<T>
    {
        public string RptFileName { get; set; }
        public string ReportTitle { get; set; }
        public List<T> DataSource { get; set; }
        public bool IsPassParamToCr { get; set; }
        public List<Parameters> ListParameters { get; set; }



    }



    public class Parameters
    {
        public string ParameterName { get; set; }
        public object ParameterValue { get; set; }
        public static readonly string rutaReporte = "~/Reportes";
        private static string[] listaMeses = { "ENE", "FEB", "MAR", "ABR", "MAY", "JUN", "JUL", "AGO", "SEP", "OCT", "NOV", "DIC" };
        

        public enum ReportName
        {
            EstadoCuenta,
            CuotasVencidas,
            Financiamientos_atrasados,
            RegistroNCF,
            IngresoCuotas,
            FinHeader,
            FinDetalle,
            CreditosBancarios,
            DebitosBancarios,
            CuadreCaja,
            ReciboIngreso,
            ConciliacionBancaria,
            CarteraPrestamos
        }


        static string CurrentCriterio = "";

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
                currentDate = $"{date.Value.ToString("dd")}/{returMonthName(date.Value.Month)}/{date.Value.ToString("yyyy")}";
            }
            return currentDate;
        }
        public static DataTable CreateDataTable<T>(IEnumerable<T> list)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static void guardarReporte(HttpResponseBase Response, ReportName _ReportName, List<Parameters> Listcriterios, string intialPath)
        {
            DB_IMBRINOEntities db = new DB_IMBRINOEntities();
            ReportDocument CrReport = new ReportDocument();

            string urlReporte = intialPath;
            List<Parameters> parameters = new List<Parameters>();
            string criterios = string.Empty;
            string txt_ReportName = _ReportName.ToString();
            string data = string.Empty;
            string userName = "Imbrino";
            string tituloReporte = string.Empty;
           

            switch (_ReportName)
            {
                case ReportName.EstadoCuenta:
                    tituloReporte = "Estados de Cuenta";
                    IQueryable<VW_rptEstadoCuenta> _iqEC = db.VW_rptEstadoCuenta;
                    List<VW_rptEstadoCuenta> _listEC = new List<VW_rptEstadoCuenta>();
                    urlReporte = Path.Combine(urlReporte, txt_ReportName + ".rpt");
                    _listEC = criteriosEstadoCuenta(Listcriterios, ref criterios, ref _iqEC);
                    _listEC = _listEC.OrderBy(x => x.CLIENTE).ThenBy(x => x.Fecha).ToList();

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listEC, CrReport);
                    break;
                case ReportName.CuotasVencidas:
                    tituloReporte = "Cuotas Vencidas";
                    IQueryable<VW_rptCuotasVencidas> _iqCV = db.VW_rptCuotasVencidas;
                    List<VW_rptCuotasVencidas> _listCV = new List<VW_rptCuotasVencidas>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listCV = criteriosCuotasVencidas(Listcriterios, ref criterios, ref _iqCV);
                    _listCV = _listCV.OrderBy(x => x.CLIENTE).ThenBy(x => x.fechadt).ToList();

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listCV, CrReport);
                    break;
                case ReportName.Financiamientos_atrasados:
                    tituloReporte = "Financiamientos Atrasados";
                    IQueryable<VW_rptFinAtrasados_details> _iqFA = db.VW_rptFinAtrasados_details;
                    List<VW_rptFinAtrasados_details> _listFA = new List<VW_rptFinAtrasados_details>();
                    urlReporte = string.Format("{0}/{1}.rpt", urlReporte, txt_ReportName);
                    _listFA = criteriosFin_atrasados(Listcriterios, ref criterios, ref _iqFA);
                    _listFA = _listFA.OrderBy(x => x.CLIENTE).ThenBy(x => x.fechadt).ToList();

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listFA, CrReport);
                    break;
                case ReportName.RegistroNCF:
                    tituloReporte = "Número de Comprobante Fiscal (NCF)";

                    IQueryable<VW_rptRegistroNCF> _iqRN = db.VW_rptRegistroNCF.Where(x => x.ING_MONTOT > 0);
                    List<VW_rptRegistroNCF> _listRN = new List<VW_rptRegistroNCF>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listRN = criteriosRegistroNCF(Listcriterios, ref criterios, ref _iqRN);
                    _listRN = _listRN.OrderBy(x => x.ING_NCF).ThenBy(x => x.fechadt).ToList();

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listRN, CrReport);
                    break;

                case ReportName.IngresoCuotas:
                    tituloReporte = "Ingresos";
                    IQueryable<VW_rptRegistroNCF> _iqIC = db.VW_rptRegistroNCF.Where(x => x.ING_MONTOT > 0);
                    List<VW_rptRegistroNCF> _listIC = new List<VW_rptRegistroNCF>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listIC = criteriosIngresoCuotas(Listcriterios, ref criterios, ref _iqIC);
                    
                    var ordenarPor = Listcriterios.Where(x => x.ParameterName == "OrdenarPor").FirstOrDefault().ParameterValue;
                    switch (ordenarPor)
                    {
                        case "Cliente":
                            _listIC = _listIC.OrderBy(x => x.Cliente).ThenBy(x => x.ING_NUMREC).ToList();
                            break;
                        case "Recibo":
                            _listIC = _listIC.OrderBy(x => x.ING_NUMREC).ToList();
                            break;
                        case "Monto":
                            _listIC = _listIC.OrderByDescending(x => x.ING_MONTOT + x.OTRO).ToList();
                            break;
                    }

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listIC, CrReport);
                    break;
                case ReportName.FinHeader:
                    tituloReporte = "Financiamientos";
                    data = Listcriterios.FirstOrDefault().ParameterValue.ToString();
                    List<vw_ConsultaFin> _listFH = new List<vw_ConsultaFin>();
                    var consulta = Newtonsoft.Json.JsonConvert.DeserializeObject<List<View_consultaFinanciamientos>>(data);
                    _listFH = consulta
                                .Select(x => new vw_ConsultaFin
                                {
                                    FAC_NUMFIN = x.FinID,
                                    cliente = x.Cliente,
                                    BalanceTotal = x.detail.Balance,
                                    CUO_MONTOC = x.detail.Capital,
                                    CUO_MONTOI = x.detail.Interes,
                                    CUO_MONTOT = x.detail.Monto,
                                    CUO_NUMCUO = x.detail.Cuotas,
                                    FAC_FECHA = x.Fecha,
                                    CUO_STATUS = string.Empty
                                })
                                .OrderBy(c=>c.cliente)
                                .ToList();
                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listFH, CrReport);
                    break;
                case ReportName.FinDetalle:
                    tituloReporte = "Detalle del Financiamiento";
                    IQueryable<vw_ConsultaFinDetalle> _iqFD = db.vw_ConsultaFinDetalle;
                    List<vw_ConsultaFinDetalle> _listFD = new List<vw_ConsultaFinDetalle>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listFD = criteriosFinDetalle(Listcriterios, ref criterios, ref _iqFD);

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listFD, CrReport);
                    break;
                case ReportName.CreditosBancarios:
                    IQueryable<VW_rptCreditosBancarios> _iqCB = db.VW_rptCreditosBancarios.OrderBy(x => x.FECHA);
                    List<VW_rptCreditosBancarios> _listCB = new List<VW_rptCreditosBancarios>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listCB = criteriosCreditosBancarios(Listcriterios, ref criterios, ref _iqCB);
                    tituloReporte = $"Créditos Bancario {criterios}";

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listCB, CrReport);
                    parameters.Remove(parameters.Where(x => x.ParameterName == "Criterios").FirstOrDefault());
                    break;
                case ReportName.DebitosBancarios:
                    IQueryable<VW_rptDebitosBancarios> _iqDB = db.VW_rptDebitosBancarios.OrderBy(x => x.FECHA);
                    List<VW_rptDebitosBancarios> _listDB = new List<VW_rptDebitosBancarios>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listDB = criteriosDebitosBancarios(Listcriterios, ref criterios, ref _iqDB);
                    tituloReporte = $"Débitos Bancario {criterios}";

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listDB, CrReport);
                    parameters.Remove(parameters.Where(x => x.ParameterName == "Criterios").FirstOrDefault());
                    break;
                case ReportName.CuadreCaja:
                    View_CuadreCajaGeneral _listCC = View_generalClass.GetListCuadreCaja();
                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    criteriosCuadreCajas(Listcriterios, ref criterios);
                    var resumen = _listCC.Resumen;
                    tituloReporte = $"Cuadre de Caja {criterios}";

                    List<View_CuadreCajaReport> result = new List<View_CuadreCajaReport>();
                    foreach (var x in _listCC.Detalle)
                    {
                        var current = x.Detalle
                            .Where(j => j.Fecha.HasValue)
                            .Select(j => new View_CuadreCajaReport
                            {
                                ID = x.ID,
                                Cliente = j.Cliente,
                                Descripcion = j.Descripcion,
                                Fecha = j.Fecha.Value,
                                FechaTexto = j.FechaTexto,
                                MontoCapital = j.MontoCapital.HasValue ?
                                                    j.MontoCapital.Value : 0,
                                MontoInteres = j.MontoInteres.HasValue ? j.MontoInteres.Value : 0,
                                MontoTotal = j.MontoTotal.HasValue ? j.MontoTotal.Value : 0,
                                Recibo = j.Recibo,
                                Tipo = x.Tipo,
                                TextoTipo = x.TipoTexto
                            })
                            .ToList();
                        result.AddRange(current);
                    }
                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, result, CrReport);
                    parameters.Remove(parameters.Where(x => x.ParameterName == "Criterios").FirstOrDefault());
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "Deposito", ParameterValue = resumen.DepositoTranferencia },
                        new Parameters() { ParameterName = "IngresosNoNetos", ParameterValue = resumen.IngresosNoNetos },
                        new Parameters() { ParameterName = "OtrosIngresosNetos", ParameterValue = resumen.OtrosIngresosNetos },
                        new Parameters() { ParameterName = "CapitalRecuperado", ParameterValue = resumen.TotalCapital },
                        new Parameters() { ParameterName = "InteresGenerado", ParameterValue = resumen.TotalInteres },
                    });
                    break;
                case ReportName.ReciboIngreso:
                    IQueryable<VW_rptReciboIngreso> _iqRI = db.VW_rptReciboIngreso;
                    List<VW_rptReciboIngreso> _listRI = new List<VW_rptReciboIngreso>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listRI = criteriosReciboIngreso(Listcriterios, ref criterios, ref _iqRI);

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listRI, CrReport, false);
                    parameters.RemoveAll(j => j.ParameterName.Length > 0);
                    break;
                case ReportName.ConciliacionBancaria:
                    string FiltroCuenta = Listcriterios.Where(x => x.ParameterName == "FiltroCuenta").FirstOrDefault().ParameterValue.ToString();
                    string cuenta = FiltroCuenta.Split('#')[1].Trim();
                    DateTime? fechaCorte = getFechaCorte(cuenta);
                    IQueryable<VW_ConciliacionBancaria> _iqCONB = db.VW_ConciliacionBancaria.Where(x => x.CUENTA == cuenta).OrderBy(x => x.FECHA);
                    List<VW_ConciliacionBancaria> _listCONB = new List<VW_ConciliacionBancaria>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listCONB = criteriosConciliacionBancaria(Listcriterios, ref criterios, ref _iqCONB);
                    string filtroFechas = Listcriterios.Where(x => x.ParameterName == "isMovimiento").FirstOrDefault().ParameterValue.ToString() == "True" ?
                            "Movimiento bancario" : "Estado de cuenta";

                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listCONB, CrReport);
                    parameters.RemoveAll(j => j.ParameterName.Length > 0);
                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "FiltroFechas", ParameterValue = $"{filtroFechas} del {criterios}" },
                        new Parameters() { ParameterName = "FiltroCuenta", ParameterValue = Listcriterios.Where(x=> x.ParameterName == "FiltroCuenta").FirstOrDefault().ParameterValue },
                        new Parameters() { ParameterName = "FechaCorte", ParameterValue = returDateFormat(fechaCorte) },
                        new Parameters() { ParameterName = "BalanceCorte", ParameterValue =  Listcriterios.Where(x=> x.ParameterName == "Balance").FirstOrDefault().ParameterValue },
                        new Parameters() { ParameterName = "isMovimiento", ParameterValue =  Listcriterios.Where(x=> x.ParameterName == "isMovimiento").FirstOrDefault().ParameterValue },
                    });
                    break;
                case ReportName.CarteraPrestamos:
                    var carteraPrestamo = Task.Run<View_CarteraPrestamos>(() => { return View_generalClass.getCarteraPrestamos(); }).Result;
                    List<View_DetalleCuenta> _listCART = carteraPrestamo.Detalle.ToList();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    loadReport(tituloReporte, urlReporte, userName, criterios, parameters, _listCART, CrReport);
                    parameters.Remove(parameters.Where(x => x.ParameterName == "Criterios").FirstOrDefault());

                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "Capital", ParameterValue = carteraPrestamo.Resumen.Capital },
                        new Parameters() { ParameterName = "Interes", ParameterValue = carteraPrestamo.Resumen.Interes },
                    });
                    break;
            }

            foreach (var prt in parameters)
            {
                CrReport.SetParameterValue(prt.ParameterName, prt.ParameterValue);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            guardarPDF(CrReport, txt_ReportName, intialPath);
            CrReport.Close();
        }

        private static void loadReport<T>(string NombreReporte,string urlReporte, string userName, string criterios, List<Parameters> parameters,List<T> data , ReportDocument CrReport, bool hasHeader = true)
        {
            //Parametros del reporte
            parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue = NombreReporte },
                        new Parameters() { ParameterName = "Criterios", ParameterValue = criterios },
                        new Parameters() { ParameterName = "UserName", ParameterValue = userName }
                    });
            CrReport.Load(urlReporte);
            
            if (hasHeader)
            {
                DB_IMBRINOEntities db = new DB_IMBRINOEntities();
                var sistema = db.SISTEMA.ToList();
                for (int i = 0; i < CrReport.Subreports.Count; i++)
                {
                    if (CrReport.Subreports[i].Database.Tables.Count > 0)
                    {
                        CrReport.Subreports[i].SetDataSource(sistema);
                    }
                }                
            }
            CrReport.SetDataSource(data);


        }

        private static DateTime? getFechaCorte(string cuenta)
        {
            DB_IMBRINOEntities db = new DB_IMBRINOEntities();
            CTABANCO ctaBanco = db.CTABANCO.Where(x => x.CTA_NUMERO == cuenta).FirstOrDefault();
            DateTime? result = DateTime.MinValue;
            if (ctaBanco != null)
            {
                result = ctaBanco.CTA_FECCOR.HasValue ?
                            ctaBanco.CTA_FECCOR : result;
            }
            return result;
        }

        public static ReportDocument configureCrystalReports(ReportDocument cryRpt, string DatabaseName, string servername)
        {
            ConnectionInfo myConnectionInfo = new ConnectionInfo();
            TableLogOnInfos crtableLogoninfos = new TableLogOnInfos();
            TableLogOnInfo crtableLogoninfo = new TableLogOnInfo();
            Tables CrTables;
            try
            {
                string Password = "", UserID = "";
                try
                {
                    Password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();
                    UserID = System.Configuration.ConfigurationManager.AppSettings["UserID"].ToString();
                }
                catch (Exception)
                {

                }
                myConnectionInfo.ServerName = servername;
                myConnectionInfo.DatabaseName = DatabaseName;
                myConnectionInfo.UserID = UserID;
                myConnectionInfo.Password = Password;

                CrTables = cryRpt.Database.Tables;
                foreach (Table CrTable in CrTables)
                {
                    crtableLogoninfo = CrTable.LogOnInfo;
                    crtableLogoninfo.ConnectionInfo = myConnectionInfo;
                    CrTable.ApplyLogOnInfo(crtableLogoninfo);
                }
            }
            catch (Exception ex)
            {

            }
            return cryRpt;
        }

        private static void guardarPDF(ReportDocument CrReport, string _reportName, string initialPah)
        {
            string path = string.Format("{0}\\{1}.pdf", initialPah, _reportName);

            // Si la carpeta no existe se crea
            if (!Directory.Exists(initialPah))
                Directory.CreateDirectory(initialPah);

            // Eliminar si existe el archivo
            if (File.Exists(path))
                File.Delete(path);

            CrReport.ExportToDisk(ExportFormatType.PortableDocFormat, path);
        }

        private static List<T> Criterios<T>(List<Parameters> listcriterios, ref string criterios, ref IQueryable<T> list)
        {
            List<T> data = list.ToList();
            var dataw = data.GetType();

            return data;
        }
        private static List<VW_rptEstadoCuenta> criteriosEstadoCuenta(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_rptEstadoCuenta> list)
        {
            double MONTO_FINANCIAR = 0, BALANCE_ACTUAL = 0;

            List<string> _listCriterio = new List<string>();
            DateTime fecha;

            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "fechaCorte":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fecha);
                            list = list.Where(x => x.Fecha < fecha);
                            CurrentCriterio = string.Format("Fecha corte {0}", cri.ParameterValue);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "montoFinanciar":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {

                            double.TryParse(cri.ParameterValue.ToString(), out MONTO_FINANCIAR);
                            list = list.Where(e => e.MONTO_FINANCIAR > MONTO_FINANCIAR);
                            CurrentCriterio = string.Format("Clientes con monto a financiar mayor de {0}", String.Format("{0:n}", cri.ParameterValue));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "montoActual":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {

                            double.TryParse(cri.ParameterValue.ToString(), out BALANCE_ACTUAL);
                            list = list.Where(e => e.BALANCE_ACTUAL > BALANCE_ACTUAL);
                            CurrentCriterio = string.Format("Clientes con con balance actual mayor de {0}", String.Format("{0:n}", cri.ParameterValue));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "ultimoPago":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fecha);
                            list = list.Where(x => x.Fecha > fecha);
                            CurrentCriterio = string.Format("Clientes con fecha de ultimo pago mayor de {0}", cri.ParameterValue);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }

            criterios = string.Join("\n-", _listCriterio.ToArray());
            return list.ToList();
        }

        private static List<VW_rptCuotasVencidas> criteriosCuotasVencidas(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_rptCuotasVencidas> list)
        {
            int cuotaVencida = 0;
            DateTime fecha;
            List<string> _listCriterio = new List<string>();
            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "fechaCorte":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fecha);
                            list = list.Where(x => x.fechadt < fecha);
                            CurrentCriterio = string.Format("Fecha corte {0}", cri.ParameterValue);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "Cliente":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            list = list.Where(e => e.CodigoCLiente.ToString() == cri.ParameterValue.ToString());
                            CurrentCriterio = "Un cliente en específico";
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "clienteCuotaV":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false && cri.ParameterValue.ToString() == "true")
                        {
                            int.TryParse(cri.ParameterValue.ToString(), out cuotaVencida);
                            list = clientesVariascuotas(list);
                            CurrentCriterio = "Clientes con más de una cuota vencida";
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "ultimoPago":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fecha);
                            list = list.Where(x => x.fechadt > fecha);
                            CurrentCriterio = string.Format("Clientes con fecha de ultimo pago mayor de {0}", cri.ParameterValue);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }

            criterios = "- " + string.Join("\n- ", _listCriterio.ToArray());
            return list.ToList();
        }

        private static List<VW_rptFinAtrasados_details> criteriosFin_atrasados(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_rptFinAtrasados_details> list)
        {
            DateTime fecha;
            List<string> _listCriterio = new List<string>();
            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "fechaCorte":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fecha);
                            list = list.Where(x => x.fechadt < fecha);
                            CurrentCriterio = string.Format("Fecha corte {0}", cri.ParameterValue);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "Cliente":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            list = list.Where(e => e.CodigoCLiente.ToString() == cri.ParameterValue.ToString());
                            CurrentCriterio = "Un cliente en específico";
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "ultimoPago":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fecha);
                            list = list.Where(x => x.fechadt > fecha);
                            CurrentCriterio = string.Format("Clientes con fecha de ultimo pago mayor de {0}", cri.ParameterValue);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }

            criterios = "- " + string.Join("\n- ", _listCriterio.ToArray());
            return list.ToList();
        }

        private static List<VW_rptRegistroNCF> criteriosRegistroNCF(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_rptRegistroNCF> list)
        {
            List<string> _listCriterio = new List<string>();
            string fechaFormateada = string.Empty;
            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "fechaDesde":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out DateTime fechaDesde);
                            list = list.Where(x => x.fechadt >= fechaDesde);
                            fechaFormateada = string.Format("{0}/{1}/{2}", fechaDesde.ToString("dd"), returMonthName(fechaDesde.Month), fechaDesde.ToString("yy"));
                            CurrentCriterio = $"Fecha desde: {fechaFormateada}";
                            _listCriterio.Add(CurrentCriterio);

                        }
                        break;
                    case "fechaHasta":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out DateTime fechaHasta);
                            list = list.Where(x => x.fechadt <= fechaHasta);
                            fechaFormateada = string.Format("{0}/{1}/{2}", fechaHasta.ToString("dd"), returMonthName(fechaHasta.Month), fechaHasta.ToString("yy"));
                            CurrentCriterio = $"{CurrentCriterio} y {string.Format("Fecha hasta: {0}", fechaFormateada)}";
                            _listCriterio.RemoveAt(0);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }

            criterios = "- " + string.Join("\n- ", _listCriterio.ToArray());
            return list.ToList();
        }

        private static List<VW_rptRegistroNCF> criteriosIngresoCuotas(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_rptRegistroNCF> list)
        {
            DateTime fechaDesde, fechaHasta;
            List<string> _listCriterio = new List<string>();
            string fechaFormateada = string.Empty;
            int idCliente = 0, idFin = 0;
            foreach (var cri in listcriterios)
            {
                if (string.IsNullOrEmpty(cri.ParameterValue.ToString()))
                {
                    continue;
                }
                switch (cri.ParameterName)
                {
                    case "fechaDesde":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaDesde);
                            list = list.Where(x => x.fechadt >= fechaDesde);
                            fechaFormateada = string.Format("{0}/{1}/{2}", fechaDesde.ToString("dd"), returMonthName(fechaDesde.Month), fechaDesde.ToString("yy"));
                            CurrentCriterio = string.Format("Fecha desde: {0}", fechaFormateada);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "fechaHasta":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaHasta);
                            list = list.Where(x => x.fechadt <= fechaHasta);
                            fechaFormateada = string.Format("{0}/{1}/{2}", fechaHasta.ToString("dd"), returMonthName(fechaHasta.Month), fechaHasta.ToString("yy"));
                            CurrentCriterio = $"{CurrentCriterio} y {string.Format("Fecha hasta: {0}", fechaFormateada)}";
                            _listCriterio.RemoveAt(0);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;

                    case "Cliente":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            int.TryParse(cri.ParameterValue.ToString(), out idCliente);
                            list = list.Where(x => x.ClienteId == idCliente);
                            CurrentCriterio = string.Format("Un cliente en específico");
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "finID":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            int.TryParse(cri.ParameterValue.ToString(), out idFin);
                            list = list.Where(x => x.ING_NUMFIN == idFin);
                            CurrentCriterio = string.Format("Financiamiento en especifico");
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "OrdenarPor":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            string textoOrdenadoPor = cri.ParameterValue.ToString().ToLower() == "recibo" ?
                                                        "número de recibo" :
                                                        cri.ParameterValue.ToString();

                            CurrentCriterio = string.Format("Reporte ordenado por {0} ", textoOrdenadoPor);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }

            criterios = "- " + string.Join("\n- ", _listCriterio.ToArray());
            return list.ToList();
        }

        private static List<vw_ConsultaFinDetalle> criteriosFinDetalle(List<Parameters> listcriterios, ref string criterios, ref IQueryable<vw_ConsultaFinDetalle> list)
        {
            List<string> _listCriterio = new List<string>();
            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "FindID":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            int.TryParse(cri.ParameterValue.ToString(), out int numFin);
                            list = list.Where(x => x.ING_NUMFIN == numFin);
                            CurrentCriterio = string.Format($"Un financiamiento en específico ({numFin})");
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }

            }

            criterios = "- " + string.Join("\n- ", _listCriterio.ToArray());
            return list.ToList();
        }

        private static IQueryable<VW_rptCuotasVencidas> clientesVariascuotas(IQueryable<VW_rptCuotasVencidas> list)
        {
            List<VW_rptCuotasVencidas> listaCUotas = new List<VW_rptCuotasVencidas>();

            var listClientesMasUna = list.GroupBy(x => x.CLIENTE).Where(x => x.Count() > 1).Select(x => new
            {
                cliente = x.GroupBy(x2 => x2.CodigoCLiente)

            }).ToList();


            foreach (var item in listClientesMasUna)
            {
                var Listcliente = item.cliente.FirstOrDefault().ToList();
                listaCUotas.AddRange(Listcliente);

            }


            return listaCUotas.AsQueryable();


        }

        private static List<VW_rptCreditosBancarios> criteriosCreditosBancarios(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_rptCreditosBancarios> list)
        {
            DateTime fechaDesde, fechaHasta;
            List<string> _listCriterio = new List<string>();

            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "FechaDesde":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaDesde);
                            list = list.Where(x => x.FECHA >= fechaDesde);
                            CurrentCriterio = string.Format("{0}/{1}/{2}", fechaDesde.ToString("dd"), returMonthName(fechaDesde.Month), fechaDesde.ToString("yy"));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "FechaHasta":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaHasta);
                            list = list.Where(x => x.FECHA <= fechaHasta);
                            CurrentCriterio = string.Format("{0}/{1}/{2}", fechaHasta.ToString("dd"), returMonthName(fechaHasta.Month), fechaHasta.ToString("yy"));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }

            criterios = string.Join(" al ", _listCriterio.ToArray());
            return list.ToList();
        }

        private static List<VW_rptDebitosBancarios> criteriosDebitosBancarios(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_rptDebitosBancarios> list)
        {
            DateTime fechaDesde, fechaHasta;
            List<string> _listCriterio = new List<string>();

            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "FechaDesde":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaDesde);
                            list = list.Where(x => x.FECHA >= fechaDesde);
                            CurrentCriterio = string.Format("{0}/{1}/{2}", fechaDesde.ToString("dd"), returMonthName(fechaDesde.Month), fechaDesde.ToString("yy"));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "FechaHasta":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaHasta);
                            list = list.Where(x => x.FECHA <= fechaHasta);
                            CurrentCriterio = string.Format("{0}/{1}/{2}", fechaHasta.ToString("dd"), returMonthName(fechaHasta.Month), fechaHasta.ToString("yy"));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }

            criterios = string.Join(" al ", _listCriterio.ToArray());
            return list.ToList();
        }

        private static void criteriosCuadreCajas(List<Parameters> listcriterios, ref string criterios)
        {
            DateTime fechaDesde, fechaHasta;
            List<string> _listCriterio = new List<string>();

            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "FechaDesde":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaDesde);
                            CurrentCriterio = string.Format("{0}/{1}/{2}", fechaDesde.ToString("dd"), returMonthName(fechaDesde.Month), fechaDesde.ToString("yy"));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "FechaHasta":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaHasta);
                            CurrentCriterio = string.Format("{0}/{1}/{2}", fechaHasta.ToString("dd"), returMonthName(fechaHasta.Month), fechaHasta.ToString("yy"));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }

            criterios = string.Join(" al ", _listCriterio.ToArray());
        }

        private static List<VW_rptReciboIngreso> criteriosReciboIngreso(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_rptReciboIngreso> list)
        {
            List<string> _listCriterio = new List<string>();

            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "ReciboID":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            int.TryParse(cri.ParameterValue.ToString(), out int recibo);
                            list = list.Where(x => x.Recibo == recibo);
                        }
                        break;
                }
            }

            return list.ToList();
        }

        private static List<VW_ConciliacionBancaria> criteriosConciliacionBancaria(List<Parameters> listcriterios, ref string criterios, ref IQueryable<VW_ConciliacionBancaria> list)
        {

            List<string> _listCriterio = new List<string>();

            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "FechaDesde":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out DateTime fechaDesde);
                            list = list.Where(x => x.FECHA >= fechaDesde);
                            CurrentCriterio = string.Format("{0}/{1}/{2}", fechaDesde.ToString("dd"), returMonthName(fechaDesde.Month), fechaDesde.ToString("yyyy"));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "FechaHasta":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out DateTime fechaHasta);
                            list = list.Where(x => x.FECHA <= fechaHasta);
                            CurrentCriterio = string.Format("{0}/{1}/{2}", fechaHasta.ToString("dd"), returMonthName(fechaHasta.Month), fechaHasta.ToString("yyyy"));
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                }
            }
            criterios = string.Join(" al ", _listCriterio.ToArray());
            return list.ToList();
        }

        private static View_CuadreCajaResumen getResumenCuadreCaja()
        {
            View_CuadreCajaGeneral ListCuadreCaja = View_generalClass.GetListCuadreCaja();
            return ListCuadreCaja.Resumen;
        }

        public static FileStreamResult viewReportPDF(string url)
        {
            var fileStream = new FileStream(url,
              FileMode.Open,
              FileAccess.Read
               );

            var fsResult = new FileStreamResult(fileStream, "application/pdf");
            fileStream.Flush();


            return fsResult;
        }


    }


}