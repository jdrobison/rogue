using System.Diagnostics;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    bool EQSTR(string? a, string? b) => string.Equals(a, b);

    bool EQSTR(string? a, string? b, int len) => string.Compare(a, indexA: 0, b, indexB: 0, len) == 0;

    readonly string[] h_names =      /* strings for hitting */
    [
        " scored an excellent hit on ",
        " hit ",
        " have injured ",
        " swing and hit ",
        " scored an excellent hit on ",
        " hit ",
        " has injured ",
        " swings and hits "
    ];

    readonly string[] m_names =      /* strings for missing */
    [
        " miss",
        " swing and miss",
        " barely miss",
        " don't hit",
        " misses",
        " swings and misses",
        " barely misses",
        " doesn't hit",
    ];

    /*
     * adjustments to hit probabilities due to strength
     */
    readonly int[] str_plus =
    [
        -7, -6, -5, -4, -3, -2, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1,
        1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3,
    ];

    /*
     * adjustments to damage done due to strength
     */
    readonly int[] add_dam =
    [
        -7, -6, -5, -4, -3, -2, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 3,
        3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6
    ];

    /// <summary>
    /// The player attacks the monster.
    /// </summary>
    bool fight(coord mp, THING weap, bool thrown)
    {
        THING? tp;
        bool did_hit = true;
        string mname;
        char ch;

        /*
         * Find the monster we want to fight
         */
        if ((tp = moat(mp.y, mp.x)) == null)
        {
            debug("Fight what @ %d,%d", mp.y, mp.x);
            return false;
        }

        /*
         * Since we are fighting, things are not quiet so no healing takes
         * place.
         */
        count = 0;
        quiet = 0;
        runto(mp);

        /*
         * Let him know it was really a xeroc (if it was one).
         */
        ch = '\0';
        if (tp.t_type == 'X' && tp.t_disguise != 'X' && !on(player, ISBLIND))
        {
            tp.t_disguise = 'X';
            if (on(player, ISHALU))
            {
                ch = (char) (rnd(26) + 'A');
                mvaddch(tp.t_pos.y, tp.t_pos.x, ch);
            }
            msg(choose_str("heavy!  That's a nasty critter!",
                       "wait!  That's a xeroc!"));
            if (!thrown)
                return false;
        }
        mname = set_mname(tp);
        did_hit = false;
        has_hit = (terse && !to_death);
        if (roll_em(player, tp, weap, thrown))
        {
            did_hit = false;
            if (thrown)
                thunk(weap, mname, terse);
            else
                hit(null, mname, terse);
            if (on(player, CANHUH))
            {
                did_hit = true;
                tp.t_flags |= ISHUH;
                player.t_flags &= ~CANHUH;
                endmsg();
                has_hit = false;
                msg("your hands stop glowing %s", pick_color("red"));
            }
            if (tp.t_stats.s_hpt <= 0)
                killed(tp, true);
            else if (did_hit && !on(player, ISBLIND))
                msg("%s appears confused", mname);
            did_hit = true;
        }
        else if (thrown)
            bounce(weap, mname, terse);
        else
            miss(null, mname, terse);
        return did_hit;
    }

    /// <summary>
    /// The monster attacks the player
    /// </summary>
    int attack(THING mp)
    {
        THING? updated_mp = mp;
        string mname;
        int oldhp;

        /*
         * Since this is an attack, stop running and any healing that was
         * going on at the time.
         */
        running = false;
        count = 0;
        quiet = 0;
        if (to_death && !on(mp, ISTARGET))
        {
            to_death = false;
            kamikaze = false;
        }
        if (mp.t_type == 'X' && mp.t_disguise != 'X' && !on(player, ISBLIND))
        {
            mp.t_disguise = 'X';
            if (on(player, ISHALU))
                mvaddch(mp.t_pos.y, mp.t_pos.x, (char) (rnd(26) + 'A'));
        }
        mname = set_mname(mp);
        oldhp = pstats.s_hpt;

        if (roll_em(mp, player, null, false))
        {
            if (mp.t_type != 'I')
            {
                if (has_hit)
                    addmsg(".  ");
                hit(mname, null, false);
            }
            else
            {
                if (has_hit)
                    endmsg();
            }

            has_hit = false;

            if (pstats.s_hpt <= 0)
            {
                death(mp.t_type);  /* Bye bye life ... */
            }
            else if (!kamikaze)
            {
                oldhp -= pstats.s_hpt;
                if (oldhp > max_hit)
                    max_hit = oldhp;
                if (pstats.s_hpt <= max_hit)
                    to_death = false;
            }

            if (!on(mp, ISCANC))
            {
                switch (mp.t_type)
                {
                    case 'A':
                        /*
                         * If an aquator hits, you can lose armor class.
                         */
                        rust_armor(cur_armor);
                        break;

                    case 'I':
                        /*
                         * The ice monster freezes you
                         */
                        player.t_flags &= ~ISRUN;
                        if (no_command == 0)
                        {
                            addmsg("you are frozen");
                            if (!terse)
                                addmsg(" by the %s", mname);
                            endmsg();
                        }
                        no_command += rnd(2) + 2;
                        if (no_command > BORE_LEVEL)
                            death('h');
                        break;

                    case 'R':
                        /*
                         * Rattlesnakes have poisonous bites
                         */
                        if (!save(VS_POISON))
                        {
                            if (!ISWEARING(R_SUSTSTR))
                            {
                                chg_str(-1);
                                if (!terse)
                                    msg("you feel a bite in your leg and now feel weaker");
                                else
                                    msg("a bite has weakened you");
                            }
                            else if (!to_death)
                            {
                                if (!terse)
                                    msg("a bite momentarily weakens you");
                                else
                                    msg("bite has no effect");
                            }
                        }
                        break;

                    case 'W':
                    case 'V':
                        /*
                         * Wraiths might drain energy levels, and Vampires
                         * can steal max_hp
                         */
                        if (rnd(100) < (mp.t_type == 'W' ? 15 : 30))
                        {
                            int fewer;

                            if (mp.t_type == 'W')
                            {
                                if (pstats.s_exp == 0)
                                    death('W');     /* All levels gone */
                                if (--pstats.s_lvl == 0)
                                {
                                    pstats.s_exp = 0;
                                    pstats.s_lvl = 1;
                                }
                                else
                                    pstats.s_exp = e_levels[pstats.s_lvl-1]+1;
                                fewer = roll(1, 10);
                            }
                            else
                                fewer = roll(1, 3);
                            pstats.s_hpt -= fewer;
                            max_hp -= fewer;
                            if (pstats.s_hpt <= 0)
                                pstats.s_hpt = 1;
                            if (max_hp <= 0)
                                death(mp.t_type);
                            msg("you suddenly feel weaker");
                        }
                        break;

                    case 'F':
                        /*
                         * Venus Flytrap stops the poor guy from moving
                         */
                        player.t_flags |= ISHELD;
                        GetMonster('F').m_stats.s_dmg = $"{++vf_hit}x1";
                        if (--pstats.s_hpt <= 0)
                            death('F');
                        break;

                    case 'L':
                    {
                        /*
                         * Leperachaun steals some gold
                         */
                        int lastpurse;

                        lastpurse = purse;
                        purse -= GOLDCALC;
                        if (!save(VS_MAGIC))
                            purse -= GOLDCALC + GOLDCALC + GOLDCALC + GOLDCALC;
                        if (purse < 0)
                            purse = 0;
                        remove_mon(mp.t_pos, mp, false);
                        updated_mp = null;
                        if (purse != lastpurse)
                            msg("your purse feels lighter");
                        break;
                    }

                    case 'N':
                    {
                        int nobj = 0;

                        /*
                         * Nymph's steal a magic item, look through the pack
                         * and pick out one we like.
                         */
                        THING? steal = null;

                        for (THING? obj = pack; obj != null; obj = next(obj))
                        {
                            if (obj != cur_armor &&
                                obj != cur_weapon &&
                                obj != cur_ring[LEFT] &&
                                obj != cur_ring[RIGHT] &&
                                is_magic(obj) &&
                                rnd(++nobj) == 0)
                            {
                                steal = obj;
                            }
                        }

                        if (steal != null)
                        {
                            if (moat(mp.t_pos.y, mp.t_pos.x) is THING monster)
                                remove_mon(mp.t_pos, monster, false);
                            updated_mp = null;
                            leave_pack(steal, false, false);
                            msg("she stole %s!", inv_name(steal, true));
                            discard(steal);
                        }

                        break;
                    }
                }
            }
        }
        else if (mp.t_type != 'I')
        {
            if (has_hit)
            {
                addmsg(".  ");
                has_hit = false;
            }
            if (mp.t_type == 'F')
            {
                pstats.s_hpt -= vf_hit;
                if (pstats.s_hpt <= 0)
                    death(mp.t_type);  /* Bye bye life ... */
            }
            miss(mname, null, false);
        }

        if (fight_flush && !to_death)
            flush_type();
        count = 0;
        status();

        if (updated_mp == null)
            return (-1);
        else
            return (0);
    }

    /// <summary>
    /// return the monster name for the given monster
    /// </summary>
    /// <param name="tp"></param>
    string set_mname(THING tp)
    {
        char ch;
        string mname;

        if (!see_monst(tp) && !on(player, SEEMONST))
            return (terse ? "it" : "something");
        else if (on(player, ISHALU))
        {
            move(tp.t_pos.y, tp.t_pos.x);
            ch = inch();
            if (!char.IsAsciiLetterUpper(ch))
                ch = (char) rnd(26);
            else
                ch -= 'A';
            mname = monsters[ch].m_name;
        }
        else
            mname = monsters[tp.t_type - 'A'].m_name;

        return $"the {mname}";
    }

    /// <summary>
    /// Returns true if the swing hits
    /// </summary>
    bool swing(int at_lvl, int op_arm, int wplus)
    {
        int res = rnd(20);
        int need = (20 - at_lvl) - op_arm;

        return (res + wplus >= need);
    }

    /// <summary>
    /// Roll several attacks
    /// </summary>
    bool roll_em(THING thatt, THING thdef, THING? weap, bool hurl)
    {
        string? damageDescription;
        int def_arm;
        bool did_hit = false;
        int hplus;
        int dplus;
        int damage;

        stats att = thatt.t_stats;
        stats def = thdef.t_stats;
        if (weap == null)
        {
            damageDescription = att.s_dmg;
            dplus = 0;
            hplus = 0;
        }
        else
        {
            hplus = (weap == null ? 0 : weap.o_hplus);
            dplus = (weap == null ? 0 : weap.o_dplus);
            if (weap == cur_weapon)
            {
                if (ISRING(LEFT, R_ADDDAM))
                    dplus += cur_ring[LEFT]!.o_arm;
                else if (ISRING(LEFT, R_ADDHIT))
                    hplus += cur_ring[LEFT]!.o_arm;
                if (ISRING(RIGHT, R_ADDDAM))
                    dplus += cur_ring[RIGHT]!.o_arm;
                else if (ISRING(RIGHT, R_ADDHIT))
                    hplus += cur_ring[RIGHT]!.o_arm;
            }
            damageDescription = weap.o_damage;
            if (hurl)
            {
                if (((weap.o_flags & ISMISL) != 0) && cur_weapon != null &&
                  cur_weapon.o_which == weap.o_launch)
                {
                    damageDescription = weap.o_hurldmg;
                    hplus += cur_weapon.o_hplus;
                    dplus += cur_weapon.o_dplus;
                }
                else if (weap.o_launch < 0)
                    damageDescription = weap.o_hurldmg;
            }
        }
        /*
         * If the creature being attacked is not running (alseep or held)
         * then the attacker gets a plus four bonus to hit.
         */
        if (!on(thdef, ISRUN))
            hplus += 4;
        def_arm = def.s_arm;
        if (thdef == player)
        {
            if (cur_armor != null)
                def_arm = cur_armor.o_arm;
            if (ISRING(LEFT, R_PROTECT))
                def_arm -= cur_ring[LEFT]!.o_arm;
            if (ISRING(RIGHT, R_PROTECT))
                def_arm -= cur_ring[RIGHT]!.o_arm;
        }

        if (Debugger.IsAttached)
            Debugger.Break();

        if (damageDescription != null)
        {
            for (int i = 0; i < damageDescription.Length; i++)
            {
                int indexX = damageDescription.IndexOf('x', i);
                if (indexX == -1)
                    break;

                ReadOnlySpan<char> span = damageDescription.AsSpan(i, indexX - i);
                if (!int.TryParse(span, out int ndice))
                    ndice = 0;

                i = indexX + 1;
                int indexSlash = damageDescription.IndexOf('/', i);

                span = (indexSlash == -1)
                    ? damageDescription.AsSpan(i)
                    : damageDescription.AsSpan(i, indexSlash - i);

                if (!int.TryParse(span, out int nsides))
                    nsides = 0;

                if (swing(att.s_lvl, def_arm, hplus + str_plus[att.s_str]))
                {
                    int proll;

                    proll = roll(ndice, nsides);
#if MASTER
                    if (ndice + nsides > 0 && proll <= 0)
                        debug("Damage for %dx%d came out %d, dplus = %d, add_dam = %d, def_arm = %d", ndice, nsides, proll, dplus, add_dam[att.s_str], def_arm);
#endif
                    damage = dplus + proll + add_dam[att.s_str];
                    def.s_hpt -= max(0, damage);
                    did_hit = true;
                }

                if (indexSlash == -1)
                    break;

                i = indexSlash;
            }
        }

        return did_hit;
    }

    /// <summary>
    /// Return the name of a combatant
    /// </summary>
    string prname(string? mname, bool upper)
    {
        string s = mname ?? "you";

        if (upper && !Char.IsUpper(s[0]))
            s = Char.ToUpper(s[0]) + s[1..];

        return s;
    }

    /// <summary>
    /// A missile hits a monster
    /// </summary>
    void thunk(THING weap, string mname, bool noend)
    {
        if (to_death)
            return;
        if (weap.o_type == WEAPON)
            addmsg("the %s hits ", weap_info[weap.o_which].oi_name);
        else
            addmsg("you hit ");
        addmsg(mname);
        if (!noend)
            endmsg();
    }

    /// <summary>
    /// Print a message to indicate a succesful hit
    /// </summary>
    void hit(string er, string? ee, bool noend)
    {
        string s;

        if (to_death)
            return;
        addmsg(prname(er, true));
        if (terse)
            s = " hit";
        else
        {
            int i = rnd(4);
            if (er != null)
                i += 4;
            s = h_names[i];
        }
        addmsg(s);
        if (!terse)
            addmsg(prname(ee, false));
        if (!noend)
            endmsg();
    }

    /// <summary>
    /// Print a message to indicate a poor swing
    /// </summary>
    void miss(string? er, string? ee, bool noend)
    {
        int i;

        if (to_death)
            return;
        addmsg(prname(er, true));
        if (terse)
            i = 0;
        else
            i = rnd(4);
        if (er != null)
            i += 4;
        addmsg(m_names[i]);
        if (!terse)
            addmsg(" %s", prname(ee, false));
        if (!noend)
            endmsg();
    }

    /// <summary>
    /// A missile misses a monster
    /// </summary>
    void bounce(THING weap, string mname, bool noend)
    {
        if (to_death)
            return;
        if (weap.o_type == WEAPON)
            addmsg("the %s misses ", weap_info[weap.o_which].oi_name);
        else
            addmsg("you missed ");
        addmsg(mname);
        if (!noend)
            endmsg();
    }

    /// <summary>
    /// Remove a monster from the screen
    /// </summary>
    void remove_mon(coord mp, THING tp, bool waskill)
    {
        THING? nexti;

        for (THING? obj = tp.t_pack; obj != null; obj = nexti)
        {
            nexti = next(obj);
            obj.o_pos = tp.t_pos;
            detach(ref tp.t_pack, obj);
            if (waskill)
                fall(obj, false);
            else
                discard(obj);
        }
        set_moat(mp.y, mp.x, null);
        mvaddch(mp.y, mp.x, tp.t_oldch);
        detach(ref mlist, tp);
        if (on(tp, ISTARGET))
        {
            kamikaze = false;
            to_death = false;
            if (fight_flush)
                flush_type();
        }
        discard(tp);
    }

    /// <summary>
    /// Called to put a monster to death
    /// </summary>
    void killed(THING tp, bool pr)
    {
        pstats.s_exp += tp.t_stats.s_exp;

        /*
         * If the monster was a venus flytrap, un-hold him
         */
        switch (tp.t_type)
        {
            case 'F':
                player.t_flags &= ~ISHELD;
                vf_hit = 0;
                GetMonster('F').m_stats.s_dmg = "000x0";
                break;

            case 'L':
                if (fallpos(tp.t_pos, out tp.t_room.r_gold) && level >= max_level)
                {
                    THING gold = new_item();
                    gold.o_type = GOLD;
                    gold.o_goldval = GOLDCALC;
                    if (save(VS_MAGIC))
                    {
                        // GOLDCALC returns a random value, so don't replace this with 4 * GOLDCALC
                        gold.o_goldval += GOLDCALC + GOLDCALC + GOLDCALC + GOLDCALC;
                    }
                    attach(ref tp.t_pack, gold);
                }
                break;
        }
        /*
         * Get rid of the monster.
         */
        string mname = set_mname(tp);
        remove_mon(tp.t_pos, tp, true);
        if (pr)
        {
            if (has_hit)
            {
                addmsg(".  Defeated ");
                has_hit = false;
            }
            else
            {
                if (!terse)
                    addmsg("you have ");
                addmsg("defeated ");
            }
            msg(mname);
        }
        /*
         * Do adjustments if he went up a level
         */
        check_level();
        if (fight_flush)
            flush_type();
    }
}
