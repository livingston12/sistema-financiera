//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SistemaImbrino.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class VW_rptReciboIngreso
    {
        public string NombreEmpresa { get; set; }
        public string DireccionEmpresa { get; set; }
        public string RncEmpresa { get; set; }
        public string TelefonoEmpresa { get; set; }
        public string Cliente { get; set; }
        public decimal BalancePendiente { get; set; }
        public int ING_NUMFIN { get; set; }
        public int Recibo { get; set; }
        public string CONCEPTO { get; set; }
        public string NCF { get; set; }
        public int Contrato { get; set; }
        public decimal Monto { get; set; }
        public string FormaPago { get; set; }
        public double ING_MONTOT { get; set; }
        public string Fecha { get; set; }
    }
}
