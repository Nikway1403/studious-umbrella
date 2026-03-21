using VoiceService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<RoomManager>();

var app = builder.Build();

app.MapHub<VoiceHub>("/voice");

app.Run();