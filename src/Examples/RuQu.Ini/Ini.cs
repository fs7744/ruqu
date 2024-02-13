//using System.Xml;
//using System.Xml.Linq;

//namespace RuQu
//{
//    public class Ini
//    {
//        private static Ini instance = new Ini();

//        public static IDictionary<string, string> Parse(string content)
//        {
//            return instance.ParseString(content);
//        }

//        public IDictionary<string, string> ParseString(string content)
//        {
//            var input = Input.From(content);
//            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
//            while (!input.IsEof)
//            {
//                if (!(Chars.IngoreWhiteSpace(input) || Comment(input) || Section(input, dict)))
//                {
//                    throw new NotSupportedException(input.Current.ToString());
//                }
//            }
//            return dict;
//        }

//        public bool Section(InputString input, Dictionary<string, string> dict)
//        {
//            throw new NotImplementedException();
//        }

//        public Predicate<IInput<char>> Comment = Chars.In(";#/").Then(Chars.Any.RepeatUntil(Chars.Is('\n')).Ingore().Then(Chars.Is('\n').Then(i => i.MoveNext())));
//    }
//}