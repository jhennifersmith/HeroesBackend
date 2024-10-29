# RpgWebApiEntityFramework
Project made following the Udemy course .NET 7 Web API & Entity Framework Jumpstart created by Patrick God

Content learned:
- Implementing a .NET Web API
	- CRUD (Create, Read, Update, Delete)
	- HTTP Methods GET, POST, PUT, DELETE
- Store data in SQL Server database
	- Entity Framework Core
	- Code-First Migration
- Token Authentication
	- JSON Web Tokens
	- Password hash & salt
- Advanced Relanshionships with EF Core
	- One-to-one, one-to-many, many-to-may
- Automatic fights & highscore

# Migrations
- dotnet build: Compila o projeto.
- dotnet run: Executa o projeto atual.
- dotnet ef migrations add <MigrationName>: Cria uma nova migration com o nome especificado.
- dotnet ef migrations remove: Remove a última migration.
- dotnet ef migrations list: Lista todas as migrations aplicadas.
- dotnet ef database update: Aplica as migrations pendentes ao banco de dados.
- dotnet ef database update <MigrationName>: Aplica as migrations até a migration especificada.