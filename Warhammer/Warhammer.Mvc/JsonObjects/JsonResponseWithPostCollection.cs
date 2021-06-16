using System;
using System.Collections.Generic;

namespace Warhammer.Mvc.JsonObjects
{
	[Serializable]
	public class JsonResponseWithPostCollection
	{
        public bool IsError { get; set; }
		public string ErrorMessage { get; set; }

		public int Count { get; set; }
		public int EditedCount { get; set; }
		public int DeletedCount { get; set; }
		public int LatestPostId { get; set; }
		public string LastUpdate { get; set; }
		public List<JsonPost> Posts { get; set; }
		public List<JsonPost> EditedPosts { get; set; }
		public List<int> DeletedPosts { get; set; }

		public string PlayerTurnMessage { get; set; }
		public bool IsCurrentPlayerTurn { get; set; }

        public string NotifyImage => "/home/icon/310";

        public JsonResponseWithPostCollection(int campaignId)
		{
			Posts = new List<JsonPost>();
			EditedPosts = new List<JsonPost>();
			DeletedPosts = new List<int>();
			ErrorMessage = string.Empty;
			LastUpdate = DateTime.UtcNow.ToString("dd MMM yyyy HH:mm:ss");
		}
	}
}