using SistemaImbrino.App_Start;
using SistemaImbrino.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Cierre_Caja
{
    [Authorize(Roles = "cierrecaja,admin")]
    public class CierreCajaController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();


        public async Task<JsonResult> cierreCaja()
        {
            _db = new DB_IMBRINOEntities();
            message result = new message() { Is_Success = false };
            try
            {
                if (await validaCajaCuadrada(result))
                {
                    var sistema = await _db.SISTEMA.FirstOrDefaultAsync();
                    if (sistema != null)
                    {
                        View_fechas fechaCierre = await fechaCierres();
                        sistema.FECHA_CIERRE = fechaCierre.FechaHastaDt;

                        _db.Entry(sistema).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        _db.usp_CierreCaja();
                        result.Is_Success = true;
                        result.Message = "Caja cerrado correctamente";
                    }
                    else
                    {
                        result.Is_Success = false;
                        result.Message = "La tabla sistema debe contener datos";
                    }

                }

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Json(result);
        }

        private async Task<bool> validaCajaCuadrada(message message)
        {
            bool result = false;
            View_CuadreCajaGeneral dataCuadreCaja =
                await Task.Run<View_CuadreCajaGeneral>(() =>
                {
                    return View_generalClass.GetListCuadreCaja();
                });


            decimal? entradas = dataCuadreCaja.Detalle.Where(x => x.Tipo != "SALIDAS").Sum(x => x.Detalle.Sum(y => y.MontoTotal));
            decimal? salidas = dataCuadreCaja.Detalle.Where(x => x.Tipo == "SALIDAS").Sum(x => x.Detalle.Sum(y => y.MontoTotal));

            result = (entradas - salidas) == 0;
            if (!result)
            {
                message.Is_Success = false;
                message.Message = "No puede cerrar la caja porque no esta cuadrada";
            }
            return result;
        }

        public async Task<ActionResult> fechasCierres()
        {
            (View_fechas data, string textoFecha) result;

            result.data = await fechaCierres();
            result.textoFecha = $"{returnDate(result.data.FechaDesdeDt.Value)} al {returnDate(result.data.FechaHastaDt)}";
            return Json(result);
        }

        public async Task<View_fechas> fechaCierres()
        {
            View_fechas data;
            data = new View_fechas()
            {
                FechaDesde = DateTime.Now.Date.ToString("yyyy-MM-dd"),
                FechaHasta = DateTime.Now.Date.ToString("yyyy-MM-dd")
            };
            try
            {
                data = await fechasCierreCaja();
            }
            catch
            {

            }
            return data;
        }

        public JsonResult ValidarDatos()
        {
            View_CuadreCajaGeneral ListCuadreCaja = View_generalClass.GetListCuadreCaja();
            bool existenDatos = ListCuadreCaja.Detalle.Any();
            return Json(existenDatos);
        }
    }
}