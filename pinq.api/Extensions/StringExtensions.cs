using System.Text;
using System.Text.RegularExpressions;

namespace pinq.api.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        input = Regex.Replace(input, @"[^\w\d]", "_");

        var stringBuilder = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];
            if (i > 0 && char.IsUpper(currentChar) && !char.IsUpper(input[i - 1])) 
                stringBuilder.Append('_');
            stringBuilder.Append(currentChar);
        }

        return stringBuilder.ToString().ToLowerInvariant();
    }
}
