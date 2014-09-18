using System;
using System.Linq.Expressions;
using EssentialCommandsPlugin.DbModels;
using SharpStar.Lib.DataTypes;

namespace EssentialCommandsPlugin.Helpers
{
    public static class DbHelper
    {

        private static Func<ProtectedPlanet, bool> _isEqualExpr;

        public static Func<ProtectedPlanet, bool> IsEqual(WorldCoordinate coordinates)
        {
            _isEqualExpr = _isEqualExpr ?? ((Expression<Func<ProtectedPlanet, bool>>)(planet => planet.Sector.Equals(coordinates.Sector) && planet.X == coordinates.X && planet.Y == coordinates.Y 
                && planet.Z == coordinates.Z && planet.Satellite == coordinates.Satellite && planet.Planet == coordinates.Planet)).Compile();

            return _isEqualExpr;
        }

    }
}
