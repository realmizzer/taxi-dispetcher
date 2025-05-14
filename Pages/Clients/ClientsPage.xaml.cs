using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TaxiDispatcher.Utils;

namespace TaxiDispatcher.Pages.Client
{
    public partial class ClientsPage : Page
    {
        private readonly DatabaseHelper db = new DatabaseHelper();

        public ClientsPage()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients()
        {
            try
            {
                string query = @"SELECT ClientID, 
                               CONCAT(FirstName, ' ', LastName) as FullName, 
                               Phone, 
                               DATE_FORMAT(RegistrationDate, '%d.%m.%Y %H:%i') as RegistrationDate, 
                               LoyaltyPoints 
                               FROM Clients
                               ORDER BY RegistrationDate DESC";

                ClientsGrid.ItemsSource = db.ExecuteQuery(query).DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addForm = new ClientForm();
                if (addForm.ShowDialog() == true)
                {
                    string query = @"INSERT INTO Clients 
                                   (FirstName, LastName, Phone) 
                                   VALUES (@FirstName, @LastName, @Phone)";

                    var parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@FirstName", addForm.FirstName),
                        new MySqlParameter("@LastName", addForm.LastName),
                        new MySqlParameter("@Phone", addForm.Phone)
                    };

                    int affectedRows = db.ExecuteNonQuery(query, parameters);
                    if (affectedRows > 0)
                    {
                        MessageBox.Show("Клиент успешно добавлен");
                        LoadClients();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}");
            }
        }

        private void RefreshClients_Click(object sender, RoutedEventArgs e)
        {
            LoadClients();
            SearchTextBox.Clear();
        }

        private void SearchClients_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchText = SearchTextBox.Text;
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    LoadClients();
                    return;
                }

                string query = @"SELECT ClientID, 
                               CONCAT(FirstName, ' ', LastName) as FullName, 
                               Phone, 
                               DATE_FORMAT(RegistrationDate, '%d.%m.%Y %H:%i') as RegistrationDate, 
                               LoyaltyPoints 
                               FROM Clients
                               WHERE FirstName LIKE @search 
                               OR LastName LIKE @search 
                               OR Phone LIKE @search
                               ORDER BY RegistrationDate DESC";

                var parameter = new MySqlParameter("@search", $"%{searchText}%");
                ClientsGrid.ItemsSource = db.ExecuteQuery(query, new[] { parameter }).DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }

        private void ShowHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ClientsGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите клиента");
                    return;
                }

                DataRowView row = (DataRowView)ClientsGrid.SelectedItem;
                int clientId = Convert.ToInt32(row["ClientID"]);

                string query = @"SELECT o.OrderID, 
                               o.PickupAddress, 
                               o.DestinationAddress, 
                               o.Price, 
                               o.Status,
                               DATE_FORMAT(o.OrderTime, '%d.%m.%Y %H:%i') as OrderTime,
                               IFNULL(CONCAT(d.FirstName, ' ', d.LastName), 'Не назначен') as Driver
                               FROM Orders o
                               LEFT JOIN Drivers d ON o.DriverID = d.DriverID
                               WHERE o.ClientID = @clientId
                               ORDER BY o.OrderTime DESC";

                var parameter = new MySqlParameter("@clientId", clientId);
                DataTable historyData = db.ExecuteQuery(query, new[] { parameter });

                if (historyData.Rows.Count == 0)
                {
                    MessageBox.Show("У этого клиента нет заказов");
                    return;
                }

                var historyWindow = new ClientHistoryWindow(historyData);
                historyWindow.Title = $"История заказов клиента ID: {clientId}";
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке истории: {ex.Message}");
            }
        }
    }
}