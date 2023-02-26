using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Maestros
{
    public class rolesUser
    {
        public int value { get; set; }
        public string name { get; set; }
        public bool hasRole { get; set; }

    }
    [Authorize(Roles = "usuarios,admin")]
    public class UsuariosController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();
        // GET: Usuarios
        public ActionResult Index()
        {
            ViewBag.ListUsuarios = _db.USUARIOS.ToList();
            return View();
        }

        public JsonResult CrearUsuario(USUARIOS usu)
        {
            message message;
            try
            {

                if (!validarUsuario(usu))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                limpiarUsuarios(usu);
                _db.USUARIOS.Add(usu);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Usuario insertado correctamente",
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

        private static void limpiarUsuarios(USUARIOS usuario)
        {
            usuario.Usuario = usuario.Usuario.Trim();
            usuario.Nombre = usuario.Nombre.Trim();
            usuario.Apellido = usuario.Apellido;
        }

        public JsonResult ActualizarUsuario(USUARIOS usu)
        {
            message message;
            try
            {
                _db = new DB_IMBRINOEntities();
                var ListUsuario = _db.USUARIOS.FirstOrDefault(x => x.Id == usu.Id);
                if (ListUsuario == null)
                {
                    message = new message()
                    {
                        Message = "Usuario no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                if (!validarUsuario(usu))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                limpiarUsuarios(usu);

                ListUsuario.Nombre = usu.Nombre;
                ListUsuario.Apellido = usu.Apellido;
                ListUsuario.Usuario = usu.Usuario;
                ListUsuario.Tipo = usu.Tipo;

                _db.Entry(ListUsuario).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                message = new message()
                {
                    Message = "Usuario actualizado correctamente",
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

        public JsonResult ActualizarPass(string newPass, int usuarioId)
        {
            message message;

            try
            {
                var usuario = db.USUARIOS.FirstOrDefault(x => x.Id == usuarioId);
                if (usuario == null)
                {
                    throw new Exception("El usuario no existe");
                }
                usuario.Pass = newPass;
                message = new message()
                {
                    Is_Success = true,
                    Message = "Contraseña cambiada correctamente"
                };
            }
            catch (Exception ex)
            {
                message = new message()
                {
                    Is_Success = false,
                    Message = ex.Message
                };
            }

            return Json(message);
        }

        public JsonResult EliminarUsuario(int id)
        {
            message message;
            try
            {
                var usuario = _db.USUARIOS.FirstOrDefault(x => x.Id == id);

                if (usuario == null)
                {
                    message = new message()
                    {
                        Message = "Usuario no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.USUARIOS.Remove(usuario);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Usuario eliminado correctamente",
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

        public JsonResult GetCurrentUsuario(int id = 0)
        {

            var currentUsuario = _db.USUARIOS.Where(x => x.Id == id)
                                            .Select(x =>
                                            new
                                            {
                                                Nombre = x.Nombre.Trim(),
                                                Usuario = x.Usuario.Trim(),
                                                Pass = string.Empty,
                                                x.Tipo,
                                                x.Apellido
                                            })
                                            .FirstOrDefault();
            return Json(currentUsuario);
        }


        public ActionResult _usuariosDetalle()
        {
            return PartialView();
        }

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public ActionResult _cambiarPass()
        {
            return PartialView();
        }
        public ActionResult _agregarRoles(int idUsuario)
        {
            var usuario = db.USUARIOS.FirstOrDefault(x => x.Id == idUsuario);
            
            var rolIds = db.USUARIO_ROLES.Where(x => x.UsuarioId == idUsuario).Select(x=>x.RolId).ToList();
            var allRoles = db.ROLES.ToList().Select(x => new rolesUser
            {
                value = x.Id,
                name = $"{x.Descripcion} ({x.Rol})",
                hasRole = rolIds.Contains(x.Id) ||
                    usuario.TIPOS_USUARIOS.Tipo == "Administrador"
            });

            ViewBag.allRoles = allRoles;
            return PartialView();
        }

        public ActionResult agregarRoles(int[] rolIds, int usuarioId)
        {
            message message;

            try
            {
                //delete
                var deleteRols = db.USUARIO_ROLES.Where(x => x.UsuarioId == usuarioId);
                db.USUARIO_ROLES.RemoveRange(deleteRols);
                db.SaveChanges();

                //add
                var addRols = rolIds.Select(x => new USUARIO_ROLES
                {
                    RolId = x,
                    UsuarioId = usuarioId
                });
                db.USUARIO_ROLES.AddRange(addRols);
                db.SaveChanges();

                message = new message()
                {
                    Is_Success = true,
                    Message = "Se agregaron correctamente los roles al usuario"
                };
                
            }
            catch (Exception ex)
            {
                message = new message()
                {
                    Is_Success = false,
                    Message = ex.Message
                };
            }

            return Json(message);
        }

        public JsonResult GetTiposUsuarios()
        {
            var list = _db.TIPOS_USUARIOS.Select(x => new { id = x.Id, value = x.Tipo }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}