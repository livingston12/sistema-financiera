using SistemaImbrino.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Maestros
{
    [Authorize(Roles = "cargos,admin")]
    public class CargosController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        public ActionResult Index()
        {
            var cargos = _db.CARGO.ToList();
            ViewBag.ListCargos = cargos;
            if(!cargos.Any())
            {
                db.CARGO.Add(new CARGO() { CAR_CODIGO = "50", CAR_DESCRI = "MORA", enable = 1 });
            }
            return View();
        }

        public JsonResult CrearCargo(CARGO CARGO)
        {
            message message;
            try
            {

                if (!validarCargo(CARGO))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                CARGO.CAR_CODIGO = lastCodCargo();
                CARGO.enable = 1;
                _db.CARGO.Add(CARGO);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cargo insertado correctamente",
                    Is_Success = true
                };
            }

            catch (Exception ex)
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

        public JsonResult ActualizarCargo(CARGO CARGO)
        {
            message message;
            try
            {
                var searchCargo = _db.CARGO.Where(x => x.CAR_CODIGO == CARGO.CAR_CODIGO).FirstOrDefault();
                if (searchCargo == null)
                {
                    message = new message()
                    {
                        Message = "Cargo no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }

                if (searchCargo.CAR_DESCRI == "MORA")
                {
                    message = new message()
                    {
                        Message = "La mora no se puede modificar",
                        Is_Success = false
                    };
                    return Json(message);
                }
                searchCargo.CAR_DESCRI = CARGO.CAR_DESCRI;
                searchCargo.enable = CARGO.enable;

                _db.Entry(searchCargo).State = EntityState.Modified;
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cargo actualizado correctamente",
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

        public JsonResult EliminarCargo(string id)
        {
            message message;
            try
            {
                var CARGO = _db.CARGO.Where(x => x.CAR_CODIGO == id).ToList();

                if (!CARGO.Any())
                {
                    message = new message()
                    {
                        Message = "Cargo no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.CARGO.Remove(CARGO.FirstOrDefault());
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cargo eliminado correctamente",
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

        public ActionResult _cargoDetalle()
        {
            return PartialView();
        }

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public JsonResult GetCurrentCargo(string id = "0")
        {
            _db = new DB_IMBRINOEntities();
            var currentCargo = _db.CARGO.Where(x => x.CAR_CODIGO == id)
                                           .Select(x =>
                                           new
                                           {
                                               x.CAR_CODIGO,
                                               CAR_DESCRI = x.CAR_DESCRI.Trim(),
                                               x.enable
                                           })
                                           .FirstOrDefault();
            return Json(currentCargo);
        }
    }
}