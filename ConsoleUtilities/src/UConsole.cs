using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ConsoleUtility {
    /// <summary>
    /// All console colors in campbell theme
    /// </summary>
    public enum ConsoleColorHex {
        Black = 0x0C0C0C,
        DarkBlue = 0x0037DA,
        DarkGreen = 0x13A10E,
        DarkCyan = 0x3A96DD,
        DarkRed = 0xC50F1F,
        DarkMagenta = 0x881798,
        DarkYellow = 0xC19C00,
        Gray = 0xCCCCCC,
        DarkGray = 0x767676,
        Blue = 0x3B78FF,
        Green = 0x16C60C,
        Cyan = 0x61D6D6,
        Red = 0xE74856,
        Magenta = 0xB4009E,
        Yellow = 0xF9F1A5,
        White = 0xF2F2F2
    }

    public static class UConsole {
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        public static string DefaultForegroundColor { get; private set; }
        public static string DefaultBackgroundColor { get; private set; }

        public static string CurrentForegroundColor { get; private set; }
        public static string CurrentBackgroundColor { get; private set; }

        public static int CursorX => Console.CursorLeft;
        public static int CursorY => Console.CursorTop;
        public static int ConsoleWidth => Console.BufferWidth;
        public static int ConsoleHeight => Console.BufferHeight;

        public static int CursorIndex {
            get {
                return CursorY * ConsoleWidth + CursorX;
            }
            set {
                SetValidCursorPosition(value);
            }
        }

        // For Suggestion Handler
        private static string _userInput = string.Empty;
        private static string _oldSuggestion = string.Empty;
        private static List<string> _suggestions = new List<string>();
        private static string _currentSuggestion = string.Empty;
        private static int _currentSuggestionIndex = 0;
        private static int _index = 0;

        static UConsole() {
            DefaultForegroundColor = GetHexFromConsoleColor(Console.ForegroundColor);
            DefaultBackgroundColor = GetHexFromConsoleColor(Console.BackgroundColor);

            CurrentForegroundColor = DefaultForegroundColor;
            CurrentBackgroundColor = DefaultBackgroundColor;

            Console.GetCursorPosition();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void SetConsoleColors(Color foregroundColor, Color backgroundColor) {
            SetConsoleForegroundColor(foregroundColor);
            SetConsoleBackgroundColor(backgroundColor);
        }

        public static void SetConsoleColors(ConsoleColor foregroundColor, ConsoleColor backgroundColor) {
            SetConsoleForegroundColor(foregroundColor);
            SetConsoleBackgroundColor(backgroundColor);
        }


        public static void SetConsoleColors(string foregroundHexColor, string backgroundHexColor) {
            SetConsoleForegroundColor(foregroundHexColor);
            SetConsoleBackgroundColor(backgroundHexColor);
        }

        public static void SetConsoleForegroundColor(Color color) {
            SetConsoleForegroundColor(GetHexFromColor(color));
        }

        public static void SetConsoleBackgroundColor(Color color) {
            SetConsoleBackgroundColor(GetHexFromColor(color));
        }

        public static void SetConsoleForegroundColor(ConsoleColor color) {
            SetConsoleForegroundColor(GetHexFromConsoleColor(color));
        }

        public static void SetConsoleBackgroundColor(ConsoleColor color) {
            SetConsoleBackgroundColor(GetHexFromConsoleColor(color));
        }


        public static void SetConsoleForegroundColor(string hexColor) {
            CurrentForegroundColor = hexColor;
            Console.Write($"\x1b[38;2;{HexToRgb(hexColor)}m");
        }

        public static void SetConsoleBackgroundColor(string hexColor) {
            CurrentBackgroundColor = hexColor;
            Console.Write($"\x1b[48;2;{HexToRgb(hexColor)}m");
        }

        public static string HexToRgb(string hexColor) {
            hexColor = hexColor.TrimStart('#');
            if(hexColor.Length >= 6) {
                int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                return $"{r};{g};{b}";
            } else {
                // Handle the case when hexColor does not contain enough characters
                // For example, return a default RGB value or throw an exception
                throw new ArgumentException("Invalid hex color format");
            }
        }

        public static void ResetColor() {
            Console.ResetColor();
        }


        public static string GetHexFromColor(Color color) {
            return $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static string GetHexFromConsoleColor(ConsoleColor color) {
            return ((int)Enum.Parse<ConsoleColorHex>(Enum.GetName(color))).ToString("X6");
        }

        public static void SetCursorPosition(int cursorX, int cursorY) {
            Console.SetCursorPosition(cursorX, cursorY);
        }

        public static void SetValidCursorPosition(int cursorX, int cursorY) {
            int calculatedIndex = GetIndexFromCursorPosition(cursorX, cursorY);

            // Ensure calculatedIndex remains within bounds
            int validIndex = Math.Max(0, Math.Min(calculatedIndex, ConsoleWidth * ConsoleHeight - 1));

            // Calculate CursorX and CursorY without using if statements
            int calculatedCursorX = (validIndex % ConsoleWidth) * ((validIndex % ConsoleWidth) > 0 ? 1 : 0);
            int calculatedCursorY = (validIndex / ConsoleWidth) * ((validIndex / ConsoleWidth) > 0 ? 1 : 0);

            Console.SetCursorPosition(calculatedCursorX, calculatedCursorY);
        }

        public static void SetValidCursorPosition(int index) {
            // Ensure calculatedIndex remains within bounds
            int validIndex = Math.Max(0, Math.Min(index, ConsoleWidth * ConsoleHeight - 1));

            // Calculate CursorX and CursorY without using if statements
            int calculatedCursorX = (validIndex % ConsoleWidth) * ((validIndex % ConsoleWidth) > 0 ? 1 : 0);
            int calculatedCursorY = (validIndex / ConsoleWidth) * ((validIndex / ConsoleWidth) > 0 ? 1 : 0);

            Console.SetCursorPosition(calculatedCursorX, calculatedCursorY);
        }

        public static (int cursorX, int cursorY) GetCursorPosition() {
            return (CursorX, CursorY);
        }

        public static int GetIndexFromCursorPosition(int cursorX, int cursorY) {
            return cursorY * ConsoleWidth + cursorX;
        }

        // Console Write And Read Command
        public static void WriteWithCursorRestore(string str) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            Write(str);
            SetValidCursorPosition(cursorX, cursorY);
        }

        public static void WriteRichTextWithCursorRestore(string richTextString) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            WriteRichText(richTextString);
            SetValidCursorPosition(cursorX, cursorY);
        }

        public static void WriteWithAtPostion(string str, int cursorX, int cursorY) {
            SetValidCursorPosition(cursorX, cursorY);
            Write(str);
        }


        public static void WriteWithCursorRestore(string str, string foregroundHexColor = null, string backgroundHexColor = null) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            Write(str, foregroundHexColor, backgroundHexColor);
            SetValidCursorPosition(cursorX, cursorY);
        }

        public static void WriteWithAtPostion(string str, int cursorX, int cursorY, string foregroundHexColor = null, string backgroundHexColor = null) {
            SetValidCursorPosition(cursorX, cursorY);
            Write(str, foregroundHexColor, backgroundHexColor);
        }

        public static void WriteWithCursorRestore(string str, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            Write(str, foregroundHexColor, backgroundHexColor);
            SetValidCursorPosition(cursorX, cursorY);
        }

        public static void WriteWithAtPostion(string str, int cursorX, int cursorY, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            SetValidCursorPosition(cursorX, cursorY);
            Write(str, foregroundHexColor, backgroundHexColor);
        }

        public static void WriteWithCursorRestore(string str, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            Write(str, foregroundHexColor, backgroundHexColor);
            SetValidCursorPosition(cursorX, cursorY);
        }

        public static void WriteWithAtPostion(string str, int cursorX, int cursorY, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            SetValidCursorPosition(cursorX, cursorY);
            Write(str, foregroundHexColor, backgroundHexColor);
        }



        public static void Write(object? value, string foregroundHexColor = null, string backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : foregroundHexColor;
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : backgroundHexColor;

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.Write(value);

            ResetColor();
        }

        public static void Write(object? value, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : GetHexFromConsoleColor(foregroundHexColor.Value);
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : GetHexFromConsoleColor(foregroundHexColor.Value);

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.Write(value);

            ResetColor();
        }

        public static void Write(object? value, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : GetHexFromColor(foregroundHexColor.Value);
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : GetHexFromColor(foregroundHexColor.Value);

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.Write(value);

            ResetColor();
        }


        public static void Write(object? value) {
            Console.Write(value);
        }

        /// <summary>
        /// Writes the given rich text to the console, applying formatting based on special tags.
        /// <remarks>
        /// Special tags:
        /// <list type="bullet">
        /// <item>[f1]: Enable bold text.</item>
        /// <item>[f0]: Disable bold text.</item>
        /// <item>[i1]: Enable italic text.</item>
        /// <item>[i0]: Disable italic text.</item>
        /// <item>[u1]: Enable underline text.</item>
        /// <item>[u0]: Disable underline text.</item>
        /// <item>["ColorName"]: Set text color to the named color (e.g., "Red", "Blue").</item>
        /// <item>["HexColor"]: Set text color to the hexadecimal color (e.g., "#FF0000").</item>
        /// </list>
        /// </remarks>
        /// </summary>
        /// <param name="value">The rich text string containing special tags for formatting.</param>
        public static void WriteRichText(string? value) {
            // Convert the input rich text to formatted text using RichTextHandler
            Console.Write(RichTextHandler.ToRichText(value));

            // Reset console color after writing the rich text
            ResetColor();
        }

        public static void WriteLine(object? value, string foregroundHexColor = null, string backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : foregroundHexColor;
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : backgroundHexColor;

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.WriteLine(value);

            ResetColor();
        }

        public static void WriteLine(object? value, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : GetHexFromConsoleColor(foregroundHexColor.Value);
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : GetHexFromConsoleColor(foregroundHexColor.Value);

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.WriteLine(value);

            ResetColor();
        }


        public static void WriteLine(object? value, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : GetHexFromColor(foregroundHexColor.Value);
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : GetHexFromColor(foregroundHexColor.Value);

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.WriteLine(value);

            ResetColor();
        }

        public static void WriteLine(object? value = null) {
            Console.WriteLine(value);
        }

        /// <summary>
        /// Writes the given rich text to the console, applying formatting based on special tags.
        /// <remarks>
        /// Special tags:
        /// <list type="bullet">
        /// <item>[f1]: Enable bold text.</item>
        /// <item>[f0]: Disable bold text.</item>
        /// <item>[i1]: Enable italic text.</item>
        /// <item>[i0]: Disable italic text.</item>
        /// <item>[u1]: Enable underline text.</item>
        /// <item>[u0]: Disable underline text.</item>
        /// <item>["ColorName"]: Set text color to the named color (e.g., "Red", "Blue").</item>
        /// <item>["HexColor"]: Set text color to the hexadecimal color (e.g., "#FF0000").</item>
        /// </list>
        /// </remarks>
        /// </summary>
        /// <param name="value">The rich text string containing special tags for formatting.</param>
        public static void WriteLineRichText(string? value) {
            // Convert the input rich text to formatted text using RichTextHandler
            Console.WriteLine(RichTextHandler.ToRichText(value));

            // Reset console color after writing the rich text
            ResetColor();
        }


        public static void DeleteChars(bool invert = false, int numberOfCharactersToDelete = 1) {
            string padding = new string(' ', numberOfCharactersToDelete);
            if(invert) {
                Write(padding);
            } else {
                // Write the padding characters and then move the cursor back
                WriteWithCursorRestore(padding);
                SetValidCursorPosition(CursorIndex - numberOfCharactersToDelete);
            }
        }

        public static string ReadLine(object? value, string foregroundHexColor = null, string backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);
            return Console.ReadLine();
        }

        public static string ReadLine(object? value, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);
            return Console.ReadLine();
        }

        public static string ReadLine(object? value, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);
            return Console.ReadLine();
        }

        public static string ReadLine(object? value = null) {
            Write(value);
            return Console.ReadLine();
        }
        /// <summary>
        /// Writes the provided rich text to the console, applying formatting based on special tags,
        /// and then reads a line of input from the user.
        /// </summary>
        /// <remarks>
        /// Special tags:
        /// <list type="bullet">
        /// <item>[f1]: Enable bold text.</item>
        /// <item>[f0]: Disable bold text.</item>
        /// <item>[i1]: Enable italic text.</item>
        /// <item>[i0]: Disable italic text.</item>
        /// <item>[u1]: Enable underline text.</item>
        /// <item>[u0]: Disable underline text.</item>
        /// <item>["ColorName"]: Set text color to the named color (e.g., "Red", "Blue").</item>
        /// <item>["HexColor"]: Set text color to the hexadecimal color (e.g., "#FF0000").</item>
        /// </list>
        /// </remarks>
        /// <param name="RichTextValue">The rich text string to be displayed.</param>
        /// <returns>The line of input read from the console.</returns>
        /// 
        public static string ReadLineRichText(string RichTextValue) {
            WriteRichText(RichTextValue);
            return Console.ReadLine();
        }

        public static ConsoleKeyInfo ReadKey() {
            return Console.ReadKey();
        }

        public static ConsoleKeyInfo ReadKey(bool intercept = false) {
            return Console.ReadKey(intercept);
        }

        public static ConsoleKeyInfo ReadKey(bool intercept = false, object? value = null) {
            Write(value);

            return Console.ReadKey(intercept);
        }

        public static ConsoleKeyInfo ReadKey(bool intercept = false, object? value = null, string foregroundHexColor = null, string backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);

            return Console.ReadKey(intercept);
        }

        public static ConsoleKeyInfo ReadKey(bool intercept = false, object? value = null, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);

            return Console.ReadKey(intercept);
        }

        public static ConsoleKeyInfo ReadKey(bool intercept = false, object? value = null, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);

            return Console.ReadKey(intercept);
        }

        public static ConsoleKeyInfo ReadKeyRichText(bool intercept = false, string RichTextValue = null) {
            WriteRichText(RichTextValue);
            return Console.ReadKey(intercept);
        }


        public static string ReadLineWithSuggestionHandler(string RichTextValue, Func<string, List<string>> handler) {
            ResetSuggestions();
            WriteRichText(RichTextValue);

            ConsoleKeyInfo keyInfo;

            do {
                keyInfo = Console.ReadKey(true);
                if(keyInfo.Key == ConsoleKey.Backspace) {
                    if(_index == 0) continue;

                    int? endWordLength = null;
                    if(keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        Regex regex = new Regex(@"(\w+|.)\s*$", RegexOptions.Compiled);
                        Match match = regex.Match(_userInput.Substring(0, _index));
                        if(match.Success) {
                            endWordLength = match.Value.Length;
                        }
                    }

                    int deleteLength = GetEndWordLength(!keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control));

                    DeleteChars(false, deleteLength);
                    _userInput = _userInput.Remove(_index - deleteLength, deleteLength);

                    HandleSuggestions(handler);
                    UpdateString(0, -deleteLength, deleteLength);
                } else if(keyInfo.Key == ConsoleKey.Delete) {
                    if(_index >= _userInput.Length) continue;
                    Debug.WriteLine($"index: {_index} | userInput length: {_userInput.Length}");
                    int deleteLength = GetStartWordLength(!keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control));

                    DeleteChars(true, deleteLength);
                    SetValidCursorPosition(CursorIndex - deleteLength);
                    _userInput = _userInput.Remove(_index, deleteLength);

                    HandleSuggestions(handler);
                    UpdateString(0, 0, _userInput.Substring(_index).Length + 2);
                } else if(keyInfo.Key == ConsoleKey.LeftArrow) {
                    if(_index == 0) continue;
                    _index--;

                    SetValidCursorPosition(CursorIndex - 1);
                } else if(keyInfo.Key == ConsoleKey.RightArrow) {
                    if(_index >= _userInput.Length) continue;
                    _index++;

                    SetValidCursorPosition(CursorIndex + 1);
                } else if(keyInfo.Key == ConsoleKey.UpArrow) {
                    _currentSuggestionIndex = (_suggestions.Count != 0) ? (_currentSuggestionIndex + 1) % _suggestions.Count : 0;
                    _currentSuggestion = _suggestions.Count > 0 ? _suggestions[_currentSuggestionIndex] : "";

                    UpdateString();
                } else if(keyInfo.Key == ConsoleKey.DownArrow) {
                    _currentSuggestionIndex = (_suggestions.Count != 0) ? ((_currentSuggestionIndex - 1) % _suggestions.Count + _suggestions.Count) % _suggestions.Count : 0;
                    _currentSuggestion = _suggestions.Count > 0 ? _suggestions[_currentSuggestionIndex] : "";

                    UpdateString();
                } else if(keyInfo.Key == ConsoleKey.Tab) {
                    if(_suggestions.Count == 0) continue;

                    int suggestionLength = _currentSuggestion.Length;

                    ResetCursorPostion();
                    _userInput = _userInput.Insert(_index, _currentSuggestion);

                    HandleSuggestions(handler);
                    UpdateString(suggestionLength);
                } else if(keyInfo.Key != ConsoleKey.Enter) {
                    _userInput = _userInput.Insert(_index, keyInfo.KeyChar.ToString());

                    HandleSuggestions(handler);
                    UpdateString(1);
                } else {
                    int paddingLength = Math.Max(0, _oldSuggestion.Length - _currentSuggestion.Length);

                    string newString = _userInput.Substring(_index, _userInput.Length - _index);
                    WriteRichTextWithCursorRestore($"{newString}[#{GetHexFromConsoleColor(ConsoleColor.DarkGray)}]{new string(' ', _oldSuggestion.Length)}");
                }
                _oldSuggestion = _currentSuggestion;
            } while(keyInfo.Key != ConsoleKey.Enter);

            return _userInput;
        }

        private static int GetStartWordLength(bool skip) {
            if (skip) return 1;
            Regex regex = new Regex(@"^\s*(\w+|.)", RegexOptions.Compiled);
            Match match = regex.Match(_userInput.Substring(_index));
            return match.Success ? match.Value.Length : 1;
        }

        private static int GetEndWordLength(bool skip) {
            if(skip) return 1;
            Regex regex = new Regex(@"(\w+|.)\s*$", RegexOptions.Compiled);
            Match match = regex.Match(_userInput.Substring(0, _index));
            return match.Success ? match.Value.Length : 1;
        }

        private static void ResetSuggestions() {
            _userInput = string.Empty;
            _oldSuggestion = string.Empty;
            _suggestions.Clear();
            _currentSuggestion = string.Empty;
            _currentSuggestionIndex = 0;
            _index = 0;
        }

        private static void ResetCursorPostion() {
            SetValidCursorPosition(CursorIndex + (_userInput.Length - _index));
            _index = _userInput.Length;
        }

        private static void HandleSuggestions(Func<string, List<string>> handler) {
            _suggestions = handler.Invoke(_userInput);
            _currentSuggestionIndex = 0;
            _currentSuggestion = _suggestions.Count > 0 ? _suggestions[_currentSuggestionIndex] : "";
        }

        private static void UpdateString(int offset = 0, int indexOffset = 0, int paddingLengthOffset = 0) {
            string newString = _userInput.Substring(_index + indexOffset, _userInput.Length - _index - indexOffset);
            int paddingLength = Math.Max(0, _oldSuggestion.Length - _currentSuggestion.Length) + paddingLengthOffset;
            _index += offset + indexOffset;

            WriteRichTextWithCursorRestore($"{newString}[#{GetHexFromConsoleColor(ConsoleColor.DarkGray)}]{_currentSuggestion}{new string(' ', paddingLength)} ");
            SetValidCursorPosition(CursorIndex + offset);
        }

        public static void Clear() {
            Console.Clear();
            Console.Write("\x1b[3J");
        }
    }
}
