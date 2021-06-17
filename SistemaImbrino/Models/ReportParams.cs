﻿using CrystalDecisions.CrystalReports.Engine;
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


        public enum ReportName
        {
            EstadoCuenta = 1,
            CuotasVencidas = 2,
            Financiamientos_atrasados = 3,
            RegistroNCF = 4,
            IngresoCuotas = 5,
            FinHeader = 6,
            FinDetalle = 7
        }


        static string CurrentCriterio = "";

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


            string urlReporte = intialPath; //Server.MapPath(string.Format("/Reportes/{0}.rpt", _ReportName.ToString()));
            List<Parameters> parameters = new List<Parameters>();
            string criterios = string.Empty;
            string txt_ReportName = string.Empty;
            string data = string.Empty;


            switch (_ReportName)
            {
                case ReportName.EstadoCuenta:
                    txt_ReportName = ReportName.EstadoCuenta.ToString();
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
                    txt_ReportName = ReportName.CuotasVencidas.ToString();

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

                    txt_ReportName = ReportName.Financiamientos_atrasados.ToString();
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
                    txt_ReportName = ReportName.RegistroNCF.ToString();

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
                    txt_ReportName = ReportName.IngresoCuotas.ToString();

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
                    txt_ReportName = ReportName.FinHeader.ToString();
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
                    txt_ReportName = ReportName.FinDetalle.ToString();

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
            }

            foreach (var prt in parameters)
            {
                CrReport.SetParameterValue(prt.ParameterName, prt.ParameterValue);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            guardarPDF(CrReport, txt_ReportName, intialPath);

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
                    //servername = System.Configuration.ConfigurationManager.AppSettings["servername"].ToString(); ;
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





        //public static void showReport(HttpResponseBase Response,string path)
        //{
        //    WebClient webClient = new WebClient();
        //    string pdf = path;
        //    byte[] fileBuffer = webClient.DownloadData(pdf);

        //    if (fileBuffer != null)
        //    {
        //        Response.ContentType = "application/pdf";
        //        Response.AddHeader("Content-length", fileBuffer.Length.ToString());
        //        Response.BinaryWrite(fileBuffer);
        //    }
        //}

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
                            //list = list.FindAll(e => e.ULTIMO_PAGO < cri.ParameterValue.ToString());
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
                            CurrentCriterio = string.Format("Un financiamiento en específico");
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