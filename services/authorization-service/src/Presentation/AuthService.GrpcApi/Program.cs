using AuthService.GrpcApi.Controllers;
using AuthService.GrpcApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.MapGrpcService<AuthorizationGrpcService>();

app.Run();