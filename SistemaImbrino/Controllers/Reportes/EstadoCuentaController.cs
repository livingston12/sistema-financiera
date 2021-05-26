using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Reportes
{
    public class EstadoCuentaController : Controller
    {
        private DB_IMBRINOEntities db = new DB_IMBRINOEntities();
        // GET: EstadoCuenta
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GenerarReportePDF(string fechaCorte = "", string montoFinanciar = "", string montoActual = "", string ultimoPago = "")
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;
            try
            {
                string intialPath = Server.MapPath(Parameters.rutaReporte);



                if (string.IsNullOrWhiteSpace(fechaCorte) && string.IsNullOrWhiteSpace(montoFinanciar)
                    && string.IsNullOrWhiteSpace(montoActual) && string.IsNullOrWhiteSpace(ultimoPago))
                {
                    mensajeReturn.Message = "Favor llenar por lo menos un filtro";
                    mensajeReturn.Is_Success = false;
                }
                else if (string.IsNullOrWhiteSpace(fechaCorte) == false && string.IsNullOrWhiteSpace(ultimoPago) == false)
                {
                    mensajeReturn.Message = "No se pueden seleccionar los filtros: \n  fecha corte y ultimo pago al mismo tiempo";
                    mensajeReturn.Is_Success = false;
                }

                if (mensajeReturn.Is_Success)
                {
                    List<Parameters> prt = new List<Parameters>() {
                    new Parameters(){ParameterName="fechaCorte",ParameterValue=fechaCorte},
                    new Parameters(){ParameterName="montoFinanciar",ParameterValue=montoFinanciar},
                    new Parameters(){ParameterName="montoActual",ParameterValue=montoActual},
                    new Parameters(){ParameterName="ultimoPago",ParameterValue=ultimoPago}  };

                    Parameters.guardarReporte(Response, ReportName.EstadoCuenta, prt, intialPath);
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.EstadoCuenta));

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