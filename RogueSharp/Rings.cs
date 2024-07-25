﻿/*
 * Routines dealing specifically with rings
 *
 * @(#)rings.c	4.19 (Berkeley) 05/29/83
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */
namespace RogueSharp;

internal partial class Program
{
    /// <summary>
    /// Put a ring on a hand
    /// </summary>
    void ring_on()
    {
        THING? obj;
        int ring;

        obj = get_item("put on", RING);
        /*
         * Make certain that it is somethings that we want to wear
         */
        if (obj == null)
            return;
        if (obj.o_type != RING)
        {
            if (!terse)
                msg("it would be difficult to wrap that around a finger");
            else
                msg("not a ring");
            return;
        }

        /*
         * find out which hand to put it on
         */
        if (is_current(obj))
            return;

        if (cur_ring[LEFT] == null && cur_ring[RIGHT] == null)
        {
            if ((ring = gethand()) < 0)
                return;
        }
        else if (cur_ring[LEFT] == null)
            ring = LEFT;
        else if (cur_ring[RIGHT] == null)
            ring = RIGHT;
        else
        {
            if (!terse)
                msg("you already have a ring on each hand");
            else
                msg("wearing two");
            return;
        }

        cur_ring[ring] = obj;

        /*
         * Calculate the effect it has on the poor guy.
         */
        switch (obj.o_which)
        {
            case R_ADDSTR:
                chg_str(obj.o_arm);
                break;
            case R_SEEINVIS:
                invis_on();
                break;
            case R_AGGR:
                aggravate();
                break;
        }

        if (!terse)
            addmsg("you are now wearing ");
        msg("%s (%c)", inv_name(obj, true), obj.o_packch);
    }

    /// <summary>
    /// take off a ring
    /// </summary>
    void ring_off()
    {
        int ring;
        THING? obj;

        if (cur_ring[LEFT] == null && cur_ring[RIGHT] == null)
        {
            if (terse)
                msg("no rings");
            else
                msg("you aren't wearing any rings");
            return;
        }
        else if (cur_ring[LEFT] == null)
            ring = RIGHT;
        else if (cur_ring[RIGHT] == null)
            ring = LEFT;
        else if ((ring = gethand()) < 0)
            return;

        mpos = 0;
        obj = cur_ring[ring];
        if (obj == null)
        {
            msg("not wearing such a ring");
            return;
        }
        
        if (dropcheck(obj))
            msg("was wearing %s(%c)", inv_name(obj, true), obj.o_packch);
    }

    /// <summary>
    /// Which hand is the hero interested in?
    /// </summary>
    int gethand()
    {
        int c;

        for (; ; )
        {
            if (terse)
                msg("left or right ring? ");
            else
                msg("left hand or right hand? ");
            if ((c = readchar().KeyChar) == ESCAPE)
                return -1;

            mpos = 0;

            if (c == 'l' || c == 'L')
                return LEFT;
            else if (c == 'r' || c == 'R')
                return RIGHT;

            if (terse)
                msg("L or R");
            else
                msg("please type L or R");
        }
    }

    private readonly int[] _ring_eat_uses = 
    {
         1,     // R_PROTECT
         1,     // R_ADDSTR
         1,     // R_SUSTSTR
        -3,     // R_SEARCH
        -5,     // R_SEEINVIS
         0,     // R_NOP
         0,     // R_AGGR
        -3,     // R_ADDHIT
        -3,     // R_ADDDAM
         2,     // R_REGEN
        -2,     // R_DIGEST
         0,     // R_TELEPORT
         1,     // R_STEALTH
         1      // R_SUSTARM
    };

    /// <summary>
    /// How much food does this ring use up?
    /// </summary>
    int ring_eat(int hand)
    {
        THING? ring = cur_ring[hand];
        if (ring is null)
            return 0;

        int eat = _ring_eat_uses[ring.o_which];

        if (eat < 0)
            eat = Convert.ToInt32(rnd(-eat) == 0);
        if (ring.o_which == R_DIGEST)
            eat = -eat;

        return eat;
    }

    /// <summary>
    /// Print ring bonuses
    /// </summary>
    string ring_num(THING obj)
    {
        if ((obj.o_flags & ISKNOW) == 0)
            return "";

        switch (obj.o_which)
        {
            case R_PROTECT:
            case R_ADDSTR:
            case R_ADDDAM:
            case R_ADDHIT:
                return $" [{num(obj.o_arm, 0, RING)}]";
            default:
                return string.Empty;
        }
    }
}
