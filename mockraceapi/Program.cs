// fly deploy
// fly logs
// curl -v 'https://mockraceapi.fly.dev/middleware/info/json?setting=bebebe&course=101'

// see https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
});
builder.Services.AddCors(
    options => {
        options.AddDefaultPolicy(
            policy => {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
});
builder.Logging.AddSimpleConsole(options => {
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss "; // "yyyy-MM-dd HH:mm:ss zzz"
});

var app = builder.Build();
// common prefix
var mware = app.MapGroup("/middleware");

// curl -v https://mockraceapi.fly.dev/middleware/
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

// /info/json?setting=courses
// /info/json?setting=splits&course=101
// /info/json?setting=categories&course=101
mware.MapGet("/info/json", (string setting, int? course) => {
    app.Logger.LogInformation("info");
    return Results.Ok($"here are setting {setting} for course {course ?? 0}");
});

// /result/json?course=102&detail=start,gender,status&splitnr=101,109,119,199
mware.MapGet("/result/json", (int course, string detail, string splitnr) => {
    app.Logger.LogInformation("results");
    return Results.Ok($"here are results for {course}");
});

app.Run();

record SomeInfo(string Name, int? Value) {}
