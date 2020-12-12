namespace Warhammer.Core.Abstract
{
	public interface ILogGenerator
	{
		string GenerateTextLog(int sessionId, bool includeOoc, IPostFormatter postFormatter);
		string GenerateHtmlLog(int sessionId, bool includeOoc, IPostFormatter postFormatter);
	}
}
