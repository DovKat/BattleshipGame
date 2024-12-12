var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowHost",
        policy =>
        {
            policy.WithOrigins("https://salmon-meadow-08f848403.4.azurestaticapps.net")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Length");
        });
});

builder.Services.AddSignalR();

var app = builder.Build();

// Use middleware
app.UseHttpsRedirection();
app.UseCors("AllowHost");

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next.Invoke();
});

app.MapHub<GameHub>("/gameHub");

app.Run();