using System.Text.RegularExpressions;

namespace RogueSharp.Helpers;

internal static class PrintfHelper
{
    /// <summary>
    /// Converts a printf-style format string to a .NET-style format string
    /// </summary>
    /// <param name="printf">The printf-style format string</param>
    /// <returns>The .NET-style format string</returns>
    public static string ConvertFormatString(string printf)
    {
#if true
        // Copilot-generated code...
        // ...which doesn't do anything with format alignment/width/precision specifiers
        // (such as "08" in "%08x", or ".2" in "%.2f")

        // Regular expression to match printf-style format specifiers
        string pattern = @"%(\d+\$)?([+-]?(?:\d+|\*)?(?:\.\d+|\*)?[hlL]?[cCdiouxXeEfgGaAnpsSZ])";
        int index = 0;

        // Replace printf-style format specifiers with .NET-style format specifiers
        string dotNetFormat = Regex.Replace(printf, pattern, match =>
        {
            return "{" + index++ + "}";
        });

        return dotNetFormat;
#else
        StringBuilder builder = new StringBuilder(printf.Length * 2);
        int paramIndex = 0;

        for (int i = 0; i < printf.Length; i++)
        {
            char ch1 = printf[i];
            char ch2 = (i < printf.Length-1) ? printf[i + 1] : '\0';

            switch (ch1)
            {
                case '%':
                    switch (ch2)
                    {
                        case '\0':
                        case '%':
                            builder.Append('%');
                            i++;
                            break;

                        case 's':
                        case 'd':
                            builder.Append('{');
                            builder.Append(paramIndex++);
                            builder.Append('}');
                            break;

                        default:
                            throw new ArgumentException($"Unsupported format specifier: '{ch2}', at index {i} of \"{printf}\"");
                    }
                    break;

                default:
                    builder.Append(ch1);
                    break;
            }
        }

        return builder.ToString();
#endif
    }
}
