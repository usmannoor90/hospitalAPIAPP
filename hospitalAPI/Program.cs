using hospitalAPI.EFData;
using hospitalAPI.Models;
using hospitalAPI.StartupFile;
using hospitalAPI.StartupServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddDbContext<EFDateContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("defult")!);
});

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();


builder.Services.AddAuthService(builder.Configuration);




builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerService();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {

        c.SwaggerEndpoint("/swagger/clients/swagger.json", "🧍 Clients API");
        c.SwaggerEndpoint("/swagger/doctors/swagger.json", "👨‍⚕️ Doctors API");
        c.SwaggerEndpoint("/swagger/nurses/swagger.json", "👩‍⚕️ Nurses API");
        c.SwaggerEndpoint("/swagger/admin/swagger.json", "🔐 Admin API");

        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
