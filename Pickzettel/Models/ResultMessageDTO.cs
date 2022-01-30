using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Models
{
    public class ResultMessageDTO
    {
        public List<string> FailedSelections { get; set; }
        public List<string> SuccessSelections { get; set; }
        public string FileName { get; set; }
    }
}
