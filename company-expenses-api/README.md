# Company Expenses API

## Struktura projektu

Projekt je rozdělen do několika knihoven:

- **company-expenses-api** - Web API (controllers, middleware, konfigurace)
- **company-expenses-models** - Entity/modely pro databázi
- **company-expenses-database** - DbContext, konfigurace entit, migrace
- **company-expenses-services** - Business logika, repository pattern

## Databáze

Projekt používá SQL Server (stejný jako auth projekt) s databází `company_expenses`.

Connection string v `appsettings.json`:

```
Server=localhost,1433;Database=company_expenses;User Id=sa;Password=Admin123!;TrustServerCertificate=True;
```

## Jak přidat novou entitu

### 1. Vytvoř model v `company-expenses-models/Entities/`

```csharp
namespace CompanyExpenses.Models.Entities;

public class Expense : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    // Foreign keys
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

### 2. Přidej DbSet do `company-expenses-database/Data/AppDbContext.cs`

```csharp
public DbSet<Expense> Expenses => Set<Expense>();
```

### 3. Vytvoř konfiguraci entity (volitelné) v `company-expenses-database/Configurations/`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyExpenses.Models.Entities;

namespace CompanyExpenses.Database.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
    }
}
```

### 4. Vytvoř migraci

```bash
cd company-expenses-api
dotnet ef migrations add InitialCreate --project ../company-expenses-database
```

### 5. Aplikuj migraci

```bash
dotnet ef database update --project ../company-expenses-database
```

## Spuštění API

```bash
cd company-expenses-api
dotnet watch run
```

API poběží na:

- HTTP: http://localhost:5200
- HTTPS: https://localhost:7200

## Endpoints

- `GET /api/health` - Health check endpoint
- OpenAPI specifikace: `/openapi/v1.json`
