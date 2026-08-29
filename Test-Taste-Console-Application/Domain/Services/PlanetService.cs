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

            //The planet payload only references its moons. Fetch every moon's detail
            //in a single call and index it by id, instead of one request per moon.
            var moonDetailsById = GetAllMoonDetailsById();

            foreach (var planet in results.Bodies)
            {
                if(planet.Moons != null)
                {
                    var detailedMoons = new Collection<MoonDto>();
                    foreach (var moonReference in planet.Moons)
                    {
                        //Use the detailed moon when we have it, otherwise keep the bare reference.
                        detailedMoons.Add(
                            moonDetailsById.TryGetValue(moonReference.URLId, out var details)
                                ? details
                                : moonReference);
                    }
                    planet.Moons = detailedMoons;
                }
                allPlanetsWithTheirMoons.Add(new Planet(planet));
            }

            return allPlanetsWithTheirMoons;
        }

        //Loads all moons in one request, keyed by their id (matches a planet moon's URLId).
        private IDictionary<string, MoonDto> GetAllMoonDetailsById()
        {
            var moonsById = new Dictionary<string, MoonDto>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var response = _httpClientService.Client
                    .GetAsync(UriPath.GetAllMoonsWithDetailsQueryParameters)
                    .Result;

                if (!response.IsSuccessStatusCode)
                {
                    Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode} (moons)");
                    return moonsById;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                var results = JsonConvert.DeserializeObject<JsonResult<MoonDto>>(content);
                if (results?.Bodies == null) return moonsById;

                foreach (var moon in results.Bodies)
                {
                    if (!string.IsNullOrEmpty(moon.Id))
                    {
                        moonsById[moon.Id] = moon;
                    }
                }
            }
            catch (Exception exception)
            {
                //Without moon details the averages fall back to 0; that's better than failing.
                Logger.Instance.Error($"Failed to load moon details: {exception.Message}");
            }

            return moonsById;
        }
    }
}
