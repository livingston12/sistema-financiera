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
    
    public partial class CLIENTE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CLIENTE()
        {
            this.FINANCY = new HashSet<FINANCY>();
        }
    
        public int CTE_CODIGO { get; set; }
        public string CTE_NOMBRE { get; set; }
        public string CTE_APELLI { get; set; }
        public string CTE_CEDULA { get; set; }
        public string CTE_TELEFO { get; set; }
        public string CTE_DIRECC { get; set; }
        public string CTE_ZONA { get; set; }
        public Nullable<int> CTE_TIPO { get; set; }
        public string CTE_TELEFO2 { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FINANCY> FINANCY { get; set; }
        public virtual ZONA ZONA { get; set; }
        public virtual TIPOCTE TIPOCTE { get; set; }
    }
}