using Auth;
using Auth.Jwt;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Observability.Extensions;
using Observability.Logging;
using RestaurantService.API.Middlewares;
using RestaurantService.Application.Validators;
using RestaurantService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RestaurantService API", Version = "v1" });

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
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, [] } });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateRestaurantRequestValidator>();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthServices();

builder.Services.AddCorrelation();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<RestaurantDbContext>().Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCorrelation();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
