using BlogAPI.API;

var builder = WebApplication.CreateBuilder(args);

// Render (and similar PaaS hosts) assign the listen port via $PORT at runtime.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services using Startup class pattern
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline
startup.Configure(app, app.Environment);

app.Run();
