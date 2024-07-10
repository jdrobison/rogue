using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RogueSharp.Things;

using Windows.Win32.System.Console;

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

#if false
/*
 * missile:
 *.Fire a missile in a given direction
 */

void
missile(int ydelta, int xdelta)
{
    THING obj;

    /*
     * Get which thing we are hurling
     */
    if ((obj = get_item("throw", WEAPON)) == null)
        return;
    if (!dropcheck(obj) || is_current(obj))
        return;
    obj = leave_pack(obj, true, false);
    do_motion(obj, ydelta, xdelta);
    /*
     * AHA! Here it has hit something.  If it is a wall or a door,
     * or if it misses (combat) the monster, put it on the floor
     */
    if (moat(obj.o_pos.y, obj.o_pos.x) == null ||
    !hit_monster(unc(obj.o_pos), obj))
        fall(obj, true);
}

/*
 * do_motion:
 *.Do the actual motion on the screen done by an object traveling
 *.across the room
 */

void
do_motion(THING* obj, int ydelta, int xdelta)
{
    int ch;

    /*
     * Come fly with us ...
     */
    obj.o_pos = hero;
    for (; ; )
    {
        /*
         * Erase the old one
         */
        if (!ce(obj.o_pos, hero) && cansee(unc(obj.o_pos)) && !terse)
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
            if (cansee(unc(obj.o_pos)) && !terse)
            {
                mvaddch(obj.o_pos.y, obj.o_pos.x, obj.o_type);
                refresh();
            }
            continue;
        }
        break;
    }
}

/*
 * fall:
 *.Drop an item someplace around here.
 */

void
fall(THING* obj, bool pr)
{
    PLACE *pp;
    static coord fpos;

    if (fallpos(&obj.o_pos, &fpos))
    {
        pp = INDEX(fpos.y, fpos.x);
        pp.p_ch = (char) obj.o_type;
        obj.o_pos = fpos;
        if (cansee(fpos.y, fpos.x))
        {
            if (pp.p_monst != null)
                pp.p_monst.t_oldch = (char) obj.o_type;
            else
                mvaddch(fpos.y, fpos.x, obj.o_type);
        }
        attach(lvl_obj, obj);
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
#endif

    /*
     * init_weapon:
     */
    /// <summary>
    /// Set up the initial goodies for a weapon
    /// </summary>
    /// <param name="weap"></param>
    /// <param name="which"></param>
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

#if false
/*
 * hit_monster:
 *.Does the missile hit the monster?
 */
int
hit_monster(int y, int x, THING* obj)
{
    static coord mp;

    mp.y = y;
    mp.x = x;
    return fight(&mp, obj, true);
}
#endif

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

#if false
/*
 * wield:
 *.Pull out a certain weapon
 */

void
wield()
{
    THING obj, *oweapon;
    char *sp;

    oweapon = cur_weapon;
    if (!dropcheck(cur_weapon))
    {
        cur_weapon = oweapon;
        return;
    }
    cur_weapon = oweapon;
    if ((obj = get_item("wield", WEAPON)) == null)
    {
bad:
        after = false;
        return;
    }

    if (obj.o_type == ARMOR)
    {
        msg("you can't wield armor");
        goto bad;
    }
    if (is_current(obj))
        goto bad;

    sp = inv_name(obj, true);
    cur_weapon = obj;
    if (!terse)
        addmsg("you are now ");
    msg("wielding %s (%c)", sp, obj.o_packch);
}

/*
 * fallpos:
 *.Pick a random position around the give (y, x) coordinates
 */
bool
fallpos(coord* pos, coord* newpos)
{
    int y, x, cnt, ch;

    cnt = 0;
    for (y = pos.y - 1; y <= pos.y + 1; y++)
        for (x = pos.x - 1; x <= pos.x + 1; x++)
        {
            /*
             * check to make certain the spot is empty, if it is,
             * put the object there, set it in the level list
             * and re-draw the room if he can see it
             */
            if (y == hero.y && x == hero.x)
                continue;
            if (((ch = chat(y, x)) == FLOOR || ch == PASSAGE)
                        && rnd(++cnt) == 0)
            {
                newpos.y = y;
                newpos.x = x;
            }
        }
    return (bool) (cnt != 0);
}
#endif
}
