// see https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

// var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // also: ASPNETCORE_APPLICATIONNAME	--applicationName
    // ApplicationName = typeof(Program).Assembly.FullName,
    // also: ASPNETCORE_ENVIRONMENT	--environment
    // EnvironmentName = Environments.Staging,
    // and then Console.WriteLine($"Environment Name: {builder.Environment.EnvironmentName}");
    WebRootPath = "middleware"
    // config: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-8.0#read-configuration
});

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
});

var app = builder.Build();
// common prefix
var mware = app.MapGroup("/middleware");

mware.MapGet("/", () =>
{
    app.Logger.LogInformation("reached it");
    var ans = Enumerable.Range(1, 5).Select(index =>
        new SomeInfo (
            "here",
            Random.Shared.Next(-100, 100)
        ))
        .ToArray();
    return ans;
});

app.Run();

record SomeInfo(string Name, int? Value) {}
