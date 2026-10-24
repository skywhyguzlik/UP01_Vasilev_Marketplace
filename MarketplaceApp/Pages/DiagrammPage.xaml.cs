using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms.DataVisualization.Charting;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class DiagrammPage : Page
    {
        private const int StatusCancelled = 5;   

        public DiagrammPage()
        {
            InitializeComponent();
            Core.RefreshAll();  

            
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));
            var currentSeries = new Series("Покупки, руб.")
            {
                IsValueShownAsLabel = true
            };
            ChartPayments.Series.Add(currentSeries);

            
            CmbUser.ItemsSource = Core.Context.Users
                .Where(u => u.RoleId == Session.RoleBuyer)
                .OrderBy(u => u.LastName)
                .ToList();
            CmbDiagram.ItemsSource = new List<SeriesChartType>
            {
                SeriesChartType.Column,
                SeriesChartType.Bar,
                SeriesChartType.Line,
                SeriesChartType.Pie
            };
            CmbDiagram.SelectedItem = SeriesChartType.Column;
            CmbUser.SelectedIndex = 0;

            InfoText.Text = "Отчёты сохраняются в папку: " + ReportsFolder;
        }

        
        private static string ReportsFolder
        {
            get
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MarketplaceReports");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        
        private List<OrderItems> GetBuyerItems(int userId)
        {
            return Core.Context.OrderItems
                .Where(i => i.Orders.BuyerId == userId && i.Orders.StatusId != StatusCancelled)
                .ToList();
        }

       
        private static decimal SumByCategory(List<OrderItems> items, int categoryId)
        {
            return items.Where(i => i.Products.CategoryId == categoryId).Sum(i => i.Quantity * i.UnitPrice);
        }

        
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized || ChartPayments.Series.Count == 0)
                return;

            if (CmbUser.SelectedItem is Users currentUser && CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                
                var color = (System.Windows.Media.Color)Application.Current.Resources["ColorPrimary"];
                currentSeries.Color = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
                currentSeries.LabelFormat = "N0";

                bool isRound = currentType == SeriesChartType.Pie || currentType == SeriesChartType.Doughnut ||
                               currentType == SeriesChartType.Funnel || currentType == SeriesChartType.Pyramid;
                if (isRound)
                    currentSeries.Color = System.Drawing.Color.Empty;   

                var items = GetBuyerItems(currentUser.UserId);
                var categoriesList = Core.Context.Categories.OrderBy(c => c.CategoryName).ToList();
                foreach (var category in categoriesList)
                {
                    decimal sum = SumByCategory(items, category.CategoryId);
                    if (isRound && sum == 0)
                        continue;                                      
                    currentSeries.Points.AddXY(category.CategoryName, sum);
                }

                ChartPayments.Titles.Clear();
                ChartPayments.Titles.Add("Покупки по категориям: " + currentUser.FIO);
                ChartPayments.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
            }
        }

        
        private const int wdStyleTitle = -63;              
        private const int wdStyleSubtitle = -75;           
        private const int wdAlignParagraphCenter = 1;      
        private const int wdLineStyleSingle = 1;           
        private const int wdCellAlignVerticalCenter = 1;   
        private const int wdColorDarkRed = 128;            
        private const int wdColorDarkGreen = 13056;        
        private const int wdPageBreak = 7;                 
        private const int wdHeaderFooterPrimary = 1;       
        private const int wdAlignPageNumberCenter = 1;     
        private const int wdFormatPDF = 17;                
        private const int xlCenter = -4108;                
        private const int xlContinuous = 1;                
        private const int xlOpenXMLWorkbook = 51;          

        
        private static dynamic StartOffice(string progId)
        {
            Type type = Type.GetTypeFromProgID(progId);
            if (type == null)
                throw new InvalidOperationException("На компьютере не установлен Microsoft Office (" + progId + ").");
            return Activator.CreateInstance(type);
        }

       
        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            var allUsers = Core.Context.Users.Where(u => u.RoleId == Session.RoleBuyer).OrderBy(u => u.LastName).ToList();
            var allCategories = Core.Context.Categories.OrderBy(c => c.CategoryName).ToList();
            if (allUsers.Count == 0)
            {
                MessageHelper.Warning("В базе нет покупателей.");
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                dynamic application = StartOffice("Word.Application");
                dynamic document = application.Documents.Add();

                foreach (var user in allUsers)
                {
                    var items = GetBuyerItems(user.UserId);

                    
                    dynamic userParagraph = document.Paragraphs.Add();
                    dynamic userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userParagraph.Style = wdStyleTitle;
                    userRange.ParagraphFormat.Alignment = wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add();   

                    
                    dynamic tableParagraph = document.Paragraphs.Add();
                    dynamic tableRange = tableParagraph.Range;
                    dynamic paymentsTable = document.Tables.Add(tableRange, allCategories.Count + 2, 2);
                    paymentsTable.Borders.InsideLineStyle = wdLineStyleSingle;
                    paymentsTable.Borders.OutsideLineStyle = wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = wdCellAlignVerticalCenter;

                    paymentsTable.Cell(1, 1).Range.Text = "Категория";
                    paymentsTable.Cell(1, 2).Range.Text = "Сумма покупок";
                    dynamic headerRow = paymentsTable.Rows.Item(1);
                    headerRow.Range.Font.Name = "Times New Roman";
                    headerRow.Range.Font.Size = 14;
                    headerRow.Range.Bold = 1;
                    headerRow.Range.ParagraphFormat.Alignment = wdAlignParagraphCenter;

                    decimal total = 0;
                    for (int i = 0; i < allCategories.Count; i++)
                    {
                        var currentCategory = allCategories[i];
                        decimal sum = SumByCategory(items, currentCategory.CategoryId);
                        total += sum;

                        dynamic cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.CategoryName;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        cellRange = paymentsTable.Cell(i + 2, 2).Range;
                        cellRange.Text = sum.ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }

                    int totalRow = allCategories.Count + 2;
                    paymentsTable.Cell(totalRow, 1).Range.Text = "Итого";
                    paymentsTable.Cell(totalRow, 2).Range.Text = total.ToString("N2") + " руб.";
                    dynamic lastRow = paymentsTable.Rows.Item(totalRow);
                    lastRow.Range.Font.Name = "Times New Roman";
                    lastRow.Range.Font.Size = 12;
                    lastRow.Range.Bold = 1;
                    document.Paragraphs.Add();   

                   
                    OrderItems maxItem = items.OrderByDescending(x => x.Quantity * x.UnitPrice).FirstOrDefault();
                    OrderItems minItem = items.OrderBy(x => x.Quantity * x.UnitPrice).FirstOrDefault();
                    if (maxItem != null)
                    {
                        dynamic maxParagraph = document.Paragraphs.Add();
                        dynamic maxRange = maxParagraph.Range;
                        maxRange.Text = "Самая дорогая покупка — " + maxItem.Products.ProductName + " за " +
                                        (maxItem.Quantity * maxItem.UnitPrice).ToString("N2") + " руб. от " +
                                        maxItem.Orders.OrderDate.ToString("dd.MM.yyyy");
                        maxParagraph.Style = wdStyleSubtitle;
                        maxRange.Font.Color = wdColorDarkRed;
                        maxRange.InsertParagraphAfter();

                        dynamic minParagraph = document.Paragraphs.Add();
                        dynamic minRange = minParagraph.Range;
                        minRange.Text = "Самая дешёвая покупка — " + minItem.Products.ProductName + " за " +
                                        (minItem.Quantity * minItem.UnitPrice).ToString("N2") + " руб. от " +
                                        minItem.Orders.OrderDate.ToString("dd.MM.yyyy");
                        minParagraph.Style = wdStyleSubtitle;
                        minRange.Font.Color = wdColorDarkGreen;
                        minRange.InsertParagraphAfter();
                    }
                    else
                    {
                        dynamic emptyParagraph = document.Paragraphs.Add();
                        emptyParagraph.Range.Text = "Покупок пока нет.";
                        emptyParagraph.Range.InsertParagraphAfter();
                    }

                    
                    if (user != allUsers.LastOrDefault())
                        document.Words.Last.InsertBreak(wdPageBreak);
                }

               
                int sectionsCount = document.Sections.Count;
                for (int s = 1; s <= sectionsCount; s++)
                {
                    dynamic section = document.Sections.Item(s);
                    dynamic headerRange = section.Headers.Item(wdHeaderFooterPrimary).Range;
                    headerRange.ParagraphFormat.Alignment = wdAlignParagraphCenter;
                    headerRange.Font.Size = 10;
                    headerRange.Text = "Маркетплейс товаров. Отчёт о покупках от " + DateTime.Now.ToString("dd.MM.yyyy");

                    section.Footers.Item(wdHeaderFooterPrimary).PageNumbers.Add(wdAlignPageNumberCenter);
                }

                string fileName = "Покупки_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string docxPath = Path.Combine(ReportsFolder, fileName + ".docx");
                string pdfPath = Path.Combine(ReportsFolder, fileName + ".pdf");
                document.SaveAs2(docxPath);
                document.SaveAs2(pdfPath, wdFormatPDF);

                application.Visible = true;
                MessageHelper.Info("Отчёт сохранён:\n" + docxPath + "\n" + pdfPath);
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось выполнить экспорт в Word. Проверьте, что установлен Microsoft Word.\n" + ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

       
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var allUsers = Core.Context.Users.Where(u => u.RoleId == Session.RoleBuyer).OrderBy(u => u.LastName).ToList();
            var allCategories = Core.Context.Categories.OrderBy(c => c.CategoryName).ToList();
            if (allUsers.Count == 0)
            {
                MessageHelper.Warning("В базе нет покупателей.");
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                dynamic application = StartOffice("Excel.Application");
                application.SheetsInNewWorkbook = allUsers.Count;   
                dynamic workbook = application.Workbooks.Add();

                var primary = (System.Windows.Media.Color)Application.Current.Resources["ColorPrimary"];
                int headerColor = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(primary.R, primary.G, primary.B));
                int whiteColor = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);

                for (int u = 0; u < allUsers.Count; u++)
                {
                    var user = allUsers[u];
                    var items = GetBuyerItems(user.UserId);

                    dynamic worksheet = workbook.Worksheets.Item(u + 1);
                    worksheet.Name = (u + 1) + ". " + user.LastName;   

                    worksheet.Range["A1"].Value2 = user.FIO;
                    worksheet.Range["A1"].Font.Bold = true;
                    worksheet.Range["A1"].Font.Size = 14;

                    worksheet.Range["A3"].Value2 = "Категория";
                    worksheet.Range["B3"].Value2 = "Количество, шт.";
                    worksheet.Range["C3"].Value2 = "Сумма покупок, руб.";
                    dynamic header = worksheet.Range["A3:C3"];
                    header.Font.Bold = true;
                    header.Font.Color = whiteColor;
                    header.Interior.Color = headerColor;
                    header.HorizontalAlignment = xlCenter;

                    int row = 4;
                    foreach (var category in allCategories)
                    {
                        var categoryItems = items.Where(x => x.Products.CategoryId == category.CategoryId).ToList();
                        worksheet.Range["A" + row].Value2 = category.CategoryName;
                        worksheet.Range["B" + row].Value2 = categoryItems.Sum(x => x.Quantity);
                        worksheet.Range["C" + row].Value2 = (double)categoryItems.Sum(x => x.Quantity * x.UnitPrice);
                        row++;
                    }

                   
                    worksheet.Range["A" + row].Value2 = "Итого";
                    worksheet.Range["B" + row].Formula = "=SUM(B4:B" + (row - 1) + ")";
                    worksheet.Range["C" + row].Formula = "=SUM(C4:C" + (row - 1) + ")";
                    worksheet.Range["A" + row + ":C" + row].Font.Bold = true;

                    worksheet.Range["A3:C" + row].Borders.LineStyle = xlContinuous;
                    worksheet.Range["C4:C" + row].NumberFormat = "#,##0.00";
                    worksheet.Columns.AutoFit();
                }

                string path = Path.Combine(ReportsFolder, "Покупки_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
                workbook.SaveAs(path, xlOpenXMLWorkbook);
                application.Visible = true;
                MessageHelper.Info("Отчёт сохранён:\n" + path);
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось выполнить экспорт в Excel. Проверьте, что установлен Microsoft Excel.\n" + ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}
