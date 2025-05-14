using System.Data;
using System.Windows;

namespace TaxiDispatcher.Pages.Client
{
    public partial class ClientHistoryWindow : Window
    {
        public ClientHistoryWindow(DataTable historyData)
        {
            InitializeComponent();
            HistoryDataGrid.ItemsSource = historyData.DefaultView;
        }
    }
}