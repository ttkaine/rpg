using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warhammer.Core.Abstract;

namespace Warhammer.Core.Concrete
{
	public class PostManager : IPostManager
	{
		public PostResult CreateTextPostForUser(int sessionId, int characterId, bool isOoc, string text, string recipientString)
		{
			throw new NotImplementedException();
		}

		public PostResult CreateDiceRollPostForUser(int sessionId, int characterId, int dieSize, int dieCount, int rollType, int rollTarget, bool reRollMaximums, bool isPrivate)
		{
			throw new NotImplementedException();
		}

		public bool DeletePostForUser(int postId)
		{
			throw new NotImplementedException();
		}

		public PostResult EditTextPostForUser(int postId, string text)
		{
			throw new NotImplementedException();
		}

		public bool RevertPostForUser(int postId)
		{
			throw new NotImplementedException();
		}
	}
}
