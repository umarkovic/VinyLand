using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
    public class Release
    {
        public Label Label { get; set; }
        public Record Record { get; set; }
        public DateTime Date { get; set; }
        
    }
}