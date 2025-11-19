using DrOcupacional.Backend.Application;
using DrOcupacional.Backend.Infrastructure;
using DrOcupacional.Backend.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configurar Swagger
builder.Services.AddSwaggerConfiguration();

// Configurar autenticação via Token Introspection (OAuth2 RFC 7662)
builder.Services.AddIntrospectAuthentication(builder.Configuration);

// Configurar CORS
builder.Services.AddCorsConfiguration(builder.Environment);

// Adicionar camadas da aplicação
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseCorsConfiguration();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
