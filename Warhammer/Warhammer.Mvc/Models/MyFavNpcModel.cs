using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class MyFavNpcModel
    {
        [UIHint("FavNpc")]
        public Person First { get; set; }
        [UIHint("FavNpc")]
        public Person Second { get; set; }
        [UIHint("FavNpc")]
        public Person Third { get; set; }

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