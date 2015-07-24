namespace Warhammer.Mvc.Abstract
{
    public interface ILinkGenerator
    {
        string ResolveCreoleLinks(string htmlContent);
        string CreoleLinksToHtml(string htmlContent);
    }
}
