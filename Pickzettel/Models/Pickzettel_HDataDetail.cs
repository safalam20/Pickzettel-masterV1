using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Models
{

    [Keyless]
    public class Pickzettel_HDataDetail
    {
        public int BelID { get; set; }
        public short Mandant { get; set; }     
        public string ZIELvReifenName { get; set; }
        public string ZIELvFelgenName{ get; set; }
        public DateTime? FreigabeDatum { get; set; }
        public string Ausland { get; set; }

    }
}
