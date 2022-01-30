using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Models
{
    [Keyless]
    public class Pickzettel_Form
    {
        public string S_Row { get; set; }
        public string S_RowName { get; set; }

        public int? S_09 { get; set; }
        public int? S_10 { get; set; }
        public int? S_11 { get; set; }
        public int? S_12 { get; set; }
        public int? S_13 { get; set; }
        public int? S_14 { get; set; }
        public int? S_15 { get; set; }
        public int? S_21 { get; set; }
        public int? S_22 { get; set; }
        public int? S_23 { get; set; }
        public int? S_24 { get; set; }

        public int? R_09 { get; set; }
        public int? R_10 { get; set; }
        public int? R_11 { get; set; }
        public int? R_12 { get; set; }
        public int? R_13 { get; set; }
        public int? R_14 { get; set; }
        public int? R_15 { get; set; }
        public int? R_21 { get; set; }
        public int? R_22 { get; set; }
        public int? R_23 { get; set; }
        public int? R_24 { get; set; }
    }
}
