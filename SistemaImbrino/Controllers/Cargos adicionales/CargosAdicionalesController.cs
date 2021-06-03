using SistemaImbrino.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Cargos_adicionales
{
    public class CargosAdicionalesController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();
        // GET: CargosAdicionales
        public ActionResult Index()
        {
            ViewBag.ListCargos = _db.OTROCARG.ToList();            
            return View();
        }
        public JsonResult CrearCargo(OTROCARG otroCargo)
        {
            message message;
            try
            {
                if (!validarCargo(otroCargo))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                llenarOtroCargo(otroCargo);
                _db.OTROCARG.Add(otroCargo);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cargo adicional insertado correctamente",
                    Is_Success = true
                };
            }

            catch (Exception)
            {

                message = new message()
                {
                    Message = MensajeErrorCath,
                    Is_Success = false
                };
                return Json(message);
            }
            return Json(message);
        }

        private void llenarOtroCargo(OTROCARG otroCargo)
        {           
            otroCargo.CAR_SECU = MaxCarSecun(otroCargo.CAR_NUMFIN);
            otroCargo.CAR_STATUS = ((int)Status.NUEVO).ToString();
            var fechaSpt = otroCargo.CAR_FECHAR.Split('-');
            otroCargo.CAR_FECHAR = $"{returMonthNumber(fechaSpt[1])}/{fechaSpt[0]}/{fechaSpt[2]}";         
        }
        public JsonResult ActualizarCargo(OTROCARG otroCargo)
        {
            message message;
            try
            {
                _db = new DB_IMBRINOEntities();
                var cargo = _db.OTROCARG.Where(x => x.id == otroCargo.id).FirstOrDefault();
                string fechaR = string.Empty;
                if (cargo == null)
                {
                    message = new message()
                    {
                        Message = "Cargo adicional no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                fechaR = otroCargo.CAR_FECHAR;
                cargo.CAR_NUMFIN = otroCargo.CAR_NUMFIN;
                cargo.CAR_MONTOT = otroCargo.CAR_MONTOT;
                cargo.CAR_CODCAR = otroCargo.CAR_CODCAR;
                cargo.CAR_FECHAR = fechaR;

                _db.Entry(cargo).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                message = new message()
                {
                    Message = "Cargo adicional actualizado correctamente",
                    Is_Success = true
                };
            }
            catch (Exception e)
            {

                message = new message()
                {
                    Message = MensajeErrorCath,
                    Is_Success = false
                };
                return Json(message);
            }
            return Json(message);
        }

        public JsonResult EliminarCargo(int id)
        {
            message message;
            try
            {
                var cargo = _db.OTROCARG.Where(x => x.id == id).ToList();

                if (!cargo.Any())
                {
                    message = new message()
                    {
                        Message = "Cargo adicional no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.OTROCARG.Remove(cargo.FirstOrDefault());
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cargo adicional eliminado correctamente",
                    Is_Success = true
                };
            }
            catch (Exception)
            {

                message = new message()
                {
                    Message = MensajeErrorCath,
                    Is_Success = false
                };
                return Json(message);
            }
            return Json(message);
        }

        public ActionResult _clientesDetalle()
        {
            return PartialView();
        }

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public JsonResult GetCurrentCargo(int id = 0)
        {
            var currentCargo = _db.OTROCARG.Where(x => x.id == id).FirstOrDefault();
            return Json(currentCargo);
        }

        public JsonResult GetTiposCargo()
        {
            var list = _db.CARGO.Select(x => new { id = x.CAR_CODIGO, value = x.CAR_DESCRI }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public static string getStatus(string statusId)
        {
            Status status = (Status)Enum.Parse(typeof(Status), statusId);
            return status.ToString();
        }
        public static string getFecha(typeFecha tipoFecha,string fecha)
        {
            try
            {
                string[] strListFecha = fecha.Split('/');
                int mes = 0;
                switch (tipoFecha)
                {
                    case typeFecha.FechaR:
                        int.TryParse(strListFecha[0], out mes);
                        fecha = $"{strListFecha[1]}-{returMonthName(mes)}-{strListFecha[2]}";
                        break;
                    case typeFecha.FechaP:
                        int.TryParse(strListFecha[1], out mes);
                        fecha = $"{strListFecha[0]}-{returMonthName(mes)}-{strListFecha[2]}";
                        break;
                    default:
                        break;
                }               
            }
            catch (Exception)
            {

                fecha = string.Empty;
            }
            return fecha;
        }
    }
}