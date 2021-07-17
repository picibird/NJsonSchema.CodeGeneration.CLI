using System.Text.RegularExpressions;

namespace NJsonSchema.CodeGeneration.CLI.Console
{
    internal class AgodaEnumNameGenerator : IEnumNameGenerator
    {

        private readonly static Regex _invalidNameCharactersPattern = new Regex(@"[^\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]");

        public string Generate(int index, string name, object value, JsonSchema schema)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "Empty";
            }

            switch (name)
            {
                case ("="):
                    name = "Eq";
                    break;
                case ("!="):
                    name = "Ne";
                    break;
                case (">"):
                    name = "Gt";
                    break;
                case ("<"):
                    name = "Lt";
                    break;
                case (">="):
                    name = "Ge";
                    break;
                case ("<="):
                    name = "Le";
                    break;
                case ("~="):
                    name = "Approx";
                    break;
            }

            if (name.StartsWith("-"))
            {
                name = "Minus" + name.Substring(1);
            }

            if (name.StartsWith("+"))
            {
                name = "Plus" + name.Substring(1);
            }

            if (name.StartsWith("_-"))
            {
                name = "__" + name.Substring(2);
            }

            return _invalidNameCharactersPattern.Replace(ConversionUtilities.ConvertToUpperCamelCase(name
                    .Replace(":", "-").Replace(@"""", @""), true), "_")
                .SnakeToCamel();
        }
    }
}