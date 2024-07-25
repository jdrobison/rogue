using System.Diagnostics;

using RogueSharp.Helpers;

using static RogueSharp.Helpers.CursesHelper;

using str_t = System.UInt32;

namespace RogueSharp;

internal partial class Program
{
    private int MAXMSG => NUMCOLS - "--More--".Length;
    private int msgbufLength => (2 * MAXMSG) + 1;

    private string _io_msgbuf = string.Empty;
    private int _io_newpos = 0;

    /// <summary>
    /// Display a message at the top of the screen.
    /// </summary>
    int msg(string? fmt, params object?[] args)
    {
        /*
         * if the string is "", just clear the line
         */
        if (fmt == null || fmt.Length == 0)
        {
            move(0, 0);
            clrtoeol();
            mpos = 0;
            return ~ESCAPE;
        }

        /*
         * otherwise add to the message and flush it out
         */
        doadd(fmt, args);
        return endmsg();
    }

    /// <summary>
    /// Add things to the current message
    /// </summary>
    void addmsg(string fmt, params object?[] args)
    {
        doadd(fmt, args);
    }

    /// <summary>
    /// Display a new msg (giving him a chance to see the previous one if it is up there with the --More--)
    /// </summary>
    int endmsg()
    {
        if (save_msg)
            huh = _io_msgbuf;

        if (mpos != 0)
        {
            look(false);
            mvaddstr(0, mpos, "--More--");
            refresh();
            if (!msg_esc)
                wait_for(' ');
            else
            {
                ConsoleKey key;

                while ((key = readchar().Key) != ConsoleKey.Spacebar)
                {
                    if (key == ConsoleKey.Escape)
                    {
                        _io_msgbuf = "";
                        mpos = 0;
                        _io_newpos = 0;
                        return ESCAPE;
                    }
                }
            }
        }

        /*
         * All messages should start with uppercase, except ones that
         * start with a pack addressing character
         */
        if (Char.IsAsciiLetterLower(_io_msgbuf[0]) && !lower_msg && _io_msgbuf[1] != ')')
        {
            mvaddch(0, 0, Char.ToUpper(_io_msgbuf[0]));
            addstr(_io_msgbuf.Substring(1));
        }
        else
        {
            mvaddstr(0, 0, _io_msgbuf);
        }

        clrtoeol();
        mpos = _io_newpos;
        _io_newpos = 0;
        _io_msgbuf = "";
        refresh();
        return ~ESCAPE;
    }

    /// <summary>
    /// Perform an add onto the message buffer
    /// </summary>
    void doadd(string fmt, params object?[] args)
    {
        string converted = PrintfHelper.ConvertFormatString(fmt);
        string addendum = string.Format(converted, args);

        if (addendum.Length + _io_newpos >= MAXMSG)
            endmsg();

        _io_msgbuf += addendum;
        _io_newpos = _io_msgbuf.Length;
    }

    /// <summary>
    /// Returns true if it is ok to step on ch
    /// </summary>
    bool step_ok(char ch)
    {
        switch (ch)
        {
            case ' ':
            case '|':
            case '-':
                return false;
            default:
                return !Char.IsAsciiLetter(ch);
        }
    }

    /// <summary>
    /// Reads and returns a character, checking for gross input errors
    /// </summary>
    ConsoleKeyInfo readchar()
    {
#if true
        return Console.ReadKey();
#else

        char ch;

        ch = (char) md_readchar();

        if (ch == 3)
        {
            quit(0);
            return (27);
        }

        return (ch);
#endif
    }

    private int _status_hpwidth = 0;
    private int _status_s_hungry = 0;
    private int _status_s_lvl = 0;
    private int _status_s_pur = -1;
    private int _status_s_hpt = 0;
    private int _status_s_arm = 0;
    private str_t _status_s_str = 0;
    private int _status_s_exp = 0;

    /// <summary>
    /// Display the important stats line.  Keep the cursor where it was.
    /// </summary>
    void status()
    {
        string[] hungry_state_name = { "", "Hungry", "Weak", "Faint" };

        if (Debugger.IsAttached)
            Debugger.Break();

        /*
         * If nothing has changed since the last status, don't
         * bother.
         */
        int armor = cur_armor?.o_arm ?? pstats.s_arm;
        if (_status_s_hpt == pstats.s_hpt && 
            _status_s_exp == pstats.s_exp && 
            _status_s_pur == purse && 
            _status_s_arm == armor && 
            _status_s_str == pstats.s_str && 
            _status_s_lvl == level && 
            _status_s_hungry == hungry_state && 
            !stat_msg)
        {
            return;
        }

        _status_s_arm = armor;

        getyx(stdscr, out int oy, out int ox);
        if (_status_s_hpt != max_hp)
        {
            int hp = max_hp;
            _status_s_hpt = max_hp;
            for (_status_hpwidth = 0; hp != 0; _status_hpwidth++)
                hp /= 10;
        }

        /*
         * Save current status
         */
        _status_s_lvl = level;
        _status_s_pur = purse;
        _status_s_hpt = pstats.s_hpt;
        _status_s_str = pstats.s_str;
        _status_s_exp = pstats.s_exp;
        _status_s_hungry = hungry_state;

        string message = string.Format(
            "Level: {0}  Gold: {1,-5}  Hp: {2}({3})  Str: {4}({5})  Arm: {6}  Exp: {7}/{8}  {9}",
            level,
            purse,
            pstats.s_hpt,
            max_hp,
            pstats.s_str,
            max_stats.s_str,
            10 - _status_s_arm,
            pstats.s_lvl,
            pstats.s_exp,
            hungry_state_name[hungry_state]);

        if (stat_msg)
        {
            move(0, 0);
            msg(message);
        }
        else
        {
            move(STATLINE, 0);
            printw(message);
        }

        clrtoeol();
        move(oy, ox);
    }

    /// <summary>
    /// Sit around until the guy types the right key
    /// </summary>
    void wait_for(char ch)
    {
        if (ch == '\n')
        {
            char c;

            while ((c = readchar().KeyChar) != '\n' && c != '\r')
                continue;
        }
        else
        {
            while (readchar().KeyChar != ch)
                continue;
        }
    }

    /// <summary>
    /// Function used to display a window and wait before returning
    /// </summary>
    void show_win(string message)
    {
        CursesWindow win = hw;

        wmove(win, 0, 0);
        waddstr(win, message);
        touchwin(win);
        wmove(win, hero.y, hero.x);
        wrefresh(win);
        wait_for(' ');
        clearok(curscr, true);
        touchwin(stdscr);
    }
}
