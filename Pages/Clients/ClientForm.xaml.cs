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
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                MessageBox.Show("Все поля обязательны для заполнения");
                return;
            }

            FirstName = FirstNameBox.Text;
            LastName = LastNameBox.Text;
            Phone = PhoneBox.Text;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}