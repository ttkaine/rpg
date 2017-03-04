using System;
using System.Collections.Generic;
using Warhammer.Core.Helpers;

namespace Warhammer.Core.Entities
{
    public partial class PriceListItem
    {

        public string Breadcrumb
        {
            get
            {
                if (Parent == null)
                {
                    return Name;
                }
                else
                {
                    return Parent.Breadcrumb + " > " + Name;
                }
            }
        }

        public List<PriceListItem> AllChildren()
        {
            List<PriceListItem> childer = new List<PriceListItem>();
            childer.AddRange(Children);

            foreach (PriceListItem child in Children)
            {
                childer.AddRange(child.AllChildren());
            }

            return childer;
        }

        public int Crowns
        {
            get
            {
                if (PriceInPence.HasValue)
                {
                    return CurrencyHelper.Crowns(PriceInPence.Value);
                }
                return 0;
            }
        }

        public int Shillings
        {
            get
            {
                if (PriceInPence.HasValue)
                {
                    return CurrencyHelper.Shillings(PriceInPence.Value);
                   
                }
                return 0;
            }
        }

        public int Pence
        {
            get
            {
                if (PriceInPence.HasValue)
                {
                    return CurrencyHelper.Pence(PriceInPence.Value);

                }
                return 0;
            }
        }

        public string DisplayPrice
        {
            get
            {
                if (PriceInPence.HasValue)
                {
                    return CurrencyHelper.DisplayString(PriceInPence.Value);
                }
                else
                {
                    return "";
                }
                    
            }
        }

        // hacky way to make a drop down.... because I'm lazy right now and can't be bothered to make a view model 
        public List<PriceListItem> AllItems { get; set; }
    }
}