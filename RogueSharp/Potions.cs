using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    struct PACT
    {
        public PACT(int flags, Action<int>? daemon, int time, string? high = null, string? straight = null)
        {
            pa_flags = flags;
            pa_daemon = daemon;
            pa_time = time;
            pa_high = high;
            pa_straight = straight;
        }

        public int pa_flags { get; }
        public Action<int>? pa_daemon { get; }
        public int pa_time { get; }
        public string? pa_high { get; }
        public string? pa_straight { get; }
    }

    PACT[] p_actions
    {
        get
        {
            return _p_actions ??=
            [
                new ( ISHUH,    unconfuse,  HUHDURATION,            /* P_CONFUSE */
                    "what a trippy feeling!",
                    "wait, what's going on here. Huh? What? Who?" ),
                new ( ISHALU,   come_down,  SEEDURATION,            /* P_LSD */
                    "oh, wow!  Everything seems so cosmic!",
                    "oh, wow!  Everything seems so cosmic!" ),
                new ( 0,        null,   0 ),                        /* P_POISON */
                new ( 0,        null,   0 ),                        /* P_STRENGTH */
                new ( CANSEE,   unsee,  SEEDURATION),               /* P_SEEINVIS */
                new ( 0,        null,   0 ),                        /* P_HEALING */
                new ( 0,        null,   0 ),                        /* P_MFIND */
                new ( 0,        null,   0 ),                        /* P_TFIND  */
                new ( 0,        null,   0 ),                        /* P_RAISE */
                new ( 0,        null,   0 ),                        /* P_XHEAL */
                new ( 0,        null,   0 ),                        /* P_HASTE */
                new ( 0,        null,   0 ),                        /* P_RESTORE */
                new ( ISBLIND,  sight,  SEEDURATION,                /* P_BLIND */
                    "oh, bummer!  Everything is dark!  Help!",
                    "a cloak of darkness falls around you" ),
                new ( ISLEVIT,  land,   HEALTIME,                   /* P_LEVIT */
                    "oh, wow!  You're floating in the air!",
                    "you start to float in the air" )
            ];
        }
    }
    private PACT[]? _p_actions;

    /// <summary>
    /// Quaff a potion from the pack
    /// </summary>
    void quaff()
    {
        THING? obj, tp, mp;
        bool discardit = false;
        bool show, trip;

        obj = get_item("quaff", POTION);
        /*
         * Make certain that it is somethings that we want to drink
         */
        if (obj == null)
            return;
        if (obj.o_type != POTION)
        {
            if (!terse)
                msg("yuk! Why would you want to drink that?");
            else
                msg("that's undrinkable");
            return;
        }
        if (obj == cur_weapon)
            cur_weapon = null;

        /*
         * Calculate the effect it has on the poor guy.
         */
        trip = on(player, ISHALU);
        discardit = (bool) (obj.o_count == 1);
        leave_pack(obj, false, false);
        switch (obj.o_which)
        {
            case P_CONFUSE:
                do_pot(P_CONFUSE, !trip);
                break;

            case P_POISON:
                pot_info[P_POISON].oi_know = true;
                if (ISWEARING(R_SUSTSTR))
                    msg("you feel momentarily sick");
                else
                {
                    chg_str(-(rnd(3) + 1));
                    msg("you feel very sick now");
                    come_down();
                }
                break;

            case P_HEALING:
                pot_info[P_HEALING].oi_know = true;
                if ((pstats.s_hpt += roll(pstats.s_lvl, 4)) > max_hp)
                    pstats.s_hpt = ++max_hp;
                sight();
                msg("you begin to feel better");
                break;

            case P_STRENGTH:
                pot_info[P_STRENGTH].oi_know = true;
                chg_str(1);
                msg("you feel stronger, now.  What bulging muscles!");
                break;

            case P_MFIND:
                player.t_flags |= SEEMONST;
                fuse(turn_see, Convert.ToInt32(true), HUHDURATION, AFTER);
                if (!turn_see(false))
                    msg("you have a %s feeling for a moment, then it passes",
                        choose_str("normal", "strange"));
                break;

            case P_TFIND:
                /*
                 * Potion of magic detection.  Show the potions and scrolls
                 */
                show = false;
                if (lvl_obj != null)
                {
                    wclear(hw);
                    for (tp = lvl_obj; tp != null; tp = next(tp))
                    {
                        if (is_magic(tp))
                        {
                            show = true;
                            wmove(hw, tp.o_pos.y, tp.o_pos.x);
                            waddch(hw, MAGIC);
                            pot_info[P_TFIND].oi_know = true;
                        }
                    }
                    for (mp = mlist; mp != null; mp = next(mp))
                    {
                        for (tp = mp.t_pack; tp != null; tp = next(tp))
                        {
                            if (is_magic(tp))
                            {
                                show = true;
                                wmove(hw, mp.t_pos.y, mp.t_pos.x);
                                waddch(hw, MAGIC);
                            }
                        }
                    }
                }
                if (show)
                {
                    pot_info[P_TFIND].oi_know = true;
                    show_win("You sense the presence of magic on this level.--More--");
                }
                else
                    msg("you have a %s feeling for a moment, then it passes",
                        choose_str("normal", "strange"));
                break;

            case P_LSD:
                if (!trip)
                {
                    if (on(player, SEEMONST))
                        turn_see(false);
                    start_daemon(visuals, 0, BEFORE);
                    seenstairs = seen_stairs();
                }
                do_pot(P_LSD, true);
                break;

            case P_SEEINVIS:
                show = on(player, CANSEE);
                do_pot(P_SEEINVIS, false, $"this potion tastes like {fruit} juice");
                if (!show)
                    invis_on();
                sight();
                break;

            case P_RAISE:
                pot_info[P_RAISE].oi_know = true;
                msg("you suddenly feel much more skillful");
                raise_level();
                break;

            case P_XHEAL:
                pot_info[P_XHEAL].oi_know = true;
                if ((pstats.s_hpt += roll(pstats.s_lvl, 8)) > max_hp)
                {
                    if (pstats.s_hpt > max_hp + pstats.s_lvl + 1)
                        ++max_hp;
                    pstats.s_hpt = ++max_hp;
                }
                sight();
                come_down();
                msg("you begin to feel much better");
                break;

            case P_HASTE:
                pot_info[P_HASTE].oi_know = true;
                after = false;
                if (add_haste(true))
                    msg("you feel yourself moving much faster");
                break;

            case P_RESTORE:
                if (ISRING(LEFT, R_ADDSTR))
                    add_str(ref pstats.s_str, -cur_ring[LEFT]!.o_arm);
                if (ISRING(RIGHT, R_ADDSTR))
                    add_str(ref pstats.s_str, -cur_ring[RIGHT]!.o_arm);
                if (pstats.s_str < max_stats.s_str)
                    pstats.s_str = max_stats.s_str;
                if (ISRING(LEFT, R_ADDSTR))
                    add_str(ref pstats.s_str, cur_ring[LEFT]!.o_arm);
                if (ISRING(RIGHT, R_ADDSTR))
                    add_str(ref pstats.s_str, cur_ring[RIGHT]!.o_arm);
                msg("hey, this tastes great.  It make you feel warm all over");
                break;

            case P_BLIND:
                do_pot(P_BLIND, true);
                break;

            case P_LEVIT:
                do_pot(P_LEVIT, true);
                break;

#if MASTER
            default:
                msg("what an odd tasting potion!");
                return;
#endif
        }

        status();

        /*
         * Throw the item away
         */

        call_it(pot_info[obj.o_which]);

        if (discardit)
            discard(obj);

        return;
    }

    /// <summary>
    /// Returns true if an object radiates magic
    /// </summary>
    bool is_magic(THING obj)
    {
        switch (obj.o_type)
        {
            case ARMOR:
                return (((obj.o_flags & ISPROT) != 0) || obj.o_arm != a_class[obj.o_which]);

            case WEAPON:
                return (obj.o_hplus != 0 || obj.o_dplus != 0);

            case POTION:
            case SCROLL:
            case STICK:
            case RING:
            case AMULET:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Turn on the ability to see invisible
    /// </summary>
    void invis_on()
    {
        player.t_flags |= CANSEE;

        for (THING? mp = mlist; mp != null; mp = next(mp))
        {
            if (on(mp, ISINVIS) && see_monst(mp) && !on(player, ISHALU))
                mvaddch(mp.t_pos.y, mp.t_pos.x, mp.t_disguise);
        }
    }

    /// <summary>
    /// Put on or off seeing monsters on this level
    /// </summary>
    void turn_see(int turn_off) => turn_see(Convert.ToBoolean(turn_off));

    /// <summary>
    /// Put on or off seeing monsters on this level
    /// </summary>
    bool turn_see(bool turn_off)
    {
        bool can_see, add_new;

        add_new = false;
        for (THING? mp = mlist; mp != null; mp = next(mp))
        {
            move(mp.t_pos.y, mp.t_pos.x);
            can_see = see_monst(mp);
            if (turn_off)
            {
                if (!can_see)
                    addch(mp.t_oldch);
            }
            else
            {
                if (!can_see)
                    standout();
                if (!on(player, ISHALU))
                    addch(mp.t_type);
                else
                    addch((char) (rnd(26) + 'A'));
                if (!can_see)
                {
                    standend();
                    add_new = true;
                }
            }
        }

        if (turn_off)
            player.t_flags &= ~SEEMONST;
        else
            player.t_flags |= SEEMONST;

        return add_new;
    }

    /// <summary>
    /// Return true if the player has seen the stairs
    /// </summary>
    bool seen_stairs()
    {
        THING? tp;

        move(stairs.y, stairs.x);
        if (inch() == STAIRS)           /* it's on the map */
            return true;
        if (ce(hero, stairs))           /* It's under him */
            return true;

        /*
         * if a monster is on the stairs, this gets hairy
         */
        if ((tp = moat(stairs.y, stairs.x)) != null)
        {
            if (see_monst(tp) && on(tp, ISRUN))     /* if it's visible and awake */
                return true;                        /* it must have moved there */

            if (on(player, SEEMONST)                /* if she can detect monster */
                && tp.t_oldch == STAIRS)            /* and there once were stairs */
                return true;                        /* it must have moved there */
        }

        return false;
    }

    /// <summary>
    /// The guy just magically went up a level.
    /// </summary>
    void raise_level()
    {
        pstats.s_exp = e_levels[pstats.s_lvl-1] + 1;
        check_level();
    }

    /// <summary>
    /// Do a potion with standard setup.  This means it uses a fuse and turns on a flag
    /// </summary>
    void do_pot(int type, bool knowit, string? message = null)
    {
        PACT pp = p_actions[type];

        if (!pot_info[type].oi_know)
            pot_info[type].oi_know = knowit;

        int t = spread(pp.pa_time);

        if (!on(player, pp.pa_flags))
        {
            player.t_flags |= pp.pa_flags;

            if (pp.pa_daemon != null)
                fuse(pp.pa_daemon, 0, t, AFTER);

            look(false);
        }
        else
        {
            if (pp.pa_daemon != null)
                lengthen(pp.pa_daemon, t);
        }

        msg(choose_str(message ?? pp.pa_high, message ?? pp.pa_straight));
    }
}
