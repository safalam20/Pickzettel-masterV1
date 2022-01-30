using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Models
{
    [Table("KTEnumerator")]
    public class UserPrinter
    {
        [Column("ID")]
        public int ID { get; set; }

        [Column("Aktiv")]
        public int? isAktiv { get; set; }

        [Column("Type1")]
        public string Type1 { get; set; }

        [Column("Type2")]
        public string Type2 { get; set; }

        [Column("Enumerator")]
        public string Enumerator { get; set; }

        [Column("Name1")]
        public string Name1 { get; set; }

        [Column("Name2")]
        public string Name2 { get; set; }

        [Column("Name3")]
        public string Name3 { get; set; }

        [Column("Pro")]
        public int isProduction { get; set; }
    }
}
