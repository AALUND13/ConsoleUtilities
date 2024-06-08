using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AALUND13.ConsoleUtility {
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

    /// <summary>
    /// Provides utility methods for interacting with the console, such as setting console colors, cursor position, and writing text with customizable colors.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the index of the cursor position in the console.
        /// </summary>
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

        /// <summary>
        /// Sets the console colors for the foreground and background with Color struct.
        /// </summary>
        /// <param name="foregroundColor">The Color struct value to set as the foreground color.</param>
        /// <param name="backgroundColor">The Color struct value to set as the background color.</param>
        public static void SetConsoleColors(Color foregroundColor, Color backgroundColor) {
            SetConsoleForegroundColor(foregroundColor);
            SetConsoleBackgroundColor(backgroundColor);
        }

        /// <summary>
        /// Sets the console colors for the foreground and background with ConsoleColor enum.
        /// </summary>
        /// <param name="foregroundColor">The ConsoleColor enum value to set as the foreground color.</param>
        /// <param name="backgroundColor">The ConsoleColor enum value to set as the background color.</param>
        public static void SetConsoleColors(ConsoleColor foregroundColor, ConsoleColor backgroundColor) {
            SetConsoleForegroundColor(foregroundColor);
            SetConsoleBackgroundColor(backgroundColor);
        }

        /// <summary>
        /// Sets the console colors for the foreground and background with Hex Color.
        /// </summary>
        /// <param name="foregroundHexColor">The hex color string to set as the foreground color.</param>
        /// <param name="backgroundHexColor">The hex color string to set as the background color.</param>
        public static void SetConsoleColors(string foregroundHexColor, string backgroundHexColor) {
            SetConsoleForegroundColor(foregroundHexColor);
            SetConsoleBackgroundColor(backgroundHexColor);
        }

        /// <summary>
        /// Sets the console foreground color with Color struct.
        /// </summary>
        /// <param name="color">The Color struct value to set as the foreground color.</param>
        public static void SetConsoleForegroundColor(Color color) {
            SetConsoleForegroundColor(GetHexFromColor(color));
        }

        /// <summary>
        /// Sets the console background color with Color struct.
        /// </summary>
        /// <param name="color">The Color struct value to set as the background color.</param>
        public static void SetConsoleBackgroundColor(Color color) {
            SetConsoleBackgroundColor(GetHexFromColor(color));
        }

        /// <summary>
        /// Sets the console foreground color with ConsoleColor enum.
        /// </summary>
        /// <param name="color">The ConsoleColor enum value to set as the foreground color.</param>
        public static void SetConsoleForegroundColor(ConsoleColor color) {
            SetConsoleForegroundColor(GetHexFromConsoleColor(color));
        }

        /// <summary>
        /// Sets the console background color with ConsoleColor enum.
        /// </summary>
        /// <param name="color">The ConsoleColor enum value to set as the background color.</param>
        public static void SetConsoleBackgroundColor(ConsoleColor color) {
            SetConsoleBackgroundColor(GetHexFromConsoleColor(color));
        }

        /// <summary>
        /// Sets the console foreground color with Hex Color.
        /// </summary>
        /// <param name="hexColor">The hex color string to set as the foreground color.</param>
        public static void SetConsoleForegroundColor(string hexColor) {
            CurrentForegroundColor = hexColor;
            Console.Write($"\x1b[38;2;{HexToRgb(hexColor)}m");
        }

        /// <summary>
        /// Sets the console background color with Hex Color.
        /// </summary>
        /// <param name="hexColor">The hex color string to set as the background color.</param>
        public static void SetConsoleBackgroundColor(string hexColor) {
            CurrentBackgroundColor = hexColor;
            Console.Write($"\x1b[48;2;{HexToRgb(hexColor)}m");
        }

        /// <summary>
        /// Converts a hex color string to an RGB string.
        /// </summary>
        /// <param name="hexColor">The hex color string to convert to an RGB string.</param>
        /// <returns>The RGB string representation of the hex color.</returns>
        public static string HexToRgb(string hexColor) {
            hexColor = hexColor.TrimStart('#');
            if(hexColor.Length >= 6) {
                int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                return $"{r};{g};{b}";
            } else {
                throw new ArgumentException("Invalid hex color format");
            }
        }

        /// <summary>
        /// Resets the console colors to the default foreground and background colors.
        /// </summary>
        public static void ResetColor() {
            Console.ResetColor();
        }

        /// <summary>
        /// Gets the hex color string from a Color struct.
        /// </summary>
        /// <param name="color">The Color struct value to convert to a hex color string.</param>
        public static string GetHexFromColor(Color color) {
            return $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Gets the hex color string from a ConsoleColor enum.
        /// </summary>
        /// <param name="color">The ConsoleColor enum value to convert to a hex color string.</param>
        public static string GetHexFromConsoleColor(ConsoleColor color) {
            return ((int)Enum.Parse<ConsoleColorHex>(Enum.GetName(color))).ToString("X6");
        }

        /// <summary>
        /// Sets the cursor position in the console.
        /// </summary>
        /// <param name="cursorX">The X coordinate of the cursor position.</param>
        /// <param name="cursorY">The Y coordinate of the cursor position.</param>
        public static void SetCursorPosition(int cursorX, int cursorY) {
            Console.SetCursorPosition(cursorX, cursorY);
        }

        /// <summary>
        /// Set the cursor position in the console with cursor X and Y.
        /// </summary>
        /// <param name="cursorX">The X coordinate of the cursor position.</param>
        /// <param name="cursorY">The Y coordinate of the cursor position.</param>
        public static void SetValidCursorPosition(int cursorX, int cursorY) {
            int calculatedIndex = GetIndexFromCursorPosition(cursorX, cursorY);

            // Ensure calculatedIndex remains within bounds
            int validIndex = Math.Max(0, Math.Min(calculatedIndex, ConsoleWidth * ConsoleHeight - 1));

            // Calculate CursorX and CursorY without using if statements
            int calculatedCursorX = (validIndex % ConsoleWidth) * ((validIndex % ConsoleWidth) > 0 ? 1 : 0);
            int calculatedCursorY = (validIndex / ConsoleWidth) * ((validIndex / ConsoleWidth) > 0 ? 1 : 0);

            Console.SetCursorPosition(calculatedCursorX, calculatedCursorY);
        }

        /// <summary>
        /// Set the cursor position in the console with cursor index.
        /// </summary>
        /// <param name="index">The index of the cursor position.</param>
        public static void SetValidCursorPosition(int index) {
            // Ensure calculatedIndex remains within bounds
            int validIndex = Math.Max(0, Math.Min(index, ConsoleWidth * ConsoleHeight - 1));

            // Calculate CursorX and CursorY without using if statements
            int calculatedCursorX = (validIndex % ConsoleWidth) * ((validIndex % ConsoleWidth) > 0 ? 1 : 0);
            int calculatedCursorY = (validIndex / ConsoleWidth) * ((validIndex / ConsoleWidth) > 0 ? 1 : 0);

            Console.SetCursorPosition(calculatedCursorX, calculatedCursorY);
        }

        /// <summary>
        /// Get the cursor position in the console.
        /// </summary>
        /// <returns>The X and Y coordinates of the cursor position.</returns>
        public static (int cursorX, int cursorY) GetCursorPosition() {
            return (CursorX, CursorY);
        }

        /// <summary>
        /// Get the index of the cursor position in the console.
        /// </summary>
        /// <returns>The index of the cursor position.</returns>
        /// <param name="cursorX">The X coordinate of the cursor position.</param>
        /// <param name="cursorY">The Y coordinate of the cursor position.</param>
        public static int GetIndexFromCursorPosition(int cursorX, int cursorY) {
            return cursorY * ConsoleWidth + cursorX;
        }

        // Console Write And Read Command

        /// <summary>
        /// Writes the given string to the console with the cursor position restored after writing.
        /// </summary>
        /// <param name="str">The string to write to the console.</param>
        public static void WriteWithCursorRestore(string str) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            Write(str);
            SetValidCursorPosition(cursorX, cursorY);
        }

        /// <summary>
        /// Writes in rich text to the console with the cursor position restored after writing.
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
        /// <param name="richTextString">The rich text string to write to the console.</param>
        public static void WriteRichTextWithCursorRestore(string richTextString) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            WriteRichText(richTextString);
            SetValidCursorPosition(cursorX, cursorY);
        }

        /// <summary>
        /// Writes the given string to the console at the specified cursor position.
        /// </summary>
        /// <param name="str">The string to write to the console.</param>
        /// <param name="cursorX">The X coordinate of the cursor position.</param>
        /// <param name="cursorY">The Y coordinate of the cursor position.</param>
        public static void WriteWithAtPostion(string str, int cursorX, int cursorY) {
            SetValidCursorPosition(cursorX, cursorY);
            Write(str);
        }

        /// <summary>
        /// Writes the given string to the console with the cursor position restored after writing.
        /// </summary>
        /// <param name="str">The string to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteWithCursorRestore(string str, string foregroundHexColor = null, string backgroundHexColor = null) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            Write(str, foregroundHexColor, backgroundHexColor);
            SetValidCursorPosition(cursorX, cursorY);
        }

        /// <summary>
        /// Writes the given string to the console at the specified cursor position.
        /// </summary>
        /// <param name="str">The string to write to the console.</param>
        /// <param name="cursorX">The X coordinate of the cursor position.</param>
        /// <param name="cursorY">The Y coordinate of the cursor position.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteWithAtPostion(string str, int cursorX, int cursorY, string foregroundHexColor = null, string backgroundHexColor = null) {
            SetValidCursorPosition(cursorX, cursorY);
            Write(str, foregroundHexColor, backgroundHexColor);
        }

        /// <summary>
        /// Writes the given string to the console with the cursor position restored after writing.
        /// </summary>
        /// <param name="str">The string to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteWithCursorRestore(string str, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            Write(str, foregroundHexColor, backgroundHexColor);
            SetValidCursorPosition(cursorX, cursorY);
        }

        /// <summary>
        /// Writes the given string to the console at the specified cursor position.
        /// </summary>
        /// <param name="str">The string to write to the console.</param>
        /// <param name="cursorX">The X coordinate of the cursor position.</param>
        /// <param name="cursorY">The Y coordinate of the cursor position.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteWithAtPostion(string str, int cursorX, int cursorY, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            SetValidCursorPosition(cursorX, cursorY);
            Write(str, foregroundHexColor, backgroundHexColor);
        }

        /// <summary>
        /// Writes the given string to the console with the cursor position restored after writing.
        /// </summary>
        /// <param name="str">The string to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteWithCursorRestore(string str, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            (int cursorX, int cursorY) = Console.GetCursorPosition();
            Write(str, foregroundHexColor, backgroundHexColor);
            SetValidCursorPosition(cursorX, cursorY);
        }

        /// <summary>
        /// Writes the given string to the console at the specified cursor position.
        /// </summary>
        /// <param name="str">The string to write to the console.</param>
        /// <param name="cursorX">The X coordinate of the cursor position.</param>
        /// <param name="cursorY">The Y coordinate of the cursor position.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteWithAtPostion(string str, int cursorX, int cursorY, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            SetValidCursorPosition(cursorX, cursorY);
            Write(str, foregroundHexColor, backgroundHexColor);
        }

        /// <summary>
        /// Writes the given object to the console.
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void Write(object? value, string foregroundHexColor = null, string backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : foregroundHexColor;
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : backgroundHexColor;

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.Write(value);

            ResetColor();
        }

        /// <summary>
        /// Writes the given object to the console.
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void Write(object? value, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : GetHexFromConsoleColor(foregroundHexColor.Value);
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : GetHexFromConsoleColor(foregroundHexColor.Value);

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.Write(value);

            ResetColor();
        }

        /// <summary>
        /// Writes the given object to the console.
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void Write(object? value, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : GetHexFromColor(foregroundHexColor.Value);
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : GetHexFromColor(foregroundHexColor.Value);

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.Write(value);

            ResetColor();
        }

        /// <summary>
        /// Writes the given object to the console.
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        public static void Write(object? value) {
            Console.Write(value);
        }

        /// <summary>
        /// Writes the given rich text to the console, applying formatting based on special tags.
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
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        public static void WriteRichText(string? value) {
            // Convert the input rich text to formatted text using RichTextHandler
            Console.Write(RichTextHandler.ToRichText(value));

            // Reset console color after writing the rich text
            ResetColor();
        }

        /// <summary>
        /// Writes the given object to the console on a new line.
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteLine(object? value, string foregroundHexColor = null, string backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : foregroundHexColor;
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : backgroundHexColor;

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.WriteLine(value);

            ResetColor();
        }

        /// <summary>
        /// Writes the given object to the console on a new line.
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteLine(object? value, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : GetHexFromConsoleColor(foregroundHexColor.Value);
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : GetHexFromConsoleColor(foregroundHexColor.Value);

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.WriteLine(value);

            ResetColor();
        }

        /// <summary>
        /// Writes the given object to the console on a new line.
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static void WriteLine(object? value, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            string newForegroundHexColor = (foregroundHexColor == null) ? DefaultForegroundColor : GetHexFromColor(foregroundHexColor.Value);
            string newBackgroundHexColor = (backgroundHexColor == null) ? DefaultBackgroundColor : GetHexFromColor(foregroundHexColor.Value);

            SetConsoleColors(newForegroundHexColor, newBackgroundHexColor);

            Console.WriteLine(value);

            ResetColor();
        }

        /// <summary>
        /// Writes the given object to the console on a new line.
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        public static void WriteLine(object? value = null) {
            Console.WriteLine(value);
        }

        /// <summary>
        /// Writes the given rich text to the console, applying formatting based on special tags.
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
        /// </summary>
        /// <param name="value">The value to write to the console.</param>
        public static void WriteLineRichText(string? value) {
            // Convert the input rich text to formatted text using RichTextHandler
            Console.WriteLine(RichTextHandler.ToRichText(value));

            // Reset console color after writing the rich text
            ResetColor();
        }

        /// <summary>
        /// Deletes the specified number of characters from the console.
        /// </summary>
        /// <param name="invert">Whether to delete characters in the opposite direction.</param>
        /// <param name="numberOfCharactersToDelete">The number of characters to delete.</param>
        public static void DeleteChars(bool invert = false, int numberOfCharactersToDelete = 1) {
            string padding = new string(' ', numberOfCharactersToDelete);
            if(invert) {
                Write(padding);
            } else {
                // Write the padding characters and then move the cursor back
                WriteWithCursorRestore(padding);
                CursorIndex -= numberOfCharactersToDelete;
            }
        }

        /// <summary>
        /// Reads a line of input from the console.
        /// </summary>
        /// <param name="value">The value to write to the console before reading the input.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static string ReadLine(object? value, string foregroundHexColor = null, string backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);
            return Console.ReadLine();
        }

        /// <summary>
        /// Reads a line of input from the console.
        /// </summary>
        /// <param name="value">The value to write to the console before reading the input.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static string ReadLine(object? value, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);
            return Console.ReadLine();
        }

        /// <summary>
        /// Reads a line of input from the console.
        /// </summary>
        /// <param name="value">The value to write to the console before reading the input.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        public static string ReadLine(object? value, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);
            return Console.ReadLine();
        }

        /// <summary>
        /// Reads a line of input from the console.
        /// </summary>
        /// <param name="value">The value to write to the console before reading the input.</param>
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
        /// <param name="RichTextValue">The rich text to write to the console before reading the input.</param>
        /// <returns>The line of input read from the console.</returns>
        public static string ReadLineRichText(string RichTextValue) {
            WriteRichText(RichTextValue);
            return Console.ReadLine();
        }

        /// <summary>
        /// Reads a key from the console.
        /// </summary>
        /// <returns>The key that was read from the console.</returns>
        public static ConsoleKeyInfo ReadKey() {
            return Console.ReadKey();
        }

        /// <summary>
        /// Reads a key from the console.
        /// </summary>
        /// <param name="intercept">Whether to display the key in the console.</param>
        /// <returns>The key that was read from the console.</returns>
        public static ConsoleKeyInfo ReadKey(bool intercept = false) {
            return Console.ReadKey(intercept);
        }

        /// <summary>
        /// Reads a key from the console.
        /// </summary>
        /// <param name="intercept">Whether to display the key in the console.</param>
        /// <param name="value">The value to write to the console before reading the key.</param>
        /// <returns>The key that was read from the console.</returns>
        public static ConsoleKeyInfo ReadKey(bool intercept = false, object? value = null) {
            Write(value);

            return Console.ReadKey(intercept);
        }

        /// <summary>
        /// Reads a key from the console.
        /// </summary>
        /// <param name="intercept">Whether to display the key in the console.</param>
        /// <param name="value">The value to write to the console before reading the key.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        /// <returns>The key that was read from the console.</returns>
        public static ConsoleKeyInfo ReadKey(bool intercept = false, object? value = null, string foregroundHexColor = null, string backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);

            return Console.ReadKey(intercept);
        }

        /// <summary>
        /// Reads a key from the console.
        /// </summary>
        /// <param name="intercept">Whether to display the key in the console.</param>
        /// <param name="value">The value to write to the console before reading the key.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        /// <returns>The key that was read from the console.</returns>
        public static ConsoleKeyInfo ReadKey(bool intercept = false, object? value = null, Color? foregroundHexColor = null, Color? backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);

            return Console.ReadKey(intercept);
        }

        /// <summary>
        /// Reads a key from the console.
        /// </summary>
        /// <param name="intercept">Whether to display the key in the console.</param>
        /// <param name="value">The value to write to the console before reading the key.</param>
        /// <param name="foregroundHexColor">The color to use for the text.</param>
        /// <param name="backgroundHexColor">The color to use for the background.</param>
        /// <returns>The key that was read from the console.</returns>
        public static ConsoleKeyInfo ReadKey(bool intercept = false, object? value = null, ConsoleColor? foregroundHexColor = null, ConsoleColor? backgroundHexColor = null) {
            Write(value, foregroundHexColor, backgroundHexColor);

            return Console.ReadKey(intercept);
        }

        /// <summary>
        /// Reads a key from the console.
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
        /// <param name="intercept">Whether to display the key in the console.</param>
        /// <param name="RichTextValue">The rich text to write to the console before reading the key.</param>
        public static ConsoleKeyInfo ReadKeyRichText(bool intercept = false, string RichTextValue = null) {
            WriteRichText(RichTextValue);
            return Console.ReadKey(intercept);
        }

        /// <summary>
        /// Read a line of input from the console with suggestions.
        /// </summary>
        /// <param name="richTextValue">The rich text to write to the console before reading the input.</param>
        /// <param name="suggestions">The list of suggestions to display.</param>
        /// <param name="ignoreCase">Whether to ignore case when comparing suggestions.</param>
        public static string ReadLineWithSuggestions(string richTextValue, List<string> suggestions, bool ignoreCase = true) {
            return ReadLineWithSuggestions(richTextValue, text => {
                if(text.Split(' ').Last().Length == 0) return new List<string>();

                List<string> substringSuggestions = new List<string>();

                // Find all suggestions that start with the last word in the input text
                // if ignoreCase is true, ignore case
                List<string> currentSuggestions = suggestions.FindAll(suggestion => ignoreCase ? suggestion.ToLower().StartsWith(text.ToLower().Split(' ').Last()) :
                    suggestion.StartsWith(text.Split(' ').Last())) ?? new List<string>();

                foreach(string currentSuggestion in currentSuggestions) {
                    int startIndex = Math.Min(text.Split(' ').Last().Length, currentSuggestion.Length); // Ensure start index doesn't exceed currentSuggestion length
                    int length = Math.Max(0, currentSuggestion.Length - text.Split(' ').Last().Length); // Calculate length of remaining portion of currentSuggestion
                    substringSuggestions.Add(currentSuggestion.Substring(startIndex, length));
                }
                return substringSuggestions;
            });
        }

        /// <summary>
        /// Read a line of input from the console with suggestions.
        /// </summary>
        /// <param name="richTextValue">The rich text to write to the console before reading the input.</param>
        /// <param name="handler">The handler to get suggestions based on the input text.</param>
        public static string ReadLineWithSuggestions(string richTextValue, Func<string, List<string>> handler) {
            ResetSuggestions();
            WriteRichText(richTextValue);

            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();

            HandleSuggestions(handler);
            UpdateString();

            _oldSuggestion = _currentSuggestion;

            do {
                //Try there to catch any exception that might occur
                try {
                    keyInfo = Console.ReadKey(true);
                } catch(Exception) {
                    continue;
                }

                if(keyInfo.Key == ConsoleKey.Backspace) {
                    if(_index == 0) continue;

                    int endWordLength = GetEndWordLength(!keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control));

                    DeleteChars(false, endWordLength);
                    _userInput = _userInput.Remove(_index - endWordLength, endWordLength);

                    HandleSuggestions(handler);
                    UpdateString(0, -endWordLength, endWordLength);
                } else if(keyInfo.Key == ConsoleKey.Delete) {
                    if(_index >= _userInput.Length) continue;
                    int startWirdLength = GetStartWordLength(!keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control));

                    DeleteChars(true, startWirdLength);
                    CursorIndex -= startWirdLength;
                    _userInput = _userInput.Remove(_index, startWirdLength);

                    HandleSuggestions(handler);
                    UpdateString(0, 0, _userInput.Substring(_index).Length + 2 + _oldSuggestion.Length);
                } else if(keyInfo.Key == ConsoleKey.LeftArrow) {
                    if(_index == 0) continue;
                    int endWordLength = GetEndWordLength(!keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control));

                    _index -= endWordLength;

                    CursorIndex -= endWordLength;
                } else if(keyInfo.Key == ConsoleKey.RightArrow) {
                    if(_index >= _userInput.Length) continue;
                    int startWirdLength = GetStartWordLength(!keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control));

                    _index += startWirdLength;

                    CursorIndex += startWirdLength;
                } else if(keyInfo.Key == ConsoleKey.Home) {
                    if(_index == 0) continue;
                    int numOfStepsToMove = _index;

                    _index -= numOfStepsToMove;

                    CursorIndex -= numOfStepsToMove;
                } else if(keyInfo.Key == ConsoleKey.End) {
                    if(_index >= _userInput.Length) continue;
                    int numOfStepsToMove = _userInput.Length - _index;

                    _index += numOfStepsToMove;

                    CursorIndex += numOfStepsToMove;
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
                    if(!char.IsControl(keyInfo.KeyChar)) {
                        _userInput = _userInput.Insert(_index, keyInfo.KeyChar.ToString());

                        HandleSuggestions(handler);
                        UpdateString(1);
                    }
                } else {
                    string newString = _userInput.Substring(_index, _userInput.Length - _index);
                    WriteRichTextWithCursorRestore($"{newString}[#{GetHexFromConsoleColor(ConsoleColor.DarkGray)}]{new string(' ', _oldSuggestion.Length)}");
                }
                _oldSuggestion = _currentSuggestion;
            } while(keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return _userInput;
        }

        private static int GetStartWordLength(bool skip) {
            if(skip) return 1;
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
            CursorIndex += (_userInput.Length - _index);
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

            Write($"{newString}");
            WriteWithCursorRestore($"{_currentSuggestion}{new string(' ', paddingLength)} ", ConsoleColor.DarkGray);
            CursorIndex += offset - newString.Length;
        }

        /// <summary>
        /// Clears the console.
        /// </summary>
        public static void Clear() {
            Console.Clear();
            Console.Write("\x1b[3J");
        }
    }
}
