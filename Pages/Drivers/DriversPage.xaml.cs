using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TaxiDispatcher.Utils;

namespace TaxiDispatcher.Pages.Drivers
{
    public partial class DriversPage : Page
    {
        private DatabaseHelper db = new DatabaseHelper();

        public DriversPage()
        {
            InitializeComponent();
            LoadDrivers();
        }

        private void LoadDrivers()
        {
            string query = "SELECT DriverID, CONCAT(FirstName, ' ', LastName) as FullName, " +
                          "Phone, CONCAT(CarModel, ' (', CarNumber, ')') as CarInfo, " +
                          "Status FROM Drivers";
            DriversGrid.ItemsSource = db.ExecuteQuery(query).DefaultView;
        }

        private void RefreshDrivers_Click(object sender, RoutedEventArgs e)
        {
            LoadDrivers();
            SearchTextBox.Clear();
            StatusComboBox.SelectedIndex = 0;
        }

        private void AddDriver_Click(object sender, RoutedEventArgs e)
        {
            var form = new DriverForm();
            if (form.ShowDialog() == true)
            {
                LoadDrivers();
            }
        }

        private void EditDriver_Click(object sender, RoutedEventArgs e)
        {
            if (DriversGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите водителя");
                return;
            }

            DataRowView row = (DataRowView)DriversGrid.SelectedItem;
            int driverId = Convert.ToInt32(row["DriverID"]);

            var form = new DriverForm(driverId);
            if (form.ShowDialog() == true)
            {
                LoadDrivers();
            }
        }

        private void SearchDrivers_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            string statusTag = ((ComboBoxItem)StatusComboBox.SelectedItem).Tag.ToString();

            string query = @"SELECT 
                                DriverID,
                                CONCAT(FirstName, ' ', LastName) as FullName,
                                Phone,
                                CONCAT(CarModel, ' (', CarNumber, ')') as CarInfo,
                                Status,
                                Rating
                                FROM Drivers
                                WHERE (
                                DriverID LIKE @search
                                OR FirstName LIKE @search
                                OR LastName LIKE @search
                                OR Phone LIKE @search)";

            if (statusTag != "All")
            {
                query += " AND Status = @status";
            }

            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("@search", $"%{searchText}%")
            };

            if (statusTag != "All")
            {
                parameters.Add(new MySqlParameter("@status", statusTag));
            }

            DriversGrid.ItemsSource = db.ExecuteQuery(query, parameters.ToArray()).DefaultView;
        }
    }
}
