using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaImbrino.App_Start
{
    public class View_generalClass
    {
       
        public static View_CuadreCajaGeneral GetListCuadreCaja()
        {
            View_CuadreCajaGeneral cajaGeneral = new View_CuadreCajaGeneral();
           
                DB_IMBRINOEntities db = new DB_IMBRINOEntities();

                DateTime? fechaCierre = db.SISTEMA.FirstOrDefault().FECHA_CIERRE;

                List<String> ListOtrosIngresosNetos = new List<string>()
            {
                "OTROS_EC","OTROS_DT"
            };
                List<String> ListTotalIngresado = new List<string>()
            {
                "CUOTAS_EC","CUOTAS_DT"
            };

                var Cuotas = db.vw_CuadreCaja
                                .Where(x => x.isCuadrada == false //&&
                                        //x.fechadt >= fechaCierre
                                        )
                                .Select(x => new View_rptNCFAgrupado()
                                {
                                    FormaPago = x.ING_FORMPG,
                                    NCFDetalle = x
                                })
                                .ToList();
                var otros = db.vw_CuadreCaja
                                .Where(x => x.isCuadrada == false &&
                                        //x.fechadt >= fechaCierre &&
                                        x.montoOtro > 0 &&
                                        x.ING_MONTOT > 0
                                        )
                                .Select(x => new View_rptNCFAgrupado()
                                {
                                    Ids = "3,4",
                                    FormaPago = x.ING_FORMPG,
                                    NCFDetalle = x,
                                })
                                .ToList();
                Cuotas.AddRange(otros);

                var detalleByTipo = new View_CuadreCajaLista
                {
                    // Ingreso de Cuotas en efectivo y cheque
                    CUOTAS_EC = Cuotas
                                    .Where(x => x.Id == 1)
                                    .GroupBy(x => new
                                    {
                                        x.Id,
                                        x.Tipo,
                                        x.TipoTexto
                                    })
                                    .Select((x, i) =>
                                                new View_CuadreCaja()
                                                {
                                                    ID = x.Key.Id,
                                                    Tipo = x.Key.Tipo,
                                                    TipoTexto = x.Key.TipoTexto,
                                                    KeyID = i + 1,
                                                    TotalTipo = x.Where(z => z.Tipo == x.Key.Tipo)
                                                                .Select(z =>
                                                                    new
                                                                    {
                                                                        z.NCFDetalle.ING_NUMREC,
                                                                        z.NCFDetalle.ING_MONTOT,
                                                                        z.NCFDetalle.ING_DESCRI
                                                                    })
                                                                .Distinct()
                                                                .Sum(y => y.ING_MONTOT),
                                                    TotalTipoAnterior = Cuotas.Where(z => z.Id == x.Key.Id - 1)
                                                                .Select(z =>
                                                                    new
                                                                    {
                                                                        z.NCFDetalle.ING_NUMREC,
                                                                        z.NCFDetalle.ING_MONTOT,
                                                                        z.NCFDetalle.ING_DESCRI
                                                                    })
                                                                .Distinct()
                                                                .Sum(y => y.ING_MONTOT),
                                                    Detalle = x.Where(z => z.Tipo == x.Key.Tipo)
                                                        .GroupBy(y => y.NCFDetalle.ING_NUMREC)
                                                        .Select((z, ii) => new View_CuadreCajaDetalle
                                                        {
                                                            keyId = ii + 1,
                                                            Recibo = z.Key,
                                                            Cliente = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.Cliente,
                                                            Descripcion = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_DESCRI_NCF,
                                                            Fecha = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.fechadt,
                                                            FechaTexto = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_FECHA,
                                                            MontoCapital = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_MONTOC,
                                                            MontoInteres = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_MONTOI,
                                                            MontoTotal = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_MONTOT
                                                        })
                                                        .OrderBy(z => z.Recibo)
                                                }),
                    // Ingreso de Cuotas en deposito y tranferencia
                    CUOTAS_DT = Cuotas
                                    .Where(x => x.Id == 2)
                                    .GroupBy(x => new
                                    {
                                        x.Id,
                                        x.Tipo,
                                        x.TipoTexto
                                    })
                                    .Select((x, i) =>
                                                new View_CuadreCaja()
                                                {
                                                    ID = x.Key.Id,
                                                    Tipo = x.Key.Tipo,
                                                    TipoTexto = x.Key.TipoTexto,
                                                    KeyID = i + 1,
                                                    TotalTipo = x.Where(z => z.Tipo == x.Key.Tipo)
                                                                .Select(z =>
                                                                    new
                                                                    {
                                                                        z.NCFDetalle.ING_NUMREC,
                                                                        z.NCFDetalle.ING_MONTOT,
                                                                        z.NCFDetalle.ING_DESCRI
                                                                    })
                                                                .Distinct()
                                                                .Sum(y => y.ING_MONTOT),
                                                    TotalTipoAnterior = Cuotas.Where(z => z.Id == x.Key.Id - 1)
                                                                .Select(z =>
                                                                    new
                                                                    {
                                                                        z.NCFDetalle.ING_NUMREC,
                                                                        z.NCFDetalle.ING_MONTOT,
                                                                        z.NCFDetalle.ING_DESCRI
                                                                    })
                                                                .Distinct()
                                                                .Sum(y => y.ING_MONTOT),
                                                    Detalle = x.Where(z => z.Tipo == x.Key.Tipo)
                                                        .GroupBy(y => y.NCFDetalle.ING_NUMREC)
                                                        .Select((z, ii) => new View_CuadreCajaDetalle
                                                        {
                                                            keyId = ii + 1,
                                                            Recibo = z.Key,
                                                            Cliente = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.Cliente,
                                                            Descripcion = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_DESCRI_NCF,
                                                            Fecha = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.fechadt,
                                                            FechaTexto = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_FECHA,
                                                            MontoCapital = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_MONTOC,
                                                            MontoInteres = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_MONTOI,
                                                            MontoTotal = z.FirstOrDefault(j => j.NCFDetalle.ING_NUMREC == z.Key).NCFDetalle.ING_MONTOT
                                                        })
                                                        .OrderBy(z => z.Recibo)
                                                }),
                    // Otros cargos en efectivo y cheque
                    OTROS_EC = Cuotas
                                    .Where(x => x.Id == 3)
                                    .GroupBy(x => new
                                    {
                                        x.Id,
                                        x.Tipo,
                                        x.TipoTexto
                                    })
                                    .Select((x, i) =>
                                                new View_CuadreCaja()
                                                {
                                                    ID = x.Key.Id,
                                                    Tipo = x.Key.Tipo,
                                                    TipoTexto = x.Key.TipoTexto,
                                                    KeyID = i + 1,
                                                    TotalTipo = x.Where(z => z.Tipo == x.Key.Tipo)
                                                            .Sum(z => z.NCFDetalle.montoOtro),
                                                    TotalTipoAnterior = Cuotas.Where(z => z.Id == x.Key.Id - 1)
                                                            .Sum(z => z.NCFDetalle.montoOtro),
                                                    Detalle = x.Where(z => z.Tipo == x.Key.Tipo)
                                                        .Select((z, ii) => new View_CuadreCajaDetalle
                                                        {
                                                            keyId = ii + 1,
                                                            Recibo = z.NCFDetalle.ING_NUMREC,
                                                            Cliente = z.NCFDetalle.Cliente,
                                                            Descripcion = z.NCFDetalle.descriOtro,
                                                            Fecha = z.NCFDetalle.fechadt,
                                                            FechaTexto = z.NCFDetalle.ING_FECHA,
                                                            MontoCapital = z.NCFDetalle.ING_MONTOC,
                                                            MontoInteres = z.NCFDetalle.ING_MONTOI,
                                                            MontoTotal = z.NCFDetalle.montoOtro
                                                        })
                                                        .OrderBy(z => z.Recibo)
                                                }),
                    // Otros cargos en deposito y tranferencia
                    OTROS_DT = Cuotas
                                    .Where(x => x.Id == 4)
                                    .GroupBy(x => new
                                    {
                                        x.Id,
                                        x.Tipo,
                                        x.TipoTexto
                                    })
                                    .Select((x, i) =>
                                                new View_CuadreCaja()
                                                {
                                                    ID = x.Key.Id,
                                                    Tipo = x.Key.Tipo,
                                                    TipoTexto = x.Key.TipoTexto,
                                                    KeyID = i + 1,
                                                    TotalTipo = x.Where(z => z.Tipo == x.Key.Tipo)
                                                            .Sum(z => z.NCFDetalle.montoOtro),
                                                    TotalTipoAnterior = Cuotas.Where(z => z.Id == x.Key.Id - 1)
                                                            .Sum(z => z.NCFDetalle.montoOtro),
                                                    Detalle = x.Where(z => z.Tipo == x.Key.Tipo)
                                                        .Select((z, ii) => new View_CuadreCajaDetalle
                                                        {
                                                            keyId = ii + 1,
                                                            Recibo = z.NCFDetalle.ING_NUMREC,
                                                            Cliente = z.NCFDetalle.Cliente,
                                                            Descripcion = z.NCFDetalle.descriOtro,
                                                            Fecha = z.NCFDetalle.fechadt,
                                                            FechaTexto = z.NCFDetalle.ING_FECHA,
                                                            MontoCapital = z.NCFDetalle.ING_MONTOC,
                                                            MontoInteres = z.NCFDetalle.ING_MONTOI,
                                                            MontoTotal = z.NCFDetalle.montoOtro
                                                        })
                                                        .OrderBy(z => z.Recibo)
                                                }),
                    // Salidas 
                    SALIDAS = db.VW_CuadreCajaSalidas
                    .GroupBy(x => new
                    {
                        x.ID,
                        x.Tipo,
                        x.TextoTipo
                    })
                    .ToList()
                    .Select((x, i) =>
                    new View_CuadreCaja()
                    {
                        ID = x.Key.ID,
                        KeyID = i + 1,
                        Tipo = x.Key.Tipo,
                        TipoTexto = x.Key.TextoTipo,
                        TotalTipo = x.Sum(y => y.MontoTotal),
                        Detalle = x.Select((y, ii) => new View_CuadreCajaDetalle()
                        {
                            Cliente = y.Cliente,
                            Descripcion = y.Descripcion,
                            Fecha = y.Fecha,
                            FechaTexto = y.FechaTexto,
                            keyId = ii + 1,
                            MontoCapital = y.MontoCapital,
                            MontoInteres = y.MontoInteres,
                            MontoTotal = y.MontoTotal,
                            Recibo = y.Recibo
                        })
                    })
                };
                List<View_CuadreCaja> CuotasTotales = new List<View_CuadreCaja>();
                if (detalleByTipo.CUOTAS_EC.Any())
                {
                    CuotasTotales.AddRange(detalleByTipo.CUOTAS_EC.ToList());
                }

                if (detalleByTipo.CUOTAS_DT != null)
                {
                    CuotasTotales.AddRange(detalleByTipo.CUOTAS_DT.ToList());
                }

                if (detalleByTipo.OTROS_EC != null)
                {
                    CuotasTotales.AddRange(detalleByTipo.OTROS_EC.ToList());
                }

                if (detalleByTipo.OTROS_DT != null)
                {
                    CuotasTotales.AddRange(detalleByTipo.OTROS_DT.ToList());
                }

                if (detalleByTipo.SALIDAS != null)
                {
                    CuotasTotales.AddRange(detalleByTipo.SALIDAS.ToList());
                }

                cajaGeneral = new View_CuadreCajaGeneral()
                {
                    Detalle = CuotasTotales.OrderBy(z => z.ID),
                    Resumen = CuotasTotales
                                .Select(x => new View_CuadreCajaResumen()
                                {
                                    TotalCapital = CuotasTotales.Where(z => ListTotalIngresado
                                                        .Contains(z.Tipo))
                                                    .Sum(z => z.Detalle.Sum(y => y.MontoCapital)),
                                    TotalInteres = CuotasTotales.Where(z => ListTotalIngresado
                                                        .Contains(z.Tipo))
                                                    .Sum(z => z.Detalle.Sum(y => y.MontoInteres)),
                                    IngresosNoNetos = CuotasTotales
                                                        .Where(z => z.Tipo == "INGRESOS_NN")
                                                        .Sum(z => z.Detalle.Sum(y => y.MontoTotal)),
                                    OtrosIngresosNetos = CuotasTotales
                                                            .Where(z => ListOtrosIngresosNetos.Contains(z.Tipo))
                                                            .Sum(z => z.Detalle.Sum(y => y.MontoTotal)),
                                    DepositoTranferencia = CuotasTotales
                                                            .Where(z => z.Tipo == "SALIDAS")
                                                            .Sum(z => z.Detalle.Sum(y => y.MontoTotal)),
                                })
                                .FirstOrDefault()
                };
            
            return cajaGeneral;


        }

        public static async Task<View_CarteraPrestamos> getCarteraPrestamos()
        {
            DB_IMBRINOEntities db = new DB_IMBRINOEntities();
            View_CarteraPrestamos cartera = new View_CarteraPrestamos()
            {
                Resumen = await getCarteraPrestamoResumen(),
                Detalle = await getCarteraPrestamosDetalle()
            };
            return cartera;
        }

        public static Task<IEnumerable<View_DetalleCuenta>> getCarteraPrestamosDetalle()
        {
            DB_IMBRINOEntities db = new DB_IMBRINOEntities();

            IEnumerable<View_DetalleCuenta> cartera = db.CTABANCO
               .Select(x =>
                   new View_DetalleCuenta()
                   {
                       BancoID = x.CTA_BANCO,
                       Monto = x.CTA_BALFEC,
                       NumeroCuenta = x.CTA_NUMERO,
                   })
               .ToList();
            return Task.Run<IEnumerable<View_DetalleCuenta>>(() =>
            {
                return cartera;
            });
        }

        public static Task<View_ResumenFinaciamiento> getCarteraPrestamoResumen()
        {
            DB_IMBRINOEntities db = new DB_IMBRINOEntities();

            View_ResumenFinaciamiento resumen = db.CARTERA
                           .Select(x => new View_ResumenFinaciamiento
                           {
                               Capital = x.CAR_MONTOC.HasValue ?
                                           x.CAR_MONTOC.Value : 0,
                               Interes = x.CAR_MONTOI.HasValue ?
                                           x.CAR_MONTOI.Value : 0
                           })
                           .FirstOrDefault();
            return Task.Run<View_ResumenFinaciamiento>(() =>
            {
                return resumen;
            });
        }

    }
}