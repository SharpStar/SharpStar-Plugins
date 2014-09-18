using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using SharpStar.Lib.DataTypes;

namespace EssentialCommandsPlugin.Extensions
{
    public static class DbExtensions
    {

        public static WorldCoordinate ToWorldCoordinate(this ProtectedPlanet planet)
        {
            return new WorldCoordinate
            {
                Planet = planet.Planet,
                Satellite = planet.Satellite,
                Sector = planet.Sector,
                X = planet.X,
                Y = planet.Y,
                Z = planet.Z
            };
        }

    }
}
