using SistemaImbrino.Extentions;
using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SistemaImbrino.Controllers
{
    public class LoginController : Controller
    {
        private DB_IMBRINOEntities db = new DB_IMBRINOEntities();
        // GET: Login
        public ActionResult Index()
        {
            if (this.Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public ActionResult Login()
        {
            string user = Request.Form["username"].ToUpper().ToString();
            string pass = Request.Form["password"].ToString();
            var currentUser = validUser(user, pass);

            if (currentUser != null)
            {
                var isValidLogin = validateDateToLogin(currentUser);
                if (!isValidLogin)
                {
                    return Json("Su usuario esta bloqueado temporalmente, favor pagar el servicio para seguir utilizandolo");
                }
                string roles = "";
                List<int> userRoles = db.USUARIO_ROLES
                                    .Where(x => x.UsuarioId == currentUser.Id)
                                    .Select(x => x.RolId)
                                    .ToList();
                var listRoles = db.ROLES
                                    .Where(x => userRoles.Contains(x.Id))
                                    .Select(x => x.Rol)
                                    .ToList();

                if (currentUser.Tipo == 1)
                {
                    listRoles.Add("admin");
                }

                roles = string.Join(";", listRoles);
                var authUser =
                    new FormsAuthenticationTicket
                    (
                        1,
                        user,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(60),
                        true,
                        roles
                    );

                string encryptedUser = FormsAuthentication.Encrypt(authUser);

                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedUser);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                return Redirect(Url.Action("Index", "Home"));
            }
            else
            {
                return Json("Usuario o contraseña incorrectos");
            }
        }

        private USUARIOS validUser(string user, string pass)
        {
            var currentUser = db.USUARIOS.Where(x => x.Usuario == user && x.Pass == pass).FirstOrDefault();
            return currentUser;
        }

        private bool validateDateToLogin(USUARIOS usuario)
        {
            bool validated = false;
            DateTime dateRegister = DateTime.Parse("02/13/2023");
            DateTime? dateLogin = dateRegister.AddDays(1);
            validated = dateLogin == null ? true : dateLogin.NotExpirationDate();

            return validated;
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return this.RedirectToAction("Index");
        }
    }
}