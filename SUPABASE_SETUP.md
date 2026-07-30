# Supabase Database Setup

## Prerequisites
- Supabase account (free at https://supabase.com)
- .NET 8 SDK installed locally

## Step 1: Create Supabase Project

1. Go to https://supabase.com and sign in/create account
2. Click **New Project**
3. Fill in project details:
   - **Name**: `blog-platform` (or your preference)
   - **Database Password**: Create a strong password (save this!)
   - **Region**: Choose closest to your location
4. Click **Create new project** and wait for initialization (~3-5 minutes)

## Step 2: Get Connection String

1. In Supabase dashboard, go to **Project Settings** → **Database**
2. Under **Connection string**, select **Connection pooling**
3. Copy the PostgreSQL connection string
4. Format: `postgresql://postgres:[PASSWORD]@[HOST]:[PORT]/[DATABASE]?sslmode=require`

## Step 3: Update Configuration

Replace the connection string in `backend/BlogAPI.API/appsettings.Development.json`:

```json
"DefaultConnection": "postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres?sslmode=require"
```

## Step 4: Run Migrations

From the `backend/BlogAPI.API` directory:

```bash
dotnet ef database update
```

This will create all tables defined in `BlogContext`.

## Connection Details Reference

Your Supabase connection details can always be found at:
- **Host**: `db.[PROJECT-REF].supabase.co`
- **Port**: `5432`
- **Database**: `postgres`
- **User**: `postgres`
- **Password**: Your chosen password during setup

## Verify Connection

Test the connection with:

```bash
psql postgresql://postgres:[PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres
```

## Troubleshooting

**"Connection refused"**
- Verify Supabase project is running (check dashboard)
- Check firewall/network allowing port 5432

**"SSL connection error"**
- Ensure `sslmode=require` is in connection string
- Update `Microsoft.EntityFrameworkCore.Npgsql` to latest version

**"Authentication failed"**
- Double-check password and username in connection string
- Reset password in Supabase if needed

## Next Steps

Once connected and migrated:
1. Run the backend: `dotnet run`
2. Install frontend dependencies: `npm install`
3. Run frontend: `ng serve`
