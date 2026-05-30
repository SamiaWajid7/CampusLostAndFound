# Campus Lost and Found Portal

A full-stack ASP.NET Core MVC application for managing lost and found items on a college campus.

## Features

- **User Authentication**: Email/password registration and login with ASP.NET Identity
- **Item Management**: Report lost or found items with images, categories, and locations
- **Claim System**: Users can submit claims for items with verification workflow
- **Admin Dashboard**: Manage items, review claims, view statistics and audit logs
- **In-App Notifications**: Real-time notifications for claim updates
- **Search & Filter**: Find items by category, location, type, and keywords

## Tech Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, Bootstrap Icons, jQuery
- **File Storage**: Local file system (configurable)

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019 or later (or SQL Server Express/LocalDB)
- Visual Studio 2022 or VS Code

### Setup Instructions

1. **Clone or Download** the project

2. **Create the Database**
   ```bash
   # Run the SQL scripts in order:
   # 1. Database/01_CreateDatabase.sql
   # 2. Database/02_SeedData.sql
   ```

3. **Update Connection String**
   
   Edit `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=CampusLostAndFoundDb;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

4. **Run the Application**
   ```bash
   cd CampusLostAndFound.Web
   dotnet run
   ```

5. **Access the Application**
   - Open `https://localhost:5001` or `http://localhost:5000`
   - Default Admin: `admin@campus.edu` / `Admin@123`

## Project Structure

```
CampusLostAndFound/
├── Database/
│   ├── 01_CreateDatabase.sql    # Database schema
│   └── 02_SeedData.sql          # Initial data
├── CampusLostAndFound.Web/
│   ├── Controllers/             # MVC Controllers
│   ├── Data/                    # DbContext
│   ├── Models/
│   │   ├── DTOs/               # Data Transfer Objects
│   │   ├── Entities/           # Database entities
│   │   └── Enums/              # Enumerations
│   ├── Services/               # Business logic
│   ├── Views/                  # Razor views
│   ├── wwwroot/               # Static files
│   ├── Program.cs             # App entry point
│   └── appsettings.json       # Configuration
└── CampusLostAndFound.sln     # Solution file
```

## User Roles

- **Student**: Can report items, submit claims, manage their items
- **Admin**: Full access to manage all items, review claims, view audit logs

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Items` | GET | List all items |
| `/Items/Create` | GET/POST | Report new item |
| `/Items/Details/{id}` | GET | View item details |
| `/Claims/Create` | POST | Submit a claim |
| `/Admin/Dashboard` | GET | Admin statistics |
| `/Admin/Claims` | GET | Manage claims |

## Configuration

### File Upload Settings

```json
{
  "FileUpload": {
    "MaxFileSize": 5242880,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"],
    "UploadPath": "wwwroot/uploads"
  }
}
```

### Identity Settings

```json
{
  "Identity": {
    "Password": {
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequiredLength": 6
    }
  }
}
```

## Deployment

### IIS Deployment

1. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Create IIS site pointing to publish folder

3. Ensure app pool uses "No Managed Code"

### Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CampusLostAndFound.Web.dll"]
```

## License

MIT License - Feel free to use for educational purposes.
