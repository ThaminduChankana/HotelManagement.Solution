using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Session;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
QuestPDF.Settings.License = LicenseType.Community;

// For accessing microservices using API Gateway
var serviceNames = new[] { "UserServiceAPI", "RoomServiceAPI", "ReservationServiceAPI" };

foreach (var serviceName in serviceNames)
{
    builder.Services.AddHttpClient(serviceName, client =>
    {
        client.BaseAddress = new Uri("http://172.171.48.15/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
}

// For direct microservice access
// builder.Services.AddHttpClient("UserServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("https://hotel-user-api-2025-ftgueta8hcavdrcj.canadacentral-01.azurewebsites.net/"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

// builder.Services.AddHttpClient("RoomServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("https://hotel-room-api-2025-bjecgbcwdjd2hfc3.canadacentral-01.azurewebsites.net/"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

// builder.Services.AddHttpClient("ReservationServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("https://hotel-reservation-api-2025-awfjesa0a5gybbap.canadacentral-01.azurewebsites.net/"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });


// For local services
// builder.Services.AddHttpClient("UserServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("http://localhost:8080"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

// builder.Services.AddHttpClient("RoomServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("http://localhost:5013"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

// builder.Services.AddHttpClient("ReservationServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("http://localhost:5258"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();