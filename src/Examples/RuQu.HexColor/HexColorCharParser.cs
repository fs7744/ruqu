﻿using RuQu.Reader;

namespace RuQu
{
    public class HexColorCharParser : SimpleCharParserBase<(byte red, byte green, byte blue), SimpleOptions>
    {
        protected override (byte red, byte green, byte blue) ContinueRead(IReadBuffer<char> buffer, SimpleOptions options)
        {
            var bytes = buffer.Remaining;
            if (bytes.Length > 7)
            {
                throw new FormatException("Only 7 utf-8 chars");
            }

            if (!buffer.IsFinalBlock && bytes.Length < 7)
            {
                buffer.AdvanceBuffer(0);
                return default;
            }

            if (buffer.IsFinalBlock && bytes.Length < 7)
            {
                throw new FormatException("Must 7 utf-8 chars");
            }

            if (bytes[0] is not '#')
            {
                throw new FormatException("No perfix with #");
            }

            var c = new string(bytes[1..]);

            return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
        }
    }
}