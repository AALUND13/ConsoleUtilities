using System.Drawing;
using System.Text.RegularExpressions;

namespace AALUND13.ConsoleUtility {
    public static class RichTextHandler {
        /// <summary>
        /// Converts a string with rich text tags to a string with ANSI escape codes.
        /// </summary>
        /// <remarks>
        /// Special tags:
        /// <list type="bullet">
        /// <item>[f1]: Draw on foreground.</item>
        /// <item>[f0]: Draw on background.</item>
        /// <item>[i1]: Enable italic text.</item>
        /// <item>[i0]: Disable italic text.</item>
        /// <item>[u1]: Enable underline text.</item>
        /// <item>[u0]: Disable underline text.</item>
        /// <item>["ColorName"]: Set text color to the named color (e.g., "Red", "Blue").</item>
        /// <item>["HexColor"]: Set text color to the hexadecimal color (e.g., "#FF0000").</item>
        /// </list>
        /// </remarks>
        /// <param name="text">The text to be converted to ANSI escape codes.</param>
        /// <returns>A string containing the converted ANSI escape codes.</returns>
        public static string ToRichText(string? text) {
            if(text == null) return text;

            string richText = text.ToString();
            bool background = false;

            Regex regex = new Regex(@"\[(.*?)\]");

            MatchCollection matches = regex.Matches(text);

            foreach(Match match in matches) {
                string tag = match.Groups[1].Value.ToLower();

                switch(tag) {
                    case "r":
                        richText = richText.Replace(match.Groups[0].Value, "\x1b[0m"); // Reset all formatting
                        break;
                    case "i1":
                        richText = richText.Replace(match.Groups[0].Value, "\x1b[3m"); // Italic Text
                        break;
                    case "i0":
                        richText = richText.Replace(match.Groups[0].Value, "\x1b[23m"); // Disable Italic 
                        break;
                    case "u1":
                        richText = richText.Replace(match.Groups[0].Value, "\x1b[4m"); // Underline Text
                        break;
                    case "u0":
                        richText = richText.Replace(match.Groups[0].Value, "\x1b[24m"); // Disable Underline 
                        break;
                    case "f0":
                        richText = richText.Replace(match.Groups[0].Value, "");
                        background = true; // Enable background color
                        break;
                    case "f1":
                        richText = richText.Replace(match.Groups[0].Value, "");
                        background = false; // Disable background color
                        break;
                    default:
                        try {
                            if(tag.StartsWith('#')) {
                                richText = ApplyColor(richText, match.Groups[0].Value, background, match.Groups[1].Value);
                            } else {
                                Color color = Color.FromName(tag); // Named color
                                string hexColor = UConsole.GetHexFromColor(color);
                                richText = ApplyColor(richText, match.Groups[0].Value, background, hexColor);
                            }
                        } catch(Exception) { }
                        break;
                }
            }
            return richText;
        }

        private static string ApplyColor(string richText, string match, bool background, string? hexColor = null) {
            if(hexColor == null) return richText;

            string colorCode = $"\x1b[{(background ? 48 : 38)};2;{UConsole.HexToRgb(hexColor)}m";
            return richText.Replace(match, colorCode);
        }
    }
}
