using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    private const int DRAGONSHOT = 5;    /* one chance in DRAGONSHOT that a dragon will flame */

    static coord ch_ret;                /* Where chasing takes you */

    /// <summary>
    /// Make all the running monsters move.
    /// </summary>
    void runners(int _ = 0)
    {
        THING? nxt;
        bool wastarget;

        for (THING? tp = mlist; tp != null; tp = nxt)
        {
            /* remember this in case the monster's "next" is changed */
            nxt = next(tp);
            if (!on(tp, ISHELD) && on(tp, ISRUN))
            {
                coord orig_pos = tp.t_pos;
                wastarget = on(tp, ISTARGET);
                if (move_monst(tp) == -1)
                    continue;
                if (on(tp, ISFLY) && dist_cp(hero, tp.t_pos) >= 3)
                    move_monst(tp);
                if (wastarget && !ce(orig_pos, tp.t_pos))
                {
                    tp.t_flags &= ~ISTARGET;
                    to_death = false;
                }
            }
        }
        if (has_hit)
        {
            endmsg();
            has_hit = false;
        }
    }

    /// <summary>
    /// Execute a single turn of running for a monster
    /// </summary>
    int move_monst(THING tp)
    {
        if (!on(tp, ISSLOW) || tp.t_turn)
            if (do_chase(tp) == -1)
                return (-1);
        if (on(tp, ISHASTE))
            if (do_chase(tp) == -1)
                return (-1);
        tp.t_turn = !tp.t_turn;
        return (0);
    }

    /// <summary>
    /// Make the monster's new location be the specified one, updating all the relevant state.
    /// </summary>
    void relocate(THING th, coord new_loc)
    {
        room oroom;

        if (!ce(new_loc, th.t_pos))
        {
            mvaddch(th.t_pos.y, th.t_pos.x, th.t_oldch);
            th.t_room = roomin(new_loc) ?? rooms[0];
            set_oldch(th, new_loc);
            oroom = th.t_room;
            set_moat(th.t_pos.y, th.t_pos.x, null);

            if (oroom != th.t_room)
                th.t_dest = find_dest(th);
            th.t_pos = new_loc;
            set_moat(new_loc.y, new_loc.x, th);
        }
        move(new_loc.y, new_loc.x);
        if (see_monst(th))
            addch(th.t_disguise);
        else if (on(player, SEEMONST))
        {
            standout();
            addch(th.t_type);
            standend();
        }
    }

    private coord _do_chase_this;   // Temporary destination for chaser

    /// <summary>
    /// Make one thing chase another.
    /// </summary>
    int do_chase(THING th)
    {
        room rer, ree;    /* room of chaser, room of chasee */
        int mindist = 32767, curdist;
        bool stoprun = false;  /* true means we are there */
        bool door;
        THING? obj;

        rer = th.t_room;        /* Find room of chaser */
        if (on(th, ISGREED) && rer.r_goldval == 0)
            th.t_dest = hero;    /* If gold has been taken, run after hero */
        if (th.t_dest == hero)    /* Find room of chasee */
            ree = proom;
        else
            ree = roomin(th.t_dest) ?? th.t_room;
        /*
         * We don't count doors as inside rooms for _do_chase_this routine
         */
        door = (chat(th.t_pos.y, th.t_pos.x) == DOOR);
/*
 * If the object of our desire is in a different room,
 * and we are not in a corridor, run to the door nearest to
 * our goal.
 */
over:
        if (rer != ree)
        {
            for (int i = 0; i < rer.r_nexits; i++)
            {
                coord pos = rer.r_exit[i];
                curdist = dist_cp(th.t_dest, pos);
                if (curdist < mindist)
                {
                    _do_chase_this = pos;
                    mindist = curdist;
                }
            }
            if (door)
            {
                rer = passages[flat(th.t_pos.y, th.t_pos.x) & F_PNUM];
                door = false;
                goto over;
            }
        }
        else
        {
            _do_chase_this = th.t_dest;
            /*
             * For dragons check and see if (a) the hero is on a straight
             * line from it, and (b) that it is within shooting distance,
             * but outside of striking range.
             */
            if (th.t_type == 'D' && (th.t_pos.y == hero.y || th.t_pos.x == hero.x
                || Math.Abs(th.t_pos.y - hero.y) == Math.Abs(th.t_pos.x - hero.x))
                && dist_cp(th.t_pos, hero) <= BOLT_LENGTH * BOLT_LENGTH
                && !on(th, ISCANC) && rnd(DRAGONSHOT) == 0)
            {
                delta.y = sign(hero.y - th.t_pos.y);
                delta.x = sign(hero.x - th.t_pos.x);
                if (has_hit)
                    endmsg();
                fire_bolt(th.t_pos, delta, "flame");
                running = false;
                count = 0;
                quiet = 0;
                if (to_death && !on(th, ISTARGET))
                {
                    to_death = false;
                    kamikaze = false;
                }
                return (0);
            }
        }
        /*
         * This now contains what we want to run to this time
         * so we run to it.  If we hit it we either want to fight it
         * or stop running
         */
        if (!chase(th, _do_chase_this))
        {
            if (ce(_do_chase_this, hero))
            {
                return (attack(th));
            }
            else if (ce(_do_chase_this, th.t_dest))
            {
                for (obj = lvl_obj; obj != null; obj = next(obj))
                    if (th.t_dest == obj.o_pos)
                    {
                        detach(ref lvl_obj, obj);
                        attach(ref th.t_pack, obj);
                        set_chat(
                            obj.o_pos.y, 
                            obj.o_pos.x,
                            ((th.t_room.r_flags & ISGONE) != 0) ? PASSAGE : FLOOR);

                        th.t_dest = find_dest(th);
                        break;
                    }
                if (th.t_type != 'F')
                    stoprun = true;
            }
        }
        else
        {
            if (th.t_type == 'F')
                return (0);
        }
        relocate(th, ch_ret);
        /*
         * And stop running if need be
         */
        if (stoprun && ce(th.t_pos, th.t_dest))
            th.t_flags &= ~ISRUN;
        return (0);
    }

    /// <summary>
    /// Set the oldch character for the monster
    /// </summary>
    void set_oldch(THING tp, coord cp)
    {
        char sch;

        if (ce(tp.t_pos, cp))
            return;

        sch = tp.t_oldch;
        tp.t_oldch = CCHAR(mvinch(cp.y, cp.x));
        if (!on(player, ISBLIND))
        {
            if ((sch == FLOOR || tp.t_oldch == FLOOR) && ((tp.t_room.r_flags & ISDARK) != 0))
                tp.t_oldch = ' ';
            else if (dist_cp(cp, hero) <= LAMPDIST && see_floor)
                tp.t_oldch = chat(cp.y, cp.x);
        }
    }

    /// <summary>
    /// Return true if the hero can see the monster
    /// </summary>
    bool see_monst(THING mp)
    {
        int y, x;

        if (on(player, ISBLIND))
            return false;
        if (on(mp, ISINVIS) && !on(player, CANSEE))
            return false;
        y = mp.t_pos.y;
        x = mp.t_pos.x;
        if (dist(y, x, hero.y, hero.x) < LAMPDIST)
        {
            if (y != hero.y && x != hero.x &&
                !step_ok(chat(y, hero.x)) && !step_ok(chat(hero.y, x)))
                return false;
            return true;
        }
        if (mp.t_room != proom)
            return false;
        return (mp.t_room.r_flags & ISDARK) == 0;
    }

    /// <summary>
    /// Set a monster running after the hero.
    /// </summary>
    void runto(coord runner)
    {
        /*
         * If we couldn't find him, something is funny
         */
#if MASTER
        if (moat(runner.y, runner.x) is not THING monster)
        {
            msg($"couldn't find monster in runto at {runner}");
            return;
        }
#else
        THING monster = moat(runner.y, runner.x);
#endif

        /*
         * Start the beastie running
         */
        monster.t_flags |= ISRUN;
        monster.t_flags &= ~ISHELD;
        monster.t_dest = find_dest(monster);
    }

    /// <summary>
    /// Find the spot for the chaser(er) to move closer to the 
    /// chasee(ee).  Returns true if we want to keep on chasing later
    /// false if we reach the goal.
    /// </summary>
    bool chase(THING tp, coord ee)
    {
        THING? obj;
        int x, y;
        int curdist, thisdist;
        coord er = tp.t_pos;
        char ch;
        int plcnt = 1;

        /*
         * If the thing is confused, let it move randomly. Invisible
         * Stalkers are slightly confused all of the time, and bats are
         * quite confused all the time
         */
        if ((on(tp, ISHUH)    && rnd(5) != 0) || 
            (tp.t_type == 'P' && rnd(5) == 0) ||
            (tp.t_type == 'B' && rnd(2) == 0))
        {
            /*
             * get a valid random move
             */
            ch_ret = rndmove(tp);
            curdist = dist_cp(ch_ret, ee);
            /*
             * Small chance that it will become un-confused 
             */
            if (rnd(20) == 0)
                tp.t_flags &= ~ISHUH;
        }
        /*
         * Otherwise, find the empty spot next to the chaser that is
         * closest to the chasee.
         */
        else
        {
            int ey, ex;
            /*
             * This will eventually hold where we move to get closer
             * If we can't find an empty spot, we stay where we are.
             */
            curdist = dist_cp(er, ee);
            ch_ret = er;

            ey = er.y + 1;
            if (ey >= NUMLINES - 1)
                ey = NUMLINES - 2;
            ex = er.x + 1;
            if (ex >= NUMCOLS)
                ex = NUMCOLS - 1;

            for (x = er.x - 1; x <= ex; x++)
            {
                if (x < 0)
                    continue;

                coord tryp;
                tryp.x = x;

                for (y = er.y - 1; y <= ey; y++)
                {
                    tryp.y = y;
                    if (!diag_ok(er, tryp))
                        continue;
                    ch = winat(y, x);
                    if (step_ok(ch))
                    {
                        /*
                         * If it is a scroll, it might be a scare monster scroll
                         * so we need to look it up to see what type it is.
                         */
                        if (ch == SCROLL)
                        {
                            for (obj = lvl_obj; obj != null; obj = next(obj))
                            {
                                if (y == obj.o_pos.y && x == obj.o_pos.x)
                                    break;
                            }
                            if (obj != null && obj.o_which == S_SCARE)
                                continue;
                        }
                        /*
                         * It can also be a Xeroc, which we shouldn't step on
                         */
                        if ((obj = moat(y, x)) != null && obj.t_type == 'X')
                            continue;
                        /*
                         * If we didn't find any scrolls at this place or it
                         * wasn't a scare scroll, then this place counts
                         */
                        thisdist = dist(y, x, ee.y, ee.x);
                        if (thisdist < curdist)
                        {
                            plcnt = 1;
                            ch_ret = tryp;
                            curdist = thisdist;
                        }
                        else if (thisdist == curdist && rnd(++plcnt) == 0)
                        {
                            ch_ret = tryp;
                            curdist = thisdist;
                        }
                    }
                }
            }
        }
        return (curdist != 0 && !ce(ch_ret, hero));
    }

    /// <summary>
    /// Find what room some coordinates are in. <see langword="null"/> means they aren't in any room.
    /// </summary>
    room? roomin(coord coord)
    {
        byte flags = flat(coord.y, coord.x);
        if ((flags & F_PASS) != 0)
            return passages[flags & F_PNUM];

        foreach (room room in rooms)
        {
            if (coord.x <= room.r_pos.x + room.r_max.x && room.r_pos.x <= coord.x
             && coord.y <= room.r_pos.y + room.r_max.y && room.r_pos.y <= coord.y)
                return room;
        }

        msg($"in some bizarre place ({coord.y}, {coord.x})");

#if MASTER
        Environment.Exit(1);
#endif

        return null;
    }

    /// <summary>
    /// Check to see if the move is legal if it is diagonal
    /// </summary>
    bool diag_ok(coord sp, coord ep)
    {
        if (ep.x < 0 || ep.x >= NUMCOLS || ep.y <= 0 || ep.y >= NUMLINES - 1)
            return false;
        if (ep.x == sp.x || ep.y == sp.y)
            return true;
        return step_ok(chat(ep.y, sp.x)) && step_ok(chat(sp.y, ep.x));
    }

    /// <summary>
    /// Returns true if the hero can see a certain coordinate.
    /// </summary>
    bool cansee(int y, int x)
    {
        coord pos;

        if (on(player, ISBLIND))
            return false;
        if (dist(y, x, hero.y, hero.x) < LAMPDIST)
        {
            if ((flat(y, x) & F_PASS) != 0)
                if (y != hero.y && x != hero.x && !step_ok(chat(y, hero.x)) && !step_ok(chat(hero.y, x)))
                    return false;
            return true;
        }

        /*
         * We can only see if the hero in the same room as
         * the coordinate and the room is lit or if it is close.
         */
        pos.y = y;
        pos.x = x;

        room? rer = roomin(pos);
        return ((rer == proom) && ((rer.r_flags & ISDARK) == 0));
    }

    /// <summary>
    /// find the proper destination for the monster
    /// </summary>
    coord find_dest(THING tp)
    {
        int prob;

        if ((prob = monsters[tp.t_type - 'A'].m_carry) <= 0 || tp.t_room == proom || see_monst(tp))
            return hero;

        THING? monster = tp;

        for (THING? obj = lvl_obj; obj != null; obj = next(obj))
        {
            if (obj.o_type == SCROLL && obj.o_which == S_SCARE)
                continue;

            if (roomin(obj.o_pos) == monster.t_room && rnd(100) < prob)
            {
                for (monster = mlist; monster != null; monster = next(tp))
                    if (monster.t_dest == obj.o_pos)
                        break;
                if (monster == null)
                    return obj.o_pos;
            }
        }

        return hero;
    }

    /// <summary>
    /// Calculate the "distance" between to points.  Actually,
    /// this calculates d^2, not d, but that's good enough for
    /// our purposes, since it's only used comparitively.
    /// </summary>
    int dist(int y1, int x1, int y2, int x2)
    {
        return (((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));
    }

    /// <summary>
    /// Call <see cref="dist(int, int, int, int)"/> with appropriate arguments for coords
    /// </summary>
    int dist_cp(coord c1, coord c2)
    {
        return dist(c1.y, c1.x, c2.y, c2.x);
    }
}
