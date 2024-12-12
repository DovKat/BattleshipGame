using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowHost", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000") // Replace with your frontend URL
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

app.UseEndpoints(endpoints =>
    {
        endpoints.MapHub<GameHub>("/gameHub");
    });

// Run the app
app.Run();

