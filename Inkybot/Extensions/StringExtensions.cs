using System;
using System.Linq;

namespace Inkybot.Extensions
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => input,
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };
    }
}
