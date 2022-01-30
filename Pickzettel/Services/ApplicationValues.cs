using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Services
{
    public static class ApplicationValues
    {

        //public const string pathToSaveFileTemp = @"\\dev1\sageweb\temp_pdf\";  //C:\Users\Safa.Uslu\Documents\Temp\

        public static string GetRowName(int itemRow)
        {
                 if (itemRow == 1) { return "Eilig"; }
            else if (itemRow == 2) { return "Felgen"; }
            else if (itemRow == 3) { return "Reifen"; }
            else if (itemRow == 4) { return "Stahl"; }
            else if (itemRow == 5) { return "Mix"; }
            else if (itemRow == 6) { return "Komplettrad"; }
            else if (itemRow == 7) { return "Diverse"; }
            else { return "Sum"; }
        }

        public static string GetColumnName(int itemCol)
        {
                 if (itemCol == 9)  { return "Auftrag zu erfassen"; }
            else if (itemCol == 10) { return "Vorkasse"; }
            else if (itemCol == 11) { return "Versand"; }

            else if (itemCol == 12) { return "B2B"; }
            else if (itemCol == 13) { return "B2C"; }
            else if (itemCol == 14) { return "Kond.SA"; }
            else if (itemCol == 15) { return "Zufuhr/Pallete"; }

            else if (itemCol == 21) { return "Pickstatus"; }
            else if (itemCol == 22) { return "Probleme"; }

            else if (itemCol == 23) { return "Verpackt"; }
            else{ return "Aufgeladen"; }
        }

    }
}
