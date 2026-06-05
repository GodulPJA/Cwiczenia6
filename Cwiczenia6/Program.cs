using Cwiczenia6.Data;
using Microsoft.EntityFrameworkCore;
using Cwiczenia6.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddDbContext<_2019sbdContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PJATK"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();