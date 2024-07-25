/*
 * Read and execute the user commands
 *
 * @(#)command.c    4.73 (Berkeley) 08/06/83
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */
using System.ComponentModel.Design;

using RogueSharp.Helpers;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    private ConsoleKeyInfo _command_countKey;
    private ConsoleKeyInfo _command_direction;
    private bool _command_newcount;

    /// <summary>
    /// Process the user commands
    /// </summary>
    void command()
    {
        ConsoleKeyInfo key;
        int ntimes = 1;            /* Number of player moves */
        THING? mp;

        if (on(player, ISHASTE))
            ntimes++;

        /*
         * Let the daemons start up
         */
        do_daemons(BEFORE);
        do_fuses(BEFORE);

        while (ntimes-- != 0)
        {
            again = false;

            if (has_hit)
            {
                endmsg();
                has_hit = false;
            }

            /*
             * these are illegal things for the player to be, so if any are
             * set, someone's been poking in memeory
             */
            if (on(player, ISSLOW|ISGREED|ISINVIS|ISREGEN|ISTARGET))
                Environment.Exit(1);

            look(true);
            if (!running)
                door_stop = false;
            status();
            lastscore = purse;
            move(hero.y, hero.x);
            if (!((running || (count != 0)) && jump))
                refresh();          /* Draw screen */
            take = '\0';
            after = true;

            /*
             * Read command or continue run
             */
# if MASTER
            if (wizard)
                noscore = true;
#endif
            if (no_command == 0)
            {
                if (running || to_death)
                    key = runKey;
                else if (count != 0)
                    key = _command_countKey;
                else
                {
                    key = readchar();
                    move_on = false;
                    if (mpos != 0)      /* Erase message if its there */
                        msg("");
                }
            }
            else
                key = new ConsoleKeyInfo('.', ConsoleKey.OemPeriod, shift: false, alt: false, control: false);

            bool shift   = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
            bool alt     = key.Modifiers.HasFlag(ConsoleModifiers.Alt);
            bool control = key.Modifiers.HasFlag(ConsoleModifiers.Control);

            if (no_command != 0)
            {
                if (--no_command == 0)
                {
                    player.t_flags |= ISRUN;
                    msg("you can move again");
                }
            }
            else
            {
                /*
                 * check for prefixes
                 */
                _command_newcount = false;
                if (char.IsDigit(key.KeyChar))
                {
                    count = 0;
                    _command_newcount = true;
                    while (char.IsDigit(key.KeyChar))
                    {
                        count = (count * 10) + (key.KeyChar - '0');
                        if (count > 255)
                            count = 255;
                        key = readchar();
                    }

                    _command_countKey = key;

                    /*
                     * turn off count for commands which don't make sense
                     * to repeat
                     */
                    if (control)
                    {
                        switch (key.Key)
                        {
                            case ConsoleKey.B:
                            case ConsoleKey.H:
                            case ConsoleKey.J:
                            case ConsoleKey.K:
                            case ConsoleKey.L:
                            case ConsoleKey.N:
                            case ConsoleKey.U:
                            case ConsoleKey.Y:
#if MASTER
                            case ConsoleKey.D:
                            case ConsoleKey.A:
#endif
                                break;

                            default:
                                count = 0;
                                break;
                        }
                    }
                    else
                    {
                        switch (key.KeyChar)
                        {
                            case '.':
                            case 'a':
                            case 'b':
                            case 'h':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'y':
                            case 'z':
                            case 'B':
                            case 'C':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'N':
                            case 'U':
                            case 'Y':
                                break;

                            default:
                                count = 0;
                                break;
                        }
                    }
                }

                /*
                 * execute a command
                 */
                if ((count != 0) && !running)
                    count--;

                if (key.KeyChar != 'a' && key.Key != ConsoleKey.Escape && !(running || (count != 0) || to_death))
                {
                    l_last_commKey = last_commKey;
                    l_last_dirKey = last_dirKey;
                    l_last_pick = last_pick;
                    last_commKey = key;
                    last_dirKey = default;
                    last_pick = null;
                }

over:
                if (control)
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.H:
                        case ConsoleKey.J:
                        case ConsoleKey.K:
                        case ConsoleKey.L:
                        case ConsoleKey.Y:
                        case ConsoleKey.U:
                        case ConsoleKey.B:
                        case ConsoleKey.N:
                            if (!on(player, ISBLIND))
                            {
                                door_stop = true;
                                firstmove = true;
                            }

                            if ((count != 0) && !_command_newcount)
                                key = _command_direction;
                            else
                                key = _command_direction = key.RemoveModifiers(ConsoleModifiers.Control);
                            goto over;

                        case ConsoleKey.P:
                            after = false; msg(huh);
                            break;

                        case ConsoleKey.R:
                            after = false;
                            clearok(curscr, true);
                            wrefresh(curscr);
                            break;

                        default:
                            after = false;
#if MASTER
                            if (wizard)
                            {
                                switch (key.Key)
                                {
                                    case ConsoleKey.G:
                                        inventory(lvl_obj, 0);
                                        break;

                                    case ConsoleKey.W:
                                        whatis(false, 0);
                                        break;

                                    case ConsoleKey.D:
                                        level++; new_level();
                                        break;

                                    case ConsoleKey.A:
                                        level--; new_level();
                                        break;

                                    case ConsoleKey.F:
                                        show_map();
                                        break;

                                    case ConsoleKey.T:
                                        teleport();
                                        break;

                                    case ConsoleKey.E:
                                        msg("food left: %d", food_left);
                                        break;

                                    case ConsoleKey.C:
                                        add_pass();
                                        break;

                                    case ConsoleKey.X:
                                        turn_see(on(player, SEEMONST));
                                        break;

                                    case ConsoleKey.Oem3:   // '~'
                                    {
                                        if (get_item("charge", STICK) is THING item)
                                            item.o_charges = 10000;

                                        break;
                                    }

                                    case ConsoleKey.I:
                                    {
                                        THING obj;

                                        for (int i = 0; i < 9; i++)
                                            raise_level();

                                        /*
                                         * Give him a sword (+1,+1)
                                         */
                                        obj = new_item();
                                        init_weapon(obj, TWOSWORD);
                                        obj.o_hplus = 1;
                                        obj.o_dplus = 1;
                                        add_pack(obj, true);
                                        cur_weapon = obj;

                                        /*
                                         * And his suit of armor
                                         */
                                        obj = new_item();
                                        obj.o_type = ARMOR;
                                        obj.o_which = PLATE_MAIL;
                                        obj.o_arm = -5;
                                        obj.o_flags |= ISKNOW;
                                        obj.o_count = 1;
                                        obj.o_group = 0;
                                        cur_armor = obj;
                                        add_pack(obj, true);

                                        break;
                                    }

                                    default:
                                        illcom(key);
                                        break;
                                }
                            }
                            else
#endif
                                illcom(key);
                            break;
                    }
                }
                else
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.D1:     // Shift+ConsoleKey.D1 = '!'
                            //if (key.KeyChar == '!')
                            //{
                            //    shell();
                            //    break;
                            //}
                            goto default;

                        case ConsoleKey.H:
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.NumPad4:
                            WalkOrRun(key, run: shift);
                            break;

                        case ConsoleKey.J:
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.NumPad2:
                            WalkOrRun(key, run: shift);
                            break;

                        case ConsoleKey.K:
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.NumPad8:
                            WalkOrRun(key, run: shift);
                            break;

                        case ConsoleKey.L:
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.NumPad6:
                            WalkOrRun(key, run: shift);
                            break;

                        case ConsoleKey.Y:
                        case ConsoleKey.Home:
                        case ConsoleKey.NumPad7:
                            WalkOrRun(key, run: shift);
                            break;

                        case ConsoleKey.U:
                        case ConsoleKey.PageUp:
                        case ConsoleKey.NumPad9:
                            WalkOrRun(key, run: shift);
                            break;

                        case ConsoleKey.B:
                        case ConsoleKey.End:
                        case ConsoleKey.NumPad1:
                            WalkOrRun(key, run: shift);
                            break;

                        case ConsoleKey.N:
                        case ConsoleKey.PageDown:
                        case ConsoleKey.NumPad3:
                            WalkOrRun(key, run: shift);
                            break;

                        case ConsoleKey.F:
                            kamikaze = char.IsUpper(key.KeyChar);
                            if (!get_dir())
                            {
                                after = false;
                                break;
                            }

                            delta.y += hero.y;
                            delta.x += hero.x;
                            
                            if (((mp = moat(delta.y, delta.x)) == null)
                            || ((!see_monst(mp)) && !on(player, SEEMONST)))
                            {
                                if (!terse)
                                    addmsg("I see ");
                                msg("no monster there");
                                after = false;
                            }
                            else if (diag_ok(hero, delta))
                            {
                                to_death = true;
                                max_hit = 0;
                                mp.t_flags |= ISTARGET;
                                runKey = key = dirKey;
                                goto over;
                            }
                            
                            break;

                        case ConsoleKey.T:
                            if (char.IsUpper(key.KeyChar))
                            {
                                take_off();
                            }
                            else
                            {
                                if (!get_dir())
                                    after = false;
                                else
                                    missile(delta.y, delta.x);
                            }

                            break;

                        case ConsoleKey.A:
                            if (char.IsUpper(key.KeyChar))
                                goto default;

                            if (last_commKey == default)
                            {
                                msg("you haven't typed a command yet");
                                after = false;
                            }
                            else
                            {
                                key = last_commKey;
                                again = true;
                                goto over;
                            }

                            break;

                        case ConsoleKey.Q:
                            if (shift)
                            {
                                after = false;
                                q_comm = true;
                                quit(0);
                                q_comm = false;
                            }
                            else
                            {
                                quaff();
                            }

                            break;

                        case ConsoleKey.I:
                            after = false;

                            if (char.IsLower(key.KeyChar))
                                inventory(pack, 0);
                            else
                                picky_inven();
                            break;

                        case ConsoleKey.D:
                            if (char.IsLower(key.KeyChar))
                            {
                                drop();
                            }
                            else
                            {
                                after = false;
                                discovered();
                            }

                            break;

                        case ConsoleKey.R:
                            if (char.IsLower(key.KeyChar))
                                read_scroll();
                            else
                                ring_off();
                            break;

                        case ConsoleKey.E:
                            if (char.IsUpper(key.KeyChar))
                                goto default;

                            eat();
                            break;

                        case ConsoleKey.W:
                            if (char.IsUpper(key.KeyChar))
                                wear();
                            else
                                wield();
                            break;

                        case ConsoleKey.P:
                            if (char.IsLower(key.KeyChar))
                                goto default;

                            ring_on();
                            break;

                        case ConsoleKey.O:
                            if (char.IsUpper(key.KeyChar))
                                goto default;

                            after = false;
                            option();
                            break;

                        case ConsoleKey.C:
                            if (char.IsUpper(key.KeyChar))
                                goto default;

                            after = false;
                            call();
                            break;

                        case ConsoleKey.OemPeriod:
                            if (key.KeyChar== '>')
                            {
                                after = false;
                                d_level();
                            }
                            else if (key.KeyChar == '.')
                            {
                                // Rest command
                            }
                            else
                            {
                                goto default;
                            }

                            break;

                        case ConsoleKey.OemComma:
                            if (key.KeyChar== '<')
                            {
                                after = false;
                                u_level();
                            }
                            else if (key.KeyChar == ',')
                            {
                                THING? obj;

                                for (obj = lvl_obj; obj != null; obj = next(obj))
                                {
                                    if (obj.o_pos.y == hero.y && obj.o_pos.x == hero.x)
                                        break;
                                }

                                if (obj != null)
                                {
                                    if (!levit_check())
                                        pick_up((char) obj.o_type);
                                }
                                else
                                {
                                    if (!terse)
                                        addmsg("there is ");
                                    addmsg("nothing here");
                                    if (!terse)
                                        addmsg(" to pick up");
                                    endmsg();
                                }
                            }
                            else
                            {
                                goto default;
                            }

                            break;

                        case ConsoleKey.Oem2:
                            after = false; 
                            
                            if (key.KeyChar == '?')
                                help();
                            else if (key.KeyChar == '/')
                                identify();
                            break;

                        case ConsoleKey.S:
                            if (char.IsLower(key.KeyChar))
                            {
                                search();
                            }
                            else
                            {
                                after = false;
                                save_game();
                            }

                            break;

                        case ConsoleKey.Z:
                            if (get_dir())
                                do_zap();
                            else
                                after = false;
                            break;

                        case ConsoleKey.V:
                            if (char.IsUpper(key.KeyChar))
                                goto default;

                            after = false;
                            msg("version %s. (mctesq was here)", release);
                            break;

                        case ConsoleKey.Spacebar:
                            after = false;    /* "Legal" illegal command */
                            break;

                        case ConsoleKey.D6:     // Shift+ConsoleKey.D6 = '^'
                            if (key.KeyChar != '^')
                                goto default;

                            after = false;
                            if (get_dir())
                            {
                                delta.y += hero.y;
                                delta.x += hero.x;
                                if (!terse)
                                    addmsg("You have found ");
                                if (chat(delta.y, delta.x) != TRAP)
                                    msg("no trap there");
                                else if (on(player, ISHALU))
                                    msg(tr_name[rnd(NTRAPS)]);
                                else
                                {
                                    byte floor = flat(delta.y, delta.x);
                                    msg(tr_name[floor & F_TMASK]);
                                    set_flat(delta.y, delta.x, (byte)(floor | F_SEEN));
                                }
                            }

                            break;

#if MASTER
                        case ConsoleKey.Add:
                        case ConsoleKey.OemPlus:
                            if (key.KeyChar == '+')
                            {
                                after = false;
                                if (wizard)
                                {
                                    wizard = false;
                                    turn_see(true);
                                    msg("not wizard any more");
                                }
                                else
                                {
                                    wizard = passwd();
                                    if (wizard)
                                    {
                                        noscore = true;
                                        turn_see(false);
                                        msg("you are suddenly as smart as Ken Arnold in dungeon #%d", dnum);
                                    }
                                    else
                                        msg("sorry");
                                }
                            }
                            else if (key.KeyChar == '=')
                            {
                                current(cur_ring[LEFT],  "wearing", terse ? "(L)" : "on left hand");
                                current(cur_ring[RIGHT], "wearing", terse ? "(R)" : "on right hand");
                            }
                            else
                            {
                                goto default;
                            }
                            
                            break;
#endif

                        case ConsoleKey.Escape:
                            door_stop = false;
                            count = 0;
                            after = false;
                            again = false;
                            break;

                        case ConsoleKey.M:
                            move_on = true;
                            if (!get_dir())
                                after = false;
                            else
                            {
                                key = dirKey;
                                _command_countKey = dirKey;
                                goto over;
                            }

                            break;

                        case ConsoleKey.D0:     // Shift+ConsoleKey.D0 = ')'
                            if (key.KeyChar != ')')
                                goto default;

                            after = false;
                            current(cur_weapon, "wielding", null);
                            break;

                        case ConsoleKey.Oem6:   // Shift+ConsoleKey.Oem6 = ']'
                            if (key.KeyChar != ']')
                                goto default;

                            after = false;
                            current(cur_armor, "wearing", null);
                            break;

                        case ConsoleKey.D2:     // Shift+ConsoleKey.D2 = '@'
                            if (key.KeyChar != '@')
                                goto default;

                            stat_msg = true;
                            status();
                            stat_msg = false;
                            after = false;
                            break;

                        default:
                            after = false;
#if MASTER
                            if (wizard)
                            {
                                switch (key.KeyChar)
                                {
                                    case '|':
                                        msg("@ %d,%d", hero.y, hero.x);
                                        break;
                                    case 'C':
                                        create_obj();
                                        break;
                                    case '$':
                                        msg("inpack = %d", inpack);
                                        break;
                                    case '*':
                                        pr_list();
                                        break;
                                    default:
                                        illcom(key);
                                        break;
                                }
                            }
                            else
#endif
                                illcom(key);

                            break;
                    }
                }
                /*
                 * turn off flags if no longer needed
                 */
                if (!running)
                    door_stop = false;
            }
            /*
             * If he ran into something to take, let him pick it up.
             */
            if (take != 0)
                pick_up(take);
            if (!running)
                door_stop = false;
            if (!after)
                ntimes++;

            // --- local methods ---
            void WalkOrRun(ConsoleKeyInfo key, bool run)
            {
                if (run)
                    do_run(key.ToLower());
                else
                    do_move(GetMovementDelta(key));
            }
        }

        do_daemons(AFTER);
        do_fuses(AFTER);
        if (ISRING(LEFT, R_SEARCH))
            search();
        else if (ISRING(LEFT, R_TELEPORT) && rnd(50) == 0)
            teleport();
        if (ISRING(RIGHT, R_SEARCH))
            search();
        else if (ISRING(RIGHT, R_TELEPORT) && rnd(50) == 0)
            teleport();
    }

    /// <summary>
    /// What to do with an illegal command
    /// </summary>
    void illcom(ConsoleKeyInfo key)
    {
        save_msg = false;
        count = 0;
        msg("illegal command '%s'", unctrl(key));
        save_msg = true;
    }

    /// <summary>
    /// player gropes about him to find hidden things.
    /// </summary>
    void search()
    {
        int ey = hero.y + 1;
        int ex = hero.x + 1;
        int probinc = (on(player, ISHALU) ? 3 : 0) + (on(player, ISBLIND) ? 2 : 0);
        bool found = false;

        for (int y = hero.y - 1; y <= ey; y++)
        {
            for (int x = hero.x - 1; x <= ex; x++)
            {
                if (y == hero.y && x == hero.x)
                    continue;

                byte fp = flat(y, x);

                if ((fp & F_REAL) == 0)
                {
                    switch (chat(y, x))
                    {
                        case '|':
                        case '-':
                            if (rnd(5 + probinc) != 0)
                                break;
                            set_chat(y, x, DOOR);
                            msg("a secret door");
foundone:
                            found = true;
                            set_flat(y, x, (byte)(fp |= F_REAL));
                            count = 0;
                            running = false;
                            break;

                        case FLOOR:
                            if (rnd(2 + probinc) != 0)
                                break;
                            set_chat(y, x, TRAP);
                            if (!terse)
                                addmsg("you found ");
                            if (on(player, ISHALU))
                                msg(tr_name[rnd(NTRAPS)]);
                            else
                            {
                                msg(tr_name[fp & F_TMASK]);
                                set_flat(y, x, (byte)(fp |= F_SEEN));
                            }

                            goto foundone;

                        case ' ':
                            if (rnd(3 + probinc) != 0)
                                break;
                            set_chat(y, x, PASSAGE);
                            goto foundone;
                    }
                }
            }
        }

        if (found)
            look(false);
    }

    /// <summary>
    /// Give single character help, or the whole mess if he wants it
    /// </summary>
    void help()
    {
        char helpch;
        int numprint, cnt;
        msg("character you want help for (* for all): ");
        helpch = readchar().KeyChar;
        mpos = 0;
        
        /*
         * If its not a *, print the right help string
         * or an error if he typed a funny character.
         */
        if (helpch != '*')
        {
            move(0, 0);
            foreach (h_list strp in helpstr)
            {
                if (strp.h_ch == helpch)
                {
                    lower_msg = true;
                    msg("%s%s", unctrl(strp.h_ch), strp.h_desc);
                    lower_msg = false;
                    return;
                }
            }

            msg("unknown character '%s'", unctrl(helpch));
            return;
        }

        /*
         * Here we print help for everything.
         * Then wait before we return to command mode
         */
        numprint = 0;
        foreach (h_list strp in helpstr)
        {
            if (strp.h_print)
                numprint++;
        }

        if ((numprint & 0x1) != 0)      /* round odd numbers up */
            numprint++;

        numprint /= 2;
        if (numprint > LINES - 1)
            numprint = LINES - 1;

        wclear(hw);
        cnt = 0;
        foreach (h_list strp in helpstr)
        {
            if (strp.h_print)
            {
                wmove(hw, cnt % numprint, cnt >= numprint ? COLS / 2 : 0);
                if (strp.h_ch != '\0')
                    waddstr(hw, unctrl(strp.h_ch));
                waddstr(hw, strp.h_desc);
                if (++cnt >= numprint * 2)
                    break;
            }
        }

        wmove(hw, LINES - 1, 0);
        waddstr(hw, "--Press space to continue--");
        wrefresh(hw);
        wait_for(' ');
        clearok(stdscr, true);
        //refresh();
        msg("");
        touchwin(stdscr);
        wrefresh(stdscr);
    }

    /// <summary>
    /// Tell the player what a certain thing is.
    /// </summary>
    void identify()
    {
        string str;
        h_list[] ident_list =
        [
            new ('|',       "wall of a room",       false),
            new ('-',       "wall of a room",       false),
            new (GOLD,      "gold",                 false),
            new (STAIRS,    "a staircase",          false),
            new (DOOR,      "door",                 false),
            new (FLOOR,     "room floor",           false),
            new (PLAYER,    "you",                  false),
            new (PASSAGE,   "passage",              false),
            new (TRAP,      "trap",                 false),
            new (POTION,    "potion",               false),
            new (SCROLL,    "scroll",               false),
            new (FOOD,      "food",                 false),
            new (WEAPON,    "weapon",               false),
            new (' ',       "solid rock",           false),
            new (ARMOR,     "armor",                false),
            new (AMULET,    "the Amulet of Yendor", false),
            new (RING,      "ring",                 false),
            new (STICK,     "wand or staff",        false),
        ];

        msg("what do you want identified? ");
        ConsoleKeyInfo keyInfo = readchar();
        mpos = 0;

        if (keyInfo.Key == ConsoleKey.Escape)
        {
            msg("");
            return;
        }

        if (char.IsUpper(keyInfo.KeyChar))
        {
            str = GetMonsterName(keyInfo.KeyChar);
        }
        else
        {
            str = "unknown character";
            foreach (h_list ident in ident_list)
            {
                if (ident.h_ch == keyInfo.KeyChar)
                {
                    str = ident.h_desc;
                    break;
                }
            }
        }

        msg("'%s': %s", unctrl(keyInfo), str);
    }

    /// <summary>
    /// He wants to go down a level
    /// </summary>
    void d_level()
    {
        if (levit_check())
            return;
        if (chat(hero.y, hero.x) != STAIRS)
            msg("I see no way down");
        else
        {
            level++;
            seenstairs = false;
            new_level();
        }
    }

    /// <summary>
    /// He wants to go up a level
    /// </summary>
    void u_level()
    {
        if (levit_check())
            return;
        if (chat(hero.y, hero.x) == STAIRS)
        {
            if (amulet)
            {
                level--;
                if (level == 0)
                    total_winner();
                new_level();
                msg("you feel a wrenching sensation in your gut");
            }
            else
                msg("your way is magically blocked");
        }
        else
            msg("I see no way up");
    }

    /// <summary>
    /// Check to see if she's levitating, and if she is, print an appropriate message.
    /// </summary>
    bool levit_check()
    {
        if (!on(player, ISLEVIT))
            return false;
        msg("You can't.  You're floating off the ground!");
        return true;
    }

    /// <summary>
    /// Allow a user to call a potion, scroll, or ring something
    /// </summary>
    void call()
    {
        THING? obj;
        obj_info? op;
        string? elsewise = null;
        bool know;
        bool useLabelForGuess;

        obj = get_item("call", CALLABLE);

        /*
         * Make certain that it is something that we want to wear
         */
        if (obj == null)
            return;

        switch (obj.o_type)
        {
            case RING:
                op = ring_info[obj.o_which];
                elsewise = r_stones[obj.o_which];
                goto norm;

            case POTION:
                op = pot_info[obj.o_which];
                elsewise = p_colors[obj.o_which];
                goto norm;

            case SCROLL:
                op = scr_info[obj.o_which];
                elsewise = s_names[obj.o_which];
                goto norm;

            case STICK:
                op = ws_info[obj.o_which];
                elsewise = ws_made[obj.o_which];
norm:
                know = op.oi_know;
                if (op.oi_guess != null)
                    elsewise = op.oi_guess;
                useLabelForGuess = false;
                break;

            case FOOD:
                msg("you can't call that anything");
                return;

            default:
                useLabelForGuess = true;
                know = false;
                elsewise = obj.o_label;
                op = null;
                break;
        }

        if (know)
        {
            msg("that has already been identified");
            return;
        }

        if (elsewise != null && useLabelForGuess)
        {
            if (!terse)
                addmsg("Was ");
            msg("called \"%s\"", elsewise);
        }

        if (terse)
            msg("call it: ");
        else
            msg("what do you want to call it? ");

        if (get_str(out string guess, stdscr) == NORM)
        {
            if (useLabelForGuess)
                obj.o_label = guess;
            else if (op != null)
                op.oi_guess = guess;
        }
    }

    /// <summary>
    /// Print the current weapon/armor
    /// </summary>
    void current(THING? cur, string how, string? where)
    {
        after = false;
        if (cur != null)
        {
            if (!terse)
                addmsg("you are %s (", how);
            inv_describe = false;
            addmsg("%c) %s", cur.o_packch, inv_name(cur, true));
            inv_describe = true;
            if (where != null)
                addmsg(" %s", where);
            endmsg();
        }
        else
        {
            if (!terse)
                addmsg("you are ");
            addmsg("%s nothing", how);
            if (where != null)
                addmsg(" %s", where);
            endmsg();
        }
    }
}
