using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using RogueSharp.Helpers;

using static System.Runtime.InteropServices.JavaScript.JSType;
using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    public static Program Instance { get; private set; }

    private static void Main(string[] args)
    {
        int argIndex = 0;
        bool isWizard = false;

#if MASTER
        // Check to see if he is a wizard
        if ((args.Length >= 1) && (args[argIndex].Length == 0))
        {
            //if (strcmp(PASSWD, md_crypt(md_getpass("wizard's password: "), "mT")) == 0)
            {
                isWizard = true;
                argIndex++;
            }
        }
#endif

        // parse optional seed from the environment
        if (!Int32.TryParse(Environment.GetEnvironmentVariable("RogueSeed"), out int seed))
            seed = (int) DateTime.Now.Ticks;

        Instance = new Program(seed, isWizard);
        Instance.Run(args);
    }

    private Program(int randomSeed, bool isWizard)
    {
        seed = dnum = randomSeed;
        wizard = isWizard;

        if (isWizard)
            player.t_flags |= SEEMONST;

        // initialize the console
        Console.Title = "Rogue";
        Console.WindowHeight = MAXLINES;
        Console.WindowWidth = MAXCOLS;

        /*
         * get home and options from environment
         */
        home = Environment.GetEnvironmentVariable("UserProfile") ?? @"c:\Rogue";
        file_name = Path.Combine(home, "rogue.save");
        Directory.CreateDirectory(home);

        optlist = initialize_options();
        if (Environment.GetEnvironmentVariable("RogueOpts") is string opts)
            parse_opts(opts);

        if (whoami is null || (whoami.Length == 0))
            whoami = Environment.UserName;

    }

    private void Run(string[] args)
    {
        open_score();

        //if (args.Length == 2)
        //{
        //    if (strcmp(args[1], "-s") == 0)
        //    {
        //        noscore = TRUE;
        //        score(0, -1, 0);
        //        exit(0);
        //    }
        //    else if (strcmp(args[1], "-d") == 0)
        //    {
        //        dnum = rnd(100);    /* throw away some rnd()s to break patterns */
        //        while (--dnum)
        //            rnd(100);
        //        purse = rnd(100) + 1;
        //        level = rnd(100) + 1;
        //        initscr();
        //        getltchars();
        //        death(death_monst());
        //        exit(0);
        //    }
        //}

        if (args.Length == 2)
        {
            if (restore(args[1]))
                playit();           // does not return
            else
                my_exit(1);
        }

#if MASTER
        if (wizard)
            Console.Write($"Hello {whoami}, welcome to dungeon #{dnum}");
        else
#endif
            Console.Write($"Hello {whoami}, just a moment while I dig the dungeon...");

        if (!Debugger.IsAttached)
            Thread.Sleep(TimeSpan.FromSeconds(5));

        initscr();              /* Start up cursor package */
        init_probs();           /* Set up prob tables for objects */
        init_player();          /* Set up initial player stats */
        init_names();           /* Set up names of scrolls */
        init_colors();          /* Set up colors of potions */
        init_stones();          /* Set up stone settings of rings */
        init_materials();       /* Set up materials of wands */
        //setup();

        /*
         * The screen must be at least NUMLINES x NUMCOLS
         */
        if (LINES < NUMLINES || COLS < NUMCOLS)
        {
            Console.WriteLine("\nSorry, the screen must be at least {0}x{1}", NUMLINES, NUMCOLS);
            endwin();
            my_exit(1);
        }

        /*
         * Set up windows
         */
        hw = newwin(LINES, COLS, 0, 0);
        idlok(stdscr, true);
        idlok(hw, true);
#if MASTER
        noscore = wizard;
#endif
        new_level();            /* Draw current level */
        /*
         * Start up daemons and fuses
         */
        start_daemon(runners, 0, AFTER);
        start_daemon(doctor, 0, AFTER);
        fuse(swander, 0, WANDERTIME, AFTER);
        start_daemon(stomach, 0, AFTER);
        playit();
    }

    private void open_score()
    {
        string scorefile = Path.Combine(home, "rogue.score");

        if (scoreboard != null)
        {
            scoreboard.Position = 0;
            return;
        }

        scoreboard = File.Open(scorefile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    /// <summary>
    /// Exit the program abnormally.
    /// </summary>
    void endit(int _)
    {
        fatal("Okay, bye bye!\n");
    }

    /// <summary>
    /// Exit the program, printing a message.
    /// </summary>
    void fatal(string s)
    {
        mvaddstr(LINES - 2, 0, s);
        refresh();
        endwin();
        my_exit(0);
    }

    /// <summary>
    /// Returns a random number in the range [0..<paramref name="range"/>)
    /// </summary>
    private int rnd(int range) => range == 0 ? 0 : Math.Abs(RN) % range;

    /// <summary>
    /// Returns the next random number
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int RN
    {
        get
        {
            unchecked
            {
                seed = (seed * 11109) + 13849;
                return (seed >> 16) & 0xffff; 
            }
        }
    }

    /// <summary>
    /// Roll a number of dice
    /// </summary>
    /// <param name="number">Number of dice</param>
    /// <param name="sides">The number of sides on each die</param>
    int roll(int number, int sides)
    {
        int total = 0;

        while (number-- != 0)
            total += rnd(sides) + 1;

        return total;
    }

#if false
    /*
     * tstp:
     *	Handle stop and start signals
     */

    void
    tstp(int ignored)
    {
        int y, x;
        int oy, ox;

        NOOP(ignored);

        /*
         * leave nicely
         */
        getyx(curscr, oy, ox);
        mvcur(0, COLS - 1, LINES - 1, 0);
        endwin();
        resetltchars();
        fflush(stdout);
        md_tstpsignal();

        /*
         * start back up again
         */
        md_tstpresume();
        raw();
        noecho();
        keypad(stdscr, 1);
        playltchars();
        clearok(curscr, TRUE);
        wrefresh(curscr);
        getyx(curscr, y, x);
        mvcur(y, x, oy, ox);
        fflush(stdout);
        curscr->_cury = oy;
        curscr->_curx = ox;
    }
#endif

    /// <summary>
    /// loop of the program.  Loop until the game is over,
    /// refreshing things and looking at the proper times.
    /// </summary>
    [DoesNotReturn]
    void playit()
    {
        //if (md_hasclreol())
            inv_type = INV_CLEAR;

        oldpos = hero;
        oldrp = roomin(hero) ?? new room();

        while (playing)
            command();          /* Command execution */

        endit(0);
    }

    /// <summary>
    /// Have player make certain, then exit.
    /// </summary>
    void quit(int _)
    {
        /*
         * Reset the signal in case we got here via an interrupt
         */
        if (!q_comm)
            mpos = 0;
        getyx(curscr, out int oy, out int ox);
        msg("really quit?");

        if (readchar().KeyChar == 'y')
        {
            //signal(SIGINT, leave);
            clear();
            mvprintw(LINES - 2, 0, "You quit with %d gold pieces", purse);
            move(LINES - 1, 0);
            refresh();
            score(purse, 1, '\0');
            my_exit(0);
        }
        else
        {
            move(0, 0);
            clrtoeol();
            status();
            move(oy, ox);
            refresh();
            mpos = 0;
            count = 0;
            to_death = false;
        }
    }

#if false
    /// <summary>
    /// Leave quickly, but courteously
    /// </summary>
    void
    leave(int _)
    {
        static char buf[BUFSIZ];

        setbuf(stdout, buf);    /* throw away pending output */

        if (!isendwin())
        {
            mvcur(0, COLS - 1, LINES - 1, 0);
            endwin();
        }

        putchar('\n');
        my_exit(0);
    }

    /*
     * shell:
     *	Let them escape for a while
     */

    void
    shell()
    {
        /*
         * Set the terminal back to original mode
         */
        move(LINES-1, 0);
        refresh();
        endwin();
        resetltchars();
        putchar('\n');
        in_shell = TRUE;
        after = FALSE;
        fflush(stdout);
        /*
         * Fork and do a shell
         */
        md_shellescape();

        printf("\n[Press return to continue]");
        fflush(stdout);
        noecho();
        raw();
        keypad(stdscr, 1);
        playltchars();
        in_shell = FALSE;
        wait_for('\n');
        clearok(stdscr, TRUE);
    }
#endif

    /// <summary>
    /// Leave the process properly
    /// </summary>
    void my_exit(int st)
    {
        //resetltchars();
        Environment.Exit(st);
    }
}
