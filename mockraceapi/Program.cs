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

// logging settings: moved to appsettings.json
// see https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter#set-formatter-with-configuration

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
    app.Logger.LogInformation($"info for {setting} requested");
    if (setting != "courses" && !course.HasValue) {
         return Results.BadRequest("course number must be present");
    }
    switch (setting)
    {
        case "courses":
            // return TypedResults.Ok("here is your courses list");
            var courses = new CourseInfo[] {
                new(100, "first",  "olympics"),
                new(101, "second", "olympics"),
                new(101, "third",  "olympics")
            };
            return TypedResults.Ok(
                new Dictionary<string, CourseInfo[]>{["Courses"] = courses});
        case "categories":
            return TypedResults.Ok($"here are categories for course {course}");
        case "splits":
            return TypedResults.Ok($"here are splits for course {course}");
        default:
            return TypedResults.NotFound($"do not know about {setting}");
    }
});

// /result/json?course=102&detail=start,gender,status&splitnr=101,109,119,199
mware.MapGet("/result/json", (int course, string detail, string splitnr) => {
    app.Logger.LogInformation("results");
    return Results.Ok($"here are results for {course}");
});

app.Run();

record SomeInfo(string Name, int? Value) {}

record CourseInfo(
    int    Coursenr,
    string Coursename,
    string Eventname) {}
