﻿/*
 * Read a scroll and let it happen
 *
 * @(#)scrolls.c        4.44 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using RogueSharp.Things.Monsters;

using static System.Runtime.InteropServices.JavaScript.JSType;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>
    /// Read a scroll from the pack and do the appropriate thing
    /// </summary>
    void read_scroll()
    {
        THING? obj;
        PLACE pp;
        int y, x;
        char ch;
        int i;
        bool discardit = false;
        room cur_room;
        THING? orig_obj;

        if ((obj = get_item("read", SCROLL)) == null)
            return;

        if (obj.o_type != SCROLL)
        {
            if (!terse)
                msg("there is nothing on it to read");
            else
                msg("nothing to read");
            return;
        }

        /*
         * Calculate the effect it has on the poor guy.
         */
        if (obj == cur_weapon)
            cur_weapon = null;
        /*
         * Get rid of the thing
         */
        discardit = (bool) (obj.o_count == 1);
        leave_pack(obj, false, false);
        orig_obj = obj;

        switch (obj.o_which)
        {
            case S_CONFUSE:
                /*
                 * Scroll of monster confusion.  Give him that power.
                 */
                player.t_flags |= CANHUH;
                msg("your hands begin to glow %s", pick_color("red"));
                break;
        
            case S_ARMOR:
                if (cur_armor != null)
                {
                    cur_armor.o_arm--;
                    cur_armor.o_flags &= ~ISCURSED;
                    msg("your armor glows %s for a moment", pick_color("silver"));
                }
                break;
            
            case S_HOLD:
                /*
                 * Hold monster scroll.  Stop all monsters within two spaces
                 * from chasing after the hero.
                 */
                ch = '\0';
                for (x = hero.x - 2; x <= hero.x + 2; x++)
                {
                    if (x >= 0 && x < NUMCOLS)
                    {
                        for (y = hero.y - 2; y <= hero.y + 2; y++)
                        {
                            if (y >= 0 && y <= NUMLINES - 1)
                            {
                                THING? monster = moat(y, x);
                                if (monster != null && on(monster, ISRUN))
                                {
                                    monster.t_flags &= ~ISRUN;
                                    monster.t_flags |= ISHELD;
                                    ch++;
                                }
                            }
                        }
                    }
                }

                if (ch != 0)
                {
                    addmsg("the monster");
                    if (ch > 1)
                        addmsg("s around you");
                    addmsg(" freeze");
                    if (ch == 1)
                        addmsg("s");
                    endmsg();
                    scr_info[S_HOLD].oi_know = true;
                }
                else
                {
                    msg("you feel a strange sense of loss");
                }
                break;

            case S_SLEEP:
                /*
                 * Scroll which makes you fall asleep
                 */
                scr_info[S_SLEEP].oi_know = true;
                no_command += rnd(SLEEPTIME) + 4;
                player.t_flags &= ~ISRUN;
                msg("you fall asleep");
                break;

            case S_CREATE:
            {
                coord mp = new(-1, -1);

                /*
                 * Create a monster:
                 * First look in a circle around him, next try his room
                 * break; default give up
                 */
                i = 0;
                for (y = hero.y - 1; y <= hero.y + 1; y++)
                {
                    for (x = hero.x - 1; x <= hero.x + 1; x++)
                    {
                        /*
                         * Don't put a monster in top of the player.
                         */
                        if (y == hero.y && x == hero.x)
                            continue;

                        /*
                         * Or anything else nasty
                         */
                        if (step_ok(ch = winat(y, x)))
                        {
                            if (ch == SCROLL && find_obj(y, x)?.o_which == S_SCARE)
                                continue;

                            if (rnd(++i) == 0)
                            {
                                mp.y = y;
                                mp.x = x;
                            }
                        }
                    }
                }

                if ((i == 0) || (mp.x == -1 && mp.y == -1))
                {
                    msg("you hear a faint cry of anguish in the distance");
                }
                else
                {
                    obj = new_item();
                    new_monster(obj, randmonster(false), mp);
                }
                break;
            }

            case S_ID_POTION:
            case S_ID_SCROLL:
            case S_ID_WEAPON:
            case S_ID_ARMOR:
            case S_ID_R_OR_S:
            {
                char[] id_type = { '\0', '\0', '\0', '\0', '\0', POTION, SCROLL, WEAPON, ARMOR, R_OR_S };

                /*
                 * Identify, let him figure something out
                 */
                scr_info[obj.o_which].oi_know = true;
                msg("this scroll is an %s scroll", scr_info[obj.o_which].oi_name);
                whatis(true, id_type[obj.o_which]);
                break;
            }

            case S_MAP:
                /*
                 * Scroll of magic mapping.
                 */
                scr_info[S_MAP].oi_know = true;
                msg("oh, now this scroll has a map on it");

                /*
                 * take all the things we want to keep hidden out of the window
                 */
                for (y = 1; y < NUMLINES - 1; y++)
                {
                    for (x = 0; x < NUMCOLS; x++)
                    {
                        pp = INDEX(y, x);
                        switch (ch = pp.p_ch)
                        {
                            case DOOR:
                            case STAIRS:
                                break;

                            case '-':
                            case '|':
                                if ((pp.p_flags & F_REAL) == 0)
                                {
                                    ch = pp.p_ch = DOOR;
                                    pp.p_flags |= F_REAL;
                                }
                                break;

                            case ' ':
                                if ((pp.p_flags & F_REAL) != 0)
                                    goto default;
                                pp.p_flags |= F_REAL;
                                ch = pp.p_ch = PASSAGE;
                                goto case PASSAGE;

                            case PASSAGE:
                                if ((pp.p_flags & F_REAL) == 0)
                                    pp.p_ch = PASSAGE;
                                pp.p_flags |= (F_SEEN|F_REAL);
                                ch = PASSAGE;
                                break;

                            case FLOOR:
                                if ((pp.p_flags & F_REAL) != 0)
                                    ch = ' ';
                                else
                                {
                                    ch = TRAP;
                                    pp.p_ch = TRAP;
                                    pp.p_flags |= (F_SEEN|F_REAL);
                                }
                                break;

                            default:
                                if ((pp.p_flags & F_PASS) != 0)
                                    goto case PASSAGE;
                                ch = ' ';
                                break;
                        }

                        if (ch != ' ')
                        {
                            if (pp.p_monst != null)
                                pp.p_monst.t_oldch = ch;
                            if (pp.p_monst == null || !on(player, SEEMONST))
                                mvaddch(y, x, ch);
                        }
                    }
                }
                break;

            case S_FDET:
                /*
                 * Potion of gold detection
                 */
                ch = '\0';
                wclear(hw);

                for (obj = lvl_obj; obj != null; obj = next(obj))
                {
                    if (obj.o_type == FOOD)
                    {
                        ch = ' ';
                        wmove(hw, obj.o_pos.y, obj.o_pos.x);
                        waddch(hw, FOOD);
                    }
                }

                if (ch != '\0')
                {
                    scr_info[S_FDET].oi_know = true;
                    show_win("Your nose tingles and you smell food.--More--");
                }
                else
                {
                    msg("your nose tingles");
                }
                break;

            case S_TELEP:
                /*
                 * Scroll of teleportation:
                 * Make him dissapear and reappear
                 */
                cur_room = proom;
                teleport();
                if (cur_room != proom)
                    scr_info[S_TELEP].oi_know = true;
                break;
    
            case S_ENCH:
                if (cur_weapon == null || cur_weapon.o_type != WEAPON)
                    msg("you feel a strange sense of loss");
                else
                {
                    cur_weapon.o_flags &= ~ISCURSED;
                    if (rnd(2) == 0)
                        cur_weapon.o_hplus++;
                    else
                        cur_weapon.o_dplus++;
                    msg("your %s glows %s for a moment",
                        weap_info[cur_weapon.o_which].oi_name, pick_color("blue"));
                }
                break;
            
            case S_SCARE:
                /*
                 * Reading it is a mistake and produces laughter at her
                 * poor boo boo.
                 */
                msg("you hear maniacal laughter in the distance");
                break;
            
            case S_REMOVE:
                uncurse(cur_armor);
                uncurse(cur_weapon);
                uncurse(cur_ring[LEFT]);
                uncurse(cur_ring[RIGHT]);
                msg(choose_str("you feel in touch with the Universal Onenes",
                       "you feel as if somebody is watching over you"));
                break;
            
            case S_AGGR:
                /*
                 * This scroll aggravates all the monsters on the current
                 * level and sets them running towards the hero
                 */
                aggravate();
                msg("you hear a high pitched humming noise");
                break;
            
            case S_PROTECT:
                if (cur_armor != null)
                {
                    cur_armor.o_flags |= ISPROT;
                    msg("your armor is covered by a shimmering %s shield",
                        pick_color("gold"));
                }
                else
                {
                    msg("you feel a strange sense of loss");
                }
                break;
#if MASTER
            default:
                msg("what a puzzling scroll!");
                return;
#endif
        }

        obj = orig_obj;
        look(true); /* put the result of the scroll on the screen */
        status();

        call_it(scr_info[obj.o_which]);

        if (discardit)
            discard(obj);
    }

    /// <summary>
    /// Uncurse an item
    /// </summary>
    void uncurse(THING? obj)
    {
        if (obj != null)
            obj.o_flags &= ~ISCURSED;
    }
}
