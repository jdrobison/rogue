using System.Diagnostics;
using System.Text;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>
    /// Roll her up
    /// </summary>
    void init_player()
    {
        THING obj;

        pstats = max_stats;
        food_left = HUNGERTIME;
        /*
         * Give him some food
         */
        obj = new_item();
        obj.o_type = FOOD;
        obj.o_count = 1;
        add_pack(obj, true);
        /*
         * And his suit of armor
         */
        obj = new_item();
        obj.o_type = ARMOR;
        obj.o_which = RING_MAIL;
        obj.o_arm = a_class[RING_MAIL] - 1;
        obj.o_flags |= ISKNOW;
        obj.o_count = 1;
        cur_armor = obj;
        add_pack(obj, true);
        /*
         * Give him his weaponry.  First a mace.
         */
        obj = new_item();
        init_weapon(obj, MACE);
        obj.o_hplus = 1;
        obj.o_dplus = 1;
        obj.o_flags |= ISKNOW;
        add_pack(obj, true);
        cur_weapon = obj;
        /*
         * Now a +1 bow
         */
        obj = new_item();
        init_weapon(obj, BOW);
        obj.o_hplus = 1;
        obj.o_flags |= ISKNOW;
        add_pack(obj, true);
        /*
         * Now some arrows
         */
        obj = new_item();
        init_weapon(obj, ARROW);
        obj.o_count = rnd(15) + 25;
        obj.o_flags |= ISKNOW;
        add_pack(obj, true);
    }

    #region Item components

    /*
     * Contains definitions and functions for dealing with things like
     * potions and scrolls
     */

    string[] rainbow = 
    {
        "amber",
        "aquamarine",
        "black",
        "blue",
        "brown",
        "clear",
        "crimson",
        "cyan",
        "ecru",
        "gold",
        "green",
        "grey",
        "magenta",
        "orange",
        "pink",
        "plaid",
        "purple",
        "red",
        "silver",
        "tan",
        "tangerine",
        "topaz",
        "turquoise",
        "vermilion",
        "violet",
        "white",
        "yellow",
    };

    int NCOLORS => rainbow.Length;
    int cNCOLORS => NCOLORS;

    static string[] sylls = 
    {
        "a", "ab", "ag", "aks", "ala", "an", "app", "arg", "arze", "ash",
        "bek", "bie", "bit", "bjor", "blu", "bot", "bu", "byt", "comp",
        "con", "cos", "cre", "dalf", "dan", "den", "do", "e", "eep", "el",
        "eng", "er", "ere", "erk", "esh", "evs", "fa", "fid", "fri", "fu",
        "gan", "gar", "glen", "gop", "gre", "ha", "hyd", "i", "ing", "ip",
        "ish", "it", "ite", "iv", "jo", "kho", "kli", "klis", "la", "lech",
        "mar", "me", "mi", "mic", "mik", "mon", "mung", "mur", "nej",
        "nelg", "nep", "ner", "nes", "nes", "nih", "nin", "o", "od", "ood",
        "org", "orn", "ox", "oxy", "pay", "ple", "plu", "po", "pot",
        "prok", "re", "rea", "rhov", "ri", "ro", "rog", "rok", "rol", "sa",
        "san", "sat", "sef", "seh", "shu", "ski", "sna", "sne", "snik",
        "sno", "so", "sol", "sri", "sta", "sun", "ta", "tab", "tem",
        "ther", "ti", "tox", "trol", "tue", "turs", "u", "ulk", "um", "un",
        "uni", "ur", "val", "viv", "vly", "vom", "wah", "wed", "werg",
        "wex", "whon", "wun", "xo", "y", "yot", "yu", "zant", "zeb", "zim",
        "zok", "zon", "zum",
    };

    STONE[] stones = 
    {
        new("agate",             25),
        new("alexandrite",       40),
        new("amethyst",          50),
        new("carnelian",         40),
        new("diamond",          300),
        new("emerald",          300),
        new("germanium",        225),
        new("granite",            5),
        new("garnet",            50),
        new("jade",             150),
        new("kryptonite",       300),
        new("lapis lazuli",      50),
        new("moonstone",         50),
        new("obsidian",          15),
        new("onyx",              60),
        new("opal",             200),
        new("pearl",            220),
        new("peridot",           63),
        new("ruby",             350),
        new("sapphire",         285),
        new("stibotantalite",   200),
        new("tiger eye",         50),
        new("topaz",             60),
        new("turquoise",         70),
        new("taaffeite",        300),
        new("zircon",            80),
    };

    int NSTONES => stones.Length;
    int cNSTONES => NSTONES;

    string[] wood =
    {
        "avocado wood",
        "balsa",
        "bamboo",
        "banyan",
        "birch",
        "cedar",
        "cherry",
        "cinnibar",
        "cypress",
        "dogwood",
        "driftwood",
        "ebony",
        "elm",
        "eucalyptus",
        "fall",
        "hemlock",
        "holly",
        "ironwood",
        "kukui wood",
        "mahogany",
        "manzanita",
        "maple",
        "oaken",
        "persimmon wood",
        "pecan",
        "pine",
        "poplar",
        "redwood",
        "rosewood",
        "spruce",
        "teak",
        "walnut",
        "zebrawood",
    };

    int NWOOD => wood.Length;
    int cNWOOD => NWOOD;

    string[] metal =
    {
        "aluminum",
        "beryllium",
        "bone",
        "brass",
        "bronze",
        "copper",
        "electrum",
        "gold",
        "iron",
        "lead",
        "magnesium",
        "mercury",
        "nickel",
        "pewter",
        "platinum",
        "steel",
        "silver",
        "silicon",
        "tin",
        "titanium",
        "tungsten",
        "zinc",
    };

    int NMETAL => metal.Length;
    int cNMETAL => NMETAL;

    #endregion Item components

    /// <summary>
    /// Initialize the potion color scheme for this time
    /// </summary>
    void init_colors()
    {
        int j;
        bool[] used = new bool[NCOLORS];

        for (int i = 0; i < MAXPOTIONS; i++)
        {
            do
            {
                j = rnd(NCOLORS);
            } while (used[j]);

            used[j] = true;
            p_colors[i] = rainbow[j];
        }
    }

    /// <summary>
    /// Generate the names of the various scrolls
    /// </summary>
    void init_names()
    {
        StringBuilder builder = new();

        for (int i = 0; i < MAXSCROLLS; i++)
        {
            builder.Clear();

            for (int nwords = rnd(3) + 2; nwords != 0; nwords--)
            {
                for (int nsyl = rnd(3) + 1; nsyl != 0; nsyl--)
                {
                    builder.Append(sylls[rnd(sylls.Length)]);
                }

                if (nwords > 1)
                    builder.Append(' ');
            }

            s_names[i] = builder.ToString();
        }
    }

    /// <summary>
    /// Initialize the ring stone setting scheme for this time
    /// </summary>
    void init_stones()
    {
        int j;
        bool[] used = new bool[NSTONES];

        for (int i = 0; i < MAXRINGS; i++)
        {
            do
            {
                j = rnd(NSTONES);
            } while (used[j]);

            used[j] = true;

            r_stones[i] = stones[j].st_name;
            ring_info[i].oi_worth += stones[j].st_value;
        }
    }

    /// <summary>
    /// Initialize the construction materials for wands and staffs
    /// </summary>
    void init_materials()
    {
        int j;
        string str;
        bool[] used = new bool[NWOOD];
        bool[] metused = new bool[NMETAL];

        for (int i = 0; i < MAXSTICKS; i++)
        {
            for (; ; )
            {
                if (rnd(2) == 0)
                {
                    j = rnd(NMETAL);
                    if (!metused[j])
                    {
                        ws_type[i] = "wand";
                        str = metal[j];
                        metused[j] = true;
                        break;
                    }
                }
                else
                {
                    j = rnd(NWOOD);
                    if (!used[j])
                    {
                        ws_type[i] = "staff";
                        str = wood[j];
                        used[j] = true;
                        break;
                    }
                }
            }

            ws_made[i] = str;
        }
    }

    /// <summary>
    /// Sum up the probabilities for items appearing
    /// </summary>
    void sumprobs(obj_info[] infos, string name)
    {
        for (int i = 1; i < infos.Length; i++)
        {
            infos[i].oi_prob += infos[i - 1].oi_prob;
        }

        badcheck(name, infos);
    }

    /// <summary>
    /// Initialize the probabilities for the various items
    /// </summary>
    void init_probs()
    {
        sumprobs(things, "things");
        sumprobs(pot_info, "potions");
        sumprobs(scr_info, "scrolls");
        sumprobs(ring_info, "rings");
        sumprobs(ws_info, "sticks");
        sumprobs(weap_info, "weapons");
        sumprobs(arm_info, "armor");
    }

    /// <summary>
    /// Check to see if a series of probabilities sums to 100
    /// </summary>
    [Conditional("MASTER")]
    void badcheck(string name, obj_info[] infos)
    {
        if (infos.Last().oi_prob == 100)
            return;

        Console.WriteLine($"\nBad percentages for {name} (length = {infos.Length}):");
        foreach (obj_info info in infos)
        {
            Console.WriteLine($"{info.oi_prob,3}% {info.oi_name}");
        }

        Console.Write("[hit RETURN to continue]");

        while (Console.ReadKey().Key != ConsoleKey.Enter)
            continue;
    }

    /// <summary>
    /// If he is hallucinating, pick a random color name and return it,
    /// otherwise return the given color.
    /// </summary>
    string pick_color(string color)
    {
        return (on(player, ISHALU) ? rainbow[rnd(NCOLORS)] : color);
    }
}
