using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using ReservationService.Data;
using ReservationService.Repositories;
using ReservationService.Services;
using System.Net.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with Azure SQL
builder.Services.AddDbContext<ReservationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(120);
    }));

// Configure HTTP Client for RoomService 
builder.Services.AddHttpClient<IRoomService, RoomService>(client =>
{
    var roomServiceUrl = builder.Configuration["RoomServiceUrl"] ?? "https://hotel-room-api-2024-bua5d4cseba4d8bj.canadacentral-01.azurewebsites.net";

    if (roomServiceUrl.Contains("localhost") && roomServiceUrl.StartsWith("https"))
    {
        roomServiceUrl = roomServiceUrl.Replace("https://", "http://");
    }

    client.BaseAddress = new Uri(roomServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "ReservationService/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // Disable SSL validation entirely for development
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
    }

    return handler;
})
.AddPolicyHandler(GetRetryPolicy());

// Add services
builder.Services.AddScoped<IReservationService, ReservationService.Services.ReservationService>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Test database connection on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReservationContext>();
    try
    {
        await context.Database.CanConnectAsync();
        Console.WriteLine("Database connection successful");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}");
    }
}

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        );
}

