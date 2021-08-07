﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class DB_IMBRINOEntities : DbContext
    {
        public DB_IMBRINOEntities()
            : base("name=DB_IMBRINOEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<INGOTRO> INGOTRO { get; set; }
        public virtual DbSet<CLIENTE> CLIENTE { get; set; }
        public virtual DbSet<archivos_subir> archivos_subir { get; set; }
        public virtual DbSet<TIPOINT> TIPOINT { get; set; }
        public virtual DbSet<FORMAPG> FORMAPG { get; set; }
        public virtual DbSet<TIPOFIN> TIPOFIN { get; set; }
        public virtual DbSet<FIADOR> FIADOR { get; set; }
        public virtual DbSet<VENDEDOR> VENDEDOR { get; set; }
        public virtual DbSet<FACTURA> FACTURA { get; set; }
        public virtual DbSet<FINANCY> FINANCY { get; set; }
        public virtual DbSet<CARTERA> CARTERA { get; set; }
        public virtual DbSet<ESTDCTA> ESTDCTA { get; set; }
        public virtual DbSet<VW_rptCuotasVencidas> VW_rptCuotasVencidas { get; set; }
        public virtual DbSet<VW_rptFinAtrasados> VW_rptFinAtrasados { get; set; }
        public virtual DbSet<VW_rptFinAtrasados_details> VW_rptFinAtrasados_details { get; set; }
        public virtual DbSet<VW_rptEstadoCuenta> VW_rptEstadoCuenta { get; set; }
        public virtual DbSet<INGCUOTA> INGCUOTA { get; set; }
        public virtual DbSet<ABOCUOTA> ABOCUOTA { get; set; }
        public virtual DbSet<VW_rptRegistroNCF> VW_rptRegistroNCF { get; set; }
        public virtual DbSet<Vw_SearchClientes_Fiadores> Vw_SearchClientes_Fiadores { get; set; }
        public virtual DbSet<ZONA> ZONA { get; set; }
        public virtual DbSet<TIPOCTE> TIPOCTE { get; set; }
        public virtual DbSet<CUOTA> CUOTA { get; set; }
        public virtual DbSet<INGRESO> INGRESO { get; set; }
        public virtual DbSet<OTROCARG> OTROCARG { get; set; }
        public virtual DbSet<ABOOCARG> ABOOCARG { get; set; }
        public virtual DbSet<CARGO> CARGO { get; set; }
        public virtual DbSet<vw_ConsultaFinDetalle> vw_ConsultaFinDetalle { get; set; }
        public virtual DbSet<vw_ConsultaFin> vw_ConsultaFin { get; set; }
        public virtual DbSet<TIPOCR1> TIPOCR1 { get; set; }
        public virtual DbSet<TIPOCR2> TIPOCR2 { get; set; }
        public virtual DbSet<BANCO> BANCO { get; set; }
        public virtual DbSet<CTABANCO> CTABANCO { get; set; }
        public virtual DbSet<OTROSCR> OTROSCR { get; set; }
        public virtual DbSet<TIPODB1> TIPODB1 { get; set; }
        public virtual DbSet<TIPODB2> TIPODB2 { get; set; }
        public virtual DbSet<OTROSDB> OTROSDB { get; set; }
        public virtual DbSet<VW_rptCreditosBancarios> VW_rptCreditosBancarios { get; set; }
        public virtual DbSet<VW_rptDebitosBancarios> VW_rptDebitosBancarios { get; set; }
        public virtual DbSet<vw_CuadreCaja> vw_CuadreCaja { get; set; }
        public virtual DbSet<VW_rptReciboIngreso> VW_rptReciboIngreso { get; set; }
    
        public virtual ObjectResult<ColumnsTables_Result> ColumnsTables(string table_name)
        {
            var table_nameParameter = table_name != null ?
                new ObjectParameter("table_name", table_name) :
                new ObjectParameter("table_name", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<ColumnsTables_Result>("ColumnsTables", table_nameParameter);
        }
    
        public virtual ObjectResult<sp_cuotasVencidas_Result> sp_cuotasVencidas(Nullable<System.DateTime> fecha_Pago)
        {
            var fecha_PagoParameter = fecha_Pago.HasValue ?
                new ObjectParameter("Fecha_Pago", fecha_Pago) :
                new ObjectParameter("Fecha_Pago", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_cuotasVencidas_Result>("sp_cuotasVencidas", fecha_PagoParameter);
        }
    }
}
