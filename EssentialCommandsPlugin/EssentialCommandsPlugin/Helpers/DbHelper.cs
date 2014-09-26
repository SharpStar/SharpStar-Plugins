using System;
using System.Linq.Expressions;
using EssentialCommandsPlugin.DbModels;
using SharpStar.Lib.DataTypes;

namespace EssentialCommandsPlugin.Helpers
{
    public static class DbHelper
    {

        public static Func<ProtectedPlanet, bool> IsEqual(WorldCoordinate coordinates)
        {
            return (planet => planet.Sector.Equals(coordinates.Sector) && planet.X == coordinates.X && planet.Y == coordinates.Y
                && planet.Z == coordinates.Z && planet.Satellite == coordinates.Satellite && planet.Planet == coordinates.Planet);
        }

    }
}
