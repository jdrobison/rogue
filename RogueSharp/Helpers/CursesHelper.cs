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
    private static void mvwaddch(CursesWindow window, int y, int x, char ch) => window.mvaddch(y, x, ch);

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
    #region getyx

    /// <summary>
    /// Returns the (row, column) position of the cursor in the window
    /// </summary>
    /// <param name="y">The row of the cursor</param>
    /// <param name="x">The column of the cursor</param>
    internal static void getyx(CursesWindow window, out int y, out int x) => window.getyx(out y, out x);

    #endregion getyx
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

        CursesWindow.CreateDefaultWindow(width: COLS, height: LINES);
        move(0, 0);
        refresh();
        return stdscr;
    }

    #endregion initscr
    #region killchar

    /// <summary>
    /// Returns current erase character for the console
    /// </summary>
    internal static char killchar() => (char) ConsoleKey.Escape;

    #endregion killchar
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
    #region newwin

    /// <summary>
    /// Create a new window with the specified width and height
    /// </summary>
    /// <param name="width">Window width</param>
    /// <param name="height">Window height</param>
    internal static CursesWindow newwin(int width, int height, int top, int left) => new CursesWindow(width, height, top, left);

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
    #region stdscr

    public static CursesWindow stdscr => CursesWindow.Default;

    #endregion stdscr
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

    #endregion unctrl
}
