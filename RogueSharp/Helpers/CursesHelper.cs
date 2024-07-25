namespace RogueSharp.Helpers;

/// <summary>
/// Simple wrapper adapting the curses API library to the .NET console API
/// https://github.com/mcdaniel/curses_tutorial
/// </summary>
internal static class CursesHelper
{
    #region addch

    /// <summary>
    /// Write a character to the default window
    /// </summary>
    internal static void addch(char ch) => CursesWindow.Default.addch(ch);

    /// <summary>
    /// Write a character at a given position on the default window
    /// </summary>
    internal static void mvaddch(int y, int x, char ch) => CursesWindow.Default.mvaddch(y, x, ch);

    /// <summary>
    /// Write a character at a given position on the window
    /// </summary>
    internal static void mvwaddch(CursesWindow window, int y, int x, char ch) => window.mvaddch(y, x, ch);

    /// <summary>
    /// Write a character to the window
    /// </summary>
    internal static void waddch(CursesWindow window, char ch) => window.addch(ch);
    
    #endregion addch
    #region addstr

    /// <summary>
    /// Write a string to the default window
    /// </summary>
    internal static void addstr(string s) => CursesWindow.Default.addstr(s);

    /// <summary>
    /// Write a string at a given position on the default window
    /// </summary>
    internal static void mvaddstr(int y, int x, string s) => CursesWindow.Default.mvaddstr(y, x, s);

    /// <summary>
    /// Write a string at a given position on the window
    /// </summary>
    internal static void mvwaddstr(CursesWindow window, int y, int x, string s) => window.mvaddstr(y, x, s);

    /// <summary>
    /// Write a string to the window
    /// </summary>
    internal static void waddstr(CursesWindow window, string s) => window.addstr(s);
    
    #endregion addstr
    #region clear

    /// <summary>
    /// Clear the default window
    /// </summary>
    internal static void clear() => CursesWindow.Default.clear();

    /// <summary>
    /// Clear the window
    /// </summary>
    internal static void wclear(CursesWindow window) => window.clear();

    #endregion clear
    #region clearok

    /// <summary>
    /// If <paramref name="value"/> is <see langword="true"/> the next call to <see cref="refresh"/> 
    /// with this window will clear the screen completely and redraw the entire screen from scratch. 
    /// </summary>
    internal static void clearok(CursesWindow window, bool value) => window.clearok(value);

    #endregion clearok
    #region clrtoeol

    /// <summary>
    /// Clear the remainder of the current line in the default window
    /// </summary>
    internal static void clrtoeol() => CursesWindow.Default.clrtoeol();

    /// <summary>
    /// Clear the remainder of the current line in the window
    /// </summary>
    internal static void wclrtoeol(CursesWindow window) => window.clrtoeol();

    #endregion clrtoeol
    #region COLS

    public static int COLS { get; } = Console.WindowWidth;

    #endregion COLS
    #region curscr

    public static CursesWindow curscr { get; set; } = CursesWindow.Default;

    #endregion curscr
    #region delwin

    /// <summary>
    /// Deletes <paramref name="window"/>, freeing all memory associated with it. 
    /// Subwindows must be deleted before the main window can be deleted.
    /// </summary>
    internal static void delwin(CursesWindow window) => window.delwin();

    #endregion delwin
    #region endwin

    /// <summary>
    /// This routine restores tty modes, moves the cursor to the lower left-hand corner of the 
    /// screen and resets the terminal into the proper non-visual mode. Calling <see cref="refresh"/> 
    /// or <see cref="doupdate"/> after a temporary escape causes the program to resume visual mode.
    /// </summary>
    internal static void endwin()
    {
        move(y: LINES - 1, x: 0);
    }

    #endregion endwin
    #region erase

    /// <summary>
    /// Copies the erase character to every position in the default window
    /// </summary>
    internal static void erase() => CursesWindow.Default.erase();

    /// <summary>
    /// Copies the erase character to every position in the window
    /// </summary>
    internal static void werase(CursesWindow window) => window.erase();

    #endregion erase
    #region erasechar

    /// <summary>
    /// Returns erase character for the console
    /// </summary>
    internal static char erasechar() => CursesWindow.EraseCharacter;

    #endregion erasechar
    #region flushinp

    /// <summary>
    /// Discards any typeahead that has been typed by the user and has not yet been read by the program.
    /// </summary>
    public static void flushinp()
    {
        while (Console.KeyAvailable)
        {
            _ = Console.ReadKey(intercept: true);
        }
    }

    #endregion flushinp
    #region getyx

    /// <summary>
    /// Returns the (row, column) position of the cursor in the window
    /// </summary>
    /// <param name="y">The row of the cursor</param>
    /// <param name="x">The column of the cursor</param>
    internal static void getyx(CursesWindow window, out int y, out int x) => window.getyx(out y, out x);

    #endregion getyx
    #region getstr

    /// <summary>
    /// Gets a string from the terminal associated with the default window.  Calls characters from the 
    /// console and places each received character in <paramref name="s"/> until a newline is 
    /// received, which is also placed in <paramref name="s"/>. The erase and kill characters 
    /// user are processed.
    internal static void getstr(out string s) => CursesWindow.Default.getstr(out s);

    /// <summary>
    /// Gets a string from the terminal associated with <paramref name="window"/>.  Calls characters from the 
    /// console and places each received character in <paramref name="s"/> until a newline is 
    /// received, which is also placed in <paramref name="s"/>. The erase and kill characters 
    /// user are processed.
    internal static void wgetstr(CursesWindow window, out string s) => window.getstr(out s);

    #endregion getstr
    #region idlok

    /// <summary>
    /// 
    /// </summary>
    internal static void idlok(CursesWindow window, bool value) => window.idlok(value);

    #endregion idlok
    #region inch

    /// <summary>
    /// Returns the character at the current cursor position in the default window
    /// </summary>
    internal static char inch() => CursesWindow.Default.inch();

    /// <summary>
    /// Returns the character at the given position in the default window
    /// </summary>
    internal static char mvinch(int y, int x) => CursesWindow.Default.mvinch(y, x);

    /// <summary>
    /// Returns the character at the given position in the window
    /// </summary>
    internal static char mvwinch(CursesWindow window, int y, int x) => window.mvinch(y, x);

    /// <summary>
    /// Returns the character at the current cursor position in the window
    /// </summary>
    internal static char winch(CursesWindow window) => window.inch();

    #endregion inch
    #region initscr

    public static CursesWindow initscr()
    {
        Console.CursorTop = Console.CursorLeft = 0;

        CursesWindow.CreateDefaultWindow(LINES, COLS);
        move(0, 0);
        refresh();
        return stdscr;
    }

    #endregion initscr
    #region keypad

    internal static void keypad(CursesWindow window, bool value) => window.keypad(value);

    #endregion keypad
    #region killchar

    /// <summary>
    /// Returns current erase character for the console
    /// </summary>
    internal static char killchar() => (char) CursesWindow.KillKey;

    #endregion killchar
    #region leaveok

    /// <summary>
    /// If <paramref name="value"/> is <see langword="true"/> the cursor can be left wherever
    /// the next update happens to leave it.
    /// </summary>
    internal static void leaveok(CursesWindow window, bool value) => window.leaveok(value);

    #endregion leaveok
    #region LINES

    public static int LINES { get; } = Console.WindowHeight;

    #endregion LINES
    #region move

    /// <summary>
    /// Moves the cursor to the given position in the default window
    /// </summary>
    /// <param name="y">The row of the cursor</param>
    /// <param name="x">The column of the cursor</param>
    internal static void move(int y, int x) => CursesWindow.Default.move(y, x);

    /// <summary>
    /// Moves the cursor to the specified position in the window
    /// </summary>
    /// <param name="y">The row of the cursor</param>
    /// <param name="x">The column of the cursor</param>
    internal static void wmove(CursesWindow window, int y, int x) => window.move(y, x);

    #endregion move
    #region mvwin

    /// <summary>
    /// Moves a window to the specified position.
    /// </summary>
    /// <param name="top">Top coordinate of the window, relative to the physical screen</param>
    /// <param name="left">Left coordinate of the window, relative to the physical screen</param>
    /// <returns>
    /// <see langword="true"/> if the window was moved, or <see langword="false"/> if moving the 
    /// window would cause it to be off the screen.
    /// </returns>
    internal static bool mvwin(CursesWindow window, int top, int left) => window.mvwin(top, left);

    #endregion mvwin
    #region newwin

    /// <summary>
    /// Create a new window with the specified width and height
    /// </summary>
    /// <param name="nLines">Window height</param>
    /// <param name="nCols">Window width</param>
    /// <param name="top">Top coordinate of the window, relative to the physical screen</param>
    /// <param name="left">Left coordinate of the window, relative to the physical screen</param>
    internal static CursesWindow newwin(int nLines, int nCols, int top, int left) 
        => CursesWindow.CreateInstance(nLines, nCols, top, left);

    #endregion newwin
    #region printw

    /// <summary>
    /// Write a formatted string to the default window
    /// </summary>
    internal static void printw(string format, params object[] args) => CursesWindow.Default.printw(format, args);

    /// <summary>
    /// Write a formatted string to the window
    /// </summary>
    internal static void mvprintw(int y, int x, string format, params object[] args) => CursesWindow.Default.mvprintw(y, x, format, args);

    /// <summary>
    /// Write a formatted string to the window
    /// </summary>
    internal static void mvwprintw(CursesWindow window, int y, int x, string format, params object[] args) => window.mvprintw(y, x, format, args);

    /// <summary>
    /// Write a formatted string to the window
    /// </summary>
    internal static void wprintw(CursesWindow window, string format, params object[] args) => window.printw(format, args);

    #endregion printw
    #region refresh

    /// <summary>
    /// Refreshes the default window
    /// </summary>
    internal static void refresh() => CursesWindow.Default.refresh();

    /// <summary>
    /// Refreshes the window
    /// </summary>
    internal static void wrefresh(CursesWindow window) => window.refresh();

    #endregion refresh
    #region resetltchars

    public static void resetltchars()
    { }

    #endregion resetltchars
    #region standend

    /// <summary>
    /// Removes all attributes from the default window.
    /// </summary>
    public static void standend() => CursesWindow.Default.standend();

    /// <summary>
    /// Removes all attributes from a window.
    /// </summary>
    public static void wstandend(CursesWindow window) => window.standend();

    #endregion standend
    #region standout

    /// <summary>
    /// Applies the standout attribute to the default window.
    /// </summary>
    public static void standout() => CursesWindow.Default.standout();

    /// <summary>
    /// Applies the standout attribute to a window.
    /// </summary>
    public static void wstandout(CursesWindow window) => window.standout();

    #endregion standout
    #region stdscr

    public static CursesWindow stdscr => CursesWindow.Default;

    #endregion stdscr
    #region subwin

    /// <summary>
    /// Create a new sub-window or <paramref name="parent"/> with the specified width and height
    /// </summary>
    /// <param name="parent">The parent window</param>
    /// <param name="nLines">Window height</param>
    /// <param name="nCols">Window width</param>
    /// <param name="top">Top coordinate of the window, relative to the physical screen</param>
    /// <param name="left">Left coordinate of the window, relative to the physical screen</param>
    internal static CursesWindow subwin(CursesWindow parent, int nLines, int nCols, int top, int left) 
        => CursesWindow.CreateInstance(parent, nLines, nCols, top, left);

    #endregion subwin
    #region touchwin

    /// <summary>
    /// Marks an entire window as changed, so that the next call to <see cref="refresh"/> will 
    /// refresh the entire window.
    /// </summary>
    /// <param name="window">The window</param>
    internal static void touchwin(CursesWindow window) => window.touch();

    #endregion touchwin
    #region unctrl

    /// <summary>
    /// Returns a character string which is a printable representation of <paramref name="ch"/>.
    /// Control characters are displayed in the ^X notation. Printing characters are displayed as is.
    /// </summary>
    public static string unctrl(char ch) => ch.ToString();

    /// <summary>
    /// Returns a character string which is a printable representation of <paramref name="ch"/>.
    /// Control characters are displayed in the ^X notation. Printing characters are displayed as is.
    /// </summary>
    public static string unctrl(int ch) => unctrl((char) ch);

    /// <summary>
    /// Returns a character string which is a printable representation of <paramref name="key"/>.
    /// Control characters are displayed in the ^X notation. Printing characters are displayed as is.
    /// </summary>
    public static string unctrl(ConsoleKeyInfo key)
    {
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            char ch = key.Modifiers.HasFlag(ConsoleModifiers.Shift)
                ? char.ToUpper((char) key.Key)
                : char.ToLower((char) key.Key);

            return $"^{ch}";
        }

        return unctrl(key.KeyChar);
    }

    #endregion unctrl
}
