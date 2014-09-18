using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EssentialCommandsPlugin.DbModels;
using SharpStar.Lib.DataTypes;

namespace EssentialCommandsPlugin.Extensions
{
    public static class DbHelper
    {

        private static Func<ProtectedPlanet, bool> _isEqualExpr;

        public static Func<ProtectedPlanet, bool> IsEqual(WorldCoordinate coordinates)
        {
            return _isEqualExpr ?? ((Expression<Func<ProtectedPlanet, bool>>)(planet => planet.Sector.Equals(coordinates.Sector) && planet.X == coordinates.X && planet.Y == coordinates.Y 
                && planet.Z == coordinates.Z && planet.Satellite == coordinates.Satellite && planet.Planet.Equals(coordinates.Planet))).Compile();
        }

    }
}
