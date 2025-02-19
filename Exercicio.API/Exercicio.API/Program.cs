using Exercicio.API.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner.
builder.Services.AddControllers();

// Configurar o DbContext com a connection string
builder.Services.AddDbContext<AplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var supportedCultures = new[] { "pt-BR", "en-US" };
var localizationOpions = new RequestLocalizationOptions()
    .SetDefaultCulture("pt-BR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() 
              .AllowAnyMethod()
              .AllowAnyHeader(); 
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLocalization(localizationOpions);
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();