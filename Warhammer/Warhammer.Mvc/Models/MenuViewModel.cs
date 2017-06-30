using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Mvc.Models
{
    public class MenuViewModel
    {
        public MenuViewModel()
        {
            LeftMenu = new List<MenuItemViewModel>();
            RightMenu = new List<MenuItemViewModel>();
        }

        public List<MenuItemViewModel> LeftMenu { get; set; }
        public List<MenuItemViewModel> RightMenu { get; set; }
        public List<MenuItemViewModel> CreateMenu { get; set; }
    }


    public class MenuItemViewModel
    {
       
        public MenuItemViewModel()
        {
            SubMenu = new List<MenuItemViewModel>();
        }

        public bool IsDropDown
        {
            get { return SubMenu.Any(); }
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public string IconUrl { get; set; }
        public List<MenuItemViewModel> SubMenu { get; set; }
        public string AltText { get; set; }
        public string IconCssClass { get; set; }
        public string CssClass { get; set; }
    }
}