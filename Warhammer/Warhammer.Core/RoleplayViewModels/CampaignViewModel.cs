using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Warhammer.Core.RoleplayViewModels
{
	public class CampaignViewModel
	{
		public int ID { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsClosed { get; set; }
		public int GmId { get; set; }

		public CampaignViewModel()
		{
			ID = -1;
			Title = string.Empty;
			Description = string.Empty;
			GmId = -1;
		}
	}
}
