using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Test_Taste_Console_Application.Domain.DataTransferObjects;

namespace Test_Taste_Console_Application.Domain.Objects
{
    public class Planet
    {
        public string Id { get; set; }
        public float SemiMajorAxis { get; set; }
        public ICollection<Moon> Moons { get; set; }

        //Average gravity of the moons that report one, 0 when there are none.
        public float AverageMoonGravity
        {
            get
            {
                var withGravity = Moons?.Where(m => m.Gravity > 0).ToArray() ?? Array.Empty<Moon>();
                return withGravity.Any() ? withGravity.Average(m => m.Gravity) : 0.0f;
            }
        }

        //The planet's own average temperature in Kelvin. The API has no temperature
        //for any moon, so this is used instead of a moon-based average.
        public int AverageTemperature { get; set; }

        public Planet(PlanetDto planetDto)
        {
            Id = planetDto.Id;
            SemiMajorAxis = planetDto.SemiMajorAxis;
            AverageTemperature = planetDto.AverageTemperature;
            Moons = new Collection<Moon>();
            if(planetDto.Moons != null)
            {
                foreach (MoonDto moonDto in planetDto.Moons)
                {
                    Moons.Add(new Moon(moonDto));
                }
            }
        }

        public Boolean HasMoons()
        {
            return (Moons != null && Moons.Count > 0);
        }
    }
}
