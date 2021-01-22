using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace VinyLand.Models
{
    public class Ad
    {
        
        public Record Record { get; set; }
        public Artist Artist { get; set; }
        public List<Song> Songs { get; set; }
        public Label Label { get; set; }
        public User User { get; set; }
        public Condition Condition { get; set; }
        public Genre Genre { get; set; }
        public List<SelectListItem> slcond { get; set; }
        public List<SelectListItem> slgen { get; set; }
        public List<SelectListItem> slart { get; set; }
        public List<SelectListItem> sllab { get; set; }

        public static string Serialize(Ad ad)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(ad);
        }

        
        public static Ad Deserialize(string data)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<Ad>(data);
        }


    }
}