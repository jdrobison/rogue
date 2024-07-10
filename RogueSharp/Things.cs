using System.Text;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
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

#if false
    /*
     * drop:
     *    Put something down
     */

    void
    drop()
    {
        char ch;
        THING *obj;

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
        obj = leave_pack(obj, true, (bool) !ISMULT(obj.o_type));
        /*
         * Link it into the level object list
         */
        attach(lvl_obj, obj);
        chat(hero.y, hero.x) = (char) obj.o_type;
        flat(hero.y, hero.x) |= F_DROPPED;
        obj.o_pos = hero;
        if (obj.o_type == AMULET)
            amulet = false;
        msg("dropped %s", inv_name(obj, true));
    }

    /*
     * dropcheck:
     *    Do special checks for dropping or unweilding|unwearing|unringing
     */
    bool
    dropcheck(THING obj)
    {
        if (obj == null)
            return true;
        if (obj != cur_armor && obj != cur_weapon
        && obj != cur_ring[LEFT] && obj != cur_ring[RIGHT])
            return true;
        if (obj.o_flags & ISCURSED)
        {
            msg("you can't.  It appears to be cursed");
            return false;
        }
        if (obj == cur_weapon)
            cur_weapon = null;
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

    /*
     * new_thing:
     *    Return a new thing
     */
    THING
    new_thing()
    {
        THING *cur;
        int r;

        cur = new_item();
        cur.o_hplus = 0;
        cur.o_dplus = 0;
        strncpy(cur.o_damage, "0x0", sizeof(cur.o_damage));
        strncpy(cur.o_hurldmg, "0x0", sizeof(cur.o_hurldmg));
        cur.o_arm = 11;
        cur.o_count = 1;
        cur.o_group = 0;
        cur.o_flags = 0;
        /*
         * Decide what kind of object it will be
         * If we haven't had food for a while, let it be food.
         */
        switch (no_food > 3 ? 2 : pick_one(things, NUMTHINGS))
        {
            case 0:
                cur.o_type = POTION;
                cur.o_which = pick_one(pot_info, MAXPOTIONS);
                break;
case 1:
        cur.o_type = SCROLL;
                cur.o_which = pick_one(scr_info, MAXSCROLLS);
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
        init_weapon(cur, pick_one(weap_info, MAXWEAPONS));
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
                cur.o_which = pick_one(arm_info, MAXARMORS);
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
                cur.o_which = pick_one(ring_info, MAXRINGS);
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
                }
                break;
case 6:
        cur.o_type = STICK;
                cur.o_which = pick_one(ws_info, MAXSTICKS);
                fix_stick(cur);
#if MASTER
break;
default:
                debug("Picked a bad kind of object");
                wait_for(' ');
#endif
        }
        return cur;
    }

    /*
     * pick_one:
     *    Pick an item out of a list of nitems possible objects
     */
    int
    pick_one(obj_info* info, int nitems)
    {
        obj_info *end;
        obj_info *start;
        int i;

        start = info;
        for (end = &info[nitems], i = rnd(100); info < end; info++)
            if (i < info.oi_prob)
                break;
        if (info == end)
        {
#if MASTER
            if (wizard)
            {
                msg("bad pick_one: %d from %d items", i, nitems);
                for (info = start; info < end; info++)
                    msg("%s: %d%%", info.oi_name, info.oi_prob);
            }
#endif
            info = start;
        }
        return (int) (info - start);
    }

    /*
     * discovered:
     *    list what the player has discovered in this game of a certain type
     */
    static int line_cnt = 0;

    static bool newpage = false;

    static char *lastfmt, *lastarg;


void
discovered()
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
            ch = readchar();
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

    /*
     * print_disc:
     *    Print what we've discovered of type 'type'
     */
    void
    print_disc(char type)
    {
        int MAX4(int a,int b,int c,int d) => Math.Max(Math.Max(a,b), Math.Max(c,d));

        obj_info *info = null;
        int i, maxnum = 0, num_found;
        static THING obj;
        static int order[MAX4(MAXSCROLLS, MAXPOTIONS, MAXRINGS, MAXSTICKS)];

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
        }
        set_order(order, maxnum);
        obj.o_count = 1;
        obj.o_flags = 0;
        num_found = 0;
        for (i = 0; i < maxnum; i++)
            if (info[order[i]].oi_know || info[order[i]].oi_guess)
            {
                obj.o_type = type;
                obj.o_which = order[i];
                add_line("%s", inv_name(&obj, false));
                num_found++;
            }
        if (num_found == 0)
            add_line(nothing(type), null);
    }

    /*
     * set_order:
     *    Set up order for list
     */

    void
    set_order(int* order, int numthings)
    {
        int i, r, t;

        for (i = 0; i< numthings; i++)
            order[i] = i;

        for (i = numthings; i > 0; i--)
        {
            r = rnd(i);
            t = order[i - 1];
            order[i - 1] = order[r];
            order[r] = t;
        }
    }

    /*
     * add_line:
     *    Add a line to the list of discoveries
     */
    /* VARARGS1 */
    char
    add_line(char* fmt, char* arg)
    {
        WINDOW *tw, *sw;
        int x, y;
        char *prompt = "--Press space to continue--";
        static int maxlen = -1;

        if (line_cnt == 0)
        {
            wclear(hw);
            if (inv_type == INV_SLOW)
                mpos = 0;
        }
        if (inv_type == INV_SLOW)
        {
            if (*fmt != '\0')
                if (msg(fmt, arg) == ESCAPE)
                    return ESCAPE;
            line_cnt++;
        }
        else
        {
            if (maxlen < 0)
                maxlen = (int) strlen(prompt);
            if (line_cnt >= LINES - 1 || fmt == null)
            {
                if (inv_type == INV_OVER && fmt == null && !newpage)
                {
                    msg("");
                    refresh();
                    tw = newwin(line_cnt + 1, maxlen + 2, 0, COLS - maxlen - 3);
                    sw = subwin(tw, line_cnt + 1, maxlen + 1, 0, COLS - maxlen - 2);
                    for (y = 0; y <= line_cnt; y++)
                    {
                        wmove(sw, y, 0);
                        for (x = 0; x <= maxlen; x++)
                            waddch(sw, mvwinch(hw, y, x));
                    }
                    wmove(tw, line_cnt, 1);
                    waddstr(tw, prompt);
                    /*
                     * if there are lines below, use 'em
                     */
                    if (LINES > NUMLINES)
                    {
                        if (NUMLINES + line_cnt > LINES)
                            mvwin(tw, LINES - (line_cnt + 1), COLS - maxlen - 3);
                        else
                            mvwin(tw, NUMLINES, 0);
                    }
                    touchwin(tw);
                    wrefresh(tw);
                    wait_for(' ');
                    if (md_hasclreol())
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
                newpage = true;
                line_cnt = 0;
                maxlen = (int) strlen(prompt);
            }
            if (fmt != null && !(line_cnt == 0 && *fmt == '\0'))
            {
                mvwprintw(hw, line_cnt++, 0, fmt, arg);
                getyx(hw, y, x);
                if (maxlen < x)
                    maxlen = x;
                lastfmt = fmt;
                lastarg = arg;
            }
        }
        return ~ESCAPE;
    }

    /*
     * end_line:
     *    End the list of lines
     */

    void
    end_line()
    {
        if (inv_type != INV_SLOW)
        {
            if (line_cnt == 1 && !newpage)
            {
                mpos = 0;
                msg(lastfmt, lastarg);
            }
            else
                add_line((char*) null, null);
        }
        line_cnt = 0;
        newpage = false;
    }

    /*
     * nothing:
     *    Set up prbuf so that message for "nothing found" is there
     */
    char*
    nothing(char type)
    {
        char *sp, *tystr = null;

        if (terse)
            sprintf(prbuf, "Nothing");
        else
            sprintf(prbuf, "Haven't discovered anything");
        if (type != '*')
        {
            sp = &prbuf[strlen(prbuf)];
            switch (type)
            {
                case POTION:
                    tystr = "potion";
                    break;
case SCROLL: tystr = "scroll";
                    break;
case RING: tystr = "ring";
                    break;
case STICK: tystr = "stick";
            }
            sprintf(sp, " about any %ss", tystr);
        }
        return prbuf;
    }
#endif

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

#if false
#if MASTER
    /*
     * pr_list:
     *    List possible potions, scrolls, etc. for wizard.
     */

    void
    pr_list()
    {
        int ch;

        if (!terse)
            addmsg("for ");
        addmsg("what type");
        if (!terse)
            addmsg(" of object do you want a list");
        msg("? ");
        ch = readchar();
        switch (ch)
        {
            case POTION:
                pr_spec(pot_info, MAXPOTIONS);
                break;
            case SCROLL:
                pr_spec(scr_info, MAXSCROLLS);
                break;
            case RING:
                pr_spec(ring_info, MAXRINGS);
                break;
            case STICK:
                pr_spec(ws_info, MAXSTICKS);
                break;
            case ARMOR:
                pr_spec(arm_info, MAXARMORS);
                break;
            case WEAPON:
                pr_spec(weap_info, MAXWEAPONS);
                break;
            default:
                return;
        }
    }

    /*
     * pr_spec:
     *    Print specific list of possible items to choose from
     */

    void
    pr_spec(obj_info* info, int nitems)
    {
        obj_info *endp;
        int i, lastprob;

        endp = &info[nitems];
        lastprob = 0;
        for (i = '0'; info < endp; i++)
        {
            if (i == '9' + 1)
                i = 'a';
            sprintf(prbuf, "%c: %%s (%d%%%%)", i, info.oi_prob - lastprob);
            lastprob = info.oi_prob;
            add_line(prbuf, info.oi_name);
            info++;
        }
        end_line();
    }
#endif
#endif
}
