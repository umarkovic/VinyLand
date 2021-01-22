using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VinyLand.Models;

namespace VinyLand.ModelView
{
    public class RecBuy
    {
        public List<SelectListItem> slreplace { get; set; }
        [Required (ErrorMessage = "Choose one option")]
        public string radio { get; set; }
        public Record Record { get; set; }
        public string recordSeller { get; set; }
        public User User { get; set; }
    }
}