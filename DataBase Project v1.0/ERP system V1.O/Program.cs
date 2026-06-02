using WarqERP.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();

Database.CreateDatabase();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
