using System.Net.NetworkInformation;
using System.Text;

using RogueSharp.Helpers;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    private bool _things_newpage;
    private int _things_line_cnt;
    private string? _things_lastfmt;
    private object? _things_lastarg;

    /// <summary>
    /// Return the name of something as it would appear in an  inventory.
    /// </summary>
    string inv_name(THING obj, bool drop)
    {
        obj_info op;
        string name;
        StringBuilder builder = new();
        int which = obj.o_which;

        switch (obj.o_type)
        {
            case POTION:
                nameit(builder, obj, "potion", p_colors[which], pot_info[which], nullstr);
                break;

            case RING:
                nameit(builder, obj, "ring", r_stones[which], ring_info[which], ring_num);
                break;

            case STICK:
                nameit(builder, obj, ws_type[which], ws_made[which], ws_info[which], charge_str);
                break;

            case SCROLL:
                if (obj.o_count == 1)
                    builder.Append("A scroll ");
                else
                    builder.AppendFormat("{0} scrolls", obj.o_count);

                op = scr_info[which];

                if (op.oi_know)
                    builder.AppendFormat("of {0}", op.oi_name);
                else if (op.oi_guess != null)
                    builder.AppendFormat("called {0}", op.oi_guess);
                else
                    builder.AppendFormat("titled {0}", s_names[which]);
                break;

            case FOOD:
                if (which == 1)
                {
                    if (obj.o_count == 1)
                        builder.AppendFormat("A{0} {1}", vowelstr(fruit), fruit);
                    else
                        builder.AppendFormat("{0} {1}s", obj.o_count, fruit);
                }
                else if (obj.o_count == 1)
                    builder.Append("Some food");
                else
                    builder.AppendFormat("{0} rations of food", obj.o_count);
                break;

            case WEAPON:
                name = weap_info[which].oi_name;
                if (obj.o_count > 1)
                    builder.AppendFormat("{0} ", obj.o_count);
                else
                    builder.AppendFormat("A{0} ", vowelstr(name));

                if ((obj.o_flags & ISKNOW) != 0)
                    builder.AppendFormat("{0} {1}", num(obj.o_hplus, obj.o_dplus, WEAPON), name);
                else
                    builder.Append(name);

                if (obj.o_count > 1)
                    builder.Append("s");
                if (obj.o_label != null)
                    builder.AppendFormat(" called {0}", obj.o_label);
                break;

            case ARMOR:
                name = arm_info[which].oi_name;
                if ((obj.o_flags & ISKNOW) != 0)
                {
                    builder.AppendFormat("{0} {1} [", num(a_class[which] - obj.o_arm, 0, ARMOR), name);
                    if (!terse)
                        builder.Append("protection ");
                    builder.AppendFormat("{0}]", a_class[which]);
                }
                else
                    builder.Append(name);

                if (obj.o_label != null)
                    builder.AppendFormat(" called {0}", obj.o_label);
                break;

            case AMULET:
                builder.Append("The Amulet of Yendor");
                break;

            case GOLD:
                builder.AppendFormat("{0} Gold pieces", obj.o_goldval);
                break;

            default:
                debug("Picked up something funny %s", unctrl(obj.o_type));
                builder.AppendFormat("Something bizarre {0}", unctrl(obj.o_type));
                break;
        }

        if (inv_describe)
        {
            if (obj == cur_armor)
                builder.Append(" (being worn)");
            if (obj == cur_weapon)
                builder.Append(" (weapon in hand)");
            if (obj == cur_ring[LEFT])
                builder.Append(" (on left hand)");
            else if (obj == cur_ring[RIGHT])
                builder.Append(" (on right hand)");
        }

        if (drop && Char.IsUpper(builder[0]))
            builder[0] = Char.ToLower(builder[0]);
        else if (!drop && Char.IsLower(builder[0]))
            builder[0] = Char.ToUpper(builder[0]);

        return builder.ToString();
    }

    /// <summary>
    /// Put something down
    /// </summary>
    void drop()
    {
        char ch;
        THING? obj;

        ch = chat(hero.y, hero.x);
        if (ch != FLOOR && ch != PASSAGE)
        {
            after = false;
            msg("there is something there already");
            return;
        }

        if ((obj = get_item("drop", 0)) == null)
            return;
        if (!dropcheck(obj))
            return;

        obj = leave_pack(obj, true, !ISMULT(obj.o_type));

        /*
         * Link it into the level object list
         */
        attach(ref lvl_obj, obj);
        set_chat(hero.y, hero.x, (char) obj.o_type);
        byte fl = flat(hero.y, hero.x);
        set_flat(hero.y, hero.x, (byte) (fl | F_DROPPED));
        obj.o_pos = hero;
        if (obj.o_type == AMULET)
            amulet = false;
        msg("dropped %s", inv_name(obj, true));
    }

    /// <summary>
    /// Do special checks for dropping or unweilding|unwearing|unringing
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    bool dropcheck(THING? obj)
    {
        if (obj == null)
            return true;

        if (obj != cur_armor && obj != cur_weapon
            && obj != cur_ring[LEFT] && obj != cur_ring[RIGHT])
            return true;

        if ((obj.o_flags & ISCURSED) != 0)
        {
            msg("you can't.  It appears to be cursed");
            return false;
        }

        if (obj == cur_weapon)
        {
            cur_weapon = null;
        }
        else if (obj == cur_armor)
        {
            waste_time();
            cur_armor = null;
        }
        else
        {
            cur_ring[obj == cur_ring[LEFT] ? LEFT : RIGHT] = null;
            switch (obj.o_which)
            {
                case R_ADDSTR:
                    chg_str(-obj.o_arm);
                    break;
                case R_SEEINVIS:
                    unsee();
                    extinguish(unsee);
                    break;
            }
        }

        return true;
    }

    /// <summary>
    /// Return a new thing
    /// </summary>
    THING new_thing()
    {
        THING cur;
        int r;

        cur = new_item();
        cur.o_hplus = 0;
        cur.o_dplus = 0;
        cur.o_damage = "0x0";
        cur.o_hurldmg = "0x0";
        cur.o_arm = 11;
        cur.o_count = 1;
        cur.o_group = 0;
        cur.o_flags = 0;

        /*
         * Decide what kind of object it will be
         * If we haven't had food for a while, let it be food.
         */
        switch (no_food > 3 ? 2 : pick_one(things))
        {
            case 0:
                cur.o_type = POTION;
                cur.o_which = pick_one(pot_info);
                break;

            case 1:
                cur.o_type = SCROLL;
                cur.o_which = pick_one(scr_info);
                break;

            case 2:
                cur.o_type = FOOD;
                no_food = 0;
                if (rnd(10) != 0)
                    cur.o_which = 0;
                else
                    cur.o_which = 1;
                break;

            case 3:
                init_weapon(cur, pick_one(weap_info));
                if ((r = rnd(100)) < 10)
                {
                    cur.o_flags |= ISCURSED;
                    cur.o_hplus -= rnd(3) + 1;
                }
                else if (r < 15)
                    cur.o_hplus += rnd(3) + 1;
                break;

            case 4:
                cur.o_type = ARMOR;
                cur.o_which = pick_one(arm_info);
                cur.o_arm = a_class[cur.o_which];
                if ((r = rnd(100)) < 20)
                {
                    cur.o_flags |= ISCURSED;
                    cur.o_arm += rnd(3) + 1;
                }
                else if (r < 28)
                    cur.o_arm -= rnd(3) + 1;
                break;

            case 5:
                cur.o_type = RING;
                cur.o_which = pick_one(ring_info);
                switch (cur.o_which)
                {
                    case R_ADDSTR:
                    case R_PROTECT:
                    case R_ADDHIT:
                    case R_ADDDAM:
                        if ((cur.o_arm = rnd(3)) == 0)
                        {
                            cur.o_arm = -1;
                            cur.o_flags |= ISCURSED;
                        }

                        break;
                    case R_AGGR:
                    case R_TELEPORT:
                        cur.o_flags |= ISCURSED;
                        break;
                }

                break;

            case 6:
                cur.o_type = STICK;
                cur.o_which = pick_one(ws_info);
                fix_stick(cur);
                break;

#if MASTER
            default:
                debug("Picked a bad kind of object");
                wait_for(' ');
                break;
#endif
        }

        return cur;
    }

    /// <summary>
    /// Pick an item out of a collection of possible objects
    /// </summary>
    int pick_one(obj_info[] items)
    {
        int value = rnd(100);

        for (int i = 0; i<items.Length; i++)
        {
            obj_info item = items[i];
            if (value < item.oi_prob)
                return i;
        }

#if MASTER
        if (wizard)
        {
            msg($"bad pick_one: {value} from {items.Length} items");
            foreach (obj_info item in items)
            {
                msg($"{item.oi_name}: {item.oi_prob}%");
            }
        }
#endif

        return 0;
    }

    /// <summary>
    /// list what the player has discovered in this game of a certain type
    /// </summary>
    void discovered()
    {
        char ch;
        bool disc_list;

        do
        {
            disc_list = false;
            if (!terse)
                addmsg("for ");
            addmsg("what type");
            if (!terse)
                addmsg(" of object do you want a list");
            msg("? (* for all)");
            ch = readchar().KeyChar;
            switch (ch)
            {
                case ESCAPE:
                    msg("");
                    return;

                case POTION:
                case SCROLL:
                case RING:
                case STICK:
                case '*':
                    disc_list = true;
                    break;

                default:
                    if (terse)
                        msg("Not a type");
                    else
                        msg("Please type one of %c%c%c%c (ESCAPE to quit)", POTION, SCROLL, RING, STICK);
                    break;
            }
        } while (!disc_list);

        if (ch == '*')
        {
            print_disc(POTION);
            add_line("", null);
            print_disc(SCROLL);
            add_line("", null);
            print_disc(RING);
            add_line("", null);
            print_disc(STICK);
            end_line();
        }
        else
        {
            print_disc(ch);
            end_line();
        }
    }

    /// <summary>
    /// Print what we've discovered of type 'type'
    /// </summary>
    void print_disc(char type)
    {
        static int MAX4(int a,int b,int c,int d) => Math.Max(Math.Max(a,b), Math.Max(c,d));

        obj_info[] info;
        int i, maxnum, num_found;
        int[] order = new int[MAX4(MAXSCROLLS, MAXPOTIONS, MAXRINGS, MAXSTICKS)];

        switch (type)
        {
            case SCROLL:
                maxnum = MAXSCROLLS;
                info = scr_info;
                break;
            case POTION:
                maxnum = MAXPOTIONS;
                info = pot_info;
                break;
            case RING:
                maxnum = MAXRINGS;
                info = ring_info;
                break;
            case STICK:
                maxnum = MAXSTICKS;
                info = ws_info;
                break;
            default:
                return;
        }

        set_order(order, maxnum);

        THING obj = new();
        obj.o_count = 1;
        obj.o_flags = 0;
        num_found = 0;

        for (i = 0; i < maxnum; i++)
        {
            if (info[order[i]].oi_know || (info[order[i]].oi_guess != null))
            {
                obj.o_type = type;
                obj.o_which = order[i];
                add_line("%s", inv_name(obj, false));
                num_found++;
            }
        }

        if (num_found == 0)
            add_line(nothing(type), null);
    }

    /// <summary>
    /// Set up order for list
    /// </summary>
    void set_order(int[] order, int numthings)
    {
        for (int i = 0; i < numthings; i++)
        {
            order[i] = i;
        }

        for (int i = numthings; i > 0; i--)
        {
            int r = rnd(i);
            int t = order[i - 1];
            order[i - 1] = order[r];
            order[r] = t;
        }
    }

    private int _add_line_maxlen = -1;

    /// <summary>
    /// Add a line to the list of discoveries
    /// </summary>
    char add_line(string? fmt, object? arg)
    {
        const string prompt = "--Press space to continue--";

        if (_things_line_cnt == 0)
        {
            wclear(hw);
            if (inv_type == INV_SLOW)
                mpos = 0;
        }

        if (inv_type == INV_SLOW)
        {
            if ((fmt != null) && (fmt != string.Empty))
            {
                if (msg(fmt, arg) == ESCAPE)
                    return ESCAPE;
            }

            _things_line_cnt++;
        }
        else
        {
            if (_add_line_maxlen < 0)
                _add_line_maxlen = prompt.Length;

            if (_things_line_cnt >= LINES - 1 || fmt == null)
            {
                if (inv_type == INV_OVER && fmt == null && !_things_newpage)
                {
                    msg("");
                    refresh();
                    CursesWindow tw = newwin(_things_line_cnt + 1, _add_line_maxlen + 2, 0, COLS - _add_line_maxlen - 3);
                    CursesWindow sw = subwin(tw, _things_line_cnt + 1, _add_line_maxlen + 1, 0, COLS - _add_line_maxlen - 2);
                    
                    for (int y = 0; y <= _things_line_cnt; y++)
                    {
                        wmove(sw, y, 0);
                        for (int x = 0; x <= _add_line_maxlen; x++)
                            waddch(sw, mvwinch(hw, y, x));
                    }
                    
                    wmove(tw, _things_line_cnt, 1);
                    waddstr(tw, prompt);
                    
                    /*
                     * if there are lines below, use 'em
                     */
                    if (LINES > NUMLINES)
                    {
                        if (NUMLINES + _things_line_cnt > LINES)
                            mvwin(tw, LINES - (_things_line_cnt + 1), COLS - _add_line_maxlen - 3);
                        else
                            mvwin(tw, NUMLINES, 0);
                    }
                
                    touchwin(tw);
                    wrefresh(tw);
                    wait_for(' ');
                    
                    //if (md_hasclreol())
                    {
                        werase(tw);
                        leaveok(tw, true);
                        wrefresh(tw);
                    }
                    
                    delwin(tw);
                    touchwin(stdscr);
                }
                else
                {
                    wmove(hw, LINES - 1, 0);
                    waddstr(hw, prompt);
                    wrefresh(hw);
                    wait_for(' ');
                    clearok(curscr, true);
                    wclear(hw);
                    touchwin(stdscr);
                }
                
                _things_newpage = true;
                _things_line_cnt = 0;
                _add_line_maxlen = prompt.Length;
            }

            if (fmt != null && !(_things_line_cnt == 0 && fmt == string.Empty))
            {
                mvwprintw(hw, _things_line_cnt++, 0, fmt, arg ?? string.Empty);
                getyx(hw, out int y, out int x);

                if (_add_line_maxlen < x)
                    _add_line_maxlen = x;

                _things_lastfmt = fmt;
                _things_lastarg = arg;
            }
        }

        return unchecked((char) ~ESCAPE);
    }

    /// <summary>
    /// End the list of lines
    /// </summary>
    void end_line()
    {
        if (inv_type != INV_SLOW)
        {
            if (_things_line_cnt == 1 && !_things_newpage)
            {
                mpos = 0;

                if (_things_lastfmt != null)
                    msg(_things_lastfmt, _things_lastarg);
            }
            else
            {
                add_line(null, null);
            }
        }

        _things_line_cnt = 0;
        _things_newpage = false;
    }

    /// <summary>
    /// Returns a message indicating "nothing found"
    /// </summary>
    string nothing(char type)
    {
        StringBuilder builder = new(64);

        if (terse)
            builder.Append("Nothing");
        else
            builder.Append("Haven't discovered anything");

        if (type != '*')
        {
            string description = type switch
            {
                POTION => "potion",
                SCROLL => "scroll",
                RING => "ring",
                STICK => "stick",
                _ => "unknown",
            };

            builder.AppendFormat(" about any %ss", description);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Give the proper name to a potion, stick, or ring and places the description
    /// in <paramref name="builder"/>.
    /// </summary>
    void nameit(
        StringBuilder builder,
        THING obj,
        string type,
        string which,
        obj_info op,
        Func<THING, string> prfunc)
    {
        if (op.oi_know || (op.oi_guess != null))
        {
            if (obj.o_count == 1)
                builder.AppendFormat("A {0} ", type);
            else
                builder.AppendFormat("{0} {1}s ", obj.o_count, type);

            if (op.oi_know)
                builder.AppendFormat("of {0}{1}({2})", op.oi_name, prfunc(obj), which);
            else if (op.oi_guess != null)
                builder.AppendFormat("called {0}{1}({2})", op.oi_guess, prfunc(obj), which);
        }
        else if (obj.o_count == 1)
            builder.AppendFormat("A{0} {1} {2}", vowelstr(which), which, type);
        else
            builder.AppendFormat("{0} {1} {2}s", obj.o_count, which, type);
    }

    /// <summary>
    /// Return a pointer to a null-length string
    /// </summary>
    string nullstr(THING ignored) => string.Empty;

#if MASTER
    /// <summary>
    /// List possible potions, scrolls, etc. for wizard.
    /// </summary>
    void pr_list()
    {
        int ch;

        if (!terse)
            addmsg("for ");
        addmsg("what type");
        if (!terse)
            addmsg(" of object do you want a list");
        msg("? ");
        ch = readchar().KeyChar;

        switch (ch)
        {
            case POTION:
                pr_spec(pot_info);
                break;
            case SCROLL:
                pr_spec(scr_info);
                break;
            case RING:
                pr_spec(ring_info);
                break;
            case STICK:
                pr_spec(ws_info);
                break;
            case ARMOR:
                pr_spec(arm_info);
                break;
            case WEAPON:
                pr_spec(weap_info);
                break;
            default:
                return;
        }
    }

    /// <summary>
    /// Print specific list of possible items to choose from
    /// </summary>
    void pr_spec(obj_info[] info)
    {
        char ch = '0';
        int lastprob = 0;

        foreach (obj_info item in info)
        {
            string format = $"{ch}: %s ({item.oi_prob - lastprob}%%)";
            lastprob = item.oi_prob;
            add_line(format, item.oi_name);

            if (++ch == '9' + 1)
                ch = 'a';
        }

        end_line();
    }
#endif
}
