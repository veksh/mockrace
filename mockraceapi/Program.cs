// fly deploy
// fly logs
// curl -v 'https://mockraceapi.fly.dev/middleware/info/json?setting=bebebe&course=101'

// see https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

using System.ComponentModel.DataAnnotations;
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
mware.MapGet("/result/json", (int course, string detail, string splitnr, int count = 3) => {
    app.Logger.LogInformation($"results for {course} requested");
    var courseInfo = courses!.Find(x => x.Course.Coursenr == course.ToString());
    if (courseInfo == null) {
        return Results.NotFound($"do not know about course {course}");
    }
    var splitsToInclude = splitnr.Split(",");
    // var res = new List<Dictionary<string, string>>{};
    // res.Add(new Dictionary<string, string>{
    //     ["pobedil"] = "krokodil"
    // });
    // var res = Enumerable.Range(1, count).Select(index =>
    //     new Dictionary<string, string>{
    //         ["pobedil"] = "krokodil"
    //     }).ToList();
    var res = Enumerable.Range(1, count).Select(index => {
        var reachedStage = Random.Shared.Next(0, splitsToInclude.Length + 1);
        var runner = courseInfo.Splits
            .Where(p => splitsToInclude.Contains(p.Splitnr))
            .ToDictionary(
                p => p.Splitname,
                p => reachedStage >= Convert.ToUInt16(p.ID) ? "00:01:02.3" : "-");
        return runner;
    }).ToList();
    return TypedResults.Ok(
        new Dictionary<string,List<Dictionary<string, string>>>{
            ["Course"] = res});
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

