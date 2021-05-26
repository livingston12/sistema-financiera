using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Reportes
{
    public class RegistroNCFController : Controller
    {
        // GET: RegistroNCF
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GenerarReportePDF(string fechaDesde = "", string fechaHasta = "")
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;
            try
            {
                string intialPath = Server.MapPath(Parameters.rutaReporte);



                if (string.IsNullOrWhiteSpace(fechaDesde) && string.IsNullOrWhiteSpace(fechaHasta)) 
                {
                    mensajeReturn.Message = "Favor llenar por lo menos un filtro";
                    mensajeReturn.Is_Success = false;
                }
              

                if (mensajeReturn.Is_Success)
                {
                    List<Parameters> prt = new List<Parameters>() {
                    new Parameters(){ParameterName="fechaDesde",ParameterValue=fechaDesde},
                    new Parameters(){ParameterName="fechaHasta",ParameterValue=fechaHasta}
                      };

                    Parameters.guardarReporte(Response, ReportName.RegistroNCF, prt, intialPath);
                }

            }
            catch (Exception e)
            {
                mensajeReturn.Message = "Error inesperado: " + e.Message;
                mensajeReturn.Is_Success = false;
            }

            //return View(true);
            //return true;



            return Json(mensajeReturn);
        }

        public ActionResult generateReport()
        {

            try
            {
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.RegistroNCF));

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