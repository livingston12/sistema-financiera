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
    
    public partial class VW_rptEstadoCuenta
    {
        public int C__FIN { get; set; }
        public string CLIENTE { get; set; }
        public double MONTO_FINANCIAR { get; set; }
        public double BALANCE_INCIAL { get; set; }
        public double BALANCE_ACTUAL { get; set; }
        public double BALANCE_CAPITAL { get; set; }
        public double BALANCE_INTERES { get; set; }
        public int CUOTA_TOTAL { get; set; }
        public int CUOTA_PENDIENTE { get; set; }
        public string ULTIMO_PAGO { get; set; }
        public string EMPRESA { get; set; }
        public System.DateTime Fecha { get; set; }
    }
}