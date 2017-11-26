using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warhammer.Core.RoleplayViewModels
{
    public class PostedImageViewModel
    {
        public int ID { get; set; }
        public byte[] Image { get; set; }
        public string MimeType { get; set; }

        public PostedImageViewModel()
        {
            ID = -1;
        }
    }
}
