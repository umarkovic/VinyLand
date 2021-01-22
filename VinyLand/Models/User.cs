using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
	public class User: Neo4j.AspNet.Identity.ApplicationUser
	{
		
		public string PersonName { get; set; }
		public string PersonSurName { get; set; }
		public string Nickname { get; set; }
		public string Picture { get; set; }
		public int SoldRecords { get; set; }



	}
}