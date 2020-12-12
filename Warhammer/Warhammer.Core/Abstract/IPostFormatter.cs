namespace Warhammer.Core.Abstract
{
    public interface IPostFormatter
    {
        string ApplyPostFormatting(string postContent, bool preserveParagraphs);
        string RemoveHtmlAndMarkdown(string postContent);
    }
}