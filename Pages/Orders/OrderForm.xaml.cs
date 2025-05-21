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
        private int? currentDriverId = null;

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
            string query = isEditMode
                ? "SELECT DriverID, CONCAT(FirstName, ' ', LastName) as FullName FROM Drivers"
                : "SELECT DriverID, CONCAT(FirstName, ' ', LastName) as FullName FROM Drivers WHERE Status = 'Available'";

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

                // Загрузка клиента
                foreach (DataRowView item in ClientComboBox.Items)
                {
                    if (Convert.ToInt32(item["ClientID"]) == Convert.ToInt32(row["ClientID"]))
                    {
                        ClientComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Загрузка водителя
                if (row["DriverID"] != DBNull.Value)
                {
                    currentDriverId = Convert.ToInt32(row["DriverID"]);
                    string driverQuery = "SELECT DriverID, CONCAT(FirstName, ' ', LastName) as FullName FROM Drivers WHERE DriverID = @driverId";
                    var driverData = db.ExecuteQuery(driverQuery, new[] { new MySqlParameter("@driverId", currentDriverId) });

                    if (driverData.Rows.Count > 0)
                    {
                        // Сохраняем текущий источник данных
                        var currentSource = DriverComboBox.ItemsSource as DataView;
                        var combinedTable = currentSource?.Table?.Clone() ?? driverData.Clone();

                        // Добавляем текущего водителя
                        combinedTable.ImportRow(driverData.Rows[0]);

                        // Добавляем остальных водителей
                        if (currentSource != null)
                        {
                            foreach (DataRowView drv in currentSource)
                            {
                                if (Convert.ToInt32(drv["DriverID"]) != currentDriverId)
                                {
                                    combinedTable.ImportRow(drv.Row);
                                }
                            }
                        }

                        DriverComboBox.ItemsSource = combinedTable.DefaultView;

                        // Выбираем текущего водителя
                        foreach (DataRowView item in DriverComboBox.Items)
                        {
                            if (Convert.ToInt32(item["DriverID"]) == currentDriverId)
                            {
                                DriverComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }

                // Загрузка способа оплаты
                if (row["PaymentMethod"] != DBNull.Value)
                {
                    foreach (ComboBoxItem item in PaymentMethodComboBox.Items)
                    {
                        if (item.Tag.ToString() == row["PaymentMethod"].ToString())
                        {
                            PaymentMethodComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Загрузка статуса
                if (row["Status"] != DBNull.Value)
                {
                    foreach (ComboBoxItem item in StatusComboBox.Items)
                    {
                        if (item.Tag.ToString() == row["Status"].ToString())
                        {
                            StatusComboBox.SelectedItem = item;
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
                int? newDriverId = DriverComboBox.SelectedValue != null ?
                    Convert.ToInt32(DriverComboBox.SelectedValue) : (int?)null;

                string paymentMethod = (PaymentMethodComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                string status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

                string query;
                MySqlParameter[] parameters;

                if (isEditMode)
                {
                    query = @"UPDATE Orders SET 
                            ClientID = @clientId,
                            DriverID = @driverId,
                            PickupAddress = @pickup,
                            DestinationAddress = @destination,
                            PaymentMethod = @paymentMethod,
                            Status = @status
                            WHERE OrderID = @orderId";

                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@clientId", clientId),
                        new MySqlParameter("@driverId", newDriverId ?? (object)DBNull.Value),
                        new MySqlParameter("@pickup", PickupTextBox.Text),
                        new MySqlParameter("@destination", DestinationTextBox.Text),
                        new MySqlParameter("@paymentMethod", paymentMethod ?? (object)DBNull.Value),
                        new MySqlParameter("@status", status ?? (object)DBNull.Value),
                        new MySqlParameter("@orderId", orderId)
                    };

                    // Обновляем статусы водителей
                    if (currentDriverId.HasValue && currentDriverId != newDriverId)
                    {
                        // Освобождаем предыдущего водителя
                        db.ExecuteNonQuery(
                            "UPDATE Drivers SET Status = 'Available' WHERE DriverID = @driverId",
                            new[] { new MySqlParameter("@driverId", currentDriverId.Value) } );
                    }
                }
                else
                {
                    query = @"INSERT INTO Orders 
                            (ClientID, DriverID, PickupAddress, DestinationAddress, PaymentMethod, Status) 
                            VALUES (@clientId, @driverId, @pickup, @destination, @paymentMethod, @status)";

                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@clientId", clientId),
                        new MySqlParameter("@driverId", newDriverId ?? (object)DBNull.Value),
                        new MySqlParameter("@pickup", PickupTextBox.Text),
                        new MySqlParameter("@destination", DestinationTextBox.Text),
                        new MySqlParameter("@paymentMethod", paymentMethod ?? (object)DBNull.Value),
                        new MySqlParameter("@status", status ?? (object)DBNull.Value)
                    };
                }

                int affectedRows = db.ExecuteNonQuery(query, parameters);
                if (affectedRows > 0)
                {
                    // Обновляем статус нового водителя
                    if (newDriverId.HasValue)
                    {
                        string driverStatus = status == "Completed" || status == "Cancelled"
                            ? "Available"
                            : "Busy";

                        db.ExecuteNonQuery(
                            "UPDATE Drivers SET Status = @status WHERE DriverID = @driverId",
                            new[]
                            {
                                new MySqlParameter("@status", driverStatus),
                                new MySqlParameter("@driverId", newDriverId.Value)
                            });
                    }

                    DialogResult = true;
                    Close();
                }
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