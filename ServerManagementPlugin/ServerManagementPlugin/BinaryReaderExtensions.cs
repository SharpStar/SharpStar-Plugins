using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerManagementPlugin
{
    public static class BinaryReaderExtensions
    {

        public static string ReadStringEx(this BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();

            char character;

            while ((character = reader.ReadChar()) != '\0')
            {
                sb.Append(character);
            }

            return sb.ToString();
        }

    }
}
