using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerManagementPlugin
{
    public class A2SInfoResponse
    {

        public byte Header { get; set; }

        public byte Protocol { get; set; }

        public string Name { get; set; }

        public string Map { get; set; }

        public string Folder { get; set; }

        public string Game { get; set; }

        public short ID { get; set; }

        public byte Players { get; set; }

        public byte MaxPlayers { get; set; }

        public byte Bots { get; set; }

        public byte ServerType { get; set; }

        public byte Environment { get; set; }

        public byte Visibility { get; set; }

        public byte VAC { get; set; }

        public string Version { get; set; }

        public byte ExtraDataFlag { get; set; }

        public short Port { get; set; }

        public long SteamId { get; set; }

        public short SourceTvPort { get; set; }

        public string SourceTvName { get; set; }

        public string Keywords { get; set; }


        public long GameId { get; set; }

        public static A2SInfoResponse FromStream(Stream s)
        {
            A2SInfoResponse resp = new A2SInfoResponse();

            using (BinaryReader reader = new BinaryReader(s))
            {
                resp.Header = reader.ReadByte();
                resp.Protocol = reader.ReadByte();
                resp.Name = reader.ReadStringEx();
                resp.Map = reader.ReadStringEx();
                resp.Folder = reader.ReadStringEx();
                resp.Game = reader.ReadStringEx();
                resp.ID = reader.ReadInt16();
                resp.Players = reader.ReadByte();
                resp.MaxPlayers = reader.ReadByte();
                resp.Bots = reader.ReadByte();
                resp.ServerType = reader.ReadByte();
                resp.Environment = reader.ReadByte();
                resp.Visibility = reader.ReadByte();
                resp.VAC = reader.ReadByte();
                resp.Version = reader.ReadStringEx();
                resp.ExtraDataFlag = reader.ReadByte();

                if ((resp.ExtraDataFlag & 0x80) == 0x80)
                {
                    resp.Port = reader.ReadInt16();
                }

                if ((resp.ExtraDataFlag & 0x10) == 0x10)
                {
                    resp.SteamId = reader.ReadInt64();
                }

                if ((resp.ExtraDataFlag & 0x40) == 0x40)
                {
                    resp.SourceTvPort = reader.ReadInt16();
                }

                if ((resp.ExtraDataFlag & 0x20) == 0x20)
                {
                    resp.Keywords = reader.ReadStringEx();
                }

                if ((resp.ExtraDataFlag & 0x01) == 0x01)
                {
                    resp.GameId = reader.ReadInt64();
                }

            }

            return resp;
        }

    }
}
