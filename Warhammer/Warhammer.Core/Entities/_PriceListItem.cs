using System;
using System.Collections.Generic;

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
                    return (int) Math.Floor(PriceInPence.Value/240);
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
                    return ((int)Math.Floor(PriceInPence.Value / 12)) - (Crowns * 20);
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
                    return ((int)Math.Floor(PriceInPence.Value)) - (Crowns * 240) - (Shillings * 12);
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
                    string display = "";
                    if (Crowns > 0)
                    {
                        display = $"{Crowns}GC";
                    }
                    if (Shillings > 0)
                    {
                        display = $"{display} {Shillings}/";
                        if (Pence > 0)
                        {
                            display = $"{display}{Pence}";
                        }
                        else
                        {
                            display = display + "-";
                        }
                    }
                    else
                    {
                        if (Crowns > 0 && Pence > 0)
                        {
                            display = $"{display} -/{Pence}";
                        }
                        if (Pence > 0)
                        {
                            if (PriceInPence - Pence > 0)
                            {
                                if ((PriceInPence - Pence) == 0.25m)
                                {
                                    display = $"{Pence}&#188;d";
                                }
                                else if ((PriceInPence - Pence) == 0.5m)
                                {
                                    display = $"{Pence}&#189;d";
                                }
                                else if ((PriceInPence - Pence) == 0.75m)
                                {
                                    display = $"{Pence}&#190;d";
                                }
                                else
                                {
                                    display = $"{(int)Math.Round(PriceInPence.Value, 0)}d";
                                }
                            }
                            else
                            {  
                                display = $"{(int)Math.Round(PriceInPence.Value, 0)}d";
                            }

                        }
                    }


                    return display;
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