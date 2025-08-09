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

// builder.Services.AddHttpClient("UserServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("https://hotel-user-api-2024-b4fmgub0hhhxegd2.canadacentral-01.azurewebsites.net/"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

// builder.Services.AddHttpClient("RoomServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("https://hotel-room-api-2024-bua5d4cseba4d8bj.canadacentral-01.azurewebsites.net"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

// builder.Services.AddHttpClient("ReservationServiceAPI", client =>
// {
//     client.BaseAddress = new Uri("https://hotel-reservation-api-2024-a5gzhsaddncnf2bv.canadacentral-01.azurewebsites.net"); 
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

builder.Services.AddHttpClient("UserServiceAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:8080"); // Local UserService
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("RoomServiceAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5013"); // Local RoomService  
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("ReservationServiceAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5253"); // Local ReservationService
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


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
