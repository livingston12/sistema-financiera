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
    
    public partial class TransactionLogs
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string NameMethod { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> UserId { get; set; }
    }
}