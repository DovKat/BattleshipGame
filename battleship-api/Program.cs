using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowHost", policy =>
        {
            policy
                .WithOrigins("https://white-flower-0ba0ec203.4.azurestaticapps.net") // Replace with your frontend URL
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Length");
        });
    })
    .AddSignalR() //https://battleshipgamehub.service.signalr.net
    .AddAzureSignalR(options => options.ConnectionString = "Endpoint=https://secondaryhub.service.signalr.net;AccessKey=1zaJ0v67QM59KZTWfFlbkvB3xzI8OBie2ZdfU9b3bNYekGe8OAUBJQQJ99ALACi5YpzXJ3w3AAAAASRSfoko;Version=1.0;");
var app = builder.Build();

// Apply CORS policy before other middleware


app.UseHttpsRedirection();
// Ensure routing comes after CORS middleware
app.UseRouting();
app.UseCors("AllowHost");
// SignalR hub mapping
app.MapHub<GameHub>("/gameHub").RequireCors("AllowHost");

// Run the app
app.Run();

