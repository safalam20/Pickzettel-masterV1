using Pickzettel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Data
{
    interface IPickzettelRepository
    {
        
        Task<List<Pickzettel_Form>> GetAllPickNumbers();
        Task<List<Pickzettel_HData>> GetHeaderDataBySelection(int col, int row);
        Task<List<Pickzettel_HDataDetail>> GetHeaderDataDetailBySelection(int BelID);
        Task<List<Pickzettel_HData>> GetSumHeaderData(int col);
        Task<List<Pickzettel_IData>> GetSumIData(int col);
        Task<List<Pickzettel_IData>> GetIDataBySelection(int col, int row);
        Task<int> ChangePrintStatus(int BelID, string ColumnName, string username);
        Task<List<CheckFunction>> GetCheckInfoByBelID(int BelID);
        Task<List<UserPrinter>> GetUserPrintersByUsername(string userName);
    }
}
