using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Reportes
{
    public class ReImprimirReciboController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        // GET: ReImprimirRecibo
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GenerarReportePDF(string recibo = "")
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;
            try
            {
                string intialPath = Server.MapPath(Parameters.rutaReporte);

                if (string.IsNullOrWhiteSpace(recibo))
                {
                    mensajeReturn.Message = "Favor llenar el campo recibo";
                    mensajeReturn.Is_Success = false;
                }

                if (mensajeReturn.Is_Success)
                {
                    List<Parameters> prt = new List<Parameters>()
                    {
                        new Parameters(){ParameterName="ReciboID",ParameterValue=recibo}
                    };

                    Parameters.guardarReporte(Response, ReportName.ReciboIngreso, prt, intialPath);
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.ReciboIngreso));

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