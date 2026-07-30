# Database Migrations & Deployment

## Creating Migrations

After updating your entity models, create a migration:

```bash
cd backend/BlogAPI.API
dotnet ef migrations add MigrationName --project ../BlogAPI.Data --startup-project .
```

## Applying Migrations

Update the database with the latest migration:

```bash
cd backend/BlogAPI.API
dotnet ef database update
```

## Initial Setup (First Time)

1. **Install EF Core CLI** (if not already installed):
```bash
dotnet tool install --global dotnet-ef
```

2. **Create Initial Migration**:
```bash
cd backend/BlogAPI.API
dotnet ef migrations add InitialCreate --project ../BlogAPI.Data --startup-project .
```

3. **Apply Migration to Supabase**:
Update `appsettings.Development.json` with your Supabase connection string, then:
```bash
dotnet ef database update
```

## Verifying Migrations

Check applied migrations:
```bash
dotnet ef migrations list
```

View migration script without applying:
```bash
dotnet ef migrations script --idempotent > migration.sql
```

## Reverting Migrations

Remove the latest migration:
```bash
dotnet ef migrations remove
```

Revert database to specific migration:
```bash
dotnet ef database update NameOfMigration
```

## Migration Files Generated

- `InitialCreate.cs` - Migration definition with Up() and Down() methods
- `InitialCreate.Designer.cs` - Designer metadata
- `BlogContextModelSnapshot.cs` - Current model snapshot

## Common Commands

| Command | Purpose |
|---------|---------|
| `dotnet ef migrations add CreateUsersTable` | Create new migration |
| `dotnet ef database update` | Apply all pending migrations |
| `dotnet ef database update InitialCreate` | Revert to specific migration |
| `dotnet ef migrations remove` | Remove last migration (before push) |
| `dotnet ef migrations list` | List all migrations |

## Troubleshooting

**"Unable to find project"**
- Ensure you're in `backend/BlogAPI.API` directory
- Use `--project` and `--startup-project` flags correctly

**"Could not connect to Supabase"**
- Verify connection string in `appsettings.Development.json`
- Check Supabase project is active
- Verify firewall allows port 5432

**"The target database is not yet created"**
- Run `dotnet ef database update` to create schema

**Need to make changes to migration**
- If not pushed: `dotnet ef migrations remove`, then modify models and create new migration
- If already pushed: Create a new migration with the changes
