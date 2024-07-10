using RogueSharp.Helpers;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    private int MAXMSG => NUMCOLS - "--More--".Length;
    private int msgbufLength => (2 * MAXMSG) + 1;

    static string msgbuf; 
    static int newpos = 0;

    /// <summary>
    /// Display a message at the top of the screen.
    /// </summary>
    int msg(string fmt, params object[] args)
    {
        /*
         * if the string is "", just clear the line
         */
        if (fmt.Length == 0)
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
    void addmsg(string fmt, params object[] args)
    {
        doadd(fmt, args);
    }

    /// <summary>
    /// Display a new msg (giving him a chance to see the previous one if it is up there with the --More--)
    /// </summary>
    int endmsg()
    {
        if (save_msg)
            huh = msgbuf;
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
                        msgbuf = "";
                        mpos = 0;
                        newpos = 0;
                        return ESCAPE;
                    }
                }
            }
        }

        /*
         * All messages should start with uppercase, except ones that
         * start with a pack addressing character
         */
        if (Char.IsAsciiLetterLower(msgbuf[0]) && !lower_msg && msgbuf[1] != ')')
        {
            mvaddch(0, 0, Char.ToUpper(msgbuf[0]));
            addstr(msgbuf.Substring(1));
        }
        else
        {
            mvaddstr(0, 0, msgbuf);
        }

        clrtoeol();
        mpos = newpos;
        newpos = 0;
        msgbuf = "";
        refresh();
        return ~ESCAPE;
    }

    /// <summary>
    /// Perform an add onto the message buffer
    /// </summary>
    void doadd(string fmt, params object[] args)
    {
        string converted = PrintfHelper.ConvertFormatString(fmt);
        string addendum = string.Format(converted, args);

        if (addendum.Length + newpos >= MAXMSG)
            endmsg();

        msgbuf += addendum;
        newpos = msgbuf.Length;
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

#if false
    /*
     * status:
     *	Display the important stats line.  Keep the cursor where it was.
     */
    void
    status()
    {
        register int oy, ox, temp;
        static int hpwidth = 0;
        static int s_hungry = 0;
        static int s_lvl = 0;
        static int s_pur = -1;
        static int s_hp = 0;
        static int s_arm = 0;
        static str_t s_str = 0;
        static int s_exp = 0;
        static char *state_name[] =
    {
    "", "Hungry", "Weak", "Faint"
    };

        /*
         * If nothing has changed since the last status, don't
         * bother.
         */
        temp = (cur_armor != NULL ? cur_armor->o_arm : pstats.s_arm);
        if (s_hp == pstats.s_hpt && s_exp == pstats.s_exp && s_pur == purse
        && s_arm == temp && s_str == pstats.s_str && s_lvl == level
        && s_hungry == hungry_state
        && !stat_msg
        )
            return;

        s_arm = temp;

        getyx(stdscr, oy, ox);
        if (s_hp != max_hp)
        {
            temp = max_hp;
            s_hp = max_hp;
            for (hpwidth = 0; temp; hpwidth++)
                temp /= 10;
        }

        /*
         * Save current status
         */
        s_lvl = level;
        s_pur = purse;
        s_hp = pstats.s_hpt;
        s_str = pstats.s_str;
        s_exp = pstats.s_exp;
        s_hungry = hungry_state;

        if (stat_msg)
        {
            move(0, 0);
            msg("Level: %d  Gold: %-5d  Hp: %*d(%*d)  Str: %2d(%d)  Arm: %-2d  Exp: %d/%ld  %s",
            level, purse, hpwidth, pstats.s_hpt, hpwidth, max_hp, pstats.s_str,
            max_stats.s_str, 10 - s_arm, pstats.s_lvl, pstats.s_exp,
            state_name[hungry_state]);
        }
        else
        {
            move(STATLINE, 0);

            printw("Level: %d  Gold: %-5d  Hp: %*d(%*d)  Str: %2d(%d)  Arm: %-2d  Exp: %d/%d  %s",
            level, purse, hpwidth, pstats.s_hpt, hpwidth, max_hp, pstats.s_str,
            max_stats.s_str, 10 - s_arm, pstats.s_lvl, pstats.s_exp,
            state_name[hungry_state]);
        }

        clrtoeol();
        move(oy, ox);
    }
#endif

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

#if false
    /*
     * show_win:
     *	Function used to display a window and wait before returning
     */
    void
    show_win(char* message)
    {
        WINDOW *win;

        win = hw;
        wmove(win, 0, 0);
        waddstr(win, message);
        touchwin(win);
        wmove(win, hero.y, hero.x);
        wrefresh(win);
        wait_for(' ');
        clearok(curscr, TRUE);
        touchwin(stdscr);
    }
#endif
}
