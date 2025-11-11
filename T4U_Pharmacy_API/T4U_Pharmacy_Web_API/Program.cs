using Microsoft.EntityFrameworkCore;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Implement;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;
using T4U_Pharmacy_Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DbContext, KDAN_TESTContext>();

builder.Services.AddScoped<IGenericRepository<Customer>, GenericRepository<Customer>>();
builder.Services.AddScoped<CustomerService>();

builder.Services.AddScoped<IGenericRepository<Pharmacy>, GenericRepository<Pharmacy>>();
builder.Services.AddScoped<PharmacyService>();

builder.Services.AddScoped<DBService>();

// 資料庫連線
string connString = builder.Configuration.GetConnectionString("WebDbConnection");
builder.Services.AddDbContext<KDAN_TESTContext>(options =>
{
    options.UseNpgsql(connString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
