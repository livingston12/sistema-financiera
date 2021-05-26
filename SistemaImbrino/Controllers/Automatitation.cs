using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaImbrino.Controllers
{
    public class Automatitation
    {
        private DB_IMBRINOEntities db = new DB_IMBRINOEntities();
        
        public  void automatizarArchivos()
        {
            db = new DB_IMBRINOEntities();
            db.Database.CommandTimeout = 0;
            OleDbConnection con = new OleDbConnection();
            try
            {
                
                string rutaArchivos = System.Configuration.ConfigurationManager.AppSettings["rutaArchivos"];
               
                con.ConnectionString = string.Format("Provider=VFPOLEDB.1;Data Source={0};Collating Sequence=machine;",rutaArchivos);
                con.Open();
                

                OleDbCommand ocmd = con.CreateCommand();           
                ocmd.CommandTimeout = 0;
              

                var archivos_subir = db.archivos_subir.Where(x => x.subir == true).ToList();
                foreach (var archivo in archivos_subir)
                {
                    var columnms = db.ColumnsTables(archivo.nombreTabla);
                    ColumnsTables_Result columnasTabla = db.ColumnsTables(archivo.nombreTabla).FirstOrDefault();
                 
                    ocmd.CommandText = columnasTabla.select_insert; //string.Format("SELECT * FROM {0}.DBF", archivo.nombreArchivo);
                   
                    DataTable dt = new DataTable();
                    dt.Load(ocmd.ExecuteReader());
                    
                    bulkInsert(dt, "STG." + archivo.nombreTabla);

                   
                }
                con.Close();
            }
            catch (Exception e)
            {
                var error = e.ToString();

            }
        }

        // Se utiliza para insertar varios registros con una sola coneccion sql
        private void bulkInsert(DataTable dt, string nombreTabla)
        {
            using (SqlConnection connection = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                SqlCommand command = new SqlCommand($"TRUNCATE TABLE {nombreTabla}", connection);
                
                // Crear sql bulk copy
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                // Tabla destino a insertar
                bulkCopy.DestinationTableName = nombreTabla;
                connection.Open();

                // Truncate table
                command.ExecuteNonQuery();

                // se escriben los datos del "dataTable"
                bulkCopy.WriteToServer(dt);
                connection.Close();






            }
        }

        public Boolean executeSqlCommand(String SQL, List<SqlParameter> parametros)
        {
            Boolean r = false;

            try
            {
                using (var db2 = new DB_IMBRINOEntities())
                {
                    db2.Database.ExecuteSqlCommand(SQL, parametros.ToArray());
                   
                    r = true;
                }
              
            }

            catch (Exception)
            {
                r = false;
            }

            return r;
        }
    }
}