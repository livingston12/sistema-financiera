using SistemaImbrino.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Maestros
{
    [Authorize(Roles = "cuentasbancarias,admin")]
    public class CuentasBancariasController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        public ActionResult Index()
        {
            ViewBag.ListCuentas = _db.CTABANCO.ToList();
            return View();
        }

        public JsonResult CrearCuenta(CTABANCO CUENTA)
        {
            message message;
            try
            {

                if (!validarCuenta(CUENTA))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                CUENTA.CTA_CODIGO = lastCodCuenta();
                CUENTA.CTA_BALCOR = "0";
                CUENTA.CTA_BALFEC = "0";

                _db.CTABANCO.Add(CUENTA);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cuenta Bancaria insertada correctamente",
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

        public JsonResult ActualizarCuenta(CTABANCO CUENTA)
        {
            message message;
            try
            {
                var searchCuenta = _db.CTABANCO.Where(x => x.CTA_CODIGO == CUENTA.CTA_CODIGO).FirstOrDefault();
                if (searchCuenta == null)
                {
                    message = new message()
                    {
                        Message = "Cuenta Bancaria no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                searchCuenta.CTA_BANCO = CUENTA.CTA_BANCO;
                searchCuenta.CTA_NUMERO = CUENTA.CTA_NUMERO;
                searchCuenta.CTA_BALCOR = "0";
                searchCuenta.CTA_BALFEC = "0";
                searchCuenta.CTA_FECCOR = null;

                _db.Entry(searchCuenta).State = EntityState.Modified;
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cuenta Bancaria actualizada correctamente",
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

        public JsonResult EliminarCuenta(int id)
        {
            message message;
            try
            {
                var CUENTA = _db.CTABANCO.Where(x => x.CTA_CODIGO == id).ToList();

                if (!CUENTA.Any())
                {
                    message = new message()
                    {
                        Message = "Cuenta Bancaria no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.CTABANCO.Remove(CUENTA.FirstOrDefault());
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cuenta Bancaria eliminado correctamente",
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

        public ActionResult _cuentasDetalle()
        {
            return PartialView();
        }

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public JsonResult GetCurrentCuenta(int id = 0)
        {
            _db = new DB_IMBRINOEntities();
            var currentCuenta = _db.CTABANCO.Where(x => x.CTA_CODIGO == id)
                                           .Select(x =>
                                           new
                                           {
                                               x.CTA_NUMERO,
                                               x.CTA_BANCO,
                                               x.CTA_BALCOR,
                                               x.CTA_BALFEC,
                                               x.CTA_FECCOR
                                           })
                                                   .FirstOrDefault();
            return Json(currentCuenta);
        }

        public JsonResult GetBancos()
        {
            var list = _db.BANCO.Select(x => new { id = x.BCO_CODIGO, value = x.BCO_ABREVI }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}