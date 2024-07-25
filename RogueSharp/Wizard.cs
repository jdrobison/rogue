/*
 * Special wizard commands (some of which are also non-wizard commands
 * under strange circumstances)
 *
 * @(#)wizard.c    4.30 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */
using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>
    /// What a certain object is
    /// </summary>
    void whatis(bool insist, int type)
    {
        THING? obj;

        if (pack == null)
        {
            msg("you don't have anything in your pack to identify");
            return;
        }

        for (; ; )
        {
            obj = get_item("identify", type);
            if (insist)
            {
                if (n_objs == 0)
                    return;
                else if (obj == null)
                    msg("you must identify something");
                else if ((type != 0) && obj.o_type != type &&
                   !(type == R_OR_S && (obj.o_type == RING || obj.o_type == STICK)))
                    msg("you must identify a %s", type_name(type));
                else
                    break;
            }
            else
                break;
        }

        if (obj == null)
            return;

        switch (obj.o_type)
        {
            case SCROLL:
                set_know(obj, scr_info);
                break;
            case POTION:
                set_know(obj, pot_info);
                break;
            case STICK:
                set_know(obj, ws_info);
                break;
            case WEAPON:
            case ARMOR:
                obj.o_flags |= ISKNOW;
                break;
            case RING:
                set_know(obj, ring_info);
                break;
        }

        msg(inv_name(obj, false));
    }

    /// <summary>
    /// Set things up when we really know what a thing is
    /// </summary>
    void set_know(THING obj, obj_info[] info)
    {
        info[obj.o_which].oi_know = true;
        info[obj.o_which].oi_guess = null;
        obj.o_flags |= ISKNOW;
    }

    /// <summary>
    /// Return the name of the type
    /// </summary>
    string type_name(int type)
    {
        h_list[] tlist =
        [
            new(POTION, "potion",      false),
            new(SCROLL, "scroll",      false),
            new(FOOD,   "food",        false),
            new(R_OR_S, "ring, wand or staff", false),
            new(RING,   "ring",        false),
            new(STICK,  "wand or staff",   false),
            new(WEAPON, "weapon",      false),
            new(ARMOR,  "suit of armor",   false),
        ];

        foreach (h_list entry in tlist)
        {
            if (type == entry.h_ch)
                return entry.h_desc;
        }

        /* NOTREACHED */
        return string.Empty;
    }

#if MASTER
    /// <summary>
    /// wizard command for getting anything he wants
    /// </summary>
    void create_obj()
    {
        THING obj;
        char ch, bless;

        obj = new_item();
        msg("type of item: ");
        obj.o_type = readchar().KeyChar;
        mpos = 0;
        msg("which %c do you want? (0-f)", obj.o_type);
        obj.o_which = char.IsDigit((ch = readchar().KeyChar)) ? ch - '0' : ch - 'a' + 10;
        obj.o_group = 0;
        obj.o_count = 1;
        mpos = 0;

        if (obj.o_type == WEAPON || obj.o_type == ARMOR)
        {
            msg("blessing? (+,-,n)");
            bless = readchar().KeyChar;
            mpos = 0;
            if (bless == '-')
                obj.o_flags |= ISCURSED;
            if (obj.o_type == WEAPON)
            {
                init_weapon(obj, obj.o_which);
                if (bless == '-')
                    obj.o_hplus -= rnd(3)+1;
                if (bless == '+')
                    obj.o_hplus += rnd(3)+1;
            }
            else
            {
                obj.o_arm = a_class[obj.o_which];
                if (bless == '-')
                    obj.o_arm += rnd(3)+1;
                if (bless == '+')
                    obj.o_arm -= rnd(3)+1;
            }
        }
        else if (obj.o_type == RING)
        {
            switch (obj.o_which)
            {
                case R_PROTECT:
                case R_ADDSTR:
                case R_ADDHIT:
                case R_ADDDAM:
                    msg("blessing? (+,-,n)");
                    bless = readchar().KeyChar;
                    mpos = 0;
                    if (bless == '-')
                        obj.o_flags |= ISCURSED;
                    obj.o_arm = (bless == '-' ? -1 : rnd(2) + 1);
                    break;

                case R_AGGR:
                case R_TELEPORT:
                    obj.o_flags |= ISCURSED;
                    break;
            }
        }
        else if (obj.o_type == STICK)
        {
            fix_stick(obj);
        }
        else if (obj.o_type == GOLD)
        {
            msg("how much?");
            get_num(ref obj.o_goldval, stdscr);
        }

        add_pack(obj, false);
    }
#endif

    /// <summary>
    /// Bamf the hero someplace else
    /// </summary>
    void teleport()
    {
        mvaddch(hero.y, hero.x, floor_at());
        find_floor(null, out coord pos, 0, true);

        if (roomin(pos) != proom)
        {
            leave_room(hero);
            hero = pos;
            enter_room(hero);
        }
        else
        {
            hero = pos;
            look(true);
        }

        mvaddch(hero.y, hero.x, PLAYER);

        /*
         * turn off ISHELD in case teleportation was done while fighting
         * a Flytrap
         */
        if (on(player, ISHELD))
        {
            player.t_flags &= ~ISHELD;
            vf_hit = 0;
            monsters['F'-'A'].m_stats.s_dmg = "000x0";
        }

        no_move = 0;
        count = 0;
        running = false;
        flush_type();
    }

#if MASTER
    /// <summary>
    /// See if user knows password
    /// </summary>
    bool passwd()
    {
#if true
        return true;
#else
        string sp, c;
    static char buf[MAXSTR];

    msg("wizard's Password:");
    mpos = 0;
    sp = buf;
    while ((c = readchar()) != '\n' && c != '\r' && c != ESCAPE)
        if (c == md_killchar())
            sp = buf;
        else if (c == md_erasechar() && sp > buf)
            sp--;
        else
            *sp++ = c;
    if (sp == buf)
        return false;
    *sp = '\0';
    return (strcmp(PASSWD, md_crypt(buf, "mT")) == 0);
#endif
    }

    /// <summary>
    /// Print out the map for the wizard
    /// </summary>
    void show_map()
    {
        wclear(hw);

        for (int y = 1; y < NUMLINES - 1; y++)
        {
            for (int x = 0; x < NUMCOLS; x++)
            {
                byte real = flat(y, x);

                if ((real & F_REAL) == 0)
                    wstandout(hw);

                wmove(hw, y, x);
                waddch(hw, chat(y, x));

                if ((real & F_REAL) == 0)
                    wstandend(hw);
            }
        }

        show_win("---More (level map)---");
    }
#endif
}
