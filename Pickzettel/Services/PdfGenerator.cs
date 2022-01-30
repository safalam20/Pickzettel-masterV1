using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pickzettel.Models;
using Microsoft.Extensions.DependencyInjection;
using Pickzettel.Data;
using Microsoft.Win32;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Pickzettel.Services
{
    public class PdfGenerator
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<PdfGenerator> _logger;
        public PdfGenerator(IServiceScopeFactory scope, ILogger<PdfGenerator> logger)
        {
            scopeFactory = scope;
            _logger = logger;
        }

        string rowName;
        string columnName;
        int barcodeWidth = 6;

        public async Task<ResultMessageDTO> handlePrintRequest(List<PrintHelperDTO> selectedItemsToPrint, int row,int col,string userName,string printerName)
        {
            List<string> FailedSelections = new List<string>();
            List<string> SuccessSelections = new List<string>();
            ResultMessageDTO resultMessages = new ResultMessageDTO();

            rowName = ApplicationValues.GetRowName(row);
            columnName = ApplicationValues.GetColumnName(col);

            var html = "";


            using (var scope = scopeFactory.CreateScope())
            {
                
                var repo = scope.ServiceProvider.GetRequiredService<IPickzettelRepository>();
         
                // OFFENE BELEGE
                //**************************************
                if(col==12 || col==13 || col==14 || col == 15)
                {
                    foreach(PrintHelperDTO beleg in selectedItemsToPrint)
                    {
                        //This Belegs shouldnt be printed before. Check if there is any log.
                        //If there is a log, it means that it is already printed. skip that beleg and inform client.
                        CheckFunction CheckInfo = (await repo.GetCheckInfoByBelID(beleg.HData.BelID)).FirstOrDefault();
                        if(CheckInfo is not null)
                        {
                           FailedSelections.Add($"{beleg.HData.BelegJahr + "-" + beleg.HData.Belegnummer}# wurde bereits von #{CheckInfo.SUser} gedruckt");
                        }
                        else
                        {
                            //Try to change status of the Beleg
                            //*******************
                            await repo.ChangePrintStatus(beleg.HData.BelID, beleg.HData.S_Column, userName);

                            //Check if the status changed by the current user
                            //*************************
                            CheckInfo = (await repo.GetCheckInfoByBelID(beleg.HData.BelID)).FirstOrDefault();
                            //Status not changed still 0..
                            if (CheckInfo is null)
                            {
                               FailedSelections.Add($"{beleg.HData.BelegJahr + "-" + beleg.HData.Belegnummer}# nicht erfolgreich (datenbank)..");
                            }
                            else
                            {
                                if (CheckInfo.SUser.Equals(userName))
                                {
                                    //Load with Detail DATA
                                    beleg.HDataDetail = (await repo.GetHeaderDataDetailBySelection(beleg.HData.BelID)).FirstOrDefault();

                                    html = html + createHtmlForOneItem(beleg,true,col);
                                    //SuccessSelections.Add(beleg.HData.BelID);
                                    SuccessSelections.Add(beleg.HData.BelegJahr + "-" + beleg.HData.Belegnummer);
                                }
                                else
                                {
                                    FailedSelections.Add($"{beleg.HData.BelegJahr + "-" + beleg.HData.Belegnummer}# wurde bereits von #{CheckInfo.SUser} gedruckt");
                                }
                            }

                        }
                    }
                }


                //These Belegs are already printed. Printing again.
                //***********************************************
                else
                {
                    foreach(PrintHelperDTO beleg in selectedItemsToPrint)
                    {
                        // Get the user who printed before
                        CheckFunction CheckInfo = (await repo.GetCheckInfoByBelID(beleg.HData.BelID)).FirstOrDefault();
                        if(CheckInfo is not null)
                        {
                            beleg.CheckInfo = CheckInfo;
                        }

                        //Add current print to log
                        //*******************
                        await repo.ChangePrintStatus(beleg.HData.BelID, beleg.HData.S_Column, userName);

                        //Load with Detail DATA
                        beleg.HDataDetail = (await repo.GetHeaderDataDetailBySelection(beleg.HData.BelID)).FirstOrDefault();

                        html = html + createHtmlForOneItem(beleg,true,col);
                        SuccessSelections.Add(beleg.HData.BelegJahr + "-" + beleg.HData.Belegnummer);
                    }                    
                }             
            }

            if (html != "")
            {
                var pdf = Pdf
                    .From(html.ToString())
                    .WithObjectSetting("web.userStyleSheet", @"wwwroot\css\pdf.css")
                    .WithObjectSetting("footer.left", $"{col}-{columnName} / {row}-{rowName}")
                    .WithObjectSetting("footer.center", userName)
                    .WithObjectSetting("footer.right", $"{DateTime.Now}")
                    .WithObjectSetting("footer.fontSize", "9")
                    .WithGlobalSetting("margin.top", "0.2cm")
                    .WithObjectSetting("web.defaultEncoding", "utf-8")
                    .Content();

                var RandomFileName = $"{Guid.NewGuid().ToString()}.pdf";
                var path = @"wwwroot\" + RandomFileName;
                resultMessages.FileName = RandomFileName;

                try
                {
                    File.WriteAllBytes(path, pdf);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error saving PDF file: " + ex.Message);

                }

                //IF USER AIMS PDF DONT SEND TO PRINTER
                if (!printerName.Equals("PDF"))
                {
                    try
                    {
                        //SumatraPrint(path, printerName);
                        TEST_FoxitPrint(path, printerName);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Error printing Sumatra: "+printerName+"--" + e.Message);
                    }
                  
                    //After print delete the file
                    File.Delete(path);
                }

            }

            resultMessages.FailedSelections = FailedSelections;
            resultMessages.SuccessSelections = SuccessSelections;
            return resultMessages;
        }
        public async Task<ResultMessageDTO> handleExportRequest(List<PrintHelperDTO> selectedItemsToPrint, int row, int col,string userName,string printerName)
        {
            
            List<string> FailedSelections = new List<string>();
            List<string> SuccessSelections = new List<string>();
            ResultMessageDTO resultMessage = new ResultMessageDTO();


            rowName = ApplicationValues.GetRowName(row);
            columnName = ApplicationValues.GetColumnName(col);
            //categoryInfo(row, col);
            var html = "";

            using (var scope = scopeFactory.CreateScope())
            {

                var repo = scope.ServiceProvider.GetRequiredService<IPickzettelRepository>();

                foreach (PrintHelperDTO dTO in selectedItemsToPrint)
                {
                    //Load with Detail DATA
                    dTO.HDataDetail = (await repo.GetHeaderDataDetailBySelection(dTO.HData.BelID)).FirstOrDefault();

                    html = html + createHtmlForOneItem(dTO, false, col);
         
                    SuccessSelections.Add(dTO.HData.BelegJahr + "-" + dTO.HData.Belegnummer);
                }
            }


            if (html != "")
            {
                var pdf = Pdf
                    .From(html.ToString())
                    .WithObjectSetting("web.userStyleSheet", @"wwwroot\css\pdf.css")
                    .WithObjectSetting("footer.left", $"{col}-{columnName} / {row}-{rowName}")
                    .WithObjectSetting("footer.center", userName)
                    .WithObjectSetting("footer.right", $"{DateTime.Now}")
                    .WithObjectSetting("footer.fontSize", "9")
                    .WithGlobalSetting("margin.top", "0.2cm")
                    .WithObjectSetting("web.defaultEncoding", "utf-8")
                    .Content();

                var RandomFileName = $"{Guid.NewGuid().ToString()}.pdf";
                var path = @"wwwroot\" + RandomFileName;
                resultMessage.FileName = RandomFileName;

                try
                {
                    File.WriteAllBytes(path, pdf);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);

                }
              
                
                //IF USER AIMS PDF DONT SEND TO PRINTER
                if (!printerName.Equals("PDF"))
                {
                    // Create an instance of the Printer
                    //IPrinter printer = new Printer();

                    // Print the file
                    //printer.PrintRawFile(printerName, path);

                    //SumatraPrint(path, printerName);
                    TEST_FoxitPrint(path, printerName);

                    //After print delete the file
                    File.Delete(path);
                }

               
            }
            resultMessage.FailedSelections = FailedSelections;
            resultMessage.SuccessSelections = SuccessSelections;
            return resultMessage;

        }

        public async Task<ResultMessageDTO> handleTestPrintRequest(List<PrintHelperDTO> selectedItemsToPrint, int row, int col, string userName, string printerName)
        {

            List<string> FailedSelections = new List<string>();
            List<string> SuccessSelections = new List<string>();
            ResultMessageDTO resultMessage = new ResultMessageDTO();

            rowName = ApplicationValues.GetRowName(row);
            columnName = ApplicationValues.GetColumnName(col);
            //categoryInfo(row, col);
            var html = "";

            using (var scope = scopeFactory.CreateScope())
            {

                var repo = scope.ServiceProvider.GetRequiredService<IPickzettelRepository>();

                foreach (PrintHelperDTO dTO in selectedItemsToPrint)
                {

                    //Load with Detail DATA
                    dTO.HDataDetail = (await repo.GetHeaderDataDetailBySelection(dTO.HData.BelID)).FirstOrDefault();


                    html = html + createHtmlForOneItem(dTO, true, col);
                    SuccessSelections.Add(dTO.HData.BelegJahr + "-" + dTO.HData.Belegnummer);
                }

            }


            if (html != "")
            {
                var pdf = Pdf
                    .From(html.ToString())
                    .WithObjectSetting("web.userStyleSheet", @"wwwroot\css\pdf.css")
                    .WithObjectSetting("footer.left", $"{col}-{columnName} / {row}-{rowName}")
                    .WithObjectSetting("footer.center", userName)
                    .WithObjectSetting("footer.right", $"{DateTime.Now}")
                    .WithObjectSetting("footer.fontSize", "9")
                    .WithGlobalSetting("margin.top", "0.2cm")
                    .WithObjectSetting("web.defaultEncoding", "utf-8")
                    .Content();

                var RandomFileName = $"{Guid.NewGuid().ToString()}.pdf";
                var path = @"wwwroot\" + RandomFileName;
                resultMessage.FileName = RandomFileName;

                try
                {
                    File.WriteAllBytes(path, pdf);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error saving PDF file: " + ex.Message);

                }


                //IF USER AIMS PDF DONT SEND TO PRINTER
                if (!printerName.Equals("PDF"))
                {
                    // Create an instance of the Printer
                    //IPrinter printer = new Printer();

                    // Print the file
                    //printer.PrintRawFile(printerName, path);

                    //SumatraPrint(path, printerName);
                    TEST_FoxitPrint(path, printerName);

                    //After print delete the file
                    File.Delete(path);
                }


            }
            resultMessage.FailedSelections = FailedSelections;
            resultMessage.SuccessSelections = SuccessSelections;
            return resultMessage;

        }


        public string provideBase64(string data)
        {          
            BarcodeLib.Barcode b = new BarcodeLib.Barcode();
            Image img = b.Encode(BarcodeLib.TYPE.CODE128, $"{data}", Color.Black, Color.White, 500, 150);
            //img.Save("myfile.png", ImageFormat.Png);

            byte[] imageBytes = imageToByteArray(img);

            // Convert byte[] to Base64 String
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        public string createHtmlForOneItem(PrintHelperDTO selectedItem,bool isPrint, int col)
        {
            int totalPage;
            if (selectedItem.IDatas is not null)
            {

                if (selectedItem.IDatas.Count < 8)
                {
                    totalPage = 1;
                }
                else
                {

                    if ((selectedItem.IDatas.Count - 7) % 10 == 0)
                    {
                        totalPage = ((selectedItem.IDatas.Count - 7) / 10) + 1;
                    }
                    else
                    {
                        totalPage = ((selectedItem.IDatas.Count - 7) / 10) + 2;
                    }
                }
            }
            else
            {
                totalPage = 1;
            }

            var html = new StringBuilder();
            html.Append("<!DOCTYPE html>");
            html.Append("<html>");
            html.Append("<head>");
            html.Append("</head>");
            html.Append("<body>");
            html.Append("<div class=\"container\" style=\"page-break-before:always\" >");
            //Page number
            html.Append($"<div class=\"font-italic mb-3\" style=\"padding:0px margin:0px; text-align:center;\">" +
                $"Seite 1 von {totalPage} </div>");
            if(selectedItem.CheckInfo is not null && selectedItem.CheckInfo.SUser is not null)
            {
                html.Append($"<p class=\"font-italic mb-1\" style=\"font-size:70%;\">Zuletz gedruckt, {selectedItem.CheckInfo.info}</p>");
            }
            
            html.Append("<table class=\"table\">");

            //1. Line
            html.Append("<tr>");
            html.Append("<td rowspan=\"3\" colspan=\"2\" class=\"border border-secondary align-middle\">");
            if (isPrint)
            {
                html.Append("<img src=\"data:image/png;base64,");
                html.Append(provideBase64(selectedItem.HData.BelID.ToString()));
                html.Append("\" style=\"width: 200px; height: 40px; \">");
            }                  
            html.Append("</td>");
            html.Append($"<td colspan=\"1\" class=\"align-middle text-right font-weight-bold\">Bearbeiter: </td>");
            html.Append($"<td colspan=\"1\" class=\"align-middle text-center\">{selectedItem.HData.Bearbeiter}</td>");
            html.Append( "<td colspan=\"2\" class=\"border border-secondary align-middle text-center font-weight-bold\">VERKAUF</td>");
            html.Append("</tr>");

            html.Append("<tr>");
          //  html.Append( "<td colspan=\"1\" class=\"align-middle text-right font-weight-bold\">Ziel Reifen: </td>");
            html.Append($"<td rowspan=\"2\" colspan=\"2\" class=\"font-weight-bold border border-secondary align-middle text-center bigfont\">{selectedItem.HDataDetail.Ausland}</td>");
            html.Append($"<td colspan=\"2\" class=\"border border-secondary align-middle text-center font-weight-bold\">{selectedItem.HData.Mandant} - {selectedItem.HData.BelID}</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            //html.Append($"<td colspan=\"1\" class=\"align-middle text-right font-weight-bold\">Ziel Felgen: </td>");
            //html.Append($"<td colspan=\"1\" class=\"align-middle text-center\">{selectedItem.HDataDetail.ZIELvFelgenName}</td>");
            html.Append($"<td colspan=\"2\" class=\"border border-secondary align-middle text-center font-weight-bold\">{selectedItem.HData.BelegJahr} - {selectedItem.HData.Belegnummer}</td>");
            html.Append("</tr>");
          

            //2. Line
            html.Append("<tr>");
            html.Append($"<td colspan=\"1\" class=\"border border-secondary font-weight-bold\">{selectedItem.HData.A0Empfaenger}</td>");
            html.Append($"<td colspan=\"3\" class=\"border border-secondary font-weight-bold\">{selectedItem.HData.A0Name}</td>");
            html.Append($"<td colspan=\"2\" class=\"text-center\"><span class=\"font-weight-bold\">Order Date: </span>{selectedItem.HData.Belegdatum?.ToString("dd/MM/yyyy")}</td>");
            html.Append("</tr>");

            //3. Line
            html.Append("<tr>");
            html.Append($"<td colspan=\"2\" class=\"border border-secondary font-weight-bold\">{selectedItem.HData.A1Name}</td>");
            html.Append("<td class=\"font-weight-bold text-right\">Freigabe:</td>");
            html.Append($"<td class=\"text-left\">{selectedItem.HDataDetail.FreigabeDatum?.ToString("dd/MM/yyyy HH:mm")}</td>");
            html.Append($"<td class=\"border border-secondary text-center\"><span class=\"font-weight-bold\">Gewicht: </span>{string.Format("{0:0.00}", selectedItem.HData.Gewicht)}</td>");
            html.Append($"<td class=\"border border-secondary text-center\"><span class=\"font-weight-bold\">Felgen: </span>{(int?)selectedItem.HData.S_Felgen}</td>");

            // html.Append("<td class=\"border border-secondary font-weight-bold text-right\" >Felgen:</td>");
            // html.Append($"<td class=\"border border-secondary\">{(int?)selectedItem.HData.S_Felgen}</td>");
            html.Append("</tr>");

            //4. Line
            html.Append("<tr>");
            html.Append($"<td colspan=\"2\" class=\"border border-secondary font-weight-bold\">{selectedItem.HData.A1Strasse}</td>");
            html.Append("<td class=\"font-weight-bold text-right\">Print Station:</td>");
            html.Append($"<td class=\"text-left\">{selectedItem.HData.USER_MXWMSDruckstation}</td>");

            html.Append($"<td class=\"border border-secondary text-center\"><span class=\"font-weight-bold\">TOR: </span>{selectedItem.HData.USER_MXWMSTOR} </td>");
            html.Append($"<td class=\"border border-secondary text-center\"><span class=\"font-weight-bold\">Reifen: </span>{(int?)selectedItem.HData.S_Reifen}</td>");

            //html.Append($"<td class=\"border border-secondary font-weight-bold text-right\">Reifen:</td>");
            //html.Append($"<td class=\"border border-secondary\">{(int?)selectedItem.HData.S_Reifen}</td>");
            html.Append("</tr>");

            //5. Line
            html.Append("<tr>");
            html.Append($"<td class=\"border border-secondary font-weight-bold\">{selectedItem.HData.A1PLZ}</td>");
            html.Append($"<td class=\"border border-secondary\">{selectedItem.HData.A1Ort}</td>");

            html.Append($"<td class=\"font-weight-bold text-right\">Pack Type:</td>");
            html.Append($"<td class=\"text-left\">{selectedItem.HData.USER_MXWMSPakettyp}</td>");

            html.Append($"<td class=\"font-weight-bold border border-secondary text-center bigfontXL\">{selectedItem.HData.Versand}-{selectedItem.HData.USER_ELEtiketten}</td>");
            html.Append($"<td class=\"border border-secondary text-center\"><span class=\"font-weight-bold\">Stahl: </span>{(int?)selectedItem.HData.S_Stahl}</td>");

            //html.Append($"<td class=\"border border-secondary font-weight-bold text-right\">Stahl:</td>");
            //html.Append($"<td class=\"border border-secondary\">{(int?)selectedItem.HData.S_Stahl}</td>");
            html.Append("</tr>");

            ////6. Line
            //html.Append("<tr>");
            //html.Append($"<td class=\"border border-secondary\"></td>");
            //html.Append($"<td class=\"border border-secondary\"></td>");

            //html.Append($"<td class=\"border border-secondary font-weight-bold text-right\">Tor:</td>");
            //html.Append($"<td class=\"border border-secondary\">{selectedItem.HData.USER_MXWMSTOR} </td>");

            //html.Append($"<td class=\"border border-secondary text-center bigfont\">{selectedItem.HData.Versand}-{selectedItem.HData.USER_ELEtiketten}</td>");
            //html.Append($"<td class=\"border border-secondary text-center\">Gewicht: </span>{string.Format("{0:0.00}", selectedItem.HData.Gewicht)}</td>");
            //html.Append("</tr>");


            //Artikels
            html.Append("</table>");
            html.Append("<br>");

            int artikelPosition = 1;
            bool isHeaderOn = true;
            int artikelCountCurrent = 0;
            bool isFirstPage = true;
            int artikelProPage = 10;
            int atPage = 2;

            foreach (Pickzettel_IData artikel in selectedItem.IDatas)
            {
                if (isHeaderOn)
                {
                    //Second Table
                    html.Append("<table class=\"table\">");

                    //HEADER
                    html.Append("<tr>");
                    html.Append("<th class=\"align-middle border border-secondary\">No</th>");
                    html.Append("<th class=\"text-center align-middle border border-secondary\">ITEM NUMBER</th>");
                    html.Append("<th class=\"text-center align-middle border border-secondary\">INTERN</th>");
                    //html.Append("<th class=\"text-center align-middle border border-secondary\">ITEM NAME</th>");
                    html.Append("<th class=\"text-center align-middle border border-secondary\">PART PIECE</th>");
                    html.Append("<th class=\"text-center align-middle border border-secondary\">STOCK ADRESS</th>");
                    html.Append("<th class=\"text-center align-middle border border-secondary\">STOCK AMOUNT</th>");
                    html.Append("</tr>");
                }
                isHeaderOn = false;

                if(artikel.PosType == 1)
                { 
                    if (col!=22 || artikel.GGBestellt>artikel.GGGeliefert)
                    {
                        html.Append("<tr class=\"firstrow\">");
                        html.Append($"<td class=\"align-middle posborder font-weight-bold mt-0 pt-0\">{artikelPosition++}</td>");
                        html.Append($"<td class=\"text-center arBorder bigfont mt-0 pt-0 bigfontXL\">{artikel.Artikelnummer}</br>");
                        if (isPrint)
                        {
                            html.Append("<img src=\"data:image/png;base64,");
                            html.Append(provideBase64(artikel.Artikelnummer));
                            if (artikel.Artikelnummer.Length > barcodeWidth)
                            {
                                html.Append("\" style=\"width: 400px; height: 40px; margin:0px; padding:0px; margin-top:0px;\">");
                            }
                            else
                            {
                                html.Append("\" style=\"width: 200px; height: 40px; margin:0px; padding:0px; margin-top:0px;\">");
                            }
                        }

                        html.Append("</td>");

                        html.Append($"<td class=\"text-center myBorder bigfont mt-0 pt-0 bigfontXL\">{artikel.USER_InternID}</br>");
                        if (isPrint)
                        {
                            html.Append("<img src=\"data:image/png;base64,");
                            html.Append(provideBase64(artikel.USER_InternID.ToString()));
                            html.Append("\" style=\"width: 200px; height: 40px; margin:0px; padding:0px; margin-top:0px;\">");
                        }
                        html.Append("</td>");


                        //html.Append($"<td class=\"border-right border-secondary mybezeichnung align-middle bigfont\">{artikel.Bezeichnung1}</td>");
                        if (col == 22)
                        {
                            html.Append($"<td class=\"myBorder text-center bigfontXL align-middle mt-0 pt-0\">{((int?)artikel.GGBestellt)-((int?)artikel.GGGeliefert)}</td>");
                        }
                        else
                        {
                            html.Append($"<td class=\"myBorder text-center bigfontXL align-middle mt-0 pt-0\">{(int?)artikel.Menge}</td>");
                        }
                    
                        html.Append($"<td class=\"myBorder text-center align-middle mt-0 pt-0\">{artikel.LagerHalle}</td>");
                        html.Append($"<td class=\"myBorder text-center align-middle m-0 p-0\">{(int?)artikel.Bestand}</br>");
                       // html.Append("<hr style=\"border-top: 3px dotted #bbb;\">");
                        html.Append((int?)artikel.Lagerbestand);
                        html.Append("</td>");
                        html.Append("</tr>");

                        html.Append($"<tr class=\"secondrow\"><td class=\"posborder m-0 p-0 pl-2 text-center\">{artikel.Position}</td><td bezBorder colspan=\"5\" class=\"m-0 p-0 pl-2 text-left\">{artikel.Bezeichnung1}</td></tr>");
                        artikelCountCurrent++;
                    }
                }
                else
                {
                    html.Append("<tr class=\"firstrow\">");
                    html.Append($"<tr class=\"secondrow\"><td class=\"posborder m-0 p-0 pl-2 text-center\">{artikel.Position}</td><td bezBorder colspan=\"5\" class=\"m-0 p-0 pl-2 text-left\">{artikel.Langtext}</td></tr>");
                }



                if (isFirstPage) { artikelProPage = 7; }
                if (artikelCountCurrent == selectedItem.IDatas.Count || artikelCountCurrent == artikelProPage)
                {
                    html.Append("</table>");
                    isHeaderOn = true;
                    artikelCountCurrent = 0;
                    if (artikelPosition - 1 != selectedItem.IDatas.Count)
                    {
                        html.Append($"<div  class=\"font-italic mb-3\" style=\"padding:0px margin:0px; text-align:center; page-break-before:always\">Seite {atPage++} von {totalPage} </div>");
                    }

                    artikelProPage = 10;
                    isFirstPage = false;
                }
                if (artikelPosition == selectedItem.IDatas.Count) { isFirstPage = true; atPage = 2; }

            }

            html.Append("</table>");
            html.Append("</div>");
            html.Append("</body>");
            html.Append("</html>");

            return html.ToString();
        }

       

        public byte[] imageToByteArray(System.Drawing.Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public void SumatraPrint(string pdfFile, string printer)
        {
            var exePath = Registry.LocalMachine.OpenSubKey(
         @"SOFTWARE\Microsoft\Windows\CurrentVersion" +
         @"\App Paths\SumatraPDF.exe").GetValue("").ToString();

          var args = $"-print-to \"{printer}\" {pdfFile}";

         
            var process = Process.Start(exePath, args);
            process.WaitForExit();
        }

        public void TEST_PDFtoPrinter(string pdfFile, string printer)
        {          
            var exePath = "PDFtoPrinter.exe";
            var args = $"\"{pdfFile}\" \"{printer}\"";
            var process = Process.Start(exePath, args);
            process.WaitForExit();
        }
        public void TEST_FoxitPrint(string pdfFile, string printer)
        {
            var exePath = Registry.LocalMachine.OpenSubKey(
         @"SOFTWARE\Microsoft\Windows\CurrentVersion" +
         @"\App Paths\FoxitPDFReader.exe").GetValue("").ToString();

            var args = $"-t \"{pdfFile}\" \"{printer}\" ";


            var process = Process.Start(exePath, args);
            process.WaitForExit();
        }


    }
}
