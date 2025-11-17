using Microsoft.EntityFrameworkCore;
using System.Reflection;
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
builder.Services.AddSwaggerGen(c =>
{
    // 取得 XML 文件路徑
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // 告訴 Swagger 要載入 XML 註解
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // 再加上共用 Model 專案的 XML
    var xmlFileModel = "T4U_Pharmacy_Common_Model.xml";
    var xmlPathModel = Path.Combine(AppContext.BaseDirectory, xmlFileModel);
    if (File.Exists(xmlPathModel))
    {
        c.IncludeXmlComments(xmlPathModel);
    }
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DbContext, KDAN_TESTContext>();

builder.Services.AddScoped<IGenericRepository<Customer>, GenericRepository<Customer>>();
builder.Services.AddScoped<CustomerService>();

builder.Services.AddScoped<IGenericRepository<Pharmacy>, GenericRepository<Pharmacy>>();
builder.Services.AddScoped<PharmacyService>();

builder.Services.AddScoped<IGenericRepository<PharmacyMask>, GenericRepository<PharmacyMask>>();
builder.Services.AddScoped<PharmacyMasksService>();

builder.Services.AddScoped<IGenericRepository<PurchaseHistory>, GenericRepository<PurchaseHistory>>();
builder.Services.AddScoped<PurchaseHistoryService>();

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
