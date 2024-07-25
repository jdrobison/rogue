using System.Diagnostics;
using System.Text;

using Microsoft;

namespace RogueSharp.Helpers;

internal class CursesWindow
{
    internal static CursesWindow Default { get; private set; } = null!;

    internal const ConsoleKey EraseKey = ConsoleKey.Backspace;
    internal const ConsoleKey KillKey = ConsoleKey.Escape;
    internal const char EraseCharacter = ' ';

    private CursesWindow? _parent;
    private readonly char[] _buffer;
    private bool _idlok;
    private ushort _attributes;

    #region Creation

    private CursesWindow(CursesWindow? parent, int height, int width, int top, int left)
    {
        Requires.Range(height >= 0, nameof(height));
        Requires.Range(width >= 0, nameof(width));
        Requires.Range(top >= 0 && top < CursesHelper.LINES, nameof(top));
        Requires.Range(left >= 0 && left < CursesHelper.COLS, nameof(left));

        Width  = (width > 0) ? width : CursesHelper.COLS - left;
        Height = (height > 0) ? height : CursesHelper.LINES - top;
        Top    = top;
        Left   = left;

        _parent = parent;
        _buffer = parent?._buffer ?? new char[height * width];

        erase();
    }

    internal static CursesWindow CreateInstance(int height, int width, int top, int left)
    {
        return new CursesWindow(parent: null, height, width, top, left);
    }

    internal static CursesWindow CreateInstance(CursesWindow parent, int height, int width, int top, int left)
    {
        Requires.Range(height <= parent.Height, nameof(height));
        Requires.Range(width <= parent.Width, nameof(width));
        Requires.Range(top >= parent.Top, nameof(top));
        Requires.Range(left >= parent.Left, nameof(left));

        return new CursesWindow(parent, height, width, top, left);
    }

    internal static void CreateDefaultWindow(int height, int width)
    {
        Debug.Assert(Default == null, "Default window has already been created");
        Default = CreateInstance(width, height, 0, 0);
    }

    #endregion Creation
    #region Properties   
        
    /// <summary>
    /// Width of the window
    /// </summary>
    internal int Width { get; }
        
    /// <summary>
    /// Height of the window
    /// </summary>
    internal int Height { get; }

    /// <summary>
    /// Top coordinate of the window, relative to the console
    /// </summary>
    internal int Top { get; private set; }

    /// <summary>
    /// Left coordinate of the window, relative to the console
    /// </summary>
    internal int Left { get; private set; }

    /// <summary>
    /// Top coordinate of the cursor, relative to the window
    /// </summary>
    internal int CursorTop 
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
    internal int CursorLeft
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

    #region addch

    /// <summary>
    /// Add a character to the window and advance the cursor
    /// </summary>
    internal void addch(char ch)
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
    internal void mvaddch(int y, int x, char ch)
    {
        move(y, x);
        addch(ch);
    }

    #endregion addch
    #region addstr

    /// <summary>
    /// Write a string to the window
    /// </summary>
    internal void addstr(string s)
    {
        foreach (char ch in s)
        {
            addch(ch);
        }
    }

    /// <summary>
    /// Write a string at a given position on the window
    /// </summary>
    internal void mvaddstr(int y, int x, string s)
    {
        move(y, x);
        addstr(s);
    }

    #endregion addstr
    #region clear

    /// <summary>
    /// Clear the window
    /// </summary>
    internal void clear()
    {
        erase();
        clearok(true);
    }

    #endregion clear
    #region clearok

    /// <summary>
    /// If <paramref name="value"/> is <see langword="true"/> the next call to <see cref="refresh"/> 
    /// with this window will clear the screen completely and redraw the entire screen from scratch. 
    /// </summary>
    internal void clearok(bool _)
    {
        // we always refresh the entire window, so there's nothing to do here
    }

    #endregion clearok
    #region clrtoeol

    /// <summary>
    /// Clear the remainder of the current line
    /// </summary>
    internal void clrtoeol()
    {
        Debug.Fail($"Break into debugger to verify behavior of {nameof(CursesWindow)}.{nameof(clrtoeol)}");

        Span<char> spanToClear = _buffer.AsSpan(start: CursorIndex, length: Width - CursorLeft);
        erase(spanToClear);
    }

    #endregion clrtoeol
    #region delwin

    /// <summary>
    /// Deletes the window, freeing all memory associated with it. 
    /// Subwindows must be deleted before the main window can be deleted.
    /// </summary>
    internal void delwin()
    { }

    #endregion delwin
    #region erase

    /// <summary>
    /// Copies <see cref="EraseCharacter"/> to every position in the window
    /// </summary>
    internal void erase()
    {
        if (_parent != null)
        {
            // for a sub-window, erase each row individually so we only
            // erase the portion of the buffer that belongs to this window
            for (int y = 0; y < Height; y++)
            {
                int rowStart = BufferIndexOf(y + Top, 0);
                erase(_buffer.AsSpan(rowStart, Width));
            }
        }
        else
        {
            erase(_buffer.AsSpan());
        }
    }

    /// <summary>
    /// Copies <see cref="EraseCharacter"/> to every position in <paramref name="span"/>
    /// </summary>
    private static void erase(Span<char> span)
    {
        span.Fill(EraseCharacter);
    }

    #endregion erase
    #region getch

    /// <summary>
    /// Gets a string from the terminal associated with the window.  Calls wgetch(3XCURSES) and place each received character in str until a newline is received, which is also placed in str . The erase and kill characters set by the user are processed.
    /// </summary>
    internal char getch()
    {
        char ch = GetConsoleKey().KeyChar;

        addch(ch);
        refresh();

        return ch;
    }

    private ConsoleKeyInfo GetConsoleKey() => Console.ReadKey(intercept: true);

    #endregion getch
    #region getstr

    /// <summary>
    /// Gets a string from the terminal associated with the window.  Calls characters from the 
    /// console and places each received character in <paramref name="s"/> until a newline is 
    /// received, which is also placed in <paramref name="s"/>. The erase and kill characters 
    /// user are processed.
    /// </summary>
    internal void getstr(out string s)
    {
        // TODO: remove this
        if (Debugger.IsAttached)
            Debugger.Break();

        StringBuilder builder = new();

        while (true)
        {
            ConsoleKeyInfo keyInfo = GetConsoleKey();

            addch(keyInfo.KeyChar);
            refresh();

            switch (keyInfo.Key)
            {
                case EraseKey:
                    if (builder.Length > 0)
                        builder.Length--;
                    break;

                case KillKey:
                    builder.Clear();
                    break;

                case ConsoleKey.Enter:
                    builder.Append(keyInfo.KeyChar);
                    s = builder.ToString();
                    return;

                default:
                    builder.Append(keyInfo.KeyChar);
                    break;
            }
        }
    }

    #endregion getstr
    #region getyx

    /// <summary>
    /// Returns the (row, column) position of the cursor in the window
    /// </summary>
    /// <param name="y">The row of the cursor</param>
    /// <param name="x">The column of the cursor</param>
    internal void getyx(out int y, out int x)
    {
        y = CursorTop;
        x = CursorLeft;
    }

    #endregion getyx
    #region idlok

    /// <summary>
    /// 
    /// </summary>
    internal void idlok(bool value) => _idlok = value;

    #endregion idlok
    #region inch

    /// <summary>
    /// Returns the character at the current cursor position
    /// </summary>
    internal char inch() => _buffer[CursorIndex];

    /// <summary>
    /// Returns the character at the given position
    /// </summary>
    internal char mvinch(int y, int x)
    {
        move(y, x);
        return inch();
    }

    #endregion inch
    #region keypad

    internal void keypad(bool _)
    {
        // there's nothing to do here
    }

    #endregion keypad
    #region leaveok

    /// <summary>
    /// If <paramref name="value"/> is <see langword="true"/> the cursor can be left wherever
    /// the next update happens to leave it.
    /// </summary>
    internal void leaveok(bool _)
    {
        // there's nothing to do here
    }

    #endregion leaveok
    #region move

    /// <summary>
    /// Moves the cursor to the given position in the window
    /// </summary>
    /// <param name="y">The row of the cursor</param>
    /// <param name="x">The column of the cursor</param>
    internal void move(int y, int x)
    {
        Requires.Range(y >= 0 && y < Height, nameof(y));
        Requires.Range(x >= 0 && x < Width, nameof(x));

        CursorTop = y;
        CursorLeft = x;
    }

    #endregion move
    #region mvwin

    /// <summary>
    /// Moves the window to the specified position.
    /// </summary>
    /// <param name="top">Top coordinate of the window, relative to the physical screen</param>
    /// <param name="left">Left coordinate of the window, relative to the physical screen</param>
    /// <returns>
    /// <see langword="true"/> if the window was moved, or <see langword="false"/> if moving the 
    /// window would cause it to be off the screen.
    /// </returns>
    internal bool mvwin(int top, int left)
    {
        if ((top < 0) || (left < 0) || (top + Height > CursesHelper.LINES) || (left + Width > CursesHelper.COLS))
            return false;

        Top = top;
        Left = left;
        return true;
    }

    #endregion mvwin
    #region printw

    /// <summary>
    /// Write a formatted string to the window
    /// </summary>
    internal void printw(string format, params object[] args)
    {
        Debug.Fail($"Break into debugger to verify behavior of {nameof(CursesWindow)}.{nameof(printw)}");

        string s = string.Format(PrintfHelper.ConvertFormatString(format), args);
        addstr(s);
    }

    /// <summary>
    /// Write a formatted string to the window
    /// </summary>
    internal void mvprintw(int y, int x, string format, params object[] args)
    {
        move(y, x);
        printw(format, args);
    }

    #endregion printw
    #region refresh

    /// <summary>
    /// Refreshes the window, copying the buffer to the physical console
    /// </summary>
    internal void refresh()
    {
        if (_parent is not null)
            _parent.refresh();
        else
            ConsoleHelper.WriteBufferToConsole(_buffer, Top, Left, Width, Height); 
    }

    #endregion refresh
    #region standend

    /// <summary>
    /// Removes all attributes from the window.
    /// </summary>
    internal void standend()
    {
        _attributes = 0;
    }

    #endregion standend
    #region standout

    /// <summary>
    /// Applies the standout attribute to the default window.
    /// </summary>
    internal void standout()
    {
        // TODO
    }

    #endregion standout
    #region touch

    /// <summary>
    /// Marks an entire window as changed, so that the next call to <see cref="refresh"/> will 
    /// refresh the entire window.
    /// </summary>
    internal void touch()
    {
        // we always refresh the entire window, so there's nothing to do here
    }

    #endregion touch

    #endregion Public methods
    #region Private methods

    /// <summary>
    /// Advances the cursor to the next position in the window
    /// </summary>
    private void AdvanceCursor()
    {
        // TODO: handle cursor already being at the bottom-right corner of the window

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
        // adjust the row and column if this is a subwindow
        if (_parent is not null)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            row += Top - _parent.Top;
            col += Left - _parent.Left;
        }

        int i = (row * Width) + col;
        Debug.Assert(i >= 0 && i < _buffer.Length, $"Buffer index for ({row},{col}) is out of range");

        return i;
    }

    #endregion Private methods
}
