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
        return Results.Ok(
            new Dictionary<string, CourseData[]>{["Courses"] = data});
    }

    if (!course.HasValue) {
        return TypedResults.BadRequest("course parameter missing");
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
// https://mockraceapi.fly.dev/middleware/result/json?s&course=101&detail=yes&splitnr=101,109,111&count=10&detail=star,gender,first,last,status
mware.MapGet("/result/json", (int course, string detail, string splitnr, int? count) => {
    app.Logger.LogInformation($"results for {course} requested");
    var courseInfo = courses!.Find(x => x.Course.Coursenr == course.ToString());
    if (courseInfo == null) {
        return Results.NotFound($"do not know about course {course}");
    }

    if (!count.HasValue) {
        count = courseInfo.Meta.DefCount;
    }
    app.Logger.LogInformation($"will make up for {count} results");

    var detailsToInclude = detail.Split(",");
    var detailMakers = new Dictionary<string, Func<int, string>> {
        ["start"]  = n => n.ToString(),
        ["gender"] = n => n % 2 == 0 ? "M" : "W",
        ["status"] = n => "-",
        ["first"]  = n => n % 2 == 0 ? "Mr" : "Ms",
        ["last"]   = n => "Runner " + n.ToString()
    };

    var splitsToInclude = splitnr.Split(",");
    // curr split for runner n, 1..count
    // <= 0 means "not started", 0 "started" ... len(splits)-1 "finished"
    var reachedSplitCalc = new Dictionary<string, Func<int, int>> {
        // from "not started" to "finished" with equal probability
        ["random"] = n => Random.Shared.Next(-1, splitsToInclude.Length),
        // first start at minute 0 and finish at 59, the rest start every minute
        // splits are of equal length, so 1st is reached after 60/Nsplits minutes etc
        ["linear"] = n => (int)System.Math.Floor(
            (decimal)(DateTime.Now.Minute-(n-1))
            /((decimal)59/(splitsToInclude.Length-1)))
    };

    var res = Enumerable.Range(1, (int)count).Select(index => {
        // var reachedSplit = reachedSplitCalc["linear"](index);
        var reachedSplit = reachedSplitCalc[courseInfo.Meta.Schedule](index);
        var runner = courseInfo.Splits
            .Where(p => splitsToInclude.Contains(p.Splitnr))
            .Zip(Enumerable.Range(0, splitsToInclude.Length))
            .ToDictionary(
                p => p.First.Splitname + "_Time",
                p => reachedSplit >= Convert.ToUInt16(p.Second)
                    ? string.Format("00:{0:00}:00.{1}",
                        Convert.ToUInt16(p.Second),
                        index)
                    : "-");

        detailsToInclude
            .Where(d => detailMakers.ContainsKey(d))
            .ToList()
            .ForEach(d => runner.Add(d, detailMakers[d](index)));
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

record CourseMeta (
    string Schedule,
    int DefCount){}

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
    CourseMeta Meta,
    SplitData[] Splits,
    CategoryData[] Categories
) {}

