using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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
            ReciboIngreso ,
            ConciliacionBancaria
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
            string txt_ReportName = string.Empty;
            string data = string.Empty;


            switch (_ReportName)
            {
                case ReportName.EstadoCuenta:
                    txt_ReportName = _ReportName.ToString();
                    IQueryable<VW_rptEstadoCuenta> _iqEC = db.VW_rptEstadoCuenta;
                    List<VW_rptEstadoCuenta> _listEC = new List<VW_rptEstadoCuenta>();

                    urlReporte = Path.Combine(urlReporte, txt_ReportName + ".rpt"); //string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listEC = criteriosEstadoCuenta(Listcriterios, ref criterios, ref _iqEC);
                    _listEC = _listEC.OrderBy(x => x.CLIENTE).ThenBy(x => x.Fecha).ToList();

                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue ="Estado cuenta" },
                        new Parameters() { ParameterName = "Criterios", ParameterValue = criterios },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }
                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listEC);
                    break;
                case ReportName.CuotasVencidas:
                    txt_ReportName = _ReportName.ToString();

                    IQueryable<VW_rptCuotasVencidas> _iqCV = db.VW_rptCuotasVencidas;
                    List<VW_rptCuotasVencidas> _listCV = new List<VW_rptCuotasVencidas>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listCV = criteriosCuotasVencidas(Listcriterios, ref criterios, ref _iqCV);
                    _listCV = _listCV.OrderBy(x => x.CLIENTE).ThenBy(x => x.fechadt).ToList();
                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue ="Cuotas vencidas" },
                        new Parameters() { ParameterName = "Criterios", ParameterValue = criterios },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }
                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listCV);

                    break;
                case ReportName.Financiamientos_atrasados:

                    txt_ReportName = _ReportName.ToString();
                    IQueryable<VW_rptFinAtrasados_details> _iqFA = db.VW_rptFinAtrasados_details;
                    List<VW_rptFinAtrasados_details> _listFA = new List<VW_rptFinAtrasados_details>();
                    urlReporte = string.Format("{0}/{1}.rpt", urlReporte, txt_ReportName);
                    _listFA = criteriosFin_atrasados(Listcriterios, ref criterios, ref _iqFA);
                    _listFA = _listFA.OrderBy(x => x.CLIENTE).ThenBy(x => x.fechadt).ToList();

                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue = "Financiamientos atrasados" },
                        new Parameters() { ParameterName = "Criterios", ParameterValue = criterios },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }


                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listFA);
                    break;
                case ReportName.RegistroNCF:
                    txt_ReportName = _ReportName.ToString();

                    IQueryable<VW_rptRegistroNCF> _iqRN = db.VW_rptRegistroNCF;
                    List<VW_rptRegistroNCF> _listRN = new List<VW_rptRegistroNCF>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listRN = criteriosRegistroNCF(Listcriterios, ref criterios, ref _iqRN);
                    _listRN = _listRN.OrderBy(x => x.ING_NCF).ThenBy(x => x.fechadt).ToList();
                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue ="ARCHIVO PARA REGISTRO DE NCF" },
                        new Parameters() { ParameterName = "Criterios", ParameterValue = criterios },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }


                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listRN);
                    break;

                case ReportName.IngresoCuotas:
                    txt_ReportName = _ReportName.ToString();

                    IQueryable<VW_rptRegistroNCF> _iqIC = db.VW_rptRegistroNCF;
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
                            _listIC = _listIC.OrderByDescending(x => x.ING_MONTOT).ToList();
                            break;
                    }

                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue ="REPORTE INGRESOS POR CUOTAS" },
                        new Parameters() { ParameterName = "Criterios", ParameterValue = criterios },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }
                    });

                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listIC);
                    break;
                case ReportName.FinHeader:
                    txt_ReportName = _ReportName.ToString();
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
                                .ToList();
                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);

                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue ="REPORTE FINANCIAMIENTOS" },
                        new Parameters() { ParameterName = "Criterios", ParameterValue = criterios },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }
                    });

                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listFH);
                    break;
                case ReportName.FinDetalle:
                    txt_ReportName = _ReportName.ToString();

                    IQueryable<vw_ConsultaFinDetalle> _iqFD = db.vw_ConsultaFinDetalle;
                    List<vw_ConsultaFinDetalle> _listFD = new List<vw_ConsultaFinDetalle>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listFD = criteriosFinDetalle(Listcriterios, ref criterios, ref _iqFD);
                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue ="ARCHIVO PARA REGISTRO DE NCF" },
                        new Parameters() { ParameterName = "Criterios", ParameterValue = criterios },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }
                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listFD);
                    break;
                case ReportName.CreditosBancarios:
                    txt_ReportName = _ReportName.ToString();

                    IQueryable<VW_rptCreditosBancarios> _iqCB = db.VW_rptCreditosBancarios.OrderBy(x=>x.FECHA);
                    List<VW_rptCreditosBancarios> _listCB = new List<VW_rptCreditosBancarios>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listCB = criteriosCreditosBancarios(Listcriterios, ref criterios, ref _iqCB);

                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue = $"Créditos bancarios {criterios}" },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }
                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listCB);
                    break;
                case ReportName.DebitosBancarios:
                    txt_ReportName = _ReportName.ToString();

                    IQueryable<VW_rptDebitosBancarios> _iqDB = db.VW_rptDebitosBancarios.OrderBy(x=>x.FECHA);
                    List<VW_rptDebitosBancarios> _listDB = new List<VW_rptDebitosBancarios>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listDB = criteriosDebitosBancarios(Listcriterios, ref criterios, ref _iqDB);

                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue = $"Débitos bancarios {criterios}" },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" }
                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listDB);
                    break;
                case ReportName.CuadreCaja:
                    txt_ReportName = _ReportName.ToString();

                    IQueryable<vw_CuadreCaja> _iqCC = db.vw_CuadreCaja;
                    List<vw_CuadreCaja> _listCC = new List<vw_CuadreCaja>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listCC = criteriosCuadreCajas(Listcriterios, ref criterios, ref _iqCC);
                    var resumen = getResumenCuadreCaja();

                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "NombreReporte", ParameterValue = $"Cuadre caja {criterios}" },
                        new Parameters() { ParameterName = "UserName", ParameterValue = "Imbrino" },
                        new Parameters() { ParameterName = "Deposito", ParameterValue = resumen.DepositoTranferencia },
                        new Parameters() { ParameterName = "IngresosNoNetos", ParameterValue = resumen.IngresosNoNetos },
                        new Parameters() { ParameterName = "OtrosIngresosNetos", ParameterValue = resumen.OtrosIngresosNetos }
                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listCC);
                    break;
                case ReportName.ReciboIngreso:
                    txt_ReportName = _ReportName.ToString();

                    IQueryable<VW_rptReciboIngreso> _iqRI = db.VW_rptReciboIngreso;
                    List<VW_rptReciboIngreso> _listRI = new List<VW_rptReciboIngreso>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listRI = criteriosReciboIngreso(Listcriterios, ref criterios, ref _iqRI);
                  
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listRI);
                    break;
                case ReportName.ConciliacionBancaria:
                    txt_ReportName = _ReportName.ToString();
                    string FiltroCuenta = Listcriterios.Where(x => x.ParameterName == "FiltroCuenta").FirstOrDefault().ParameterValue.ToString();
                    string cuenta = FiltroCuenta.Split('#')[1].Trim();
                    DateTime? fechaCorte = getFechaCorte(cuenta);
                    IQueryable<VW_ConciliacionBancaria> _iqCONB = db.VW_ConciliacionBancaria.Where(x=>x.CUENTA == cuenta).OrderBy(x=> x.FECHA);
                    List<VW_ConciliacionBancaria> _listCONB = new List<VW_ConciliacionBancaria>();

                    urlReporte = string.Format("{0}\\{1}.rpt", urlReporte, txt_ReportName);
                    _listCONB = criteriosConciliacionBancaria(Listcriterios, ref criterios, ref _iqCONB);
                    string filtroFechas = Listcriterios.Where(x => x.ParameterName == "isMovimiento").FirstOrDefault().ParameterValue.ToString() == "True" ?
                            "Movimiento bancario" : "Estado de cuenta";
                    //Parametros del reporte
                    parameters.AddRange(new List<Parameters>
                    {
                        new Parameters() { ParameterName = "FiltroFechas", ParameterValue = $"{filtroFechas} del {criterios}" },
                        new Parameters() { ParameterName = "FiltroCuenta", ParameterValue = Listcriterios.Where(x=> x.ParameterName == "FiltroCuenta").FirstOrDefault().ParameterValue },
                        new Parameters() { ParameterName = "FechaCorte", ParameterValue = returDateFormat(fechaCorte) },
                        new Parameters() { ParameterName = "BalanceCorte", ParameterValue =  Listcriterios.Where(x=> x.ParameterName == "Balance").FirstOrDefault().ParameterValue },
                        new Parameters() { ParameterName = "isMovimiento", ParameterValue =  Listcriterios.Where(x=> x.ParameterName == "isMovimiento").FirstOrDefault().ParameterValue },
                    });
                    CrReport.Load(urlReporte);
                    CrReport.SetDataSource(_listCONB);
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
            ConnectionInfo crConnectionInfo = new ConnectionInfo();
            Tables CrTables;
            try
            {
                string Password = "", UserID = "";
                try
                {
                    Password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();
                    UserID = System.Configuration.ConfigurationManager.AppSettings["UserID"].ToString(); ;
                }
                catch (Exception)
                {

                }
                myConnectionInfo.ServerName = servername;
                myConnectionInfo.DatabaseName = DatabaseName;
                myConnectionInfo.UserID = UserID;
                myConnectionInfo.Password = Password;

                CrTables = cryRpt.Database.Tables;
                foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in CrTables)
                {
                    crtableLogoninfo = CrTable.LogOnInfo;
                    crtableLogoninfo.ConnectionInfo = crConnectionInfo;
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
                            //list = list.FindAll(e => e.ULTIMO_PAGO < cri.ParameterValue.ToString());
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

            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "fechaDesde":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out DateTime fechaDesde);
                            list = list.Where(x => x.fechadt >= fechaDesde);
                            CurrentCriterio = string.Format("Fecha desde {0}", cri.ParameterValue);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "fechaHasta":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out DateTime fechaHasta);
                            list = list.Where(x => x.fechadt <= fechaHasta);
                            CurrentCriterio = string.Format("Fecha hasta {0}", cri.ParameterValue);
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
            int idCliente = 0, idFin = 0;
            foreach (var cri in listcriterios)
            {
                switch (cri.ParameterName)
                {
                    case "fechaDesde":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaDesde);
                            list = list.Where(x => x.fechadt >= fechaDesde);
                            CurrentCriterio = string.Format("Fecha desde {0}", cri.ParameterValue);
                            _listCriterio.Add(CurrentCriterio);
                        }
                        break;
                    case "fechaHasta":
                        if (string.IsNullOrWhiteSpace(cri.ParameterValue.ToString()) == false)
                        {
                            DateTime.TryParse(cri.ParameterValue.ToString(), out fechaHasta);
                            list = list.Where(x => x.fechadt <= fechaHasta);
                            CurrentCriterio = string.Format("Fecha hasta {0}", cri.ParameterValue);
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
                            CurrentCriterio = string.Format("Reporte ordenado por {0} ", cri.ParameterValue.ToString());
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

        private static List<vw_CuadreCaja> criteriosCuadreCajas(List<Parameters> listcriterios, ref string criterios, ref IQueryable<vw_CuadreCaja> list)
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
            return list.ToList();
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
            DB_IMBRINOEntities db = new DB_IMBRINOEntities();
            List<String> ListOtrosIngresosNetos = new List<string>()
            {
                "OTROS_EC","OTROS_DT"
            };

            var resumen = db.vw_CuadreCaja
               .GroupBy(x => new
               {
                   x.ID,
                   x.Tipo,
                   x.TextoTipo
               })
               .Select(x => new View_CuadreCajaResumen()
               {
                   IngresosNoNetos = db.vw_CuadreCaja.Where(z => z.Tipo == "INGRESOS_NN")
                                   .Sum(z => z.MontoTotal),
                   OtrosIngresosNetos =
                           db.vw_CuadreCaja.Where(z => ListOtrosIngresosNetos
                                       .Contains(z.Tipo))
                               .Sum(z => z.MontoTotal),
                   DepositoTranferencia = db.vw_CuadreCaja.Where(z => z.Tipo == "SALIDAS")
                                   .Sum(z => z.MontoTotal),
               })
               .FirstOrDefault();
            return resumen;
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