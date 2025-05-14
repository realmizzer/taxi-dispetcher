-- Создание базы данных
CREATE DATABASE TaxiDispatcher;
USE TaxiDispatcher;

-- Таблица водителей
CREATE TABLE Drivers (
    DriverID INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    LicenseNumber VARCHAR(20) UNIQUE NOT NULL,
    Phone VARCHAR(15) NOT NULL,
    CarModel VARCHAR(50) NOT NULL,
    CarNumber VARCHAR(15) UNIQUE NOT NULL,
    Status ENUM('Available', 'OnRide', 'Offline') DEFAULT 'Available',
    Rating DECIMAL(3,2) DEFAULT 0.0
);

-- Таблица клиентов
CREATE TABLE Clients (
    ClientID INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Phone VARCHAR(15) UNIQUE NOT NULL,
    RegistrationDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    LoyaltyPoints INT DEFAULT 0
);

-- Таблица заказов
CREATE TABLE Orders (
    OrderID INT AUTO_INCREMENT PRIMARY KEY,
    ClientID INT NOT NULL,
    DriverID INT,
    PickupAddress VARCHAR(100) NOT NULL,
    DestinationAddress VARCHAR(100) NOT NULL,
    OrderTime DATETIME DEFAULT CURRENT_TIMESTAMP,
    StartTime DATETIME,
    EndTime DATETIME,
    Status ENUM('Pending', 'Assigned', 'InProgress', 'Completed', 'Cancelled') DEFAULT 'Pending',
    Price DECIMAL(10,2),
    PaymentMethod ENUM('Cash', 'Card', 'MobilePay'),
    FOREIGN KEY (ClientID) REFERENCES Clients(ClientID),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);

-- Таблица платежей
CREATE TABLE Payments (
    PaymentID INT AUTO_INCREMENT PRIMARY KEY,
    OrderID INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentTime DATETIME DEFAULT CURRENT_TIMESTAMP,
    Status ENUM('Pending', 'Completed', 'Failed') DEFAULT 'Pending',
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);