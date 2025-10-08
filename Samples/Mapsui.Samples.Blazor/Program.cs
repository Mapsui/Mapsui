#pragma warning disable IDE0005 // Using directive is unnecessary. The dotnet format on the build server is just wrong
using Mapsui.Samples.Blazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Where(m => m != "text/html"); // Exclude HTML files from compression
});

Mapsui.Logging.Logger.LogDelegate += (l, m, e) => Console.WriteLine(m + e?.Message);

var app = builder.Build();

// Only keep the code that is valid for Blazor WebAssembly.
await app.RunAsync();
