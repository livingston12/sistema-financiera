using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_Cobrar
    {
        public List<cl_cobro> Cobros { get; set; }
        
     
    }

    public class cl_cobro
    {
        public string tipoCobro { get; set; }
        public string numCuota { get; set; }       
        //public string num_TotalCuota { get; set; }
        public int numfin { get; set; }
        //public double descuentoMora { get; set; }
        //public double descuentoInteres { get; set; }
        public decimal montoPagado { get; set; }
        public decimal mora { get; set; }
        public double otroCargo { get; set; }
    }
}