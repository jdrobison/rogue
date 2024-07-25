﻿using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>
    /// A do-nothing daemon
    /// </summary>
    void noop(int _ = 0)
    {
    }

    /// <summary>
    /// A healing daemon that restores hit points after rest
    /// </summary>
    void doctor(int _ = 0)
    {
        int lv, ohp;

        lv = pstats.s_lvl;
        ohp = pstats.s_hpt;
        quiet++;
        if (lv < 8)
        {
            if (quiet + (lv << 1) > 20)
                pstats.s_hpt++;
        }
        else
        if (quiet >= 3)
            pstats.s_hpt += rnd(lv - 7) + 1;
        if (ISRING(LEFT, R_REGEN))
            pstats.s_hpt++;
        if (ISRING(RIGHT, R_REGEN))
            pstats.s_hpt++;
        if (ohp != pstats.s_hpt)
        {
            if (pstats.s_hpt > max_hp)
                pstats.s_hpt = max_hp;
            quiet = 0;
        }
    }

    /// <summary>
    /// Called when it is time to start rolling for wandering monsters
    /// </summary>
    void swander(int _ = 0)
    {
        start_daemon(rollwand, 0, BEFORE);
    }

    int rollwand_between = 0;

    /// <summary>
    /// Called to roll to see if a wandering monster starts up
    /// </summary>
    void rollwand(int _ = 0)
    {
        if (++rollwand_between >= 4)
        {
            if (roll(1, 6) == 4)
            {
                wanderer();
                kill_daemon(rollwand);
                fuse(swander, 0, WANDERTIME, BEFORE);
            }

            rollwand_between = 0;
        }
    }

    /// <summary>
    /// Release the poor player from his confusion
    /// </summary>
    void unconfuse(int _ = 0)
    {
        player.t_flags &= ~ISHUH;
        msg("you feel less %s now", choose_str("trippy", "confused"));
    }

    /// <summary>
    /// Turn off the ability to see invisible
    /// </summary>
    void unsee(int _ = 0)
    {
        THING? th;

        for (th = mlist; th != null; th = next(th))
        {
            if (on(th, ISINVIS) && see_monst(th))
                mvaddch(th.t_pos.y, th.t_pos.x, th.t_oldch);
        }

        player.t_flags &= ~CANSEE;
    }

    /// <summary>
    /// He gets his sight back
    /// </summary>
    void sight(int _ = 0)
    {
        if (on(player, ISBLIND))
        {
            extinguish(sight);
            player.t_flags &= ~ISBLIND;
            if ((proom.r_flags & ISGONE) == 0)
                enter_room(hero);
            msg(choose_str("far out!  Everything is all cosmic again",
                       "the veil of darkness lifts"));
        }
    }

    /// <summary>
    /// End the hasting
    /// </summary>
    void nohaste(int _ = 0)
    {
        player.t_flags &= ~ISHASTE;
        msg("you feel yourself slowing down");
    }

    /// <summary>
    /// Digest the hero's food
    /// </summary>
    void stomach(int _ = 0)
    {
        int oldfood;
        int orig_hungry = hungry_state;

        if (food_left <= 0)
        {
            if (food_left-- < -STARVETIME)
                death('s');
            /*
             * the hero is fainting
             */
            if ((no_command != 0) || (rnd(5) != 0))
                return;
            no_command += rnd(8) + 4;
            hungry_state = 3;
            
            if (!terse)
            {
                addmsg(choose_str("the munchies overpower your motor capabilities.  ",
                          "you feel too weak from lack of food.  ")!);
            }

            msg(choose_str("You freak out", "You faint"));
        }
        else
        {
            oldfood = food_left;
            food_left -= ring_eat(LEFT) + ring_eat(RIGHT) + 1 - Convert.ToInt32(amulet);

            if (food_left < MORETIME && oldfood >= MORETIME)
            {
                hungry_state = 2;
                msg(choose_str("the munchies are interfering with your motor capabilites",
                       "you are starting to feel weak"));
            }
            else if (food_left < 2 * MORETIME && oldfood >= 2 * MORETIME)
            {
                hungry_state = 1;
                if (terse)
                    msg(choose_str("getting the munchies", "getting hungry"));
                else
                    msg(choose_str("you are getting the munchies",
                               "you are starting to get hungry"));
            }
        }

        if (hungry_state != orig_hungry)
        {
            player.t_flags &= ~ISRUN;
            running = false;
            to_death = false;
            count = 0;
        }
    }

    /// <summary>
    /// Take the hero down off her acid trip.
    /// </summary>
    void come_down(int _ = 0)
    {
        bool seemonst;

        if (!on(player, ISHALU))
            return;

        kill_daemon(visuals);
        player.t_flags &= ~ISHALU;

        if (on(player, ISBLIND))
            return;

        /*
         * undo the things
         */
        for (THING? tp = lvl_obj; tp != null; tp = next(tp))
        {
            if (cansee(tp.o_pos.y, tp.o_pos.x))
                mvaddch(tp.o_pos.y, tp.o_pos.x, (char) tp.o_type);
        }

        /*
         * undo the monsters
         */
        seemonst = on(player, SEEMONST);
        for (THING? tp = mlist; tp != null; tp = next(tp))
        {
            move(tp.t_pos.y, tp.t_pos.x);
            if (cansee(tp.t_pos.y, tp.t_pos.x))
            {
                if (!on(tp, ISINVIS) || on(player, CANSEE))
                    addch(tp.t_disguise);
                else
                    addch(chat(tp.t_pos.y, tp.t_pos.x));
            }
            else if (seemonst)
            {
                standout();
                addch(tp.t_type);
                standend();
            }
        }

        msg("Everything looks SO boring now.");
    }

    /// <summary>
    /// Change the characters for the player
    /// </summary>
    void visuals(int _ = 0)
    {
        bool seemonst;

        if (!after || (running && jump))
            return;
        /*
         * change the things
         */
        for (THING? tp = lvl_obj; tp != null; tp = next(tp))
        {
            if (cansee(tp.o_pos.y, tp.o_pos.x))
                mvaddch(tp.o_pos.y, tp.o_pos.x, rnd_thing());
        }

        /*
         * change the stairs
         */
        if (!seenstairs && cansee(stairs.y, stairs.x))
            mvaddch(stairs.y, stairs.x, rnd_thing());

        /*
         * change the monsters
         */
        seemonst = on(player, SEEMONST);
        for (THING? tp = mlist; tp != null; tp = next(tp))
        {
            move(tp.t_pos.y, tp.t_pos.x);
            if (see_monst(tp))
            {
                if (tp.t_type == 'X' && tp.t_disguise != 'X')
                    addch(rnd_thing());
                else
                    addch((char)(rnd(26) + 'A'));
            }
            else if (seemonst)
            {
                standout();
                addch((char)(rnd(26) + 'A'));
                standend();
            }
        }
    }

    /// <summary>
    /// *	Land from a levitation potion
    /// </summary>
    void land(int _ = 0)
    {
        player.t_flags &= ~ISLEVIT;
        msg(choose_str("bummer!  You've hit the ground", "you float gently to the ground"));
    }
}
