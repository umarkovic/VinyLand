using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
	public class Record
	{
		public Artist Artist { get; set; }
		
		public int Id { get; set; }
		[Required (ErrorMessage ="This field is required")]
		public string Title { get; set; }
		public Label Label { get; set; }
		[Required(ErrorMessage = "This field is required")]
		public string Format { get; set; }
		[Required(ErrorMessage = "This field is required")]
		public float Price { get; set; }
		public Genre Genre { get; set; }
		public List<Song> Songs { get; set; }
		public Condition Condition { get; set; }
		public string Picture { get; set; }
	}
}