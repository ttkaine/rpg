using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Abstract;

namespace Warhammer.Mvc.Concrete
{
    public class LinkGenerator : ILinkGenerator
    {
        private readonly IAuthenticatedDataProvider _data;
        readonly Regex _regex = new Regex(@"\[\[.*?\|??.*?\]\]");
        private readonly UrlHelper _urlHelper;
        public LinkGenerator(IAuthenticatedDataProvider data)
        {
            _data = data;
            _urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
        }

        public string ResolveCreoleLinks(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return htmlContent;
            }

            foreach (Match match in _regex.Matches(htmlContent))
            {
                string pageName;
                string linkName;
                GetLinkDetails(match, out pageName, out linkName);
                Page page = _data.GetPage(pageName);
                if (page != null)
                {
                    htmlContent = htmlContent.Replace(match.Value, string.Format("[[{0}|{1}]]", page.Id, linkName));
                }
            }

            return htmlContent;
        }

        private static string GetLinkDetails(Match match, out string pageName, out string linkName)
        {
            string link = match.Value;
            pageName = link.Substring(2, (link.IndexOfAny(new[] {'|', ']'}) - 2));
            
            if (link.Contains("|"))
            {
                linkName = link.Substring(link.IndexOf('|') + 1, ((link.IndexOf(']') - 1) - link.IndexOf('|')));
            }
            else
            {
                linkName = pageName;
            }
            return link;
        }


        public string CreoleLinksToHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return htmlContent;
            }
            foreach (Match match in _regex.Matches(htmlContent))
            {
                string pageName;
                string linkName;
                string link = GetLinkDetails(match, out pageName, out linkName);
                int pageId;
                Page page;
                if (int.TryParse(pageName, out pageId))
                {
                    page = _data.GetPage(pageId);
                }
                else
                {
                    page = _data.GetPage(pageName); 
                }
                string anchorHtml;
                if (page != null)
                {
                    string anchorHref = _urlHelper.Action("Index", "page", new {id = page.Id});
                    anchorHtml = string.Format("<a href=\"{0}\">{1}</a>", anchorHref, linkName);
                }
                else
                {
                    string anchorHref = _urlHelper.Action("Index", "Create");
                    anchorHtml = string.Format("<a class=\"missing-link\" href=\"{0}\">{1}</a>", anchorHref, linkName);     
                }
                htmlContent = htmlContent.Replace(link, anchorHtml);
            }
            return htmlContent;
        }



    }
}