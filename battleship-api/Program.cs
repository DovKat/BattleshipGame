using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowHost",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => 
                                       new Uri(origin).Host == "https://salmon-meadow-08f848403.4.azurestaticapps.net/")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


builder.Services.AddSignalR();

var app = builder.Build();


app.UseCors("AllowHost"); 

app.UseHttpsRedirection();
app.UseRouting();


app.MapHub<GameHub>("/gameHub");

app.Run();
