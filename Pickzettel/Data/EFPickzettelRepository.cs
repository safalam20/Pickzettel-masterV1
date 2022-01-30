using Pickzettel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace Pickzettel.Data
{
    public class EFPickzettelRepository : IPickzettelRepository
    {

        private PickzettelContext context;
        public EFPickzettelRepository(PickzettelContext ctx)
        {
            context = ctx;
        }
     
        public async Task<List<Pickzettel_Form>> GetAllPickNumbers()
        {
            var pickZettelForms = await context.PickZettelForms
                .FromSqlRaw("select * from fnKT_lgr_Pickzettel_Form()").ToListAsync();

            return pickZettelForms;
        }

        public async Task<List<UserPrinter>> GetUserPrintersByUsername(string userName)
        {
            var UserPrinters = await context.UserPrinters
                //.Where(u=>u.isAktiv==1 && u.Type1== "Pickzettel" && u.Type2 == "Benutzer" && u.Enumerator.Equals(userName))
                .Where(u=>u.Enumerator.Equals(userName) && u.Type1.Equals("Pickzettel") && u.Type2.Equals("Benutzer"))
                .ToListAsync();

            return UserPrinters;
        }
        public async Task<List<Pickzettel_HData>> GetHeaderDataBySelection(int col, int row)
        {
            var colNo = new SqlParameter("@col", col);
            var rowNo = new SqlParameter("@row", row);       

            var headerData = await context.PickzettelHDatas
                .FromSqlRaw("Select * from fnKT_lgr_Pickzettel_HData (@col, @row, null)",colNo,rowNo)
                .ToListAsync();

            return headerData;
        }
        public async Task<List<Pickzettel_HDataDetail>> GetHeaderDataDetailBySelection(int BelID)
        {
            var sBelID = new SqlParameter("@BelID", BelID);

            var headerDataDetail = await context.PickzettelHDataDetails
                .FromSqlRaw("Select * from [dbo].[fnKT_lgr_Pickzettel_HDataDetail] (@BelID)", sBelID)
                .ToListAsync();

            return headerDataDetail;
        }
        public async Task<List<Pickzettel_HData>> GetSumHeaderData(int col)
        {
            var colNo = new SqlParameter("@col", col);

            var headerData = await context.PickzettelHDatas
                .FromSqlRaw("Select * from fnKT_lgr_Pickzettel_HData (@col, null, null)", colNo)
                .ToListAsync();

            return headerData;
        }
        public async Task<List<Pickzettel_IData>> GetSumIData(int col)
        {
            var colNo = new SqlParameter("@col", col);

            var IData = await context.PickzettelIDatas
                .FromSqlRaw("Select * from fnKT_lgr_Pickzettel_IData (@col, null) order by Mandant, BelID, Position", colNo)
                .ToListAsync();

            return IData;
        }

        public async Task<List<Pickzettel_IData>> GetIDataBySelection(int col, int row)
        {
            var colNo = new SqlParameter("@col", col);
            var rowNo = new SqlParameter("@row", row);

            var IData = await context.PickzettelIDatas
                .FromSqlRaw("Select * from fnKT_lgr_Pickzettel_IData(@col, @row) order by Mandant, BelID, Position", colNo, rowNo)
                .ToListAsync();

            return IData;
        }
        public async Task<int> ChangePrintStatus(int BelID,string ColumnName, string username)
        {
            var Belid = new SqlParameter("@BelID",BelID );
            var columnName = new SqlParameter("@cName", ColumnName);
            var userName = new SqlParameter("@userName", username);

            var count = await context.Database
                .ExecuteSqlRawAsync("EXEC [dbo].[spKT_lgr_Pickzettel_Drucken] @BelID, @userName,@cName", Belid, userName, columnName);

            return count;
        }
        public async Task<List<CheckFunction>> GetCheckInfoByBelID(int BelID)
        {
            var belid = new SqlParameter("@belid", BelID);
            
            var CheckInfo = await context.GetCheckFunction
                .FromSqlRaw("Select * From [dbo].[fnKT_lgr_Pickzettel_Check] (@belid)", belid)
                .ToListAsync();

            return CheckInfo;
        }


    }
}
