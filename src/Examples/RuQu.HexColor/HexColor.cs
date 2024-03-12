using RuQu.Reader;

namespace RuQu
{
    public static class HexColor
    {
        public static readonly HexColorStreamParser StreamParser = new HexColorStreamParser();
        public static readonly HexColorCharParser CharParser = new HexColorCharParser();
    }
}