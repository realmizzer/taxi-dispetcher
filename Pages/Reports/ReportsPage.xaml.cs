using iTextSharp.text;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TaxiDispatcher.Utils;

namespace TaxiDispatcher.Pages.Reports
{
    public partial class ReportsPage : Page
    {
        private readonly DatabaseHelper db = new DatabaseHelper();

        public ReportsPage()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            EndDatePicker.SelectedDate = DateTime.Today;
            GenerateReport_Click(null, null);
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.Today.AddDays(-7);
                DateTime endDate = EndDatePicker.SelectedDate ?? DateTime.Today;

                string reportType = ((ComboBoxItem)ReportTypeCombo.SelectedItem).Content.ToString();
                DataTable reportData = GenerateReportData(reportType, startDate, endDate);

                ReportDataGrid.ItemsSource = reportData.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации отчета: {ex.Message}");
            }
        }

        private DataTable GenerateReportData(string reportType, DateTime startDate, DateTime endDate)
        {
            string query = reportType switch
            {
                "Отчет по заказам" => GetOrdersReportQuery(),
                "Отчет по водителям" => GetDriversReportQuery(),
                _ => GetFinancialReportQuery()
            };

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@startDate", startDate.ToString("yyyy-MM-dd")),
                new MySqlParameter("@endDate", endDate.ToString("yyyy-MM-dd 23:59:59"))
            };

            return db.ExecuteQuery(query, parameters);
        }

        private string GetOrdersReportQuery()
        {
            return @"SELECT 
                    o.OrderID,
                    CONCAT(c.FirstName, ' ', c.LastName) as Client,
                    IFNULL(CONCAT(d.FirstName, ' ', d.LastName), 'Не назначен') as Driver,
                    o.PickupAddress as 'Откуда',
                    o.DestinationAddress as 'Куда',
                    o.Price as 'Стоимость',
                    o.Status as 'Статус',
                    DATE_FORMAT(o.OrderTime, '%d.%m.%Y %H:%i') as 'Дата заказа'
                FROM Orders o
                JOIN Clients c ON o.ClientID = c.ClientID
                LEFT JOIN Drivers d ON o.DriverID = d.DriverID
                WHERE o.OrderTime BETWEEN @startDate AND @endDate
                ORDER BY o.OrderTime DESC";
        }

        private string GetDriversReportQuery()
        {
            return @"SELECT 
                    d.DriverID,
                    CONCAT(d.FirstName, ' ', d.LastName) as 'Водитель',
                    COUNT(o.OrderID) as 'Кол-во заказов',
                    SUM(o.Price) as 'Общий доход',
                    AVG(o.Price) as 'Средний заказ',
                    d.Rating as 'Рейтинг'
                FROM Drivers d
                LEFT JOIN Orders o ON d.DriverID = o.DriverID 
                    AND o.OrderTime BETWEEN @startDate AND @endDate
                GROUP BY d.DriverID
                ORDER BY SUM(o.Price) DESC";
        }

        private string GetFinancialReportQuery()
        {
            return @"SELECT 
                    DATE_FORMAT(o.OrderTime, '%d.%m.%Y') as 'Дата',
                    COUNT(o.OrderID) as 'Кол-во заказов',
                    SUM(o.Price) as 'Общий доход',
                    AVG(o.Price) as 'Средний заказ',
                    o.PaymentMethod as 'Способ оплаты'
                FROM Orders o
                WHERE o.OrderTime BETWEEN @startDate AND @endDate
                GROUP BY DATE(o.OrderTime), o.PaymentMethod
                ORDER BY DATE(o.OrderTime) DESC";
        }

        private void ExportToPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ReportDataGrid.ItemsSource == null || ReportDataGrid.Items.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта");
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Отчет_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                        PdfWriter.GetInstance(document, stream);

                        document.Open();

                        // Заголовок отчета
                        string reportType = ((ComboBoxItem)ReportTypeCombo.SelectedItem).Content.ToString();
                        DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.Today.AddDays(-7);
                        DateTime endDate = EndDatePicker.SelectedDate ?? DateTime.Today;

                        document.Add(new Paragraph($"Отчет: {reportType}") { Alignment = Element.ALIGN_CENTER });
                        document.Add(new Paragraph($"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}"));
                        document.Add(Chunk.NEWLINE);

                        // Создаем таблицу
                        PdfPTable table = new PdfPTable(ReportDataGrid.Columns.Count);
                        table.WidthPercentage = 100;

                        // Добавляем заголовки
                        foreach (DataGridColumn column in ReportDataGrid.Columns)
                        {
                            if (column.Header != null)
                            {
                                table.AddCell(new Phrase(column.Header.ToString()));
                            }
                        }

                        // Добавляем данные
                        foreach (var item in ReportDataGrid.Items)
                        {
                            if (item is DataRowView row)
                            {
                                for (int i = 0; i < ReportDataGrid.Columns.Count; i++)
                                {
                                    var column = ReportDataGrid.Columns[i] as DataGridTextColumn;
                                    if (column != null)
                                    {
                                        var binding = column.Binding as Binding;
                                        if (binding != null)
                                        {
                                            string propertyName = binding.Path.Path;
                                            table.AddCell(new Phrase(row[propertyName].ToString()));
                                        }
                                    }
                                }
                            }
                        }

                        document.Add(table);
                        document.Close();
                    }

                    MessageBox.Show("Отчет успешно экспортирован в PDF");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в PDF: {ex.Message}");
            }
        }
    }
}