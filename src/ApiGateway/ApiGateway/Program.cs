using Auth.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-Id"))
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString();
    await next();
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/auth/swagger/v1/swagger.json", "Auth Service");
    c.SwaggerEndpoint("/swagger/restaurant/swagger/v1/swagger.json", "Restaurant Service");
    c.SwaggerEndpoint("/swagger/order/swagger/v1/swagger.json", "Order Service");
    c.SwaggerEndpoint("/swagger/payment/swagger/v1/swagger.json", "Payment Service");
    c.SwaggerEndpoint("/swagger/notification/swagger/v1/swagger.json", "Notification Service");
    c.SwaggerEndpoint("/swagger/delivery/swagger/v1/swagger.json", "Delivery Service");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();
