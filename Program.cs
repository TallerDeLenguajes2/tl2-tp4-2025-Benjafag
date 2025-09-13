var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); // AGREGAR para usar swagger
builder.Services.AddSwaggerGen(); // AGREGAR para usar swagger

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger(); // AGREGAR para usar swagger
  app.UseSwaggerUI(); // AGREGAR para usar swagger
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

