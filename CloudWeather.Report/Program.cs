using CloudWeather.Report.BusinessLogic;
using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddTransient<IWeatherReportAggregator, WeatherReportAggregator>();
builder.Services.AddOptions();
builder.Services.Configure<WeatherDataConfig>(builder.Configuration.GetSection("WeatherDataConfig"));

builder.Services.AddDbContext<WeatherReportDbContext>(
    options =>
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));

    }, ServiceLifetime.Transient);
var app = builder.Build();

app.MapGet("/weather-report/{zip}",async (string zip, [FromQuery] int? days,IWeatherReportAggregator weatherAgg)=>{
    if(days == null || days < 1 || days > 30) {
        return Results.BadRequest("Enter a 'days' query between 1 and 30");
    }
    var response = await weatherAgg.BuildWeeklyReport(zip, days.Value);
    return Results.Ok(response);
});

app.Run();