using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_cliente
    {
        public int CTE_CODIGO { get; set; }
        [Required(ErrorMessage ="El campo {0} es requerido")]
        public string CTE_NOMBRE { get; set; }
        public string CTE_APELLI { get; set; }
        public string CTE_CEDULA { get; set; }
        public string CTE_TELEFO { get; set; }
        public string CTE_DIRECC { get; set; }
        public string CTE_ZONA { get; set; }
        public Nullable<int> CTE_TIPO { get; set; }
    }
}