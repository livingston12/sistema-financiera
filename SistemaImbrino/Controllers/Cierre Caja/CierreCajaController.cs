using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Cierre_Caja
{
    public class CierreCajaController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        public JsonResult cierreCaja()
        {
            message result = new message() { Is_Success = false};
            try
            {
                if (validaCajaCuadrada(result))
                {
                    _db.usp_CierreCaja();
                    result.Is_Success = true;
                    result.Message = "Caja cerrado correctamente";
                }
                
            }
            catch (Exception)
            {
                result.Message = MensajeErrorCath;
            }          
            return Json(result);
        }

        private bool validaCajaCuadrada(message message)
        {
            bool result = false;
            IEnumerable<vw_CuadreCaja> dataCuadreCaja =
                _db.vw_CuadreCaja;

            decimal entradas = dataCuadreCaja.Where(x => x.Tipo != "SALIDAS").Sum(x => x.MontoTotal);
            decimal salidas = dataCuadreCaja.Where(x => x.Tipo == "SALIDAS").Sum(x => x.MontoTotal);
            result = (entradas - salidas) == 0;
            if (!result)
            {
                message.Is_Success = false;
                message.Message = "No puede cerrar la caja porque no esta cuadrada";
            }
            return result;
        }
    }
}