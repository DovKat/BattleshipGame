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
    .AddSignalR();

var app = builder.Build();

// Apply CORS policy before other middleware
app.UseCors("AllowHost");

app.UseHttpsRedirection();
// Ensure routing comes after CORS middleware
app.UseRouting();

// SignalR hub mapping
app.MapHub<GameHub>("/gameHub").RequireCors("AllowHost");

// Run the app
app.Run();

