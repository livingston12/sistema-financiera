using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Reportes
{
    public class IngresoCuotasController : Controller
    {
        private DB_IMBRINOEntities db = new DB_IMBRINOEntities();


        public JsonResult Getclients()
        {

            var listClients = db.VW_rptRegistroNCF.Select(x => new { id = x.ClienteId, value = x.Cliente }).Distinct().OrderBy(x => x.value).ToList();
            return Json(listClients, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFIns(string clienteID)
        {
            int ID = 0;
            int.TryParse(clienteID,out ID);
            var listFIns = db.VW_rptRegistroNCF.Where(x=>x.ClienteId == ID).Select(x => new { id = x.ING_NUMFIN, value = x.ING_NUMFIN }).Distinct().OrderBy(x => x.id).ToList();
            return Json(listFIns, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerarReportePDF(string fechaDesde = "", string fechaHasta = "", string Cliente = "", string finID = "", string OrdenarPor ="")
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;

            string intialPath = Server.MapPath(Parameters.rutaReporte);

            try
            {
                
              
                if (mensajeReturn.Is_Success)
                {              

                    List<Parameters> prt = new List<Parameters>()
                    {
                        new Parameters(){ParameterName="fechaDesde",ParameterValue=fechaDesde},
                        new Parameters(){ParameterName="fechaHasta",ParameterValue=fechaHasta},
                        new Parameters(){ParameterName="Cliente",ParameterValue=Cliente},
                        new Parameters(){ParameterName="finID",ParameterValue=finID},
                        new Parameters(){ParameterName="OrdenarPor",ParameterValue=OrdenarPor}
                    };              

                    Parameters.guardarReporte(Response, ReportName.IngresoCuotas, prt, intialPath);
                }

            }
            catch (Exception e)
            {
                mensajeReturn.Message = "Error inesperado: " + e.Message + " " + intialPath;
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.IngresoCuotas));

                var fsResult = Parameters.viewReportPDF(ruta);
                return fsResult;
            }
            catch (Exception e)
            {
                return Content("Error inesperado en el reporte: " + e.Message);

            }

            //return File(ruta, "application/pdf", "EstadoCuenta");
        }
    }
}