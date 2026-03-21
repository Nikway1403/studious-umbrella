using Gateway.Api.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("Gateway", new OpenApiInfo
    {
        Title = "Gateway",
        Version = "v1"
    });

    c.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));
});

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();