using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Models
{

    [Keyless]
    public class Pickzettel_HData
    {
        public int BelID { get; set; }
        public short Mandant { get; set; }
        public string S_Column { get; set; }
        public string S_Row { get; set; }
        public int? VorID { get; set; }
        public string Belegkennzeichen { get; set; }
        public short? BelegJahr { get; set; }
        public int? Belegnummer { get; set; }
        public DateTime? Belegdatum { get; set; }
        public string Matchcode { get; set; }
        public string Referenznummer { get; set; }
        public string A0Empfaenger { get; set; }
        public string A0Matchcode { get; set; }
        public string A0Name { get; set; }
        public string A0Ort { get; set; }
        public string A1Name { get; set; }
        public string A1Ort { get; set; }
        public string A1PLZ { get; set; }
        public string Versand { get; set; }
        public short? USER_PickGedruckt { get; set; }
        public string USER_HTKPaketnummern { get; set; }
        public string USER_MXWMSDruckstation { get; set; }
        public string USER_MXWMSPakettyp { get; set; }
        public int? USER_ELEtiketten { get; set; }
        public int? USER_MXWMSTOR { get; set; }        
        public decimal? S_Reifen { get; set; }
        public decimal? S_Felgen { get; set; }
        public decimal? S_Stahl { get; set; }
        public string Bearbeiter { get; set; }
        public int? AuftragNummer { get; set; }
        public decimal? Gewicht { get; set; }
        public string A1Strasse { get; set; }

        [NotMapped]
        public bool isChecked { get; set; } = false;

        
    }
}
