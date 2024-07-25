using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>
    /// used to hold the new hero position
    /// </summary>
    private coord _move_nh;

    /// <summary>
    /// Determines whether <paramref name="key"/> is a movement key
    /// </summary>
    bool IsMovementKey(ConsoleKeyInfo key)
    {
        switch (dirKey.Key)
        {
            // move left
            case ConsoleKey.H:
            case ConsoleKey.LeftArrow:
            case ConsoleKey.NumPad4:
                return true;

            // move down
            case ConsoleKey.J:
            case ConsoleKey.DownArrow:
            case ConsoleKey.NumPad2:
                return true;

            // move up
            case ConsoleKey.K:
            case ConsoleKey.UpArrow:
            case ConsoleKey.NumPad8:
                return true;

            // move right
            case ConsoleKey.L:
            case ConsoleKey.RightArrow:
            case ConsoleKey.NumPad6:
                return true;

            // move diagonally up and left
            case ConsoleKey.Y:
            case ConsoleKey.Home:
            case ConsoleKey.NumPad7:
                return true;

            // move diagonally up and right
            case ConsoleKey.U:
            case ConsoleKey.PageUp:
            case ConsoleKey.NumPad9:
                return true;

            // move diagonally down and right
            case ConsoleKey.N:
            case ConsoleKey.PageDown:
            case ConsoleKey.NumPad3:
                return true;

            // move diagonally down and left
            case ConsoleKey.B:
            case ConsoleKey.End:
            case ConsoleKey.NumPad1:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines the whether <paramref name="key"/> is a movement key
    /// </summary>
    coord GetMovementDelta(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            // move left
            case ConsoleKey.H:
            case ConsoleKey.LeftArrow:
            case ConsoleKey.NumPad4:
                return new coord(0, -1);

            // move down
            case ConsoleKey.J:
            case ConsoleKey.DownArrow:
            case ConsoleKey.NumPad2:
                return new coord(1, 0);

            // move up
            case ConsoleKey.K:
            case ConsoleKey.UpArrow:
            case ConsoleKey.NumPad8:
                return new coord(-1, 0);

            // move right
            case ConsoleKey.L:
            case ConsoleKey.RightArrow:
            case ConsoleKey.NumPad6:
                return new coord(0, 1);

            // move diagonally up and left
            case ConsoleKey.Y:
            case ConsoleKey.Home:
            case ConsoleKey.NumPad7:
                return new coord(-1, -1);

            // move diagonally up and right
            case ConsoleKey.U:
            case ConsoleKey.PageUp:
            case ConsoleKey.NumPad9:
                return new coord(-1, 1);

            // move diagonally down and right
            case ConsoleKey.N:
            case ConsoleKey.PageDown:
            case ConsoleKey.NumPad3:
                return new coord(1, 1);

            // move diagonally down and left
            case ConsoleKey.B:
            case ConsoleKey.End:
            case ConsoleKey.NumPad1:
                return new coord(1, -1);

            default:
                return new coord(0, 0);
        }
    }

    /// <summary>
    /// Start the hero running
    /// </summary>
    void do_run(ConsoleKeyInfo key)
    {
        running = true;
        after = false;
        runKey = key;
    }

    /// <summary>
    /// Check to see that a move is legal.  If it is handle the consequences (fighting, picking up, etc.)
    /// </summary>
    void do_move(coord vector) => do_move(vector.y, vector.x);

    /// <summary>
    /// Check to see that a move is legal.  If it is handle the consequences (fighting, picking up, etc.)
    /// </summary>
    void do_move(int dy, int dx)
    {
        firstmove = false;
        if (no_move != 0)
        {
            no_move--;
            msg("you are still stuck in the bear trap");
            return;
        }

        /*
         * Do a confused move (maybe)
         */
        if (on(player, ISHUH) && rnd(5) != 0)
        {
            _move_nh = rndmove(player);
            if (ce(_move_nh, hero))
            {
                after = false;
                running = false;
                to_death = false;
                return;
            }
        }
        else
        {
            _move_nh.y = hero.y + dy;
            _move_nh.x = hero.x + dx;
        }

over:
        char ch;
        byte fl;

        /*
         * Check if he tried to move off the screen or make an illegal
         * diagonal move, and stop him if he did.
         */
        if (_move_nh.x < 0 || _move_nh.x >= NUMCOLS || _move_nh.y <= 0 || _move_nh.y >= NUMLINES - 1)
        {
            ch = '|';
            fl = 0;
            goto hit_bound;
        }

        if (!diag_ok(hero, _move_nh))
        {
            after = false;
            running = false;
            return;
        }

        if (running && ce(hero, _move_nh))
            after = running = false;

        fl = flat(_move_nh.y, _move_nh.x);
        ch = winat(_move_nh.y, _move_nh.x);

        if (((fl & F_REAL) == 0) && ch == FLOOR)
        {
            if (!on(player, ISLEVIT))
            {
                ch = TRAP;
                set_chat(_move_nh.y, _move_nh.x, TRAP);
                set_flat(_move_nh.y, _move_nh.x, (byte) (fl | F_REAL));
            }
        }
        else if (on(player, ISHELD) && ch != 'F')
        {
            msg("you are being held");
            return;
        }

hit_bound:
        switch (ch)
        {
            case ' ':
            case '|':
            case '-':
                if (passgo && running && ((proom.r_flags & ISGONE) != 0)
                    && !on(player, ISBLIND))
                {
                    bool    b1, b2;

                    switch (runKey.Key)
                    {
                        case ConsoleKey.H:
                        case ConsoleKey.L:
                            b1 = (bool) (hero.y != 1 && turn_ok(hero.y - 1, hero.x));
                            b2 = (bool) (hero.y != NUMLINES - 2 && turn_ok(hero.y + 1, hero.x));
                            
                            if (!(b1 ^ b2))
                                break;
                            
                            if (b1)
                            {
                                runKey = new ConsoleKeyInfo('k', ConsoleKey.K, shift: false, alt: false, control: false);
                                dy = -1;
                            }
                            else
                            {
                                runKey = new ConsoleKeyInfo('j', ConsoleKey.J, shift: false, alt: false, control: false);
                                dy = 1;
                            }

                            dx = 0;
                            break;

                        case ConsoleKey.J:
                        case ConsoleKey.K:
                            b1 = (bool) (hero.x != 0 && turn_ok(hero.y, hero.x - 1));
                            b2 = (bool) (hero.x != NUMCOLS - 1 && turn_ok(hero.y, hero.x + 1));
                            
                            if (!(b1 ^ b2))
                                break;
                            
                            if (b1)
                            {
                                runKey = new ConsoleKeyInfo('h', ConsoleKey.H, shift: false, alt: false, control: false);
                                dx = -1;
                            }
                            else
                            {
                                runKey = new ConsoleKeyInfo('l', ConsoleKey.L, shift: false, alt: false, control: false);
                                dx = 1;
                            }
                            
                            dy = 0;
                            break;
                    }

                    turnref();

                    _move_nh.y = hero.y + dy;
                    _move_nh.x = hero.x + dx;
                    goto over;
                }

                running = false;
                after = false;
                break;

            case DOOR:
                running = false;
                if ((flat(hero.y, hero.x) & F_PASS) != 0)
                    enter_room(_move_nh);

                MoveStuff();
                break;

            case TRAP:
                ch = be_trapped(_move_nh);
                if (ch == T_DOOR || ch == T_TELEP)
                    return;

                MoveStuff();
                break;

            case PASSAGE:
                /*
                 * when you're in a corridor, you don't know if you're in
                 * a maze room or not, and there ain't no way to find out
                 * if you're leaving a maze room, so it is necessary to
                 * always recalculate proom.
                 */
                proom = roomin(hero) ?? rooms[0];
                MoveStuff();
                break;

            case FLOOR:
                if ((fl & F_REAL) == 0)
                    be_trapped(hero);

                MoveStuff();
                break;

            case STAIRS:
                seenstairs = true;
                goto default;

            default:
                running = false;
                if (char.IsUpper(ch) || (moat(_move_nh.y, _move_nh.x) != null))
                {
                    if (cur_weapon != null)
                        fight(_move_nh, cur_weapon, false);
                }
                else
                {
                    if (ch != STAIRS)
                        take = ch;

                    MoveStuff();
                }
                
                break;
        }

        // --- local method ---
        void MoveStuff()
        {
            mvaddch(hero.y, hero.x, floor_at());
            if (((fl & F_PASS) != 0) && chat(oldpos.y, oldpos.x) == DOOR)
                leave_room(_move_nh);
            hero = _move_nh;
        }
    }

    /// <summary>
    /// Decide whether it is legal to turn onto the given space
    /// </summary>
    bool turn_ok(int y, int x)
    {
        PLACE pp = INDEX(y, x);
        return (pp.p_ch == DOOR) || (pp.p_flags & (F_REAL|F_PASS)) == (F_REAL|F_PASS);
    }

    /// <summary>
    /// Decide whether to refresh at a passage turning or not
    /// </summary>
    void turnref()
    {
        PLACE pp = INDEX(hero.y, hero.x);
        if ((pp.p_flags & F_SEEN) == 0)
        {
            if (jump)
            {
                leaveok(stdscr, true);
                refresh();
                leaveok(stdscr, false);
            }

            pp.p_flags |= F_SEEN;
        }
    }

    /// <summary>
    /// Called to illuminate a room.  If it is dark, remove anything that might move.
    /// </summary>
    void door_open(room room)
    {
        if ((room.r_flags & ISGONE) != 0)
            return;

        for (int y = room.r_pos.y; y < room.r_pos.y + room.r_max.y; y++)
        {
            for (int x = room.r_pos.x; x < room.r_pos.x + room.r_max.x; x++)
            {
                if (Char.IsUpper(winat(y, x)))
                    wake_monster(y, x);
            }
        }
    }

    /// <summary>
    /// The guy stepped on a trap.... Make him pay.
    /// </summary>
    char be_trapped(coord tc)
    {
        if (on(player, ISLEVIT))
            return (char) T_RUST;        /* anything that's not a door or teleport */

        running = false;
        count = 0;

        PLACE pp = INDEX(tc.y, tc.x);
        pp.p_ch = TRAP;
        int tr = pp.p_flags & F_TMASK;
        pp.p_flags |= F_SEEN;

        switch ((int) tr)
        {
            case T_DOOR:
                level++;
                new_level();
                msg("you fell into a trap!");
                break;

            case T_BEAR:
                no_move += BEARTIME;
                msg("you are caught in a bear trap");
                break;

            case T_MYST:
                switch (rnd(11))
                {
                    case 0:
                        msg("you are suddenly in a parallel dimension");
                        break;
                    case 1:
                        msg("the light in here suddenly seems %s", rainbow[rnd(cNCOLORS)]);
                        break;
                    case 2:
                        msg("you feel a sting in the side of your neck");
                        break;
                    case 3:
                        msg("multi-colored lines swirl around you, then fade");
                        break;
                    case 4:
                        msg("a %s light flashes in your eyes", rainbow[rnd(cNCOLORS)]);
                        break;
                    case 5:
                        msg("a spike shoots past your ear!");
                        break;
                    case 6:
                        msg("%s sparks dance across your armor", rainbow[rnd(cNCOLORS)]);
                        break;
                    case 7:
                        msg("you suddenly feel very thirsty");
                        break;
                    case 8:
                        msg("you feel time speed up suddenly");
                        break;
                    case 9:
                        msg("time now seems to be going slower");
                        break;
                    case 10: 
                        msg("you pack turns %s!", rainbow[rnd(cNCOLORS)]);
                        break;
                }

                break;

            case T_SLEEP:
                no_command += SLEEPTIME;
                player.t_flags &= ~ISRUN;
                msg("a strange white mist envelops you and you fall asleep");
                break;

            case T_ARROW:
                if (swing(pstats.s_lvl - 1, pstats.s_arm, 1))
                {
                    pstats.s_hpt -= roll(1, 6);
                    if (pstats.s_hpt <= 0)
                    {
                        msg("an arrow killed you");
                        death('a');
                    }
                    else
                        msg("oh no! An arrow shot you");
                }
                else
                {
                    THING arrow = new_item();
                    init_weapon(arrow, ARROW);
                    arrow.o_count = 1;
                    arrow.o_pos = hero;
                    fall(arrow, false);
                    msg("an arrow shoots past you");
                }

                break;

            case T_TELEP:
                /*
                 * since the hero's leaving, look() won't put a TRAP
                 * down for us, so we have to do it ourself
                 */
                teleport();
                mvaddch(tc.y, tc.x, TRAP);
                break;

            case T_DART:
                if (!swing(pstats.s_lvl+1, pstats.s_arm, 1))
                {
                    msg("a small dart whizzes by your ear and vanishes");
                }
                else
                {
                    pstats.s_hpt -= roll(1, 4);
                    
                    if (pstats.s_hpt <= 0)
                    {
                        msg("a poisoned dart killed you");
                        death('d');
                    }

                    if (!ISWEARING(R_SUSTSTR) && !save(VS_POISON))
                        chg_str(-1);
                    
                    msg("a small dart just hit you in the shoulder");
                }

                break;

            case T_RUST:
                msg("a gush of water hits you on the head");
                rust_armor(cur_armor);
                break;
        }

        flush_type();
        return (char) tr;
    }

    /// <summary>
    /// Move in a random direction if the monster/person is confused
    /// </summary>
    coord rndmove(THING who)
    {
        coord ret;  /* what we will be returning */

        int y = ret.y = who.t_pos.y + rnd(3) - 1;
        int x = ret.x = who.t_pos.x + rnd(3) - 1;

        /*
         * Now check to see if that's a legal move.  If not, don't move.
         * (I.e., bump into the wall or whatever)
         */
        if (y == who.t_pos.y && x == who.t_pos.x)
            return ret;

        if (!diag_ok(who.t_pos, ret))
            return who.t_pos;

        char ch = winat(y, x);
        if (!step_ok(ch))
            return who.t_pos;

        if (ch == SCROLL)
        {
            THING? obj;

            for (obj = lvl_obj; obj != null; obj = next(obj))
            {
                if (y == obj.o_pos.y && x == obj.o_pos.x)
                    break;
            }

            if (obj != null && obj.o_which == S_SCARE)
                return who.t_pos;
        }

        return ret;
    }

    /// <summary>
    /// Rust the given armor, if it is a legal kind to rust, and we aren't wearing a magic ring.
    /// </summary>
    void rust_armor(THING? arm)
    {
        if (arm == null || arm.o_type != ARMOR || arm.o_which == LEATHER || arm.o_arm >= 9)
            return;

        if (((arm.o_flags & ISPROT) != 0) || ISWEARING(R_SUSTARM))
        {
            if (!to_death)
                msg("the rust vanishes instantly");
        }
        else
        {
            arm.o_arm++;
            if (!terse)
                msg("your armor appears to be weaker now. Oh my!");
            else
                msg("your armor weakens");
        }
    }
}
