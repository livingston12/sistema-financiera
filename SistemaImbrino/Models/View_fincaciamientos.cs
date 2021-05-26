using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_fincaciamientos 
    {
        public int numCuota { get; set; }
        public string Cliente { get; set; }
        public string Fiador { get; set; }
        public string Promotor { get; set; }
        public int TipoFin { get; set; }
        public decimal Monto { get; set; }
        public decimal Balance { get; set; }
        public decimal interes { get; set; }
        public decimal capital { get; set; }        
        public string Fecha { get; set; }
        public int FormaPago { get; set; }
        public int TipoInteres { get; set; }
        public decimal PorInt { get; set; }
        public int CuotasNormales { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int CuotasAdiccionales { get; set; }
        public string Garantia { get; set; }
        public string Observacion { get; set; }
        public string TipoCuota { get; set; }
        public List<View_fincaciamientos> listCuotasAdiccionales { get; set; }



    }
}