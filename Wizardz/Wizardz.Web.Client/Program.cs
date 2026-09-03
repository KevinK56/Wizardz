using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Wizardz.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddWizardzGame();

await builder.Build().RunAsync();
