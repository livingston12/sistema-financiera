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
    
    public partial class OTROSCR
    {
        public int ID { get; set; }
        public int BANCO { get; set; }
        public int CUENTA_BANCARIA { get; set; }
        public string NUMERO_CHEQUE { get; set; }
        public int TIPO_CREDITO { get; set; }
        public int TIPO_SALIDA { get; set; }
        public decimal MONTO { get; set; }
        public Nullable<System.DateTime> FECHA { get; set; }
        public int CLIENTE { get; set; }
        public string CONCEPTO { get; set; }
        public string BENEFICIARIO { get; set; }
        public bool Activo { get; set; }
    }
}
