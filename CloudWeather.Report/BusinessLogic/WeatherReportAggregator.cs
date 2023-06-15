using System;
using System.Text.Json;
using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using Microsoft.Extensions.Options;

namespace CloudWeather.Report.BusinessLogic
{
    /// <summary>
    /// Aggregates data from multiple exernal sources to build a weather report
    /// </summary>
    interface IWeatherReportAggregator
    {
        /// <summary>
        /// Builds and returns a weekly weather report
        /// Persists weather report data
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public Task<WeatherReport> BuildWeeklyReport(string zip, int days);

    }
    public class WeatherReportAggregator : IWeatherReportAggregator
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherReportAggregator> _logger;
        private readonly WeatherDataConfig _weatherDataConfig;
        private readonly WeatherReportDbContext _db;

        public WeatherReportAggregator(IHttpClientFactory http,
                                       ILogger<WeatherReportAggregator> logger,
                                       IOptions<WeatherDataConfig> weatherDataConfig,
                                       WeatherReportDbContext db)
        {
            _http = http;
            _logger = logger;
            _weatherDataConfig = weatherDataConfig.Value;
            _db = db;
        }

        public async Task<WeatherReport> BuildWeeklyReport(string zip, int days)
        {
            var foundRecord = _db.WeatherReport.Where(weatherReport => weatherReport.ZipCode == zip && weatherReport.days == days).FirstOrDefault();

            if(foundRecord != null) {
                return foundRecord;
	        }
            
            var httpClient = _http.CreateClient();
            var precipData = await FetchPrecipitationData(httpClient, zip, days);
            decimal totalSnow = GetTotalSnow(precipData);
            decimal totalRain = GetTotalRain(precipData);
            _logger.LogInformation(
                $"zip: {zip} over last {days} days" +
                $"total snow: {totalSnow}, rain: {totalRain}");
            var temperatureData = await FetchTemperatureData(httpClient, zip, days);
            decimal averageHighF = GetAverageHigh(temperatureData);
            decimal averageLowF = GetAverageLow(temperatureData);
            _logger.LogInformation(
                $"zip: {zip} over last {days} days" +
                $"low temp: {averageLowF}, high temp: {averageHighF}");

            WeatherReport weatherReport =  new WeatherReport
            {
                AverageHighF = averageHighF,
                AverageLowF = averageLowF,
                SnowTotalInches = totalSnow,
                RainfallTotalInches = totalRain,
                CreatedOn = DateTime.UtcNow,
                ZipCode = zip,
                days = days
            };
            _db.Add(weatherReport);
            await _db.SaveChangesAsync();
            return weatherReport;
        }

        private decimal GetAverageLow(List<TemperatureModel> temperatureData)
        {
            return Math.Round(temperatureData.Average(el => el.TempHighF));
        }

        private decimal GetAverageHigh(List<TemperatureModel> temperatureData)
        {
            return Math.Round(temperatureData.Average(el => el.TempLowF));
        }

        private decimal GetTotalRain(List<PrecipitationModel> precipData)
        {
            return Math.Round(precipData.Where(el => el.WeatherType == "rain").Sum(el => el.AmountInches), 1);
        }

        private decimal GetTotalSnow(List<PrecipitationModel> precipData)
        {
            return Math.Round(precipData.Where(el => el.WeatherType == "snow").Sum(el => el.AmountInches), 1);
        }

        private async Task<List<PrecipitationModel>> FetchPrecipitationData(HttpClient httpClient, string zip, int days)
        {
            string endpoint = BuildPrecipitationServiceEndpoint(zip, days);
            var records = await httpClient.GetAsync(endpoint);
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var data = await records.Content.ReadFromJsonAsync<List<PrecipitationModel>>(jsonSerializerOptions);
            return data ?? new List<PrecipitationModel>();

        }

        private string BuildPrecipitationServiceEndpoint(string zip, int days)
        {
            var protocol = _weatherDataConfig.PrecipDataProtocol;
            var host = _weatherDataConfig.PrecipDataHost;
            var port = _weatherDataConfig.PrecipDataPort;
            return $"{protocol}://{host}:{port}/observation/{zip}?days={days}";
        }



        private async Task<List<TemperatureModel>> FetchTemperatureData(HttpClient httpClient, string zip, int days)
        {
            string endpoint = BuildTemperatureServiceEndpoint(zip, days);
            var records = await httpClient.GetAsync(endpoint);
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var data = await records.Content.ReadFromJsonAsync<List<TemperatureModel>>(jsonSerializerOptions);
            return data ?? new List<TemperatureModel>();

        }

        private string BuildTemperatureServiceEndpoint(string zip, int days)
        {
            string protocol = _weatherDataConfig.TempDataProtocol;
            string host = _weatherDataConfig.TempDataHost;
            string port = _weatherDataConfig.TempDataPort;

            return $"{protocol}://{host}:{port}/observation/{zip}?days={days}";
        }
    
    }
}

