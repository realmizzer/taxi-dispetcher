using System.Windows;

namespace TaxiDispatcher.Pages.Client
{
    public partial class ClientForm : Window
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Phone { get; private set; }

        public ClientForm()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var firstName = FirstNameBox.Text;
            var lastName = LastNameBox.Text;
            var phone = PhoneBox.Text;
            var phoneWithoutMask = GetPhoneNumberWithoutMask(phone);

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Все поля обязательны для заполнения");
                return;
            }

            if (phoneWithoutMask.Length < 11)
            {
                MessageBox.Show("Телефон должен состоять из 11 цифр");
                return;
            }
           

            FirstName = firstName;
            LastName = lastName;
            Phone = phoneWithoutMask;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private string GetPhoneNumberWithoutMask(string phoneNumber)
        {
            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }
    }
}