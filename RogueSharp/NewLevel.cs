using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>one chance in <see cref="TREAS_ROOM"/> for a treasure room</summary>
    const int TREAS_ROOM = 20;

    /// <summary>maximum number of treasures in a treasure room</summary>
    const int MAXTREAS = 10;

    /// <summary>minimum number of treasures in a treasure room</summary>
    const int MINTREAS = 2;

    void new_level()
    {
        string sp;
        int i;

        player.t_flags &= ~ISHELD;  /* unhold when you go down just in case */
        if (level > max_level)
            max_level = level;
        /*
         * Clean things off from last level
         */
        foreach (PLACE place in places)
        {
            place.p_ch = ' ';
            place.p_flags = F_REAL;
            place.p_monst = null;
        }

        clear();

        /*
         * Free up the monsters on the last level
         */
        for (THING? monster = mlist; monster != null; monster = next(monster))
            free_list(ref monster.t_pack);

        free_list(ref mlist);

        /*
         * Throw away stuff left on the previous level (if anything)
         */
        free_list(ref lvl_obj);
        do_rooms();             /* Draw rooms */
        do_passages();          /* Draw passages */
        no_food++;
        put_things();           /* Place objects (if any) */

        /*
         * Place the traps
         */
        if (rnd(10) < level)
        {
            ntraps = rnd(level / 4) + 1;
            if (ntraps > MAXTRAPS)
                ntraps = MAXTRAPS;
            i = ntraps;
            while (i-- != 0)
            {
                /*
                 * not only wouldn't it be NICE to have traps in mazes
                 * (not that we care about being nice), since the trap
                 * number is stored where the passage number is, we
                 * can't actually do it.
                 */
                do
                {
                    find_floor(null, out stairs, 0, false);
                } while (chat(stairs.y, stairs.x) != FLOOR);
                
                byte flags = flat(stairs.y, stairs.x);

                flags &= unchecked((byte) ~F_REAL);
                flags |= (byte) rnd(NTRAPS);

                set_flat(stairs.y, stairs.x, flags);
            }
        }
        /*
         * Place the staircase down.
         */
        find_floor(null, out stairs, 0, false);
        set_chat(stairs.y, stairs.x, STAIRS);
        seenstairs = false;

        for (THING? monster = mlist; monster != null; monster = next(monster))
        {
            if (roomin(monster.t_pos) is room room)
                monster.t_room = room;
        }

        find_floor(null, out hero, 0, true);
        enter_room(hero);
        mvaddch(hero.y, hero.x, PLAYER);
        if (on(player, SEEMONST))
            turn_see(false);
        if (on(player, ISHALU))
            visuals();
    }

    /// <summary>
    /// Pick a room that is really there
    /// </summary>
    int rnd_room()
    {
        int rm;

        do
        {
            rm = rnd(MAXROOMS);
        } while ((rooms[rm].r_flags & ISGONE) != 0);
        
        return rm;
    }

    /// <summary>
    /// Put potions and scrolls on this level
    /// </summary>
    void put_things()
    {
        int i;
        THING obj;

        /*
         * Once you have found the amulet, the only way to get new stuff is
         * go down into the dungeon.
         */
        if (amulet && level < max_level)
            return;
        /*
         * check for treasure rooms, and if so, put it in.
         */
        if (rnd(TREAS_ROOM) == 0)
            treas_room();
        /*
         * Do MAXOBJ attempts to put things on a level
         */
        for (i = 0; i < MAXOBJ; i++)
            if (rnd(100) < 36)
            {
                /*
                 * Pick a new object and link it in the list
                 */
                obj = new_thing();
                attach(ref lvl_obj, obj);
                /*
                 * Put it somewhere
                 */
                find_floor(null, out obj.o_pos, 0, false);
                set_chat(obj.o_pos.y, obj.o_pos.x, (char) obj.o_type);
            }
        /*
         * If he is really deep in the dungeon and he hasn't found the
         * amulet yet, put it somewhere on the ground
         */
        if (level >= AMULETLEVEL && !amulet)
        {
            obj = new_item();
            attach(ref lvl_obj, obj);
            obj.o_hplus = 0;
            obj.o_dplus = 0;
            obj.o_damage = "0x0";
            obj.o_hurldmg = "0x0";
            obj.o_arm = 11;
            obj.o_type = AMULET;
            /*
             * Put it somewhere
             */
            find_floor(null, out obj.o_pos, 0, false);
            set_chat(obj.o_pos.y, obj.o_pos.x, AMULET);
        }
    }

    private coord _treas_room_mp;

    /// <summary>
    /// Add a treasure room
    /// </summary>
    void treas_room()
    {
        const int MAXTRIES = 10;    /* max number of tries to put down a monster */

        int nm;
        THING tp;
        room rp;
        int spots, num_monst;

        rp = rooms[rnd_room()];
        spots = (rp.r_max.y - 2) * (rp.r_max.x - 2) - MINTREAS;
        if (spots > (MAXTREAS - MINTREAS))
            spots = (MAXTREAS - MINTREAS);
        num_monst = nm = rnd(spots) + MINTREAS;
        while (nm-- != 0)
        {
            find_floor(rp, out _treas_room_mp, 2 * MAXTRIES, false);
            tp = new_thing();
            tp.o_pos = _treas_room_mp;
            attach(ref lvl_obj, tp);
            set_chat(_treas_room_mp.y, _treas_room_mp.x, (char) tp.o_type);
        }

        /*
         * fill up room with monsters from the next level down
         */

        if ((nm = rnd(spots) + MINTREAS) < num_monst + 2)
            nm = num_monst + 2;
        spots = (rp.r_max.y - 2) * (rp.r_max.x - 2);
        if (nm > spots)
            nm = spots;
        level++;
        while (nm-- != 0)
        {
            spots = 0;
            if (find_floor(rp, out _treas_room_mp, MAXTRIES, true))
            {
                tp = new_item();
                new_monster(tp, randmonster(false), _treas_room_mp);
                tp.t_flags |= ISMEAN;  /* no sloughers in THIS room */
                give_pack(tp);
            }
        }
        level--;
    }
}
