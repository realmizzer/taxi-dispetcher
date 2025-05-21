using System;
using System.Globalization;
using System.Windows.Data;

namespace TaxiDispatcher.Utils
{
    public class PhoneNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            string phone = value.ToString();
            string digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.Length < 11) return phone;

            // +7 (XXX) XXX-XX-XX
            return $"+7 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits.Substring(9, 2)}";
        }

        public object Convert(object value)
        {
            if (value == null) return string.Empty;

            string phone = value.ToString();
            string digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.Length < 11) return phone;

            // +7 (XXX) XXX-XX-XX
            return $"+7 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits.Substring(9, 2)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            return new string(value.ToString().Where(char.IsDigit).ToArray());
        }

        public object ConvertBack(object value)
        {
            if (value == null) return string.Empty;
            return new string(value.ToString().Where(char.IsDigit).ToArray());
        }
    }
}