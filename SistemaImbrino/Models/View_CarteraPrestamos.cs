using SistemaImbrino.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_CarteraPrestamos
    {
        public IEnumerable<View_DetalleCuenta> Detalle { get; set; }
        public View_ResumenFinaciamiento Resumen { get; set; }
    }
    public class View_DetalleCuenta
    {
        public string BancoID { get; set; }
        public string NumeroCuenta { get; set; }
        public string Monto { get; set; }

        public string Banco
        {
            get {
                int.TryParse(BancoID, out int id);
                return BaseController.getBanco(id).Result;
            }
        }

    }
    public class View_ResumenFinaciamiento
    {
        public double Capital { get; set; }
        public double Interes { get; set; }
    }
}