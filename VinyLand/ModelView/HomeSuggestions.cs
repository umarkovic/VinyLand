using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VinyLand.Models;

namespace VinyLand.ModelView
{
    public class HomeSuggestions
    {
        public List<Ad> Ads { get; set; }
        public List<Ad> AdsFollowing { get; set; }
    }
}