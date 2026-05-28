using Auth.Jwt;
using DeliveryService.Infrastructure.Extensions;
using DeliveryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DeliveryService API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, [] }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DeliveryDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
