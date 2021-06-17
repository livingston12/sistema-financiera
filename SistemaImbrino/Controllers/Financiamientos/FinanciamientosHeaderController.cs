using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static SistemaImbrino.Controllers.BaseController;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Financiamientos
{
    public class FinanciamientosHeaderController : Controller
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();


        public ActionResult Index()
        {
            //var listaFinanciamientos = getconsultaFinanciamientos();
            //ViewBag.ListConsultaFin = listaFinanciamientos;
            return View("ConsultaFinanciamientos");
        }

        private IEnumerable<View_consultaFinanciamientos> getconsultaFinanciamientos(string status = "5")
        {
            string strStatus = string.Empty;
            Status enumStatus = (Status)Enum.Parse(typeof(Status), status);
            strStatus = enumStatus.ToString().Replace("NUEVO_CERRADO", "Activo").ToLower();


            IQueryable<View_consultaFinanciamientos> listaFinanciamientos = _db.vw_ConsultaFin
                                            .GroupBy(x =>
                                                new
                                                {
                                                    x.FAC_NUMFIN,
                                                    x.cliente,
                                                    x.FAC_FECHA,
                                                    x.CUO_STATUS
                                                })
                                            .Select(x => new View_consultaFinanciamientos
                                            {
                                                FinID = x.Key.FAC_NUMFIN,
                                                Cliente = x.Key.cliente,
                                                Fecha = x.Key.FAC_FECHA,
                                                Estatus = x.Key.CUO_STATUS,
                                                detail = x.Where(z =>
                                                                   x.Key.FAC_NUMFIN == z.FAC_NUMFIN
                                                                   && x.Key.CUO_STATUS == z.CUO_STATUS
                                                                   )
                                                            .Select(z => new consultaFinanciamientosDetalle
                                                            {
                                                                Cuotas = z.CUO_NUMCUO,
                                                                Capital = z.CUO_MONTOC,
                                                                Interes = z.CUO_MONTOI,
                                                                Monto = z.CUO_MONTOT,
                                                                Balance = z.CUO_MONTOC + z.CUO_MONTOI
                                                            }).FirstOrDefault()
                                            }).Where(x => x.Estatus == status);
            return listaFinanciamientos;
        }

        public JsonResult getCurrentStatus(string NameStatus)
        {
            IEnumerable<View_consultaFinanciamientos> consultaFin = new List<View_consultaFinanciamientos>();
            try
            {
                var status = (Status)Enum.Parse(typeof(Status), NameStatus);
                int id = (int)status;
                consultaFin = getconsultaFinanciamientos(id.ToString());
            }
            catch (Exception)
            {

                consultaFin = null;
            }
            return Json(consultaFin);
        }
        public ActionResult PrintReport(string data = "",string status = "")
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;
            try
            {
                string intialPath = Server.MapPath(Parameters.rutaReporte);

                if (string.IsNullOrWhiteSpace(data))
                {
                    mensajeReturn.Message = "Estatus es obligatorio";
                    mensajeReturn.Is_Success = false;
                }

                if (mensajeReturn.Is_Success)
                {
                    List<Parameters> prt = new List<Parameters>()
                    {
                        new Parameters(){ParameterName="data",ParameterValue=data }
                    };
                    Parameters.guardarReporte(Response, ReportName.FinHeader, prt, intialPath);
                }

            }
            catch (Exception e)
            {
                mensajeReturn.Message = "Error inesperado: " + e.Message;
                mensajeReturn.Is_Success = false;
            }

            return Json(mensajeReturn);
        }

        public ActionResult generateReport()
        {

            try
            {
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.FinHeader));

                var fsResult = Parameters.viewReportPDF(ruta);
                return fsResult;
            }
            catch (Exception e)
            {
                return Content("Error inesperado en el reporte: " + e.Message);

            }
        }
    }
}