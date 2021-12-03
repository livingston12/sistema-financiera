using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_Financiamiento_User
    {
        public int? codCliente { get; set; }
        public int numFinanciamiento { get; set; }


        public string clienteCompleto
        {
            get { return getCliente(codCliente); }
        }

        public static string getCliente(int? id)
        {
            DB_IMBRINOEntities db = new DB_IMBRINOEntities();
            var list = db.CLIENTE.Where(x => x.CTE_CODIGO == id).ToList();
            string value = list.Any() ?
                            $"{list.FirstOrDefault().CTE_NOMBRE} {list.FirstOrDefault().CTE_APELLI}"
                            : string.Empty;
            return value.Trim();
        }

        public string clienteFinanciamiento
        {
            get { return $"{numFinanciamiento.ToString()} - {clienteCompleto}";  }
        }

    }
}