﻿namespace Warhammer.Core.RoleplayViewModels
{
	public class TextPostViewModel : PostViewModel
	{
		public string Content { get; set; }
		public bool IsOoc { get; set; }
		public bool IsRevised { get; set; }
		public bool CanRevert { get; set; }
		public string LastEdited { get; set; }
        public string ImageUrl { get; set; }

        public TextPostViewModel()
		{
			Content = string.Empty;
			LastEdited = null;
		}
	}
}
