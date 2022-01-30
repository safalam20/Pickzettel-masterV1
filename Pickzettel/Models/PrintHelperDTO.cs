using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Models
{
    public class PrintHelperDTO
    {
        public Pickzettel_HData HData{get;set;}
        public Pickzettel_HDataDetail HDataDetail { get; set; }
        public List<Pickzettel_IData> IDatas { get; set; }
        public CheckFunction CheckInfo { get; set; }
    }
}
