using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Redis.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class WeatherForecastController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<WeatherForecastController> _logger;
        public WeatherForecastController(IDistributedCache distributedCache, ILogger<WeatherForecastController> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        private static readonly string[] Summaries = new[]
        { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {

            var cacheKey = "WeatherForecastList";
            var serializedData = await _distributedCache.GetStringAsync(cacheKey);

            if (serializedData != null) //eðer rediste veri varsa  
            { 
                return JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(serializedData);
            }
            else // rediste veri yoksa olustur ve kaydet.
            {
                var weatherForeacasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                }).ToArray();


                serializedData = JsonConvert.SerializeObject(weatherForeacasts);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(10)).SetSlidingExpiration(TimeSpan.FromMinutes(2));
                //SetAbsoluteExpiration : Önbellekte depolanan bir nesnenin belirli bir süre sonra otomatik olarak önbellekten kaldýrýlmasýný saðla
                //SetSlidingExpiration: Önbellekte depolanan bir nesnenin süresini güncellemek için kullanýlan bir özelliktir.
                await _distributedCache.SetStringAsync(cacheKey, serializedData,options);
            
                return weatherForeacasts;
            }

          
        }
    }
}