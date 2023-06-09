using System.Net.Http.Json;
using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json")
    .AddEnvironmentVariables()
    .Build();

var servicesConfig = config.GetSection("Services");

var tempServiceConfig = servicesConfig.GetSection("Temperature");
string tempServiceHost = tempServiceConfig["Host"]!;
string tempServicePort = tempServiceConfig["Port"]!;

var precipServiceConfig = servicesConfig.GetSection("Precipitation");
string precipHost = precipServiceConfig["Host"]!;
string precipPort = precipServiceConfig["Port"]!;

List<string> postalcodes = new List<string>
{
    "T2V0P7",
    "S7N4W9",
    "S7S2W0"
};

Console.WriteLine("Starting Data Load");

var temperatureHttpClient = new HttpClient();
temperatureHttpClient.BaseAddress = new Uri($"http://{tempServiceHost}:{tempServicePort}");

var precipationHttpClient = new HttpClient();
precipationHttpClient.BaseAddress = new Uri($"http://{precipHost}:{precipPort}");

foreach (var postalcode in postalcodes)
{
    Console.WriteLine($"Loading data for {postalcode}");
    var days = Enumerable.Range(0, 30).Select(offset => DateTime.Now.AddDays(offset * -1)).ToList();
    foreach (var day in days)
    {
        var temps = PostTemp(postalcode, day);
        PostPrecip(temps[0], postalcode, day);
    }
}

Console.WriteLine("Data loading complete");

void PostPrecip(int lowTemp, string postalcode, DateTime day)
{
    var isPrecip = new Random().Next(2) < 1;
    PrecipitationModel precipitation;
    if (isPrecip) {
        var precipAmount = new Random().Next(1, 16);
        precipitation = new PrecipitationModel
        {
            ZipCode = postalcode,
            AmountInches = precipAmount,
            CreatedOn = day,
            WeatherType = lowTemp < 32 ? "snow" : "rain"
        };
    }
    else {
        precipitation = new PrecipitationModel
        {
            CreatedOn = day,
            AmountInches = 0,
            WeatherType = "none",
            ZipCode = postalcode
        };
    }

    var result = precipationHttpClient.PostAsJsonAsync("observation", precipitation).Result;
    if (result.IsSuccessStatusCode) {
        Console.WriteLine($"Posted precipitation for {day.Date}");
    }else {
        Console.WriteLine(result.ToString());
    }
}

List<int> PostTemp(string postalcode, DateTime day)
{
    var t1 = new Random().Next(0, 100);
    var t2 = new Random().Next(0, 100);
    var tempList = new List<int>{ t1, t2 };
    tempList.Sort();
    TemperatureModel temperature = new TemperatureModel
    {
        CreatedOn = day,
        ZipCode = postalcode,
        TempLowF = tempList[0],
        TempHighF = tempList[1]
    };

    var result = temperatureHttpClient.PostAsJsonAsync("observation",temperature).Result;
    if (result.IsSuccessStatusCode) {
        Console.WriteLine($"Posted temperature for {day.Date}");
    }
    else {
        result.ToString();
    }
    return tempList;
}