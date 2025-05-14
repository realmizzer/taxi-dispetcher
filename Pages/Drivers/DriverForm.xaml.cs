using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using TaxiDispatcher.Utils;

namespace TaxiDispatcher.Pages.Drivers
{
    public partial class DriverForm : Window
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Phone { get; private set; }
        public string LicenseNumber { get; private set; }
        public string CarModel { get; private set; }
        public string CarNumber { get; private set; }

        private readonly bool isEditMode;
        private readonly int driverId;

        // Конструктор для добавления нового водителя
        public DriverForm()
        {
            InitializeComponent();
            Title = "Добавить водителя";
        }

        // Конструктор для редактирования существующего водителя
        public DriverForm(int id) : this()
        {
            isEditMode = true;
            driverId = id;
            Title = "Редактировать водителя";
            LoadDriverData();
        }

        private void LoadDriverData()
        {
            try
            {
                string query = "SELECT * FROM Drivers WHERE DriverID = @id";
                var parameter = new MySqlParameter("@id", driverId);
                var dataTable = new DatabaseHelper().ExecuteQuery(query, new[] { parameter });

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    FirstNameBox.Text = row["FirstName"].ToString();
                    LastNameBox.Text = row["LastName"].ToString();
                    PhoneBox.Text = row["Phone"].ToString();
                    LicenseBox.Text = row["LicenseNumber"].ToString();
                    CarModelBox.Text = row["CarModel"].ToString();
                    CarNumberBox.Text = row["CarNumber"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text) ||
                string.IsNullOrWhiteSpace(LicenseBox.Text) ||
                string.IsNullOrWhiteSpace(CarModelBox.Text) ||
                string.IsNullOrWhiteSpace(CarNumberBox.Text))
            {
                MessageBox.Show("Все поля обязательны для заполнения");
                return;
            }

            if (!Regex.IsMatch(PhoneBox.Text, @"^[\d\+\-\(\)\s]{6,}$"))
            {
                MessageBox.Show("Некорректный формат телефона");
                return;
            }

            try
            {
                FirstName = FirstNameBox.Text;
                LastName = LastNameBox.Text;
                Phone = PhoneBox.Text;
                LicenseNumber = LicenseBox.Text;
                CarModel = CarModelBox.Text;
                CarNumber = CarNumberBox.Text;

                string query;
                MySqlParameter[] parameters;

                if (isEditMode)
                {
                    query = @"UPDATE Drivers SET 
                            FirstName = @firstName,
                            LastName = @lastName,
                            Phone = @phone,
                            LicenseNumber = @license,
                            CarModel = @carModel,
                            CarNumber = @carNumber
                            WHERE DriverID = @id";

                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@firstName", FirstName),
                        new MySqlParameter("@lastName", LastName),
                        new MySqlParameter("@phone", Phone),
                        new MySqlParameter("@license", LicenseNumber),
                        new MySqlParameter("@carModel", CarModel),
                        new MySqlParameter("@carNumber", CarNumber),
                        new MySqlParameter("@id", driverId)
                    };
                }
                else
                {
                    query = @"INSERT INTO Drivers 
                            (FirstName, LastName, Phone, LicenseNumber, CarModel, CarNumber, Status) 
                            VALUES (@firstName, @lastName, @phone, @license, @carModel, @carNumber, 'Available')";

                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@firstName", FirstName),
                        new MySqlParameter("@lastName", LastName),
                        new MySqlParameter("@phone", Phone),
                        new MySqlParameter("@license", LicenseNumber),
                        new MySqlParameter("@carModel", CarModel),
                        new MySqlParameter("@carNumber", CarNumber)
                    };
                }

                int affectedRows = new DatabaseHelper().ExecuteNonQuery(query, parameters);
                if (affectedRows > 0)
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Ошибка дублирования уникального ключа
            {
                MessageBox.Show("Водитель с таким номером телефона или номером автомобиля уже существует");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PhoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры, +, -, пробел и скобки
            if (!Regex.IsMatch(e.Text, @"^[\d\+\-\(\)\s]+$"))
            {
                e.Handled = true;
            }
        }
    }
}