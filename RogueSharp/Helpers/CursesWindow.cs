using System.Diagnostics;

using Microsoft;

namespace RogueSharp.Helpers;

public class CursesWindow
{
    public static CursesWindow Default { get; private set; }

    public static readonly char EraseCharacter = ' ';

    private readonly char[] _buffer;
    private bool _clearok;

    public CursesWindow(int width, int height, int top, int left)
    {
        Requires.Range(width > 0, nameof(width));
        Requires.Range(height > 0, nameof(height));
        Requires.Range(top >= 0, nameof(top));
        Requires.Range(left >= 0, nameof(left));

        Width  = width;
        Height = height;
        Top    = top;
        Left   = left;

        _buffer = new char[width * height];
        erase();
    }

    public static void CreateDefaultWindow(int width, int height)
    {
        Debug.Assert(Default == null, "Default window has already been created");
        Default = new CursesWindow(width, height, 0, 0);
    }

    #region Properties   
        
    /// <summary>
    /// Width of the window
    /// </summary>
    public int Width { get; }
        
    /// <summary>
    /// Height of the window
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Top coordinate of the window, relative to the console
    /// </summary>
    public int Top { get; }

    /// <summary>
    /// Left coordinate of the window, relative to the console
    /// </summary>
    public int Left { get; }

    /// <summary>
    /// Top coordinate of the cursor, relative to the window
    /// </summary>
    public int CursorTop 
    { 
        get => _cursorTop;
        private set
        {
            Requires.Range(value >= 0 && value < Height, nameof(value));
            _cursorTop = value;
        }
    }
    private int _cursorTop;

    /// <summary>
    /// Left coordinate of the cursor, relative to the window
    /// </summary>
    public int CursorLeft
    { 
        get => _cursorLeft;
        private set
        {
            Requires.Range(value >= 0 && value < Width, nameof(value));
            _cursorLeft = value;
        }
    }
    private int _cursorLeft;

    /// <summary>
    /// The index into <see cref="_buffer"/> of the current cursor position
    /// </summary>
    private int CursorIndex => BufferIndexOf(CursorTop, CursorLeft);

    #endregion Properties
    #region Public methods

    /// <summary>
    /// Add a character to the window and advance the cursor
    /// </summary>
    public void addch(char ch)
    {
        switch (ch)
        {
            case '\b':
                if (CursorLeft > 0)
                {
                    CursorLeft--;
                    _buffer[CursorIndex] = EraseCharacter;
                }
                break;

            case '\n':
                clrtoeol();
                CursorLeft = 0;
                CursorTop++;
                break;

            case '\r':
                CursorLeft = 0;
                break;

            case '\t':
                const int TabSize = 8;
                int spaces = TabSize - (CursorLeft % TabSize);
                for (int i = 0; i < spaces; i++)
                {
                    addch(EraseCharacter);
                }
                break;

            default:
                // convert non-printable characters to spaces
                ch = (ch < ' ') ? ' ' : ch;
                _buffer[CursorIndex] = ch;
                AdvanceCursor();
                break;
        }
    }

    /// <summary>
    /// Write a character at a given position on the window
    /// </summary>
    public void mvaddch(int y, int x, char ch)
    {
        move(y, x);
        addch(ch);
    }

    /// <summary>
    /// Write a string to the window
    /// </summary>
    public void addstr(string s)
    {
        foreach (char ch in s)
        {
            addch(ch);
        }
    }

    /// <summary>
    /// Write a string at a given position on the window
    /// </summary>
    public void mvaddstr(int y, int x, string s)
    {
        move(y, x);
        addstr(s);
    }

    /// <summary>
    /// Clear the window
    /// </summary>
    public void clear()
    {
        erase();
        clearok(true);
    }

    /// <summary>
    /// If <paramref name="value"/> is <see langword="true"/> the next call to <see cref="refresh"/> 
    /// with this window will clear the screen completely and redraw the entire screen from scratch. 
    /// </summary>
    public void clearok(bool value)
    {
        _clearok = value;
    }

    /// <summary>
    /// Clear the remainder of the current line
    /// </summary>
    public void clrtoeol()
    {
        Debug.Fail($"Break into debugger to verify behavior of {nameof(CursesWindow)}.{nameof(clrtoeol)}");

        Span<char> spanToClear = _buffer.AsSpan(start: CursorIndex, length: Width - CursorLeft);
        erase(spanToClear);
    }

    /// <summary>
    /// Copies <see cref="EraseCharacter"/> to every position in the window
    /// </summary>
    public void erase()
    {
        erase(_buffer.AsSpan());
    }

    /// <summary>
    /// Copies <see cref="EraseCharacter"/> to every position in <paramref name="span"/>
    /// </summary>
    private static void erase(Span<char> span)
    {
        span.Fill(EraseCharacter);
    }

    /// <summary>
    /// Returns the (row, column) position of the cursor in the window
    /// </summary>
    /// <param name="y">The row of the cursor</param>
    /// <param name="x">The column of the cursor</param>
    public void getyx(out int y, out int x)
    {
        y = CursorTop;
        x = CursorLeft;
    }

    /// <summary>
    /// Returns the character at the current cursor position
    /// </summary>
    public char inch() => _buffer[CursorIndex];

    /// <summary>
    /// Returns the character at the given position
    /// </summary>
    public char mvinch(int y, int x)
    {
        move(y, x);
        return inch();
    }

    /// <summary>
    /// Moves the cursor to the given position in the window
    /// </summary>
    /// <param name="y">The row of the cursor</param>
    /// <param name="x">The column of the cursor</param>
    public void move(int y, int x)
    {
        Requires.Range(y >= 0 && y < Height, nameof(y));
        Requires.Range(x >= 0 && x < Width, nameof(x));

        CursorTop = y;
        CursorLeft = x;
    }

    /// <summary>
    /// Write a formatted string to the window
    /// </summary>
    public void printw(string format, params object[] args)
    {
        Debug.Fail($"Break into debugger to verify behavior of {nameof(CursesWindow)}.{nameof(printw)}");

        string s = string.Format(PrintfHelper.ConvertFormatString(format), args);
        addstr(s);
    }

    /// <summary>
    /// Write a formatted string to the window
    /// </summary>
    public void mvprintw(int y, int x, string format, params object[] args)
    {
        move(y, x);
        printw(format, args);
    }

    /// <summary>
    /// Refreshes the window, copying the buffer to the physical console
    /// </summary>
    public void refresh()
    {
        ConsoleHelper.WriteBufferToConsole(_buffer, Top, Left, Width, Height); 
    }

    #endregion Public methods
    #region Private methods

    /// <summary>
    /// Advances the cursor to the next position in the window
    /// </summary>
    private void AdvanceCursor()
    {
        if (CursorLeft == Width - 1)
        {
            CursorLeft = 0;
            CursorTop++;
        }
        else
        {
            CursorLeft++;
        }
    }

    /// <summary>
    /// Returns the index into <see cref="_buffer"/> corresponding to the the 
    /// given (<paramref name="row"/>,<paramref name="col"/>) position
    /// </summary>
    /// <param name="row">The row in the buffer</param>
    /// <param name="col">The column in the buffer</param>
    /// <returns>The index into <see cref="_buffer"/></returns>
    private int BufferIndexOf(int row, int col)
    {
        int i = (row * Width) + col;
        Debug.Assert(i >= 0 && i < _buffer.Length, "Buffer index is out of range");

        return i;
    }

    #endregion Private methods
}
