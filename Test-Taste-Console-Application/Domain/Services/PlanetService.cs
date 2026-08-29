using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class PlanetService : IPlanetService
    {
        private readonly HttpClientService _httpClientService;

        public PlanetService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public IEnumerable<Planet> GetAllPlanets()
        {
            var allPlanetsWithTheirMoons = new Collection<Planet>();

            var response = _httpClientService.Client
                .GetAsync(UriPath.GetAllPlanetsWithMoonsQueryParameters)
                .Result;

            //If the status code isn't 200-299, then the function returns an empty collection.
            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                return allPlanetsWithTheirMoons;
            }

            var content = response.Content.ReadAsStringAsync().Result;

            //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
            var results = JsonConvert.DeserializeObject<JsonResult<PlanetDto>>(content);

            //The JSON converter can return a null object. 
            if (results == null) return allPlanetsWithTheirMoons;

            //Each planet only references its moons, so every moon is fetched on its own
            //to get the detailed data (mass, gravity, temperature).
            foreach (var planet in results.Bodies)
            {
                if(planet.Moons != null)
                {
                    var newMoonsCollection = new Collection<MoonDto>();
                    foreach (var moon in planet.Moons)
                    {
                        try
                        {
                            var moonResponse = _httpClientService.Client
                                .GetAsync(UriPath.GetMoonByIdQueryParameters + moon.URLId)
                                .Result;

                            if (!moonResponse.IsSuccessStatusCode)
                            {
                                Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{moonResponse.StatusCode} ({moon.URLId})");
                                continue;
                            }

                            var moonContent = moonResponse.Content.ReadAsStringAsync().Result;
                            var moonDto = JsonConvert.DeserializeObject<MoonDto>(moonContent);
                            if (moonDto != null) newMoonsCollection.Add(moonDto);
                        }
                        catch (Exception exception)
                        {
                            //One bad moon shouldn't stop the rest.
                            Logger.Instance.Error($"Failed to load moon '{moon.URLId}': {exception.Message}");
                        }
                    }
                    planet.Moons = newMoonsCollection;

                }
                allPlanetsWithTheirMoons.Add(new Planet(planet));
            }

            return allPlanetsWithTheirMoons;
        }
    }
}
