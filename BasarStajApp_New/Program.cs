using BasarStajApp_New.Interfaces;
using BasarStajApp_New.Services;
using BasarStajApp_New.Data; // ✅ EF için DbContext
using Microsoft.EntityFrameworkCore;
using BasarStajApp_New.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ADO.NET Connection String (DefaultConnection yerine PostgresConnection kullanmanı öneririm)
string connStr = builder.Configuration.GetConnectionString("PostgresConnection");
if (string.IsNullOrEmpty(connStr))
{
    throw new Exception("Connection string null veya boş! appsettings.json kontrol et.");
}

// ---------------- EF Core DbContext Kayıt ----------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresConnection"),
        x => x.UseNetTopologySuite()
    ));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// ---------------- Service Registrations ----------------
// ADO.NET Servisi
/*
builder.Services.AddScoped<IGeometryService, GeometryAdoNetService>(
    provider => new GeometryAdoNetService(connStr));
*/
// EF Core Servisi
builder.Services.AddScoped<IGeometryService, GeometryEfCoreService>();
/*
// InMemory Servisi (ileride eklenecek)
builder.Services.AddScoped<IGeometryService, GeometryService>();
*/
// --------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("Frontend");
app.MapControllers();

app.Run();
