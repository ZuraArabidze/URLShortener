# URL Shortener - ASP.NET Core Application

A professional URL shortening service built with ASP.NET Core 8.0, featuring PostgreSQL for data persistence, Redis for caching, comprehensive logging with Serilog, and full test coverage using xUnit.

## Features

- Shorten URLs with auto-generated or custom short codes
- Redirect to original URLs with click tracking
- Set expiration dates for URLs
- PostgreSQL database with Entity Framework Core
- Redis caching for improved performance
- Structured logging with Serilog
- Custom exception handling middleware
- Request logging middleware
- Repository pattern for data access
- Service layer for business logic
- Comprehensive unit and integration tests with xUnit
- Docker support for PostgreSQL and Redis
- RESTful API with Swagger documentation

## Architecture

The solution follows Clean Architecture principles with three main projects:

### UrlShortener.Core
Contains domain entities, interfaces, and business logic abstractions.
- **Entities**: Domain models (ShortenedUrl)
- **Interfaces**: Service and repository contracts
- **Exceptions**: Custom exception types

### UrlShortener.Infrastructure
Implements data access and external service integrations.
- **Data**: EF Core DbContext and configurations
- **Repositories**: Data access implementations
- **Services**: Cache service and URL service implementations

### UrlShortener.API
Web API layer with controllers, middleware, and DTOs.
- **Controllers**: API endpoints
- **Middleware**: Exception handling and logging
- **DTOs**: Request/Response models

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

## Getting Started

### 1. Clone the Repository

```bash
https://github.com/ZuraArabidze/URLShortener.git
cd url-shortener
```

### 2. Start Docker Containers

Start PostgreSQL and Redis using Docker Compose:

```bash
docker-compose up -d
```

This will start:
- PostgreSQL on `localhost:5432`
- Redis on `localhost:6379`

Verify containers are running:

```bash
docker ps
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Apply Database Migrations

The application automatically applies migrations on startup, but you can also run them manually:

```bash
cd src/UrlShortener.API
dotnet ef database update
```

### 5. Run the Application

```bash
cd src/UrlShortener.API
dotnet run
```

## API Endpoints

### Create Short URL
```http
POST /api/urls
Content-Type: application/json

{
  "originalUrl": "https://www.example.com",
  "customShortCode": "mycode",  // Optional
  "expiresAt": "2024-12-31T23:59:59Z"  // Optional
}
```

Response:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "shortCode": "mycode",
  "originalUrl": "https://www.example.com",
  "shortUrl": "http://localhost:5000/mycode",
  "createdAt": "2024-01-15T10:30:00Z",
  "expiresAt": "2024-12-31T23:59:59Z",
  "clickCount": 0
}
```

### Get URL Details
```http
GET /api/urls/{shortCode}
```

### Get All URLs (Paginated)
```http
GET /api/urls?pageNumber=1&pageSize=10
```

### Delete URL
```http
DELETE /api/urls/{id}
```

### Redirect to Original URL
```http
GET /{shortCode}
```

## Configuration

### Connection Strings

Edit `appsettings.json` to configure database and Redis connections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=urlshortener;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  }
}
```

### Logging

Serilog is configured to log to both console and file. Logs are stored in the `logs/` directory.

## Docker Support

### Run Everything with Docker

Build and run the entire application with Docker:

```bash
docker build -t urlshortener-api .
docker run -p 5000:80 --network urlshortener-network urlshortener-api
```

### Stop Docker Containers

```bash
docker-compose down
```

Remove volumes:
```bash
docker-compose down -v
```

## Technologies Used

- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core 8.0** - ORM
- **PostgreSQL 16** - Database
- **Redis 7** - Caching
- **Serilog** - Logging
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization

## Author

Your Name - [@yourusername](https://github.com/ZuraArabidze)

## Acknowledgments

- ASP.NET Core Documentation
- Entity Framework Core Documentation
- Redis Documentation
