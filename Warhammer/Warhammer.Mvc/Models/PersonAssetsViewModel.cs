using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class PersonAssetsViewModel
    {
        public int PersonId { get; set; }
        public List<Asset> Assets { get; set; }
        public bool AssetsJustSet { get; set; }
        public string AddAssetTitle { get; set; }
        public string AddAssetDescription { get; set; }
        public int AddAssetUpkeep { get; set; }
    }
}