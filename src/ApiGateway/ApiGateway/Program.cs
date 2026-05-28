var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/auth/swagger/v1/swagger.json", "Auth Service");
    c.SwaggerEndpoint("/swagger/restaurant/swagger/v1/swagger.json", "Restaurant Service");
    c.SwaggerEndpoint("/swagger/order/swagger/v1/swagger.json", "Order Service");
    c.SwaggerEndpoint("/swagger/delivery/swagger/v1/swagger.json", "Delivery Service");
    c.RoutePrefix = "swagger";
});

app.MapReverseProxy();

app.Run();
