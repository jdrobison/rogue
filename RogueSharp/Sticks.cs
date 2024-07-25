﻿/*
 * Functions to implement the various sticks one might find
 * while wandering around the dungeon.
 *
 * @(#)sticks.c	4.39 (Berkeley) 02/05/99
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
    /// Set up a new stick
    /// </summary>
    void fix_stick(THING stick)
    {
        if (ws_type[stick.o_which] == "staff")
            stick.o_damage = "2x3";
        else
            stick.o_damage = "1x1";

        stick.o_hurldmg = "1x1";

        switch (stick.o_which)
        {
            case WS_LIGHT:
                stick.o_charges = rnd(10) + 10;
                break;
            default:
                stick.o_charges = rnd(5) + 3;
                break;
        }
    }

    /// <summary>
    /// Perform a zap with a wand
    /// </summary>
    void do_zap()
    {
        THING? obj, tp;
        int y, x;
        string name;
        char monster, oldch;

        if ((obj = get_item("zap with", STICK)) == null)
            return;

        if (obj.o_type != STICK)
        {
            after = false;
            msg("you can't zap with that!");
            return;
        }

        if (obj.o_charges == 0)
        {
            msg("nothing happens");
            return;
        }

        switch (obj.o_which)
        {
            case WS_LIGHT:
                /*
                 * Reddy Kilowat wand.  Light up the room
                 */
                ws_info[WS_LIGHT].oi_know = true;
                if ((proom.r_flags & ISGONE) != 0)
                    msg("the corridor glows and then fades");
                else
                {
                    proom.r_flags &= ~ISDARK;
                    /*
                     * Light the room and put the player back up
                     */
                    enter_room(hero);
                    addmsg("the room is lit");
                    if (!terse)
                        addmsg(" by a shimmering %s light", pick_color("blue"));
                    endmsg();
                }

                break;

            case WS_DRAIN:
                /*
                 * take away 1/2 of hero's hit points, then take it away
                 * evenly from the monsters in the room (or next to hero
                 * if he is in a passage)
                 */
                if (pstats.s_hpt < 2)
                {
                    msg("you are too weak to use it");
                    return;
                }
                else
                    drain();
                break;

            case WS_INVIS:
            case WS_POLYMORPH:
            case WS_TELAWAY:
            case WS_TELTO:
            case WS_CANCEL:
                y = hero.y;
                x = hero.x;
                
                while (step_ok(winat(y, x)))
                {
                    y += delta.y;
                    x += delta.x;
                }

                if ((tp = moat(y, x)) != null)
                {
                    monster = tp.t_type;
                
                    if (monster == 'F')
                        player.t_flags &= ~ISHELD;
                    
                    switch (obj.o_which)
                    {
                        case WS_INVIS:
                            tp.t_flags |= ISINVIS;
                            if (cansee(y, x))
                                mvaddch(y, x, tp.t_oldch);
                            break;

                        case WS_POLYMORPH:
                        {
                            THING? pp;

                            pp = tp.t_pack;
                            detach(ref mlist, tp);
                            if (see_monst(tp))
                                mvaddch(y, x, chat(y, x));
                            oldch = tp.t_oldch;
                            delta.y = y;
                            delta.x = x;
                            new_monster(tp, monster = (char) (rnd(26) + 'A'), delta);
                            if (see_monst(tp))
                                mvaddch(y, x, monster);
                            tp.t_oldch = oldch;
                            tp.t_pack = pp;
                            ws_info[WS_POLYMORPH].oi_know |= see_monst(tp);
                            break;
                        }

                        case WS_CANCEL:
                            tp.t_flags |= ISCANC;
                            tp.t_flags &= ~(ISINVIS|CANHUH);
                            tp.t_disguise = tp.t_type;
                            if (see_monst(tp))
                                mvaddch(y, x, tp.t_disguise);
                            break;

                        case WS_TELAWAY:
                        case WS_TELTO:
                        {
                            coord new_pos;

                            if (obj.o_which == WS_TELAWAY)
                            {
                                do
                                {
                                    find_floor(null, out new_pos, 0, true);
                                } while (ce(new_pos, hero));
                            }
                            else
                            {
                                new_pos.y = hero.y + delta.y;
                                new_pos.x = hero.x + delta.x;
                            }

                            tp.t_dest = hero;
                            tp.t_flags |= ISRUN;
                            relocate(tp, new_pos);
                            break;
                        }
                    }
                }

                break;

            case WS_MISSILE:
            {
                ws_info[WS_MISSILE].oi_know = true;

                THING bolt = new();
                bolt.o_type = '*';
                bolt.o_hurldmg = "1x4";
                bolt.o_hplus = 100;
                bolt.o_dplus = 1;
                bolt.o_flags = ISMISL;
                
                if (cur_weapon != null)
                    bolt.o_launch = cur_weapon.o_which;
                
                do_motion(bolt, delta.y, delta.x);
                
                if ((tp = moat(bolt.o_pos.y, bolt.o_pos.x)) != null && !save_throw(VS_MAGIC, tp))
                    hit_monster(bolt.o_pos.y, bolt.o_pos.x, bolt);
                else if (terse)
                    msg("missle vanishes");
                else
                    msg("the missle vanishes with a puff of smoke");
                
                break;
            }

            case WS_HASTE_M:
            case WS_SLOW_M:
                y = hero.y;
                x = hero.x;
                
                while (step_ok(winat(y, x)))
                {
                    y += delta.y;
                    x += delta.x;
                }
                
                if ((tp = moat(y, x)) != null)
                {
                    if (obj.o_which == WS_HASTE_M)
                    {
                        if (on(tp, ISSLOW))
                            tp.t_flags &= ~ISSLOW;
                        else
                            tp.t_flags |= ISHASTE;
                    }
                    else
                    {
                        if (on(tp, ISHASTE))
                            tp.t_flags &= ~ISHASTE;
                        else
                            tp.t_flags |= ISSLOW;
                        tp.t_turn = true;
                    }
                
                    delta.y = y;
                    delta.x = x;
                    runto(delta);
                }
                
                break;

            case WS_ELECT:
            case WS_FIRE:
            case WS_COLD:
                if (obj.o_which == WS_ELECT)
                    name = "bolt";
                else if (obj.o_which == WS_FIRE)
                    name = "flame";
                else
                    name = "ice";
                fire_bolt(hero, delta, name);
                ws_info[obj.o_which].oi_know = true;
                break;

            case WS_NOP:
                break;

#if MASTER
            default:
                msg("what a bizarre schtick!");
                break;
#endif
        }

        obj.o_charges--;
    }

    /*
     * drain:
     */
    /// <summary>
    /// Do drain hit points from player shtick
    /// </summary>
    void drain()
    {
        room? corp;
        int cnt;
        bool inpass;
        THING?[] drainee = new THING[40];
        int draineeIndex = 0;

        /*
         * First count how many things we need to spread the hit points among
         */
        cnt = 0;
        if (chat(hero.y, hero.x) == DOOR)
            corp = passages[flat(hero.y, hero.x) & F_PNUM];
        else
            corp = null;

        inpass = (proom.r_flags & ISGONE) != 0;

        for (THING? monster = mlist; monster != null; monster = next(monster))
        {
            if (monster.t_room == proom || monster.t_room == corp ||
                (inpass && chat(monster.t_pos.y, monster.t_pos.x) == DOOR &&
                passages[flat(monster.t_pos.y, monster.t_pos.x) & F_PNUM] == proom))
            {
                drainee[draineeIndex++] = monster;
            }
        }

        if ((cnt = draineeIndex) == 0)
        {
            msg("you have a tingling feeling");
            return;
        }

        pstats.s_hpt /= 2;
        cnt = pstats.s_hpt / cnt;

        /*
         * Now zot all of the monsters
         */
        for (int i = 0; i < cnt; i++)
        {
            THING monster = drainee[i]!;

            if ((monster.t_stats.s_hpt -= cnt) <= 0)
                killed(monster, see_monst(monster));
            else
                runto(monster.t_pos);
        }
    }

    /// <summary>
    /// Fire a bolt in a given direction from a specific starting place
    /// </summary>
    void fire_bolt(coord start, coord dir, string name)
    {
        THING? tp;
        char dirch = '\0', ch;
        bool hit_hero, used, changed;
        coord pos;
        coord[] spotpos = new coord[BOLT_LENGTH];
        THING bolt = new();

        bolt.o_type = WEAPON;
        bolt.o_which = FLAME;
        bolt.o_hurldmg = "6x6";
        bolt.o_hplus = 100;
        bolt.o_dplus = 0;
        weap_info[FLAME].oi_name = name;
        
        switch (dir.y + dir.x)
        {
            case 0:
                dirch = '/';
                break;
            case 1:
            case -1:
                dirch = (dir.y == 0 ? '-' : '|');
                break;
            case 2:
            case -2:
                dirch = '\\';
                break;
        }
        
        pos = start;
        hit_hero = (start != hero);
        used = false;
        changed = false;

        int i;
        
        for (i = 0; i < spotpos.Length && !used; i++)
        {
            pos.y += dir.y;
            pos.x += dir.x;
            spotpos[i] = pos;
            ch = winat(pos.y, pos.x);

            switch (ch)
            {
                case DOOR:
                    /*
                     * this code is necessary if the hero is on a door
                     * and he fires at the wall the door is in, it would
                     * otherwise loop infinitely
                     */
                    if (ce(hero, pos))
                        goto default;
                    goto case '|';

                case '|':
                case '-':
                case ' ':
                    if (!changed)
                        hit_hero = !hit_hero;
                    changed = false;
                    dir.y = -dir.y;
                    dir.x = -dir.x;
                    i--;
                    msg("the %s bounces", name);
                    break;

                default:
                    if (!hit_hero && (tp = moat(pos.y, pos.x)) != null)
                    {
                        hit_hero = true;
                        changed = !changed;
                        tp.t_oldch = chat(pos.y, pos.x);
                        if (!save_throw(VS_MAGIC, tp))
                        {
                            bolt.o_pos = pos;
                            used = true;
                            if (tp.t_type == 'D' && (name == "flame"))
                            {
                                addmsg("the flame bounces");
                                if (!terse)
                                    addmsg(" off the dragon");
                                endmsg();
                            }
                            else
                                hit_monster(pos.y, pos.x, bolt);
                        }
                        else if (ch != 'M' || tp.t_disguise == 'M')
                        {
                            if (start == hero)
                                runto(pos);
                            if (terse)
                                msg("%s misses", name);
                            else
                                msg("the %s whizzes past %s", name, set_mname(tp));
                        }
                    }
                    else if (hit_hero && ce(pos, hero))
                    {
                        hit_hero = false;
                        changed = !changed;
                        if (!save(VS_MAGIC))
                        {
                            if ((pstats.s_hpt -= roll(6, 6)) <= 0)
                            {
                                if (start == hero)
                                    death('b');
                                else
                                    death(moat(start.y, start.x)?.t_type ?? 'D');
                            }

                            used = true;
                            
                            if (terse)
                                msg("the %s hits", name);
                            else
                                msg("you are hit by the %s", name);
                        }
                        else
                        {
                            msg("the %s whizzes by you", name);
                        }
                    }

                    mvaddch(pos.y, pos.x, dirch);
                    refresh();
                    break;
            }
        }
        
        for (int j = 0; j < i; j++)
        {
            coord c2 = spotpos[j];
            mvaddch(c2.y, c2.x, chat(c2.y, c2.x));
        }
    }

    /// <summary>
    /// Return an appropriate string for a wand charge
    /// </summary>
    string charge_str(THING obj)
    {
        if ((obj.o_flags & ISKNOW) == 0)
            return string.Empty;
        else if (terse)
            return $" [{obj.o_charges}]";
        else
            return $" [{obj.o_charges} charges]";
    }
}
