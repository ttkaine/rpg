using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warhammer.Mvc.Models
{
	public class SessionTranscriptViewModel
	{
		public int Id { get; set; }

		public string ShortName { get; set; }
		public string FullName { get; set; }

		public string Transcript { get; set; }
	}
}