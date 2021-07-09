using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_CierreCajaGeneral
    {
        public IEnumerable<View_CierreCaja> Detalle { get; set; }
        public View_CierreCajaResumen Resumen { get; set; }
    }
    public class View_CierreCaja
    {
        public int ID { get; set; }
        public string Tipo { get; set; }
        public string TipoTexto { get; set; }
        public IEnumerable<View_CierreCajaDetalle> Detalle  { get; set; }
        public decimal? TotalTipo { get; set; }
        public decimal? TotalTipoAnterior { get; set; }
    }

    public class View_CierreCajaDetalle
    {
        public string Cliente { get; set; }
        public string Recibo { get; set; }
        public string Descripcion { get; set; }
        public decimal? MontoTotal { get; set; }
        public decimal? MontoCapital { get; set; }
        public decimal MontoInteres { get; set; }
        public DateTime? Fecha { get; set; }
        public string FechaTexto { get; set; }
    }

    public class View_CierreCajaResumen
    {
        private decimal? _TotalCapital;

        public decimal? TotalCapital
        {
            get { return  _TotalCapital == null ? 0 : _TotalCapital; }
            set { _TotalCapital = value; }
        }
        private decimal? _TotalInteres;
        public decimal? TotalInteres
        {
            get { return _TotalInteres == null ? 0 : _TotalInteres; }
            set { _TotalInteres = value; }
        }
        private decimal? _IngresosNoNetos;
        public decimal? IngresosNoNetos
        {
            get { return _IngresosNoNetos == null ? 0 : _IngresosNoNetos; }
            set { _IngresosNoNetos = value; }
        }
        private decimal? _OtrosIngresosNetos;
        public decimal? OtrosIngresosNetos
        {
            get { return _OtrosIngresosNetos == null ? 0 : _OtrosIngresosNetos; }
            set { _OtrosIngresosNetos = value; }
        }
        private decimal? _DepositoTranferencia;
        public decimal? DepositoTranferencia
        {
            get { return _DepositoTranferencia == null ? 0 : _DepositoTranferencia; }
            set { _DepositoTranferencia = value; }
        }
       
        public decimal? TotalIngresado
        {
            get
            { return 
                    TotalCapital 
                    + TotalInteres 
                    + IngresosNoNetos
                    + OtrosIngresosNetos;
            }            
        }

        public decimal? TotalIngresosNeto
        {
            get
            {
                return
                      TotalInteres
                      + OtrosIngresosNetos;
            }
        }

    }
}