using System;
using System.Linq;

namespace NJsonSchema.CodeGeneration.CLI.Console
{
    public static class StringExt
    {
        public static string SnakeToCamel(this string that)
        {
            return that.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                    char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
                .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }
    }
}