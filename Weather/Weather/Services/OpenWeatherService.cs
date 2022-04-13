using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json; //Requires nuget package System.Net.Http.Json
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text.Json;

using Weather.Models;

namespace Weather.Services
{
    //You replace this class witth your own Service from Project Part A
    public class OpenWeatherService
    {
        HttpClient httpClient = new HttpClient();
        readonly string apiKey = "55c5e954856a7a98abf49f16193e4d0d"; // Your API Key

        // part of your event and cache code here
        public event EventHandler<string> WeatherForecastAvailable;


        protected virtual void OnFcAvail(Forecast e)
        {
            //WeatherForecastAvailable?.Invoke(this, e.City);
            if (WeatherForecastAvailable != null) return;
            WeatherForecastAvailable(this, string.Empty);

        }
        ConcurrentDictionary<(string, string), Forecast> cacheDictCity = new ConcurrentDictionary<(string, string), Forecast>();
        ConcurrentDictionary<(double, double, string), Forecast> cacheDictGeo = new ConcurrentDictionary<(double, double, string), Forecast>();





        public async Task<Forecast> GetForecastAsync(string City)
        {
            //part of cache code here
            string keyCity = City;
            string keyDate = DateTime.Now.ToString("yyyy-MM-dd:HH:mm");
            var key = (keyCity, keyDate);

            Forecast forecast;

            //It did not exist in the cache, calculate and add it to the cache
            if (!cacheDictCity.TryGetValue(key, out forecast))
            {
                forecast = new Forecast();


                //https://openweathermap.org/current
                var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={City}&units=metric&lang={language}&appid={apiKey}";


                forecast = await ReadWebApiAsync(uri);

                cacheDictCity[key] = forecast;
                // WeatherForecastAvailable?.Invoke(forecast, $"New forecast for {City} available");
            }


            //part of event and cache code here


            //generate an event with different message if cached data
            //else
            //{
            //    WeatherForecastAvailable?.Invoke(forecast, $"CACHED forecast for {City} available");
            //}

            return forecast;



        }
        public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
        {
            //part of cache code here
            double keyLat = latitude;
            double keyLon = longitude;
            string keyDate = DateTime.Now.ToString("yyyy-MM-dd:HH:mm");
            var key = (keyLat, keyLon, keyDate);

            Forecast forecast;

            if (!cacheDictGeo.TryGetValue(key, out forecast))
            {
                forecast = new Forecast();

                //https://openweathermap.org/current
                var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";

                forecast = await ReadWebApiAsync(uri);

                cacheDictGeo[key] = forecast;
                WeatherForecastAvailable?.Invoke(forecast, $"New forecast for {latitude}, {longitude} available");
            }

            //part of event and cache code here

            else
                WeatherForecastAvailable?.Invoke(forecast, $"CACHED forecast for {latitude}, {longitude} available");

            //generate an event with different message if cached data


            return forecast;
        }
        private async Task<Forecast> ReadWebApiAsync(string uri)
        {
            // part of your read web api code here
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            WeatherApiData wd = await response.Content.ReadFromJsonAsync<WeatherApiData>();


            // part of your data transformation to Forecast here


            Forecast forecast = new Forecast()
            {
                City = wd.city.name

            };

            forecast.Items = wd.list.Select(x => new ForecastItem
            {
                WindSpeed = x.wind.speed,
                Temperature = x.main.temp,
                DateTime = UnixTimeStampToDateTime(x.dt),
                Icon = $"https://openweathermap.org/img/w/{x.weather[0].icon}.png",
                Description = x.weather[0].description
            }).ToList();



            return forecast;
        }
            private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
