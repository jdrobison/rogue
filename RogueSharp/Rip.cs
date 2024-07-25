/*
 * File for the fun ends
 * Death or a total win
 *
 * @(#)rip.c	4.57 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */
using System.Text;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    private readonly string?[] rip = 
    {
"                       __________\n",
"                      /          \\\n",
"                     /    REST    \\\n",
"                    /      IN      \\\n",
"                   /     PEACE      \\\n",
"                  /                  \\\n",
"                  |                  |\n",
"                  |                  |\n",
"                  |   killed by a    |\n",
"                  |                  |\n",
"                  |       1980       |\n",
"                 *|     *  *  *      | *\n",
"         ________)/\\\\_//(\\/(/\\)/\\//\\/|_)_______\n",
};

    public const bool allscore = true;
    public const int NUMSCORES = 10;
    public int numscores = NUMSCORES;
    private const string Numname = "Ten";

    /// <summary>
    /// Figure score and post it.
    /// </summary>
    void score(int amount, int flags, char monst)
    {
        SCORE[] top_ten;
# if MASTER
        int prflags = 0;
#endif
        string[] reason =
        {
            "killed",
            "quit",
            "A total winner",
            "killed with Amulet"
        };

        //start_score();

        string input = string.Empty;

#if MASTER
        if ((flags >= 0) || wizard)
#else
        if (flags >= 0)
#endif
        {
            mvaddstr(LINES - 1, 0, "[Press return to continue]");
            refresh();
            wgetstr(stdscr, out input);
            endwin();
            Console.WriteLine();
            resetltchars();
            /*
             * free up space to "guarantee" there is space for the top_ten
             */
            delwin(stdscr);
            delwin(curscr);
            if (hw != null)
                delwin(hw);
        }

        top_ten = new SCORE[numscores];

        for (int i = 0; i < top_ten.Length; i++)
        {
            SCORE score = top_ten[i];

            score.sc_score = 0;
            score.sc_name = string.Empty;
            score.sc_flags = (uint) RN;
            score.sc_level = RN;
            score.sc_monster = (ushort) RN;
            score.sc_uid = (uint) RN;
        }

        //signal(SIGINT, SIG_DFL);

#if MASTER
        if (wizard)
        {
            if (input == "names")
                prflags = 1;
            else if (input == "edit")
                prflags = 2;
        }
#endif

        rd_score(top_ten);

        /*
         * Insert her in list if need be
         */
        int scoreIndex = -1;
        if (!noscore)
        {
            uint uid = (uint) md_getuid();
            for (scoreIndex = 0; scoreIndex < top_ten.Length; scoreIndex++)
            {
                if (amount > top_ten[scoreIndex].sc_score)
                    break;
                //else if (!allscore &&        /* only one score per nowin uid */
                //    flags != 2 && top_ten[scoreIndex].sc_uid == uid && top_ten[scoreIndex].sc_flags != 2)
                //    scoreIndex = top_ten.Length;
            }

            if (scoreIndex < top_ten.Length)
            {
                //if (flags != 2 && !allscore)
                //{
                //    for (scoreIndex = scp; scoreIndex<endp; scoreIndex++)
                //    {
                //        if (scoreIndex.sc_uid == uid && scoreIndex.sc_flags != 2)
                //            break;
                //    }
                //    if (scoreIndex >= endp)
                //        scoreIndex = endp - 1;
                //}
                //else
                //{
                //    scoreIndex = endp - 1;
                //}

                int scoresToMove = top_ten.Length - scoreIndex - 1;
                Array.Copy(top_ten, scoreIndex, top_ten, scoreIndex + 1, scoresToMove);

                top_ten[scoreIndex].sc_score = amount;
                top_ten[scoreIndex].sc_name = whoami;
                top_ten[scoreIndex].sc_flags = (uint) flags;
                top_ten[scoreIndex].sc_level = (flags == 2) ? max_level : level;
                top_ten[scoreIndex].sc_monster = monst;
                top_ten[scoreIndex].sc_uid = uid;
            }
        }

        /*
         * Print the list
         */
        if (flags != -1)
            Console.WriteLine();
        Console.WriteLine("Top {0} {1}:", Numname, allscore ? "Scores" : "Rogueists");
        Console.WriteLine("   Score Name");
        for (int i = 0; i < top_ten.Length; i++)
        {
            SCORE scp = top_ten[i];

            if (scp.sc_score != 0)
            {
                //if (scoreIndex == i)
                //    md_raw_standout();
                Console.Write(
                    "{0,2} {1,5} {2}: {3} on level {4}", 
                    i + 1,
                    scp.sc_score, 
                    scp.sc_name, 
                    reason[scp.sc_flags],
                    scp.sc_level);

                if (scp.sc_flags == 0 || scp.sc_flags == 3)
                    Console.Write(" by {0}", killname((char) scp.sc_monster, true));
#if MASTER
                //if (prflags == 1)
                //{
                //    printf(" (%s)", md_getrealname(scp.sc_uid));
                //}
                //else if (prflags == 2)
                //{
                //    fflush(stdout);
                //    (void) fgets(prbuf, 10, stdin);
                //    if (prbuf[0] == 'd')
                //    {
                //        int scoresToMove = top_ten.Length - scoreIndex - 1;
                //        Array.Copy(top_ten, i, top_ten, i - 1, scoresToMove);

                //        scoreIndex = endp - 1;
                //        scoreIndex.sc_score = 0;
                //        scoreIndex.sc_name = string.Empty;
                //        scoreIndex.sc_flags = RN;
                //        scoreIndex.sc_level = RN;
                //        scoreIndex.sc_monster = (ushort) RN;
                //        scp--;
                //    }
                //}

                //else
#endif
                    Console.Write(".");

                //if (sc2 == scp)
                //    md_raw_standend();

                Console.WriteLine();
            }

            else
                break;
        }
        /*
         * Update the list file
         */
        if ((scoreIndex >= 0) && (scoreIndex < top_ten.Length))
        {
            //if (lock_sc())
            //{
            //    fp = signal(SIGINT, SIG_IGN);
                wr_score(top_ten);
            //    unlock_sc();
            //    signal(SIGINT, fp);
            //}
        }
    }

    /// <summary>
    /// Do something really fun break; case he dies
    /// </summary>
    void death(char monst)
    {
        //signal(SIGINT, SIG_IGN);
        purse -= purse / 10;
        //signal(SIGINT, leave);
        clear();
        string killer = killname(monst, false);

        if (!tombstone)
        {
            mvprintw(LINES - 2, 0, "Killed by ");
            killer = killname(monst, false);
            if (monst != 's' && monst != 'h')
                printw("a%s ", vowelstr(killer));
            printw("%s with %d gold", killer, purse);
        }
        else
        {
            DateTime now = DateTime.Now;
            move(8, 0);
            foreach (string ripLine in rip)
            {
                addstr(ripLine);
            }
            mvaddstr(17, center(killer), killer);
            if (monst == 's' || monst == 'h')
                mvaddch(16, 32, ' ');
            else
                mvaddstr(16, 33, vowelstr(killer));
            mvaddstr(14, center(whoami), whoami);
            string s = $"{purse} Au";
            move(15, center(s));
            addstr(s);

            mvaddstr(18, 26, $"{now.Year,4}");
        }
        move(LINES - 1, 0);
        refresh();
        score(purse, amulet ? 3 : 0, monst);
        Console.Write("[Press return to continue]");
        Console.ReadLine();
        my_exit(0);
    }

    /*
     * center:
     *        Return the index to center the given string
     */
    int center(string str) => 28 - ((str.Length + 1) / 2);

    /// <summary>
    /// Code for a winner
    /// </summary>
    void total_winner()
    {
        THING? obj;
        obj_info? op;
        int worth = 0;
        int oldpurse;

        clear();
        standout();
        addstr("                                                               \n");
        addstr("  @   @               @   @           @          @@@  @     @  \n");
        addstr("  @   @               @@ @@           @           @   @     @  \n");
        addstr("  @   @  @@@  @   @   @ @ @  @@@   @@@@  @@@      @  @@@    @  \n");
        addstr("   @@@@ @   @ @   @   @   @     @ @   @ @   @     @   @     @  \n");
        addstr("      @ @   @ @   @   @   @  @@@@ @   @ @@@@@     @   @     @  \n");
        addstr("  @   @ @   @ @  @@   @   @ @   @ @   @ @         @   @  @     \n");
        addstr("   @@@   @@@   @@ @   @   @  @@@@  @@@@  @@@     @@@   @@   @  \n");
        addstr("                                                               \n");
        addstr("     Congratulations, you have made it to the light of day!    \n");
        standend();
        addstr("\nYou have joined the elite ranks of those who have escaped the\n");
        addstr("Dungeons of Doom alive.  You journey home and sell all your loot at\n");
        addstr("a great profit and are admitted to the Fighters' Guild.\n");
        mvaddstr(LINES - 1, 0, "--Press space to continue--");
        refresh();
        wait_for(' ');
        clear();
        mvaddstr(0, 0, "   Worth  Item\n");
        oldpurse = purse;

        for (obj = pack; obj != null; obj = next(obj))
        {
            switch (obj.o_type)
            {
                case FOOD:
                    worth = 2 * obj.o_count;
                    break;

                case WEAPON:
                    worth = weap_info[obj.o_which].oi_worth;
                    worth *= 3 * (obj.o_hplus + obj.o_dplus) + obj.o_count;
                    obj.o_flags |= ISKNOW;
                    break;

                case ARMOR:
                    worth = arm_info[obj.o_which].oi_worth;
                    worth += (9 - obj.o_arm) * 100;
                    worth += (10 * (a_class[obj.o_which] - obj.o_arm));
                    obj.o_flags |= ISKNOW;
                    break;
                
                case SCROLL:
                    worth = scr_info[obj.o_which].oi_worth;
                    worth *= obj.o_count;
                    op = scr_info[obj.o_which];
                    if (!op.oi_know)
                        worth /= 2;
                    op.oi_know = true;
                    break;

                case POTION:
                    worth = pot_info[obj.o_which].oi_worth;
                    worth *= obj.o_count;
                    op = pot_info[obj.o_which];
                    if (!op.oi_know)
                        worth /= 2;
                    op.oi_know = true;
                    break;

                case RING:
                    op = ring_info[obj.o_which];
                    worth = op.oi_worth;
                    if (obj.o_which == R_ADDSTR || obj.o_which == R_ADDDAM ||
                        obj.o_which == R_PROTECT || obj.o_which == R_ADDHIT)
                    {
                        if (obj.o_arm > 0)
                            worth += obj.o_arm * 100;
                        else
                            worth = 10;
                    }
                    if ((obj.o_flags & ISKNOW) == 0)
                        worth /= 2;
                    obj.o_flags |= ISKNOW;
                    op.oi_know = true;
                    break;

                case STICK:
                    op = ws_info[obj.o_which];
                    worth = op.oi_worth;
                    worth += 20 * obj.o_charges;
                    if ((obj.o_flags & ISKNOW) == 0)
                        worth /= 2;
                    obj.o_flags |= ISKNOW;
                    op.oi_know = true;
                    break;

                case AMULET:
                    worth = 1000;
                    break;
            }

            if (worth < 0)
                worth = 0;

            printw("%c) %5d  %s\n", obj.o_packch, worth, inv_name(obj, false));
            purse += worth;
        }
        printw("   %5d  Gold Pieces          ", oldpurse);
        refresh();
        score(purse, 2, ' ');
        my_exit(0);
    }

    /// <summary>
    /// Convert a code to a monster name
    /// </summary>
    string killname(char monst, bool doart)
    {
        string sp;
        bool article;
        h_list[] nlist = {
            new('a',   "arrow",        true),
            new('b',   "bolt",         true),
            new('d',   "dart",         true),
            new('h',   "hypothermia",      false),
            new('s',   "starvation",       false),
        };

        if (Char.IsUpper(monst))
        {
            sp = GetMonsterName(monst);
            article = true;
        }
        else
        {
            sp = "Wally the Wonder Badger";
            article = false;

            foreach (h_list reason in nlist)
            {
                if (reason.h_ch == monst)
                {
                    sp = reason.h_desc;
                    article = reason.h_print;
                    break;
                }
            }
        }

        StringBuilder builder = new(sp.Length + 5);

        if (!doart || !article)
            return sp;

        return $"a{vowelstr(sp)} {sp}";
    }

    /*
     * death_monst:
     *        Return a monster appropriate for a random death.
     */
    char death_monst()
    {
        char[] poss =
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
        'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        'a', 'b', 'h', 'd', 's',
        ' '     // This is provided to generate the "Wally the Wonder Badger" message for killer
    };

        return poss[rnd(poss.Length)];
    }
}
