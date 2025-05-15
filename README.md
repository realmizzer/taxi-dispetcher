# Диспетчер Такси
Цель: Проектирование и реализация базы данных диспетчера такси.

## Окружение
### Общее
```
.NET 8.0
Visual Studio 2022
MySQL Workbench 8.0.42
MySQL Community Server 9.3.0
```
### Зависимости
```
iTextSharp (для генерации PDF-файлов)
MySql.Data (для работы с MySQL)
System.Configuration.ConfigurationManager (использовался для подключение SQL к App)
```

## Установка
Для корректной работы приложение требуется развернуть SQL сервер. В моём случае использовался MySQL Workbench и MySQL Community Server. <br/>
После можно воспользоваться как релизной версией в GitHub, так и сбилдить само решение в Visual Studio.