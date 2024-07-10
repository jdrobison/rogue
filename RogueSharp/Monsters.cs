using System.Collections.Immutable;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Reflection.Emit;
using System.Xml.Linq;

using RogueSharp;
using RogueSharp.Things;
using RogueSharp.Things.Monsters;

using Windows.Win32.System.Console;

namespace RogueSharp;

internal partial class Program
{
#if false
    /// <summary>
    /// List of monsters in rough order of deadliness
    /// </summary>
    private static readonly ImmutableArray<Type?> Monsters =
    [
        typeof(Kestrel),
        typeof(Emu),
        typeof(Bat),
        typeof(Snake),
        typeof(Hobgoblin),
        typeof(IceMonster),
        typeof(Rattlesnake),
        typeof(Orc),
        typeof(Zombie),
        typeof(Leprechaun),
        typeof(Centaur),
        typeof(Quagga),
        typeof(Aquator),
        typeof(Nymph),
        typeof(Yeti),
        typeof(Flytrap),
        typeof(Troll),
        typeof(Wraith),
        typeof(Phantom),
        typeof(Xeroc),
        typeof(Unicorn),
        typeof(Medusa),
        typeof(Vampire),
        typeof(Griffin),
        typeof(Jabberwock),
        typeof(Dragon),
    ];

    /// <summary>
    /// List of wandering monsters in rough order of deadliness
    /// </summary>
    private static readonly ImmutableArray<Type?> WanderingMonsters =
    [
        typeof(Kestrel),
        typeof(Emu),
        typeof(Bat),
        typeof(Snake),
        typeof(Hobgoblin),
        null,
        typeof(Rattlesnake),
        typeof(Orc),
        typeof(Zombie),
        null,
        typeof(Centaur),
        typeof(Quagga),
        typeof(Aquator),
        null,
        typeof(Yeti),
        null,
        typeof(Troll),
        typeof(Wraith),
        typeof(Phantom),
        null,
        typeof(Unicorn),
        typeof(Medusa),
        typeof(Vampire),
        typeof(Griffin),
        typeof(Jabberwock),
        null,
    ];

    /// <summary>
    /// Creates a random <see cref="Monster"/>.  The higher the level, the meaner the monster.
    /// </summary>
    private Monster CreateMonster(int level, bool wander)
    {
        ImmutableArray<Type?> monsters = (wander ? WanderingMonsters : Monsters);

        while (true)
        {
            int i = level + (rnd(10) - 6);
            
            if (i < 0)
                i = rnd(5);
            if (i > monsters.Length - 1)
                i = rnd(5) + 21;

            if (monsters[i] is Type monsterType)
                return (Monster) Activator.CreateInstance(monsterType)!;
        }
    }
#endif

    /*
     * List of monsters in rough order of vorpalness
     */
    static char[] lvl_mons =
    [
        'K', 'E', 'B', 'S', 'H', 'I', 'R', 'O', 'Z', 'L', 'C', 'Q', 'A',
        'N', 'Y', 'F', 'T', 'W', 'P', 'X', 'U', 'M', 'V', 'G', 'J', 'D'
    ];

    static char[] wand_mons = 
    [
        'K',  'E', 'B',  'S', 'H', '\0', 'R',  'O', 'Z', '\0', 'C', 'Q', 'A',
        '\0', 'Y', '\0', 'T', 'W', 'P',  '\0', 'U', 'M', 'V',  'G', 'J', '\0'
    ];

    /// <summary>
    /// Pick a monster to show up.  The lower the level, the meaner the monster.
    /// </summary>
    char randmonster(bool wander)
    {
        int d;
        char[] mons;

        mons = (wander ? wand_mons : lvl_mons);
        do
        {
            d = level + (rnd(10) - 6);
            if (d < 0)
                d = rnd(5);
            if (d > 25)
                d = rnd(5) + 21;
        } while (mons[d] == 0);
        return mons[d];
    }

#if false
    /*
     * new_monster:
     *    Pick a new monster and add it to the list
     */

    void
    new_monster(THING* tp, char type, coord* cp)
    {
    struct monster *mp;
    int lev_add;

    if ((lev_add = level - AMULETLEVEL) < 0)
    lev_add = 0;
    attach(mlist, tp);
    tp->t_type = type;
    tp->t_disguise = type;
    tp->t_pos = *cp;
    move(cp->y, cp->x);
    tp->t_oldch = CCHAR(inch() );
    tp->t_room = roomin(cp);
    moat(cp->y, cp->x) = tp;
    mp = &monsters[tp->t_type-'A'];
    tp->t_stats.s_lvl = mp->m_stats.s_lvl + lev_add;
    tp->t_stats.s_maxhp = tp->t_stats.s_hpt = roll(tp->t_stats.s_lvl, 8);
    tp->t_stats.s_arm = mp->m_stats.s_arm - lev_add;
    strcpy(tp->t_stats.s_dmg, mp->m_stats.s_dmg);
    tp->t_stats.s_str = mp->m_stats.s_str;
    tp->t_stats.s_exp = mp->m_stats.s_exp + lev_add* 10 + exp_add(tp);
    tp->t_flags = mp->m_flags;
    if (level > 29)
    tp->t_flags |= ISHASTE;
    tp->t_turn = true;
    tp->t_pack = null;
    if (ISWEARING(R_AGGR))
    runto(cp);
    if (type == 'X')
    tp->t_disguise = rnd_thing();
}

/*
 * expadd:
 *    Experience to add for this monster's level/hit points
 */
int
exp_add(THING* tp)
{
    int mod;

    if (tp->t_stats.s_lvl == 1)
        mod = tp->t_stats.s_maxhp / 8;
    else
        mod = tp->t_stats.s_maxhp / 6;
    if (tp->t_stats.s_lvl > 9)
        mod *= 20;
    else if (tp->t_stats.s_lvl > 6)
        mod *= 4;
    return mod;
}

/*
 * wanderer:
 *    Create a new wandering monster and aim it at the player
 */

void
wanderer()
{
    THING *tp;
    static coord cp;

    tp = new_item();
    do
    {
        find_floor((struct room *) null, &cp, false, true);
    } while (roomin(&cp) == proom) ;
new_monster(tp, randmonster(true), &cp);
if (on(player, SEEMONST))
{
    standout();
    if (!on(player, ISHALU))
        addch(tp->t_type);
    else
        addch(rnd(26) + 'A');
    standend();
}
runto(&tp->t_pos);
#if MASTER
if (wizard)
    msg("started a wandering %s", monsters[tp->t_type-'A'].m_name);
#endif
}
#endif

    /// <summary>
    /// What to do when the hero steps next to a monster
    /// </summary>
    THING wake_monster(int y, int x)
    {
        THING tp;
        room rp;
        char ch;
        string mname;

#if MASTER
        if ((tp = moat(y, x)) == null)
            msg("can't find monster in wake_monster");
#else
        tp = moat(y, x);
        if (tp == null)
            endwin(), abort();
#endif
        ch = tp.t_type;
        /*
         * Every time he sees mean monster, it might start chasing him
         */
        if (!on(tp, ISRUN) && rnd(3) != 0 && on(tp, ISMEAN) && !on(tp, ISHELD)
            && !ISWEARING(R_STEALTH) && !on(player, ISLEVIT))
        {
            tp.t_dest = hero;
            tp.t_flags |= ISRUN;
        }
        if (ch == 'M' && !on(player, ISBLIND) && !on(player, ISHALU)
            && !on(tp, ISFOUND) && !on(tp, ISCANC) && on(tp, ISRUN))
        {
            rp = proom;
            if ((rp != null && (rp.r_flags & ISDARK) == 0)
                || dist(y, x, hero.y, hero.x) < LAMPDIST)
            {
                tp.t_flags |= ISFOUND;
                if (!save(VS_MAGIC))
                {
                    if (on(player, ISHUH))
                        lengthen(unconfuse, spread(HUHDURATION));
                    else
                        fuse(unconfuse, 0, spread(HUHDURATION), AFTER);
                    player.t_flags |= ISHUH;
                    mname = set_mname(tp);
                    addmsg(mname);
                    if (mname != "it")
                        addmsg("'");
                    msg("s gaze has confused you");
                }
            }
        }
        /*
         * Let greedy ones guard gold
         */
        if (on(tp, ISGREED) && !on(tp, ISRUN))
        {
            tp.t_flags |= ISRUN;
            if (proom.r_goldval != 0)
                tp.t_dest = proom.r_gold;
            else
                tp.t_dest = hero;
        }
        return tp;
    }

#if false
    /// <summary>
    /// Give a pack to a monster if it deserves one
    /// </summary>
    void give_pack(THING tp)
    {
        if (level >= max_level && rnd(100) < monsters[tp.t_type-'A'].m_carry)
            attach(tp.t_pack, new_thing());
    }
#endif

    /// <summary>
    /// See if a creature save against something
    /// </summary>
    bool save_throw(int which, THING tp)
    {
        int need;

        need = 14 + which - tp.t_stats.s_lvl / 2;
        return (roll(1, 20) >= need);
    }

    /// <summary>
    /// See if he saves against various nasty things
    /// </summary>
    bool save(int which)
    {
        if (which == VS_MAGIC)
        {
            if (ISRING(LEFT, R_PROTECT))
                which -= cur_ring[LEFT]!.o_arm;
            if (ISRING(RIGHT, R_PROTECT))
                which -= cur_ring[RIGHT]!.o_arm;
        }
        return save_throw(which, player);
    }
}
