using System.Collections.Generic;
using Newtonsoft.Json;

namespace Test_Taste_Console_Application.Domain.DataTransferObjects
{
    public class PlanetDto
    {
        public string Id { get; set; }
        public float SemiMajorAxis { get; set; }

        //The planet's own average temperature in Kelvin (0 when the API has no value).
        [JsonProperty("avgTemp")] public int AverageTemperature { get; set; }

        public ICollection<MoonDto> Moons { get; set; }
    }
}