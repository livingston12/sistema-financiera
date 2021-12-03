﻿namespace SistemaImbrino.Models
{
    public class PageIndex
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }


        public int TotalPages
        {
            get { return Limit == 0 ? 1 : Total / Limit; }            
        }

    }
}