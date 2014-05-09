using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.DataTypes;
using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsPlanet : WorldCoordinate
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int OwnerId { get; set; }

        public EssentialCommandsPlanet()
        {
        }

        public EssentialCommandsPlanet(WorldCoordinate other)
        {
            Sector = other.Sector;
            X = other.X;
            Y = other.Y;
            Z = other.Z;
            Planet = other.Planet;
            Satellite = other.Satellite;
        }

    }
}
