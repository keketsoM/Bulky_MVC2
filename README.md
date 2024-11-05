# Bookstore E-commerce Web application 
This is a Bookstore E-commerce Web application built with the .NET MVC framework in Visual Studio 2022 using .NET 8. It is designed to provide an online platform for customers to browse and purchase books, while allowing administrators or employees to manage stock, monitor orders, and control user access.

## Features

- User authentication and authorization.
- CRUD operations for Recording waste material.
- User authentication and authorization.
- CRUD operations for Products, Category, Company.
- Manage Users and Create users.
- Manage and filter orders.
- Create Admin, Employer, Company, User Account.
- Product Search functionality 

## Installation

1. Clone this repository to your local machine:

```bash
git clone https://github.com/yourusername/waste-management-system.git
```

2. Open the solution file (`WasteManagementSystem.sln`) in Visual Studio 2022.

3. Build the solution to restore dependencies:

```
dotnet build
```

4. Configure the database connection string in `appsettings.json` file.

5. Run the database migrations to create the necessary database schema:

```
dotnet ef database update
```

6. Start the application:

```
dotnet run
```

The application will be hosted on `https://localhost:5001` by default.

## Configuration

- Database connection string: Update the `ConnectionStrings` section in `appsettings.json` with your database connection details.

## Usage

1.  Book E-commerce.

## Acknowledgements
-  ASP.NET Core
- Entity Framework Core
-  Bootstrap
- Stripe / PayPal
-  Font Awesome
- SQL Server
-  Visual Studio 2022
