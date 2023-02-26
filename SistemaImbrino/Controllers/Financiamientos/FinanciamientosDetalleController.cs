using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Financiamientos
{
    [Authorize(Roles = "financiamientosDetalle,admin")]
    public class FinanciamientosDetalleController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        // GET: FinanciamientosDetalle
        public ActionResult Index(string id = "")
        {
            int clienteId = 0;
            int.TryParse(id, out clienteId);
            var listDetalle = _db.vw_ConsultaFinDetalle
                                .Where(x => x.ING_NUMFIN == clienteId);
                                //.Distinct()
                                //.OrderBy(x =>x.ING_NUMREC).ThenBy(x=>x.ING_FECHA);
            
            ViewBag.finID = id;
            ViewBag.Cliente = listDetalle.Any() 
                                ? listDetalle.FirstOrDefault().cliente
                                : string.Empty;
          
            return View("FinanciamientosDetalle");
        }

        public ActionResult getCurrentFindDetalle(int id =0)
        {
            IQueryable<View_consultaFinanciamientos> listDetalle = _db.usp_ConsultaFinDetalle(id)
                                 .Where(x => x.ING_NUMFIN == id)
                                 .GroupBy(x => new { x.ING_NUMREC, x.tipo })
                                 .Select(x => new View_consultaFinanciamientos
                                 {
                                     Cliente = x.Where(z => z.ING_NUMREC == x.Key.ING_NUMREC && z.tipo == x.Key.tipo)
                                                .FirstOrDefault().cliente,
                                     Fecha = x.Where(z => z.ING_NUMREC == x.Key.ING_NUMREC && z.tipo == x.Key.tipo)
                                              .FirstOrDefault().ING_FECHA,
                                     Descripcion = x.Where(z => z.ING_NUMREC == x.Key.ING_NUMREC && z.tipo == x.Key.tipo)
                                                    .FirstOrDefault().ING_DESCRI,
                                     BalanceGeneral = x.Max(z => z.ING_MONTOT),
                                     detail = new consultaFinanciamientosDetalle
                                     {
                                         Capital = x.Where(z => z.ING_NUMREC == x.Key.ING_NUMREC && z.tipo == x.Key.tipo)
                                                    .FirstOrDefault().ING_MONTOC,
                                         Interes = x.Where(z => z.ING_NUMREC == x.Key.ING_NUMREC && z.tipo == x.Key.tipo)
                                                    .FirstOrDefault().ING_MONTOI,
                                         Monto = x.Where(z => z.ING_NUMREC == x.Key.ING_NUMREC && z.tipo == x.Key.tipo)
                                                  .FirstOrDefault().ING_MONTOCAR,
                                         Tipo = x.Where(z => z.ING_NUMREC == x.Key.ING_NUMREC && z.tipo == x.Key.tipo)
                                                 .FirstOrDefault().tipo
                                     }                                    
                                 }).AsQueryable();
            
           
            return Json(listDetalle, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintReport(string FindID = "")
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;
            try
            {
                string intialPath = Server.MapPath(Parameters.rutaReporte);

                if (string.IsNullOrWhiteSpace(FindID))
                {
                    mensajeReturn.Message = "Numero financiamiento es obligatorio";
                    mensajeReturn.Is_Success = false;
                }

                if (mensajeReturn.Is_Success)
                {
                    List<Parameters> prt = new List<Parameters>()
                    {
                        new Parameters(){ParameterName="FindID",ParameterValue=FindID }
                    };
                    Parameters.guardarReporte(Response, ReportName.FinDetalle, prt, intialPath);
                }

            }
            catch (Exception e)
            {
                mensajeReturn.Message = "Error inesperado: " + e.Message;
                mensajeReturn.Is_Success = false;
            }

            return Json(mensajeReturn, JsonRequestBehavior.AllowGet);
        }

        public ActionResult generateReport()
        {

            try
            {
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.FinDetalle));

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