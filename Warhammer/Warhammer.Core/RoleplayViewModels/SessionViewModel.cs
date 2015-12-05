using System;

namespace Warhammer.Core.RoleplayViewModels
{
	public class SessionViewModel
	{
		public int ID { get; set; }
		public int CampaignId { get; set; }
		public int GmId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsClosed { get; set; }
        public int CurrentPlayerId { get; set; }
		public DateTime StartDate { get; set; }

		public SessionViewModel()
		{
			ID = -1;
			CampaignId = -1;
			Title = string.Empty;
			Description = string.Empty;
		}
	}
}
