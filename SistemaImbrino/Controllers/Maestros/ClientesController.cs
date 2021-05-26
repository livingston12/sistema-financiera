using SistemaImbrino.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Maestros
{
    public class ClientesController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        // GET: Clientes
        public ActionResult Index()
        {
            ViewBag.ListCLientes = _db.CLIENTE.ToList();
            return View();
        }

        public JsonResult CrearCliente(CLIENTE cliente)
        {
            message message;
            try
            {

                if (!validarCliente(cliente))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                cliente.CTE_CODIGO = lastCodCliente();
                _db.CLIENTE.Add(cliente);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cliente insertado correctamente",
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

        public JsonResult ActualizarCliente(CLIENTE cliente)
        {
            message message;
            try
            {
                _db = new DB_IMBRINOEntities();
                var Listcliente = _db.CLIENTE.Where(x => x.CTE_CODIGO == cliente.CTE_CODIGO).FirstOrDefault();
                if (Listcliente == null)
                {
                    message = new message()
                    {
                        Message = "Cliente no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                Listcliente.CTE_NOMBRE = cliente.CTE_NOMBRE;
                Listcliente.CTE_APELLI = cliente.CTE_APELLI;
                Listcliente.CTE_ZONA = cliente.CTE_ZONA;
                Listcliente.CTE_TIPO = cliente.CTE_TIPO;
                Listcliente.CTE_TELEFO = cliente.CTE_TELEFO;
                Listcliente.CTE_CEDULA = cliente.CTE_CEDULA;

                _db.Entry(Listcliente).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                message = new message()
                {
                    Message = "Cliente actualizado correctamente",
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

        public JsonResult EliminarCliente(int id)
        {
            message message;
            try
            {
                var cliente = _db.CLIENTE.Where(x => x.CTE_CODIGO == id).ToList();

                if (!cliente.Any())
                {
                    message = new message()
                    {
                        Message = "Cliente no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.CLIENTE.Remove(cliente.FirstOrDefault());
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Cliente eliminado correctamente",
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
        public JsonResult GetCurrentCliente(int id = 0)
        {

            var currentCliente = _db.CLIENTE.Where(x => x.CTE_CODIGO == id)
                                            .Select(x =>
                                            new
                                            {
                                                CTE_NOMBRE = x.CTE_NOMBRE.Trim(),
                                                CTE_APELLI = x.CTE_APELLI.Trim(),
                                                CTE_DIRECC = x.CTE_DIRECC.Trim(),
                                                x.CTE_CEDULA,
                                                x.CTE_TELEFO,
                                                x.CTE_ZONA,
                                                x.CTE_TIPO
                                            })
                                                    .FirstOrDefault();
            return Json(currentCliente);
            //return PartialView(currentCliente);
        }

        public JsonResult GetZonas()
        {
            //db = new DB_IMBRINOEntities();
            var list = _db.ZONA.Select(x => new { id = x.ZON_CODIGO, value = x.ZON_DESCRI }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetTiposClientes()
        {
            //db = new DB_IMBRINOEntities();
            var list = _db.TIPOCTE.Select(x => new { id = x.TIP_CODIGO, value = x.TIP_DESCRI }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);

        }

    }
}