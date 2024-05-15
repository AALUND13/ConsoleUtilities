using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ConsoleUtility {
    public static class RichTextHandler {
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
