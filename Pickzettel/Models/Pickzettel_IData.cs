using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Models
{
    [Keyless]
    public class Pickzettel_IData
    {
        //public short UserId { get; set; }
        public int BelPosID { get; set; }
        public int BelID { get; set; }
        public short Mandant { get; set; }
        public int S_Column { get; set; }
        public int? S_Row { get; set; }
        public int Position { get; set; }
        public int PosType { get; set; }
        public string Artikelnummer { get; set; }
        public string Bezeichnung1 { get; set; }
        public string Langtext { get; set; }
        public decimal? Menge { get; set; }
        public decimal? GGBestellt { get; set; }
        public decimal? GGGeliefert { get; set; }
        public decimal? GGBerechnet { get; set; }
        public string LagerHalle { get; set; }
        public decimal? Bestand { get; set; }
        public decimal? Lagerbestand { get; set; }
        public int? USER_InternID { get; set; }
    }
}
