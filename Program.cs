using System.Text.Json;
using Biblioteca;
var builder = WebApplication.CreateBuilder(args);

var datos = new AccesoADatosJSON();
Cadeteria cadeteria = datos.CargarCadeteria("./cadeteria.json", "./cadetes.json", "./pedidos.json");
builder.Services.AddSingleton(cadeteria);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

File.WriteAllText("./cadeteriaSerializada.json", JsonSerializer.Serialize(cadeteria));