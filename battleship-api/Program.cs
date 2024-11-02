using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") 
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


builder.Services.AddSignalR();

var app = builder.Build();


app.UseCors("AllowLocalhost"); 

app.Use(async (context, next) =>
{

    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next.Invoke();
});


app.MapHub<GameHub>("/gameHub");

app.Run();
