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

        //Average temperature (Kelvin) of the moons that report one. Moons without a
        //reading are skipped so a single 0 doesn't pull the average down.
        public double AverageMoonTemperature
        {
            get
            {
                var withTemp = Moons?.Where(m => m.AverageTemperature > 0).ToArray() ?? Array.Empty<Moon>();
                return withTemp.Any() ? Math.Round(withTemp.Average(m => (double)m.AverageTemperature), 2) : 0.0d;
            }
        }

        public Planet(PlanetDto planetDto)
        {
            Id = planetDto.Id;
            SemiMajorAxis = planetDto.SemiMajorAxis;
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
