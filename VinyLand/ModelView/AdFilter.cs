using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using VinyLand.Models;

namespace VinyLand.ModelView
{
    public class AdFilter
    {
        public List<Ad> ads { get; set; }
        public string genre { get; set; }
        public string artist { get; set; }
     
        public string inputUser { get; set; }

    }
}