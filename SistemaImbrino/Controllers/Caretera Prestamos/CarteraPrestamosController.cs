using SistemaImbrino.App_Start;
using SistemaImbrino.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Caretera_Prestamos
{
    [Authorize(Roles = "carteraprestamos,admin")]
    public class CarteraPrestamosController : BaseController
    {
        // GET: CarteraPrestamos
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetCarteraPrestamoDetalleAsync()
        {
            var cartera = await View_generalClass.getCarteraPrestamosDetalle();
            return Json(cartera);
        }

        public async Task<JsonResult> GetCarteraPrestamoResumenAsync()
        {
            var cartera = await View_generalClass.getCarteraPrestamoResumen();
            return Json(cartera);
        }

        public PartialViewResult _carteraPrestamo()
        {
            return PartialView();
        }

        public ActionResult PrintReport()
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;
            string intialPath = Server.MapPath(Parameters.rutaReporte);

            try
            {

                Parameters.guardarReporte(Response, ReportName.CarteraPrestamos, null, intialPath);
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.CarteraPrestamos));

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