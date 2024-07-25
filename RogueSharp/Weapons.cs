using System.Text;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    const int NO_WEAPON = -1;

    int group = 2;

    class init_weaps
    {
        public init_weaps(string dam, string hrl, int launch, int flags)
        {
            iw_dam = dam;
            iw_hrl = hrl;
            iw_launch = launch;
            iw_flags = flags;
        }

        public string iw_dam { get; }  /* Damage when wielded */
        public string iw_hrl { get; }  /* Damage when thrown */
        public int iw_launch { get; }  /* Launching weapon */
        public int iw_flags { get; }   /* Miscellaneous flags */
    }

    init_weaps[] init_dam =
    {
        new( "2x4", "1x3",  NO_WEAPON,  0               ),  /* Mace */
        new( "3x4", "1x2",  NO_WEAPON,  0               ),  /* Long sword */
        new( "1x1", "1x1",  NO_WEAPON,  0               ),  /* Bow */
        new( "1x1", "2x3",  BOW,        ISMANY|ISMISL   ),  /* Arrow */
        new( "1x6", "1x4",  NO_WEAPON,  ISMISL|ISMISL   ),  /* Dagger */
        new( "4x4", "1x2",  NO_WEAPON,  0               ),  /* 2h sword */
        new( "1x1", "1x3",  NO_WEAPON,  ISMANY|ISMISL   ),  /* Dart */
        new( "1x2", "2x4",  NO_WEAPON,  ISMANY|ISMISL   ),  /* Shuriken */
        new( "2x3", "1x6",  NO_WEAPON,  ISMISL          ),  /* Spear */
    };

    /// <summary>
    /// Fire a missile in a given direction
    /// </summary>
    void missile(int dy, int dx)
    {
        THING? obj;

        /*
         * Get which thing we are hurling
         */
        if ((obj = get_item("throw", WEAPON)) == null)
            return;
        if (!dropcheck(obj) || is_current(obj))
            return;
        obj = leave_pack(obj, true, false);
        do_motion(obj, dy, dx);

        /*
         * AHA! Here it has hit something.  If it is a wall or a door,
         * or if it misses (combat) the monster, put it on the floor
         */
        if (moat(obj.o_pos.y, obj.o_pos.x) == null || !hit_monster(obj.o_pos.y, obj.o_pos.x, obj))
            fall(obj, true);
    }

    /// <summary>
    /// Do the actual motion on the screen done by an object traveling across the room
    /// </summary>

    void do_motion(THING obj, int ydelta, int xdelta)
    {
        char ch;

        /*
         * Come fly with us ...
         */
        obj.o_pos = hero;
        for (; ; )
        {
            /*
             * Erase the old one
             */
            if (!ce(obj.o_pos, hero) && cansee(obj.o_pos.y, obj.o_pos.x) && !terse)
            {
                ch = chat(obj.o_pos.y, obj.o_pos.x);
                if (ch == FLOOR && !show_floor())
                    ch = ' ';
                mvaddch(obj.o_pos.y, obj.o_pos.x, ch);
            }

            /*
             * Get the new position
             */
            obj.o_pos.y += ydelta;
            obj.o_pos.x += xdelta;
            if (step_ok(ch = winat(obj.o_pos.y, obj.o_pos.x)) && ch != DOOR)
            {
                /*
                 * It hasn't hit anything yet, so display it
                 * If it alright.
                 */
                if (cansee(obj.o_pos.y, obj.o_pos.x) && !terse)
                {
                    mvaddch(obj.o_pos.y, obj.o_pos.x, (char) obj.o_type);
                    refresh();
                }
                continue;
            }
            break;
        }
    }

    /// <summary>
    /// Drop an item someplace around here.
    /// </summary>
    void fall(THING obj, bool pr)
    {
        if (fallpos(obj.o_pos, out coord fpos))
        {
            PLACE place = INDEX(fpos.y, fpos.x);
            place.p_ch = (char) obj.o_type;
            obj.o_pos = fpos;
            if (cansee(fpos.y, fpos.x))
            {
                if (place.p_monst != null)
                    place.p_monst.t_oldch = (char) obj.o_type;
                else
                    mvaddch(fpos.y, fpos.x, (char) obj.o_type);
            }
            attach(ref lvl_obj, obj);
            return;
        }
        if (pr)
        {
            if (has_hit)
            {
                endmsg();
                has_hit = false;
            }
            msg("the %s vanishes as it hits the ground",
                weap_info[obj.o_which].oi_name);
        }
        discard(obj);
    }

    /// <summary>
    /// Set up the initial goodies for a weapon
    /// </summary>
    void init_weapon(THING weap, int which)
    {
        init_weaps iwp = init_dam[which];

        weap.o_type = WEAPON;
        weap.o_which = which;
        weap.o_damage = iwp.iw_dam;
        weap.o_hurldmg = iwp.iw_hrl;
        weap.o_launch = iwp.iw_launch;
        weap.o_flags = iwp.iw_flags;
        weap.o_hplus = 0;
        weap.o_dplus = 0;

        if (which == DAGGER)
        {
            weap.o_count = rnd(4) + 2;
            weap.o_group = group++;
        }
        else if ((weap.o_flags & ISMANY) != 0)
        {
            weap.o_count = rnd(8) + 8;
            weap.o_group = group++;
        }
        else
        {
            weap.o_count = 1;
            weap.o_group = 0;
        }
    }

    /// <summary>
    /// Does the missile hit the monster?
    /// </summary>
    bool hit_monster(int y, int x, THING obj)
    {
        coord mp;

        mp.y = y;
        mp.x = x;
        return fight(mp, obj, true);
    }

    /// <summary>
    /// Figure out the plus number for armor/weapons
    /// </summary>
    string num(int n1, int n2, char type)
    {
        StringBuilder builder = new();
        builder.AppendFormat(n1 < 0 ? "{0}" : "+{0}", n1);

        if (type == WEAPON)
            builder.AppendFormat(n2 < 0 ? ",{0}" : ",+{0}", n2);

        return builder.ToString();
    }

    /// <summary>
    /// Pull out a certain weapon
    /// </summary>
    void wield()
    {
        THING? obj, oweapon;
        string sp;

        oweapon = cur_weapon;
        if (!dropcheck(cur_weapon))
        {
            cur_weapon = oweapon;
            return;
        }
        cur_weapon = oweapon;
        if ((obj = get_item("wield", WEAPON)) == null)
        {
            after = false;
            return;
        }

        if (obj.o_type == ARMOR)
        {
            msg("you can't wield armor");
            after = false;
            return;
        }

        if (is_current(obj))
        {
            after = false;
            return;
        }

        sp = inv_name(obj, true);
        cur_weapon = obj;
        if (!terse)
            addmsg("you are now ");
        msg("wielding %s (%c)", sp, obj.o_packch);
    }

    /// <summary>
    /// Pick a random position around the given (y, x) coordinates
    /// </summary>
    bool fallpos(coord pos, out coord newpos)
    {
        newpos = default;

        int cnt = 0;

        for (int y = pos.y - 1; y <= pos.y + 1; y++)
        {
            for (int x = pos.x - 1; x <= pos.x + 1; x++)
            {
                /*
                 * check to make certain the spot is empty, if it is,
                 * put the object there, set it in the level list
                 * and re-draw the room if he can see it
                 */
                if (y == hero.y && x == hero.x)
                    continue;

                int ch;
                if (((ch = chat(y, x)) == FLOOR || ch == PASSAGE) && rnd(++cnt) == 0)
                {
                    newpos.y = y;
                    newpos.x = x;
                }
            }
        }

        return (cnt != 0);
    }
}
