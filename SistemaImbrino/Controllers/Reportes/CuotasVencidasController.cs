using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Reportes
{
    public class CuotasVencidasController : Controller
    {
        private DB_IMBRINOEntities db = new DB_IMBRINOEntities();
        // GET: CuotasVencidas
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Getclients()
        {

            var listClients = db.VW_rptCuotasVencidas.Select(x => new { id = x.CodigoCLiente, value = x.CLIENTE}).Distinct().OrderBy(x=>x.value).ToList();
          
            return Json(listClients, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GenerarReportePDF(string fechaCorte = "", string Cliente = "", string isMultiplecuota = "", string ultimoPago = "")
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;
            string fecha_corteFormat = "",fechaUltimoPagoFormat = "";


            try
            {
                string intialPath = Server.MapPath(Parameters.rutaReporte);
                isMultiplecuota = isMultiplecuota != "true" ? "" : isMultiplecuota;


                if (string.IsNullOrWhiteSpace(fechaCorte) && string.IsNullOrWhiteSpace(Cliente)
                    && string.IsNullOrWhiteSpace(isMultiplecuota) && string.IsNullOrWhiteSpace(ultimoPago))
                {
                    mensajeReturn.Message = "Favor llenar por lo menos un filtro";
                    mensajeReturn.Is_Success = false;

                }
                else if (string.IsNullOrWhiteSpace(fechaCorte) == false && string.IsNullOrWhiteSpace(ultimoPago) == false)
                {
                    mensajeReturn.Message = "No se pueden seleccionar los filtros: \n  fecha corte y ultimo pago al mismo tiempo";
                    mensajeReturn.Is_Success = false;
                }
                else if (string.IsNullOrWhiteSpace(Cliente) == false && string.IsNullOrWhiteSpace(isMultiplecuota) == false)
                {
                    mensajeReturn.Message = "No se pueden seleccionar los filtros: \n  Cliente y varias cuotas al mismo tiempo";
                    mensajeReturn.Is_Success = false;
                }



                if (mensajeReturn.Is_Success)
                {
                    var tr = fechaCorte.Split('-');
                    if (tr.Count() == 3)
                    {
                        int month = 0;
                        int.TryParse(tr[1].ToString(),out month);
                        fecha_corteFormat = string.Format("{0}-{1}-{2}", tr[2].ToString(), BaseController.returMonthName(month), tr[0].ToString()) ;
                    }

                    var tr2 = ultimoPago.Split('-');
                    if (tr2.Count() == 3)
                    {
                        int month = 0;
                        int.TryParse(tr2[1].ToString(), out month);
                        fechaUltimoPagoFormat = string.Format("{0}-{1}-{2}", tr2[2].ToString(), BaseController.returMonthName(month), tr2[0].ToString());
                    }



                    List<Parameters> prt = new List<Parameters>() {
                new Parameters(){ParameterName="fechaCorte",ParameterValue=fechaCorte},
                new Parameters(){ParameterName="Cliente",ParameterValue=Cliente},
                new Parameters(){ParameterName="clienteCuotaV",ParameterValue=isMultiplecuota},
                new Parameters(){ParameterName="ultimoPago",ParameterValue=ultimoPago}  };

                    Parameters.guardarReporte(Response, ReportName.CuotasVencidas, prt, intialPath);
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.CuotasVencidas));

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