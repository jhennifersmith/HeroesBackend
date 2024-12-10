# Heroes Backend
This project is built with .NET 7.0. Follow the instructions below to set up and run the application locally.

## Prerequisites
Ensure you have the following tools installed on your machine:

.NET SDK: version 7.0 or later. You can download it from Microsoft's official website.
Visual Studio (optional): version 2022 or later, or another code editor of your choice.
SQL Server or any database required by the project.

## Setting Up the Project

### 1. Clone the repository:
```bash
git clone <REPOSITORY_URL>
```
```bash
cd <PROJECT_NAME>
```

### 2. Restore dependencies:
```bash
dotnet restore
```

### 3. Configure your local database on appsettings.json

### 4. Apply database migrations (if applicable):
```bash
dotnet ef database update
```
Ensure the connection string in the appsettings.json file is configured for your local environment.

### 5. Run the application:
```bash
dotnet run
```

# Useful Commands
- dotnet build: Compiles the project.
- dotnet run: Runs the current project.
- dotnet ef migrations add <MigrationName>: Creates a new migration with the specified name.
- dotnet ef migrations remove: Removes the last migration.
- dotnet ef migrations list: Lists all applied migrations.
- dotnet ef database update: Applies pending migrations to the database.
- dotnet ef database update <MigrationName>: Applies migrations up to the specified migration.