# BookingSystem

A modern web-based property booking system built with ASP.NET Core, designed for booking accommodations such as hotels, villas, and camping sites.

## Features

- **Property Listings**: Browse available accommodations (hotels, villas, camping sites)
- **Property Details**: View detailed information including location, price per night, and maximum guests
- **Image Gallery**: Property photos and visual previews
- **Location-based Search**: Properties available 
- **User Authentication**: Secure login and registration system using ASP.NET Core Identity
- **Account Management**: User registration, login, password recovery, and email confirmation
- **Booking System**: Reserve accommodations with guest capacity management
- **Responsive Design**: Mobile-friendly interface optimized for all devices

## Technologies Used

- **Backend**: ASP.NET Core MVC
- **Authentication**: ASP.NET Core Identity
- **Database**: Entity Framework Core SQL Server on local / On Azure PostgreSQL
- **File Upload**: Image management for property photos
- **Frontend**: HTML5, CSS3, JavaScript, Tailwind
- **Deployment**: Microsoft Azure (temporary hosting)

## Prerequisites

- .NET 6.0 or later
- SQL Server or SQL Server Express in local 
- Visual Studio 2022 or Visual Studio Code

## Installation

1. Clone the repository:
```bash
git clone https://github.com/PatoucheH/BookingSystem.git
cd BookingSystem
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Update the database connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookingSystemDb;Trusted_Connection=true;"
  }
}
```

4. Apply database migrations:
```bash
dotnet ef database update
```

5. Run the application:
```bash
dotnet run
```


## Configuration

### Database Setup

The application uses Entity Framework Core with SQL Server. Update the connection string in `appsettings.json` to match your database configuration.

### Authentication

The system uses ASP.NET Core Identity for user management. Default roles and permissions are configured during database initialization.

## Usage

1. **Browse Properties**: View available accommodations on the homepage
2. **Property Details**: Click on any property to see detailed information, photos, and pricing
3. **User Registration**: Create an account to make bookings
4. **Login**: Authenticate via the login page at `/Identity/Account/Login`
5. **Make Reservations**: Book properties based on guest capacity and availability
6. **Account Management**: Recover passwords and manage email confirmations

## Project Structure

```
BookingSystem/
├── Controllers/         # MVC Controllers (Property, Booking, Account)
├── Services/           # MVC Service (Property, User)
├── Models/             # Data models (Property, Booking, User)
├── Views/              # Razor views (Home, Property details)
├── Data/               # Entity Framework context and migrations
├── Areas/              # Identity area for authentication
├── wwwroot/            # Static files (CSS, JS, images)
│   ├── css/assets/     # Application assets and logos
│   └── uploads/        # Uploaded property images
└── appsettings.json    # Configuration settings
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -am 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Create a Pull Request

## Contact

For questions or support, please open an issue on the GitHub repository.

---

**Note**: The live demo may not be continuously available as this is a development project with temporary Azure hosting.
