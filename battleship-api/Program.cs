using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowHost", policy =>
        {
            policy
                .WithOrigins("https://salmon-meadow-08f848403.4.azurestaticapps.net") // Replace with your frontend URL
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Length");
        });
    })
    .AddSignalR() //https://battleshipgamehub.service.signalr.net
    .AddAzureSignalR(options => options.ConnectionString = "Endpoint=https://battleshipgamehub.service.signalr.net;AccessKey=FOZrHncHR81uX9kwxjAo3BRPxw0RaG67Q3A3R6wancwYbcHJwOiEJQQJ99ALACi5YpzXJ3w3AAAAASRS81Y0;Version=1.0;");
var app = builder.Build();

// Apply CORS policy before other middleware
app.UseCors("AllowHost");

app.UseHttpsRedirection();
// Ensure routing comes after CORS middleware
app.UseRouting();

// SignalR hub mapping
app.MapHub<GameHub>("/gameHub").RequireCors("AllowHost");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<GameHub>("/gameHub");
});
// Run the app
app.Run();

