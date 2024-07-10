using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.System.Console;

namespace RogueSharp.Helpers;

internal static class ConsoleHelper
{
    public static char GetConsoleCharAt(int row, int col)
    {
        using SafeHandle stdout = PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);

        COORD coord = new COORD
        {
            X = (short) col,
            Y = (short) row
        };

        unsafe
        {
            char* buffer = stackalloc char[2];
            PInvoke.ReadConsoleOutputCharacter(stdout, buffer, 1, coord, out _);

            return buffer[0];
        }
    }

    /// <summary>
    /// Writes the characters in a buffer to the console at the specified location.
    /// </summary>
    /// <param name="buffer">The characters to write.</param>
    /// <param name="consoleRow">
    /// The top of the upper-left coordinate of the console screen buffer rectangle to write to.
    /// </param>
    /// <param name="consoleColumn">
    /// The left of the upper-left coordinate of the console screen buffer rectangle to write to.
    /// </param>
    /// <param name="width">
    /// The width of the character buffer.  This is also the width of the console screen buffer
    /// rectangle to write to.
    /// </param>
    /// <param name="height">
    /// The height of the character buffer.  This is also the height of the console screen buffer
    /// rectangle to write to.
    /// </param>
    public static void WriteBufferToConsole(
        char[] buffer,
        int consoleRow,
        int consoleColumn,
        int width,
        int height)
    {
        WriteBufferToConsole(
            buffer,
            bufferRow: 0,
            bufferColumn: 0,
            bufferWidth: width,
            bufferHeight: height,
            consoleRow,
            consoleColumn,
            consoleWidth: width,
            consoleHeight: height);
    }

    /// <summary>
    /// Writes a sub-rectangle of the characters in a buffer to the console at the specified location.
    /// </summary>
    /// <param name="buffer">The characters to write</param>
    /// <param name="bufferRow">
    /// The top of the upper-left coordinate of the character screen buffer rectangle to write from.
    /// </param>
    /// <param name="bufferColumn">
    /// The left of the upper-left coordinate of the console screen buffer rectangle to write to.
    /// </param>
    /// <param name="bufferWidth">The overall width of the character buffer.</param>
    /// <param name="bufferHeight">The overall height of the character buffer.</param>
    /// <param name="consoleRow">
    /// The top of the upper-left coordinate of the console screen buffer rectangle to write to.
    /// </param>
    /// <param name="consoleColumn">
    /// The left of the upper-left coordinate of the console screen buffer rectangle to write to.
    /// </param>
    /// <param name="consoleWidth">The width of the console screen buffer rectangle to write to.</param>
    /// <param name="consoleHeight">The height of the console screen buffer rectangle to write to.</param>
    public static void WriteBufferToConsole(
        char[] buffer,
        int bufferRow,
        int bufferColumn,
        int bufferWidth,
        int bufferHeight,
        int consoleRow,
        int consoleColumn,
        int consoleWidth,
        int consoleHeight)
    {
        unsafe
        {
            using SafeHandle stdout = PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);

            // copy the region of the input buffer that we're going to write into a CHAR_INFO array
            int i = 0;
            CHAR_INFO* char_infos = stackalloc CHAR_INFO[consoleWidth * consoleHeight];

            for (int y = 0; y < consoleHeight; y++)
            {
                for (int x = 0; x < consoleWidth; x++)
                {
                    int bufferX = x + bufferColumn;
                    int bufferY = y + bufferRow;
                    int bufferIndex = (bufferY * bufferWidth) + bufferX;

                    var char_info = new CHAR_INFO();
                    char_info.Char.UnicodeChar = buffer[bufferIndex];
                    char_info.Attributes = 0x0007;  // FOREGROUND_BLUE | FOREGROUND_GREEN | FOREGROUND_RED

                    char_infos[i++] = char_info;
                }
            }

            // define the coordinates within char_infos to write from
            COORD origin = new COORD { X = 0, Y = 0 };

            // define the size of the output region, in character cells
            COORD size = new COORD { X = (short) consoleWidth, Y = (short) consoleHeight };

            // define the region to write to
            SMALL_RECT writeRegion = new SMALL_RECT
            {
                Left   = (short) consoleColumn,
                Top    = (short) consoleRow,
                Right  = (short)(consoleColumn + consoleWidth - 1),
                Bottom = (short)(consoleRow + consoleHeight - 1)
            };

            PInvoke.WriteConsoleOutput(
                stdout,
                char_infos[0],
                size,
                origin,
                ref writeRegion);
        }
    }
}
