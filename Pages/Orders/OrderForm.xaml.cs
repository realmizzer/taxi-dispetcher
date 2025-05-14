using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TaxiDispatcher.Utils;

namespace TaxiDispatcher.Pages.Orders
{
    public partial class OrderForm : Window
    {
        private readonly bool isEditMode;
        private readonly int orderId;
        private readonly DatabaseHelper db = new DatabaseHelper();

        public OrderForm()
        {
            InitializeComponent();
            LoadClients();
            LoadDrivers();
        }

        public OrderForm(int id) : this()
        {
            isEditMode = true;
            orderId = id;
            Title = "Редактирование заказа";
            LoadOrderData();
        }

        private void LoadClients()
        {
            string query = "SELECT ClientID, CONCAT(FirstName, ' ', LastName) as FullName FROM Clients";
            ClientComboBox.ItemsSource = db.ExecuteQuery(query).DefaultView;
        }

        private void LoadDrivers()
        {
            string query = "SELECT DriverID, CONCAT(FirstName, ' ', LastName) as FullName FROM Drivers WHERE Status = 'Available'";
            DriverComboBox.ItemsSource = db.ExecuteQuery(query).DefaultView;
        }

        private void LoadOrderData()
        {
            string query = "SELECT * FROM Orders WHERE OrderID = @id";
            var parameter = new MySqlParameter("@id", orderId);
            var dataTable = db.ExecuteQuery(query, new[] { parameter });

            if (dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                PickupTextBox.Text = row["PickupAddress"].ToString();
                DestinationTextBox.Text = row["DestinationAddress"].ToString();

                foreach (DataRowView item in ClientComboBox.Items)
                {
                    if (Convert.ToInt32(item["ClientID"]) == Convert.ToInt32(row["ClientID"]))
                    {
                        ClientComboBox.SelectedItem = item;
                        break;
                    }
                }

                if (row["DriverID"] != DBNull.Value)
                {
                    foreach (DataRowView item in DriverComboBox.Items)
                    {
                        if (Convert.ToInt32(item["DriverID"]) == Convert.ToInt32(row["DriverID"]))
                        {
                            DriverComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }

                if (row["PaymentMethod"] != DBNull.Value)
                {
                    for (int i = 0; i < PaymentMethodComboBox.Items.Count; i++)
                    {
                        var item = (ComboBoxItem)PaymentMethodComboBox.Items[i];
                        if (item.Content.ToString() == row["PaymentMethod"].ToString())
                        {
                            PaymentMethodComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ClientComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента");
                return;
            }

            if (string.IsNullOrWhiteSpace(PickupTextBox.Text) ||
                string.IsNullOrWhiteSpace(DestinationTextBox.Text))
            {
                MessageBox.Show("Заполните адреса подачи и назначения");
                return;
            }

            try
            {
                int clientId = Convert.ToInt32(ClientComboBox.SelectedValue);
                int? driverId = DriverComboBox.SelectedValue != null ?
                    Convert.ToInt32(DriverComboBox.SelectedValue) : (int?)null;

                string paymentMethod = null;
                if (PaymentMethodComboBox.SelectedItem != null)
                {
                    paymentMethod = ((ComboBoxItem)PaymentMethodComboBox.SelectedItem).Content.ToString() switch
                    {
                        "Наличные" => "Cash",
                        "Карта" => "Card",
                        "Мобильный платеж" => "MobilePay",
                        _ => null
                    };
                }

                string query;
                MySqlParameter[] parameters;

                if (isEditMode)
                {
                    query = @"UPDATE Orders SET 
                    ClientID = @clientId,
                    DriverID = @driverId,
                    PickupAddress = @pickup,
                    DestinationAddress = @destination,
                    PaymentMethod = @paymentMethod
                    WHERE OrderID = @orderId";

                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@clientId", clientId),
                        new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                        new MySqlParameter("@pickup", PickupTextBox.Text),
                        new MySqlParameter("@destination", DestinationTextBox.Text),
                        new MySqlParameter("@paymentMethod", paymentMethod ?? (object)DBNull.Value),
                        new MySqlParameter("@orderId", orderId)
                    };
                }
                else
                {
                    query = @"INSERT INTO Orders 
                    (ClientID, DriverID, PickupAddress, DestinationAddress, PaymentMethod, Status) 
                    VALUES (@clientId, @driverId, @pickup, @destination, @paymentMethod, 'Pending')";

                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@clientId", clientId),
                        new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                        new MySqlParameter("@pickup", PickupTextBox.Text),
                        new MySqlParameter("@destination", DestinationTextBox.Text),
                        new MySqlParameter("@paymentMethod", paymentMethod ?? (object)DBNull.Value)
                    };
                }

                int affectedRows = db.ExecuteNonQuery(query, parameters);
                if (affectedRows > 0)
                {
                    if (driverId.HasValue)
                    {
                        string updateDriverQuery = "UPDATE Drivers SET Status = 'OnRide' WHERE DriverID = @driverId";
                        db.ExecuteNonQuery(updateDriverQuery, new[] { new MySqlParameter("@driverId", driverId.Value) });
                    }

                    DialogResult = true;
                    Close();
                }
            }
            catch (MySqlException ex) when (ex.Number == 1265)
            {
                MessageBox.Show("Некорректное значение способа оплаты. Выберите из списка.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения заказа: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}