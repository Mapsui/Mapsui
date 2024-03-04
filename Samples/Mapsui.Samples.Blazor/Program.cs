#pragma warning disable IDE0005 // Using directive is unnecessary. The dotnet format on the build server is just wrong
using Mapsui.Samples.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

Mapsui.Logging.Logger.LogDelegate += (l, m, e) => Console.WriteLine(m + e?.Message);

await builder.Build().RunAsync();
