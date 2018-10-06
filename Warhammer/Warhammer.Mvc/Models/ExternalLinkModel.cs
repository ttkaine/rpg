using System;

namespace Warhammer.Mvc.Models
{
    public class ExternalLinkModel
    {
        public enum LinkType
        {
            Text,
            InfoButton,
            WarningButton,
            ErrorButton,
            SuccessButton,
            DefaultButton
        }

        public LinkType TypeOfLink { get; set; }
        public string Url { get; set; }
        public string Text { get; set; }
        public string AdditionalCssClasses { get; set; }

        public string CssClass
        {
            get
            {
                string classes = AdditionalCssClasses;
                switch (TypeOfLink)
                {
                    case LinkType.Text:
                        break;
                    case LinkType.InfoButton:
                        classes = " btn btn-block btn-info";
                        break;
                    case LinkType.WarningButton:
                        classes = " btn btn-block btn-warning";
                        break;
                    case LinkType.ErrorButton:
                        classes = " btn btn-block btn-error";
                        break;
                    case LinkType.SuccessButton:
                        classes = " btn btn-block btn-success";
                        break;
                    case LinkType.DefaultButton:
                        classes = " btn btn-block btn-default";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return classes;
            }
        }
    }
}