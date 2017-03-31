using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class MyFavNpcModel
    {
        [UIHint("FavNpc")]
        public PageLinkModel First { get; set; }
        [UIHint("FavNpc")]
        public PageLinkModel Second { get; set; }
        [UIHint("FavNpc")]
        public PageLinkModel Third { get; set; }

        [Required]
        public int FirstId { get; set; }
        [Required]
        public int SecondId { get; set; }
        [Required]
        public int ThirdId { get; set; }

        public SelectList ChooseFirstNpcList { get; set; }
        public SelectList ChooseSecondNpcList { get; set; }
        public SelectList ChooseThirdNpcList { get; set; }
    }
}