using System.Windows;
using TaxiDispatcher.Pages.Client;
using TaxiDispatcher.Pages.Drivers;
using TaxiDispatcher.Pages.Orders;
using TaxiDispatcher.Pages.Reports;

namespace TaxiDispatcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadPages();
        }

        private void LoadPages()
        {
            OrdersFrame.Content = new OrdersPage();
            DriversFrame.Content = new DriversPage();
            ClientsFrame.Content = new ClientsPage();
            ReportsFrame.Content = new ReportsPage();
        }
    }
}