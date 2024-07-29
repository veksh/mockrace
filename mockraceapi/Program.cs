// fly deploy
// fly logs
// curl -v 'https://mockraceapi.fly.dev/middleware/info/json?setting=bebebe&course=101'

// see https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
    options.SerializerOptions.PropertyNamingPolicy = null;
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

// read common data
var coursesFileName = "data/courses.json";
var courses = new List<CourseInfo>{};
try {
    var coursesStr = new StreamReader($"{coursesFileName}").ReadToEnd();
    courses = JsonSerializer.Deserialize<List<CourseInfo>>(coursesStr);
} catch (IOException e) {
    app.Logger.LogError(e, "could not open file {fileName}", coursesFileName);
}
// Console.WriteLine(JsonSerializer.Serialize(courses!.First().Course));

// common prefix
var mware = app.MapGroup("/middleware");

// curl -v https://mockraceapi.fly.dev/middleware/
// /info/json?setting=courses
// /info/json?setting=splits&course=101
// /info/json?setting=categories&course=101
mware.MapGet("/info/json", (string setting, int? course) => {
    app.Logger.LogInformation($"info for {setting} requested");
    if (setting == "courses") {
        var data = courses!.Select(x => x.Course).ToArray();
        return TypedResults.Ok(
            new Dictionary<string, CourseData[]>{["Courses"] = data});
    }
    if (!course.HasValue) {
        return Results.BadRequest("course number must be present");
    }
    var courseInfo = courses!.Find(x => x.Course.Coursenr == course.ToString());
    if (courseInfo == null) {
        return TypedResults.NotFound($"do not know about course {course}");
    }
    switch (setting)
    {
        case "categories":
            return TypedResults.Ok(
                new Dictionary<string, CategoryData[]>{
                    ["Categories"] = courseInfo.Categories});
        case "splits":
            return TypedResults.Ok(
                new Dictionary<string, SplitData[]>{
                    ["Splits"] = courseInfo.Splits});
        default:
            return TypedResults.NotFound($"do not know about {setting}");
    }
});

// /result/json?course=102&detail=start,gender,status&splitnr=101,109,119,199
mware.MapGet("/result/json", (int course, string detail, string splitnr) => {
    app.Logger.LogInformation("results");
    return Results.Ok($"results for {course} will be here");
});

app.Run();

record CourseData (
    string Coursenr,
    string Coursename,
    string Event,
    string Eventname,
    string Status,
    string Timeoffset,
    string Ordering,
    string Remark) {}

record SplitData (
    string Splitnr,
    string Splitname,
    string ID,
    string State,
    string ToD) {}

record CategoryData (
    string Category) {}

record CourseInfo (
    CourseData Course,
    SplitData[] Splits,
    CategoryData[] Categories
) {}

