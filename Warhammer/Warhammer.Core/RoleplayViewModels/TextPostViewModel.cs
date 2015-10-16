using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Warhammer.Core.RoleplayViewModels
{
	public class TextPostViewModel : PostViewModel
	{
		public string Content { get; set; }
		public bool IsOoc { get; set; }
		public bool IsRevised { get; set; }
		public string LastEdited { get; set; }

		public TextPostViewModel()
		{
			Content = string.Empty;
			LastEdited = null;
		}
	}
}
