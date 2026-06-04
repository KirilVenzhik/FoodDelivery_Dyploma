using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using FoodDelivery.Web;
using FoodDelivery.Web.Auth;
using FoodDelivery.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();

// Auth state
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

// API clients
builder.Services.AddScoped<IAuthService, AuthService>();
// Role API clients are registered by their feature sessions (Customer/Owner/Courier).
// >>> Pending registrations from parallel sessions go here in Phase 3. <<<

// Named HttpClient pointing at the API Gateway
builder.Services.AddHttpClient("gateway", c =>
    c.BaseAddress = new Uri("http://localhost:5000"));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("gateway"));

await builder.Build().RunAsync();
