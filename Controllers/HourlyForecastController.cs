using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using weather_retrieval.DataContracts;

namespace weather_retrieval.Controllers
{
    [Route("[controller]")]
    public class HourlyForecastController : Controller
    {
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get(string latitude, string longitude)
        {
          if (String.IsNullOrEmpty(latitude) || String.IsNullOrEmpty(longitude))
          {
            return NotFound("Latitude and Longitude are required: /hourlyforecast?latitude=xxxx&longitude=yyyy");
          }

          using (var client = new HttpClient())
          {
            try
            {
              client.BaseAddress = new Uri("http://api.weather.gov");
              client.DefaultRequestHeaders.Accept.Clear();
              client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
              client.DefaultRequestHeaders.Add("User-Agent", "web api client");

              var streamTask = client.GetStreamAsync($"/points/{latitude},{longitude}/forecast/hourly");

              var serializer = new DataContractJsonSerializer(typeof(HourlyWeather));

              var hourlyWeather = serializer.ReadObject(await streamTask) as HourlyWeather;

              return Ok(hourlyWeather.properties);
            }
            catch (HttpRequestException httpRequestException)
            {
              return NotFound("Could not retrieve hourly forecast");
            }
          }
        }
    }
}
