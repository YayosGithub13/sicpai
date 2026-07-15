Infrastructure project contains EF Core DbContext, migrations, data seeding and DI registration.

Connection string should be configured in WebUI project's appsettings.json under "ConnectionStrings:DefaultConnection".

Use the provided connection:

Server=OPDELLCDMFD10\\SQLEXPRESS;Initial Catalog=sicpaiDB;Trusted_Connection=True;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;

To add migrations use the Package Manager Console or dotnet-ef with the Infrastructure project as startup project:

cd Infrastructure

dotnet ef migrations add InitialCreate -p Infrastructure -s WebUI

dotnet ef database update -p Infrastructure -s WebUI
