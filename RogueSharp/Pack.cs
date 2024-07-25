using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>
    /// Pick up an object and add it to the pack.  If the argument is
    /// non-null use it as the linked_list pointer instead of gettting
    /// it off the ground.
    /// </summary>
    void add_pack(THING? obj, bool silent)
    {
        bool from_floor;

        from_floor = false;
        if (obj == null)
        {
            if ((obj = find_obj(hero.y, hero.x)) == null)
                return;
            from_floor = true;
        }

        /*
         * Check for and deal with scare monster scrolls
         */
        if (obj.o_type == SCROLL && obj.o_which == S_SCARE)
        {
            if ((obj.o_flags & ISFOUND) != 0)
            {
                detach(ref lvl_obj, obj);
                mvaddch(hero.y, hero.x, floor_ch());
                place_at(hero.y, hero.x).p_ch = ((proom.r_flags & ISGONE) != 0) ? PASSAGE : FLOOR;
                discard(obj);
                msg("the scroll turns to dust as you pick it up");
                return;
            }
        }

        if (pack == null)
        {
            pack = obj;
            obj.o_packch = pack_char();
            inpack++;
        }
        else
        {
            THING? lp = null;
            for (THING? op = pack; op != null; op = next(op))
            {
                if (op.o_type != obj.o_type)
                    lp = op;
                else
                {
                    while (op.o_type == obj.o_type && op.o_which != obj.o_which)
                    {
                        lp = op;
                        if (next(op) == null)
                            break;
                        else
                            op = next(op);
                    }
                    if (op.o_type == obj.o_type && op.o_which == obj.o_which)
                    {
                        if (ISMULT(op.o_type))
                        {
                            if (!pack_room(from_floor, obj))
                                return;
                            op.o_count++;

                            discard(obj);
                            obj = op;
                            lp = null;
                        }
                        else if (obj.o_group != 0)
                        {
                            lp = op;
                            while (op.o_type == obj.o_type
                                && op.o_which == obj.o_which
                                && op.o_group != obj.o_group)
                            {
                                lp = op;
                                if (next(op) == null)
                                    break;
                                else
                                    op = next(op);
                            }
                            if (op.o_type == obj.o_type
                                && op.o_which == obj.o_which
                                && op.o_group == obj.o_group)
                            {
                                op.o_count += obj.o_count;
                                inpack--;
                                if (!pack_room(from_floor, obj))
                                    return;

                                discard(obj);
                                obj = op;
                                lp = null;
                            }
                        }
                        else
                            lp = op;
                    }

                    break;
                }
            }

            if (lp != null)
            {
                if (!pack_room(from_floor, obj))
                    return;
                else
                {
                    obj.o_packch = pack_char();
                    obj.l_next = next(lp);
                    obj.l_prev = lp;
                    if (lp.l_next != null)
                        lp.l_next.l_prev = obj;
                    lp.l_next = obj;
                }
            }
        }

        obj.o_flags |= ISFOUND;

        /*
         * If this was the object of something's desire, that monster will
         * get mad and run at the hero.
         */
        for (THING? op = mlist; op != null; op = next(op))
        {
            if (op.t_dest == obj.o_pos)
                op.t_dest = hero;
        }

        if (obj.o_type == AMULET)
            amulet = true;
        /*
         * Notify the user
         */
        if (!silent)
        {
            if (!terse)
                addmsg("you now have ");
            msg("%s (%c)", inv_name(obj, !terse), obj.o_packch);
        }
    }

    /// <summary>
    /// See if there's room in the pack.  If not, print out an appropriate message
    /// </summary>
    bool pack_room(bool from_floor, THING obj)
    {
        if (++inpack > MAXPACK)
        {
            if (!terse)
                addmsg("there's ");
            addmsg("no room");
            if (!terse)
                addmsg(" in your pack");
            endmsg();
            if (from_floor)
                move_msg(obj);
            inpack = MAXPACK;
            return false;
        }

        if (from_floor)
        {
            detach(ref lvl_obj, obj);
            mvaddch(hero.y, hero.x, floor_ch());
            place_at(hero.y, hero.x).p_ch = ((proom.r_flags & ISGONE) != 0) ? PASSAGE : FLOOR;
        }

        return true;
    }

    /// <summary>
    /// Take an item out of the pack
    /// </summary>
    THING leave_pack(THING obj, bool newobj, bool all)
    {
        THING nobj;

        inpack--;
        nobj = obj;
        if (obj.o_count > 1 && !all)
        {
            last_pick = obj;
            obj.o_count--;
            if (obj.o_group != 0)
                inpack++;
            if (newobj)
            {
                nobj = obj.Clone();
                nobj.l_next = null;
                nobj.l_prev = null;
                nobj.o_count = 1;
            }
        }
        else
        {
            last_pick = null;
            pack_used[obj.o_packch - 'a'] = false;

            THING? localpack = pack;
            detach(ref localpack, obj);
            pack = localpack;
        }

        return nobj;
    }

    /// <summary>
    /// Return the next unused pack character.
    /// </summary>
    char pack_char()
    {
        for (int i = 0; i < pack_used.Length; i++)
        {
            if (!pack_used[i])
            {
                pack_used[i] = true;
                return (char)('a' + i);
            }
        }

        throw new Exception("no more space available in pack");
    }

    /// <summary>
    /// List what is in the pack.  Return true if there is something of the given type.
    /// </summary>
    bool inventory(THING? list, int type)
    {
        string inv_temp;

        n_objs = 0;
        for (; list != null; list = next(list))
        {
            if (type != 0 && type != list.o_type && !(type == CALLABLE &&
                list.o_type != FOOD && list.o_type != AMULET) &&
                !(type == R_OR_S && (list.o_type == RING || list.o_type == STICK)))
                continue;
            n_objs++;
#if MASTER
            if (list.o_packch != '\0')
                inv_temp = "%s";
            else
#endif
            inv_temp = $"{list.o_packch}) %s";
            msg_esc = true;

            if (add_line(inv_temp, inv_name(list, false)) == ESCAPE)
            {
                msg_esc = false;
                msg("");
                return true;
            }

            msg_esc = false;
        }

        if (n_objs == 0)
        {
            if (terse)
                msg(type == 0 ? "empty handed" :
                        "nothing appropriate");
            else
                msg(type == 0 ? "you are empty handed" :
                        "you don't have anything appropriate");
            return false;
        }

        end_line();
        return true;
    }

    /// <summary>
    /// Add something to characters pack.
    /// </summary>
    void pick_up(char ch)
    {
        if (on(player, ISLEVIT))
            return;

        THING? obj = find_obj(hero.y, hero.x);

        if (move_on)
        {
            if (obj != null)
                move_msg(obj);
        }
        else
        {
            switch (ch)
            {
                case GOLD:
                    if (obj == null)
                        return;
                    money(obj.o_goldval);
                    detach(ref lvl_obj, obj);
                    discard(obj);
                    proom.r_goldval = 0;
                    break;

                case ARMOR:
                case POTION:
                case FOOD:
                case WEAPON:
                case SCROLL:
                case AMULET:
                case RING:
                case STICK:
                    add_pack(null, false);
                    break;

                default:
#if MASTER
                    debug("Where did you pick a '%s' up???", unctrl(ch));
#endif
                    break;
            }
        }
    }

    /// <summary>
    /// Print out the message if you are just moving onto an object
    /// </summary>
    void move_msg(THING obj)
    {
        if (!terse)
            addmsg("you ");
        msg("moved onto %s", inv_name(obj, true));
    }

    /// <summary>
    /// Allow player to inventory a single item
    /// </summary>
    void picky_inven()
    {
        char mch;

        if (pack == null)
            msg("you aren't carrying anything");
        else if (next(pack) == null)
            msg("a) %s", inv_name(pack, false));
        else
        {
            msg(terse ? "item: " : "which item do you wish to inventory: ");
            mpos = 0;
            if ((mch = readchar().KeyChar) == ESCAPE)
            {
                msg("");
                return;
            }

            for (THING? obj = pack; obj != null; obj = next(obj))
            {
                if (mch == obj.o_packch)
                {
                    msg("%c) %s", mch, inv_name(obj, false));
                    return;
                }
            }

            msg("'%s' not in pack", unctrl(mch));
        }
    }

    /// <summary>
    /// Pick something out of a pack for a purpose
    /// </summary>
    THING? get_item(string purpose, int type)
    {
        THING? obj;
        char ch;

        if (pack == null)
            msg("you aren't carrying anything");
        else if (again)
            if (last_pick != null)
                return last_pick;
            else
                msg("you ran out");
        else
        {
            for (; ; )
            {
                if (!terse)
                    addmsg("which object do you want to ");
                addmsg(purpose);
                if (terse)
                    addmsg(" what");
                msg("? (* for list): ");
                ch = readchar().KeyChar;
                mpos = 0;
                /*
                 * Give the poor player a chance to abort the command
                 */
                if (ch == ESCAPE)
                {
                    reset_last();
                    after = false;
                    msg("");
                    return null;
                }
                n_objs = 1;     /* normal case: person types one char */
                if (ch == '*')
                {
                    mpos = 0;
                    if (inventory(pack, type))
                    {
                        after = false;
                        return null;
                    }
                    continue;
                }
                for (obj = pack; obj != null; obj = next(obj))
                    if (obj.o_packch == ch)
                        break;
                if (obj == null)
                {
                    msg("'%s' is not a valid item", unctrl(ch));
                    continue;
                }
                else
                    return obj;
            }
        }
        return null;
    }

    /// <summary>
    /// Add gold to the pack
    /// </summary>
    void money(int value)
    {
        purse += value;
        mvaddch(hero.y, hero.x, floor_ch());
        set_chat(hero.y, hero.x, ((proom.r_flags & ISGONE) != 0) ? PASSAGE : FLOOR);

        if (value > 0)
        {
            if (!terse)
                addmsg("you found ");
            msg("%d gold pieces", value);
        }
    }

    /// <summary>
    /// Return the appropriate floor character for her room
    /// </summary>
    char floor_ch()
    {
        if ((proom.r_flags & ISGONE) != 0)
            return PASSAGE;

        return (show_floor() ? FLOOR : ' ');
    }

    /// <summary>
    /// Return the character at hero's position, taking see_floor into account
    /// </summary>
    char floor_at()
    {
        char ch = chat(hero.y, hero.x);
        if (ch == FLOOR)
            return floor_ch();

        return ch;
    }

    /// <summary>
    /// Reset the last command when the current one is aborted
    /// </summary>
    void reset_last()
    {
        last_commKey = l_last_commKey;
        last_dirKey = l_last_dirKey;
        last_pick = l_last_pick;
    }
}
