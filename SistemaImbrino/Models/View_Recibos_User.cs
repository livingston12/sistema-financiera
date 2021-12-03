using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SistemaImbrino.Models;

namespace SistemaImbrino.Models
{
    public class View_Recibos_User
    {

       
        public int? numFin { get; set; }
        public int? numRecibo { get; set; }

        public int? getCodCliente()
        {
            DB_IMBRINOEntities _db = new DB_IMBRINOEntities();
           int? cliente = _db.FINANCY
                            .Where(x => x.FIN_NUMERO == numFin)
                            .FirstOrDefault().FIN_NUMCTE;
            return cliente;

        }

        public string clienteCompleto
        {
            get { return View_Financiamiento_User.getCliente(getCodCliente()); }
        }

        public string clienteRecibo
        {
            get { return $"{numRecibo.ToString()} - {clienteCompleto}"; }
        }
    }
}