using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualBasic.FileIO;

using static RogueSharp.Helpers.CursesHelper;

using WINDOW = RogueSharp.Helpers.CursesWindow;

namespace RogueSharp;

internal partial class Program
{
    enum OptionKind { Boolean, Inventory, String }

    /*
     * description of an option and what to do with it
     */
    class OPTION
    {
        public OPTION(
            OptionKind kind,
            string name,
            string prompt,
            Action putfunc,
            Func<WINDOW, int> getfunc,
            Action<object> setfunc)
        {
            o_kind = kind;
            o_name = name;
            o_prompt = prompt;
            o_putfunc = putfunc;
            o_getfunc = getfunc;
            o_setfunc = setfunc;
        }

        public OptionKind o_kind { get; }
        public string o_name { get; }                   /* option name */
        public string o_prompt { get; }                 /* prompt for interactive entry */
        public Action o_putfunc { get; }                /* function to print value */
        public Func<WINDOW, int> o_getfunc { get; }     /* function to get value interactively */
        public Action<object> o_setfunc { get; }        /* function to set value interactively */
    };

    readonly OPTION[] optlist;

    OPTION[] initialize_options() =>
    [
        new OPTION(
            OptionKind.Boolean,
            "terse",
            "Terse output",
            () => put_bool(terse),
            (win) => get_bool(ref terse, win),
            (value) => terse = (bool)value),
        new OPTION(
            OptionKind.Boolean,
            "flush",
            "Flush typeahead during battle",
            () => put_bool(fight_flush),
            (win) => get_bool(ref fight_flush, win),
            (value) => fight_flush = (bool)value),
        new OPTION(
            OptionKind.Boolean,
            "jump",
            "Show position only at end of run",
            () => put_bool(jump),
            (win) => get_bool(ref jump, win),
            (value) => jump = (bool)value),
        new OPTION(
            OptionKind.Boolean,
            "seefloor",
            "Show the lamp-illuminated floor",
            () => put_bool(see_floor),
            (win) => get_sf(win),
            (value) => see_floor = (bool)value),
        new OPTION(
            OptionKind.Boolean,
            "passgo",
            "Follow turnings in passageways",
            () => put_bool(passgo),
            (win) => get_bool(ref passgo, win),
            (value) => passgo = (bool)value),
        new OPTION(
            OptionKind.Boolean,
            "tombstone",
            "Print out tombstone when killed",
            () => put_bool(tombstone),
            (win) => get_bool(ref tombstone, win),
            (value) => tombstone = (bool)value),
        new OPTION(
            OptionKind.Inventory,
            "inven",
            "Inventory style",
            () => put_inv_t(inv_type),
            (win) => get_inv_t(ref inv_type, win),
            (value) => inv_type = (int)value),
        new OPTION(
            OptionKind.String,
            "name",
            "Name",
            () => put_str(whoami),
            (win) => get_str(out whoami, win),
            (value) => strucpy(out whoami, (string)value)),
        new OPTION(
            OptionKind.String,
            "fruit",
            "Fruit",
            () => put_str(fruit),
            (win) => get_str(out fruit, win),
            (value) => strucpy(out fruit, (string)value)),
        new OPTION(
            OptionKind.String,
            "file",
            "Save file",
            () => put_str(file_name),
            (win) => get_str(out file_name, win),
            (value) => strucpy(out file_name, (string)value)),
    ];

    /// <summary>
    /// Print and then set options from the terminal
    /// </summary>
    void option()
    {
        wclear(hw);

        /*
         * Display current values of options
         */
        foreach (OPTION op in optlist)
        {
            pr_optname(op);
            op.o_putfunc();
            waddch(hw, '\n');
        }

        /*
         * Set values
         */
        wmove(hw, 0, 0);
        for (int i = 0; i<optlist.Length; i++)
        {
            OPTION op = optlist[i];

            pr_optname(op);
            int retval = op.o_getfunc(hw);
            if (retval != 0)
            {
                if (retval == QUIT)
                    break;

                if (i > 0)
                {   /* MINUS */
                    wmove(hw, i - 1, 0);
                    i -= 2;
                }
                else    /* trying to back up beyond the top */
                {
                    Console.Write('\a');
                    wmove(hw, 0, 0);
                    i--;
                }
            }
        }

        /*
         * Switch back to original screen
         */
        wmove(hw, LINES - 1, 0);
        waddstr(hw, "--Press space to continue--");
        wrefresh(hw);
        wait_for(' ');
        clearok(curscr, true);
        touchwin(stdscr);
        after = false;
    }

    /// <summary>
    /// Print out the option name prompt
    /// </summary>
    void pr_optname(OPTION op)
    {
        wprintw(hw, "%s (\"%s\"): ", op.o_prompt, op.o_name);
    }

    /// <summary>
    /// Put out a boolean
    /// </summary>
    void put_bool(bool b)
    {
        waddstr(hw, b.ToString());
    }

    /// <summary>
    /// Put out a string
    /// </summary>
    void put_str(string s)
    {
        waddstr(hw, s);
    }

    /// <summary>
    /// Put out an inventory type
    /// </summary>
    void put_inv_t(int i)
    {
        waddstr(hw, inv_t_name[i]);
    }

    /// <summary>
    /// Allow changing a boolean option and print it out
    /// </summary>
    int get_bool(ref bool value, WINDOW win)
    {
        bool op_bad;

        op_bad = true;
        getyx(win, out int oy, out int ox);
        waddstr(win, value.ToString());
        
        while (op_bad)
        {
            wmove(win, oy, ox);
            wrefresh(win);
            switch (readchar().Key)
            {
                case ConsoleKey.T:
                    value = true;
                    op_bad = false;
                    break;
                case ConsoleKey.F:
                    value = false;
                    op_bad = false;
                    break;
                case ConsoleKey.Enter:
                    op_bad = false;
                    break;
                case ConsoleKey.Escape:
                    return QUIT;
                case ConsoleKey.OemMinus:
                case ConsoleKey.Subtract:
                    return MINUS;
                default:
                    wmove(win, oy, ox + 10);
                    waddstr(win, "(T or F)");
                    break;
            }
        }

        wmove(win, oy, ox);
        waddstr(win, value.ToString());
        waddch(win, '\n');
        return NORM;
    }

    /// <summary>
    /// Change value and handle transition problems from see_floor to !see_floor.
    /// </summary>
    int get_sf(WINDOW win)
    {
        bool    was_sf;
        int     retval;

        was_sf = see_floor;
        retval = get_bool(ref see_floor, win);
        
        if (retval == QUIT) 
            return (QUIT);
        
        if (was_sf != see_floor)
        {
            if (!see_floor)
            {
                see_floor = true;
                erase_lamp(hero, proom);
                see_floor = false;
            }
            else
                look(false);
        }

        return (NORM);
    }

    const int MAXINP = 50;      /* max string to read from terminal or environment */

    /// <summary>
    /// Set a string option
    /// </summary>
    int get_str(out string value, WINDOW win)
    {
        char c;
        StringBuilder builder = new StringBuilder(80);

        getyx(win, out int oy, out int ox);
        wrefresh(win);
        
        /*
         * loop reading in the string, and put it in a temporary buffer
         */
        for (; (c = readchar().KeyChar) != '\n' && c != '\r' && c != ESCAPE; wclrtoeol(win), wrefresh(win))
        {
            if (c == -1)
            {
                continue;
            }
            else if (c == erasechar())  /* process erase character */
            {
                if (builder.Length > 0)
                {
                    builder.Length--;
                    waddch(win, (char) ConsoleKey.Backspace);
                }

                continue;
            }
            else if (c == killchar())   /* process kill character */
            {
                builder.Clear();
                wmove(win, oy, ox);
                continue;
            }
            else if (builder.Length == 0)
            {
                if (c == '-' && win != stdscr)
                    break;
                else if (c == '~')
                {
                    builder.Clear();
                    builder.Append(home);
                    waddstr(win, home);
                    continue;
                }
            }

            if (char.IsAscii(c) && (c != ' '))
            {
                builder.Append(c);
                waddch(win, c);
            }
        }

        if (builder.Length > 0)   /* only change option if something has been typed */
            strucpy(out value, builder.ToString());
        else
            value = string.Empty;

        mvwprintw(win, oy, ox, "%s\n", value);
        wrefresh(win);
        if (win == stdscr)
            mpos += builder.Length;
        if (c == '-')
            return MINUS;
        else if (c == ESCAPE)
            return QUIT;
        else
            return NORM;
    }

    /// <summary>
    /// Get an inventory type name
    /// </summary>
    int get_inv_t(ref int value, WINDOW win)
    {
        int oy, ox;
        bool op_bad;

        op_bad = true;
        getyx(win, out oy, out ox);
        waddstr(win, inv_t_name[value]);
        
        while (op_bad)
        {
            wmove(win, oy, ox);
            wrefresh(win);
        
            switch (readchar().Key)
            {
                case ConsoleKey.O:
                    value = INV_OVER;
                    op_bad = false;
                    break;
                case ConsoleKey.S:
                    value = INV_SLOW;
                    op_bad = false;
                    break;
                case ConsoleKey.C:
                    value = INV_CLEAR;
                    op_bad = false;
                    break;
                case ConsoleKey.Enter:
                    op_bad = false;
                    break;
                case ConsoleKey.Escape:
                    return QUIT;
                case ConsoleKey.OemMinus:
                case ConsoleKey.Subtract:
                    return MINUS;
                default:
                    wmove(win, oy, ox + 15);
                    waddstr(win, "(O, S, or C)");
                    break;
            }
        }

        mvwprintw(win, oy, ox, "%s\n", inv_t_name[value]);
        return NORM;
    }

#if MASTER
    /// <summary>
    /// Get a numeric option
    /// </summary>
    int get_num(ref short value, WINDOW win)
    {
        int i;

        if ((i = get_str(out string s, win)) == NORM)
            value = short.Parse(s);

        return i;
    }

    /// <summary>
    /// Get a numeric option
    /// </summary>
    int get_num(ref int value, WINDOW win)
    {
        int i;

        if ((i = get_str(out string s, win)) == NORM)
            value = int.Parse(s);

        return i;
    }
#endif

    /// <summary>
    /// Parse options from string, usually taken from the environment.
    /// The string is a series of comma seperated values, with booleans
    /// being stated as "name" (true) or "noname" (false), and strings
    /// being "name=....", with the string being defined up to a comma
    /// or the end of the entire option string.
    /// </summary>
    void parse_opts(string str)
    {
        string[] parts = str.Split(", ", StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            /*
             * Look it up and deal with it
             */
            foreach (OPTION op in optlist)
            {
                string[] subparts = part.Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (EQSTR(subparts[0], op.o_name))
                {
                    if (op.o_kind == OptionKind.Boolean)    /* if option is a boolean */
                        op.o_setfunc(true);
                    else if (subparts.Length > 1)           /* string option */
                    {
                        /*
                         * check for type of inventory
                         */
                        if (op.o_kind == OptionKind.Inventory)
                        {
                            char ch = Char.ToUpper(subparts[1][0]);
                            for (int i = 0; i < inv_t_name.Length; i++)
                            {
                                string inv_name = inv_t_name[i];
                                if (ch == inv_name[0])
                                {
                                    inv_type = i;
                                    break;
                                }
                            }
                        }
                        else
                            op.o_setfunc(subparts[1]);
                    }

                    break;
                }

                /*
                 * check for "noname" for booleans
                 */
                else if ((op.o_kind == OptionKind.Boolean) &&
                         (string.Compare(part, indexA: 0, "no",      indexB: 0, length: 2)               == 0) &&
                         (string.Compare(part, indexA: 2, op.o_name, indexB: 0, length: part.Length - 2) == 0))
                {
                    op.o_setfunc(false);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Copy string using unctrl for things
    /// </summary>
    void strucpy(out string s1, string s2)
    {
        StringBuilder builder = new StringBuilder(s2.Length);

        foreach (char ch in s2)
        {
            if (isprint(ch) || ch == ' ')
                builder.Append(unctrl(ch));
        }

        s1 = builder.ToString();
    }

    /// <summary>
    /// Approximates the functionality of the <see langword="isprint"/> function from the C++ library
    /// </summary>
    private bool isprint(char ch) => Char.GetUnicodeCategory(ch) switch
    {
        UnicodeCategory.Control => false,
        UnicodeCategory.OtherNotAssigned => false,
        UnicodeCategory.Surrogate => false,
        _ => true
    };
}
