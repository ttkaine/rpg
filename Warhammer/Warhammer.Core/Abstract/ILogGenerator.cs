namespace Warhammer.Core.Abstract
{
	public interface ILogGenerator
	{
		string GenerateTextLog(int sessionId, bool includeOoc);
		string GenerateHtmlLog(int sessionId, bool includeOoc);
	}
}
