using SistemaImbrino.App_Start;
using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Cuadre_Caja
{
    [Authorize(Roles = "cuadrecaja,admin")]
    public class cuadreCajaController : BaseController
    {

        // GET: CierreCaja
        public ActionResult Index()
        {
            var data = View_generalClass.GetListCuadreCaja();
            ViewBag.ListCuadreCaja = data;
            ViewBag.existeDetalle = data.ExisteDetalle;
            return View();
        }

        public ActionResult _cuadreCajaResumen()
        {
            return PartialView();
        }

        public ActionResult PrintReport(string json)
        {
            message mensajeReturn = new message();
            View_fechas fechas = new View_fechas();
            mensajeReturn.Is_Success = true;

            try
            {
                fechas = Newtonsoft.Json.JsonConvert.DeserializeObject<View_fechas>(json);
                string intialPath = Server.MapPath(Parameters.rutaReporte);

                if (fechas.FechaDesdeDt == null)
                {
                    if (fechas.FechaDesdeDt == null)
                    {
                        mensajeReturn.Message = "Fecha desde es obligatoria";
                        mensajeReturn.Is_Success = false;
                    }
                }
                else if (fechas.FechaDesdeDt > fechas.FechaHastaDt)
                {
                    mensajeReturn.Message = "Fecha desde no puede ser mayor a fecha hasta";
                    mensajeReturn.Is_Success = false;
                }

                if (mensajeReturn.Is_Success)
                {
                    List<Parameters> prt = new List<Parameters>()
                    {
                        new Parameters(){ParameterName="FechaDesde",ParameterValue=fechas.FechaDesdeDt },
                        new Parameters(){ParameterName="FechaHasta",ParameterValue=fechas.FechaHastaDt }
                    };
                    Parameters.guardarReporte(Response, ReportName.CuadreCaja, prt, intialPath);
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.CuadreCaja));

                var fsResult = Parameters.viewReportPDF(ruta);
                return fsResult;
            }
            catch (Exception e)
            {
                return Content("Error inesperado en el reporte: " + e.Message);

            }
        }

        public async Task<ActionResult> fechasCierres()
        {
            (View_fechas data, string textoFecha) result;
            result.data = new View_fechas() {
                FechaDesde = DateTime.Now.Date.ToString("yyyy-MM-dd"),
                FechaHasta = DateTime.Now.Date.ToString("yyyy-MM-dd")
            };
            try
            {
                result.data = await fechasCierreCaja();
            }
            catch
            {

            }
            result.textoFecha = $"{returnDate(result.data.FechaDesdeDt.Value)} al {returnDate(result.data.FechaHastaDt)}";
          return Json(result);
        }
    }
}