﻿using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TaxiDispatcher.Utils;

namespace TaxiDispatcher.Pages.Orders
{
    public partial class OrdersPage : Page
    {
        private readonly DatabaseHelper db = new DatabaseHelper();

        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"SELECT o.OrderID, 
                       CONCAT(c.FirstName, ' ', c.LastName) as ClientName,
                       IFNULL(CONCAT(d.FirstName, ' ', d.LastName), 'Не назначен') as DriverName,
                       o.PickupAddress, 
                       o.DestinationAddress,
                       o.Status,
                       o.Price,
                       DATE_FORMAT(o.OrderTime, '%d.%m.%Y %H:%i') as OrderTime,
                       o.Status as StatusForVisibility
                       FROM Orders o
                       JOIN Clients c ON o.ClientID = c.ClientID
                       LEFT JOIN Drivers d ON o.DriverID = d.DriverID
                       ORDER BY o.OrderTime DESC";

                OrdersGrid.ItemsSource = db.ExecuteQuery(query).DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            var form = new OrderForm();
            if (form.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            int orderId = (int)button.Tag;
            var form = new OrderForm(orderId);
            if (form.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private void RefreshOrders_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void SearchOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchText = SearchTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    LoadOrders();
                    return;
                }

                string query = @"SELECT o.OrderID, 
                               CONCAT(c.FirstName, ' ', c.LastName) as ClientName,
                               IFNULL(CONCAT(d.FirstName, ' ', d.LastName), 'Не назначен') as DriverName,
                               o.PickupAddress, 
                               o.DestinationAddress,
                               o.Status,
                               o.Price,
                               DATE_FORMAT(o.OrderTime, '%d.%m.%Y %H:%i') as OrderTime
                               FROM Orders o
                               JOIN Clients c ON o.ClientID = c.ClientID
                               LEFT JOIN Drivers d ON o.DriverID = d.DriverID
                               WHERE c.FirstName LIKE @search 
                               OR c.LastName LIKE @search
                               OR d.FirstName LIKE @search
                               OR d.LastName LIKE @search
                               OR o.OrderID LIKE @search
                               OR o.PickupAddress LIKE @search
                               OR o.DestinationAddress LIKE @search
                               OR o.OrderTime LIKE @search
                               ORDER BY o.OrderTime DESC";

                var parameter = new MySqlParameter("@search", $"%{searchText}%");
                OrdersGrid.ItemsSource = db.ExecuteQuery(query, new[] { parameter }).DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }
    }
}