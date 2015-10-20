using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warhammer.Core.Abstract
{
	public interface ILogGenerator
	{
		string GenerateTextLog(int sessionId, bool includeOoc);
		string GenerateHtmlLog(int sessionId, bool includeOoc);
	}
}
