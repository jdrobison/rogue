using System.Diagnostics;

namespace RogueSharp;

internal partial class Program
{
    /*
     * Help list
     */
    class h_list
    {
        public h_list(char ch, string desc, bool print)
        {
            h_ch = ch;
            h_desc = desc;
            h_print = print;
        }

        public char h_ch      { get; }
        public string h_desc  { get; }
        public bool h_print   { get; }
    }

    /*
     * Coordinate data type
     */
    public struct coord
    {
        public int x;
        public int y;

        public coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        internal coord Clone()
        {
            coord clone;
            clone.x = x;
            clone.y = y;

            return clone;
        }

        public override string ToString() => $"(y:{y},x:{x})";

        public static bool operator ==(coord a, coord b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(coord a, coord b) => !(a == b);
    }

    /*
     * Stuff about objects
     */
    class obj_info
    {
        public obj_info(string name, int prob, int worth = 0)
        {
            oi_name = name;
            oi_prob = prob;
            oi_worth = worth;
        }

        public string oi_name { get; set; }
        public int oi_prob    { get; set; }
        public int oi_worth   { get; set; }

        public string? oi_guess;
        public bool oi_know;
    };

    /*
     * Room structure
     */
    public class room
    {
        public coord r_pos;                             /* Upper left corner */
        public coord r_max;                             /* Size of room */
        public coord r_gold;                            /* Where the gold is */
        public int r_goldval;                           /* How much the gold is worth */
        public short r_flags;                           /* info about the room */
        public int r_nexits;                            /* Number of exits */
        public readonly coord[] r_exit = new coord[12]; /* Where the exits are */
    };

    /*
     * Structure describing a fighting being
     */
    public struct stats
    {
        public stats(uint str, int exp, int lvl, int arm, int hpt, string dmg, int maxhp = 0)
        {
            s_str = str;
            s_exp = exp;
            s_lvl = lvl;
            s_arm = arm;
            s_hpt = hpt;
            s_dmg = dmg;
            s_maxhp = maxhp;
        }

        public stats(stats other) 
            : this(other.s_str, other.s_exp, other.s_lvl, other.s_arm, other.s_hpt, other.s_dmg, other.s_maxhp)
        { }

        public stats()
        {
            s_dmg = "0x0";
        }

        public uint s_str;                  /* Strength */
        public int s_exp;                   /* Experience */
        public int s_lvl;                   /* level of mastery */
        public int s_arm;                   /* Armor class */
        public int s_hpt;                   /* Hit points */
        public string s_dmg;                /* String describing damage done */
        public int  s_maxhp;                /* Max hit points */

        internal stats Clone()
        {
            return new stats(this);
        }
    };

    /*
     * Structure for monsters and player
     */
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class THING 
    {
        public THING? l_next;               /* Next thing in chain */
        public THING? l_prev;               /* Previous thing in chain */
                                            
        // data for creatures/player                  
        public coord t_pos;                 /* Position */
        public bool t_turn;                 /* If slowed, is it a turn to move */
        public char t_type;                 /* What it is */
        public char t_disguise;             /* What mimic looks like */
        public char t_oldch;                /* Character that was where it was */
        public coord t_dest;                /* Where it is running to */
        public int t_flags;                 /* State word */
        public stats t_stats = new();       /* Physical description */
        public room t_room = new();         /* Current room for thing */
        public THING? t_pack;               /* What the thing is carrying */
        public int t_reserved;             
                                            
        // data for items                 
        public int o_type;                  /* What kind of object it is */
        public coord o_pos;                 /* Where it lives on the screen */
        public string? o_text;              /* What it says if you read it */
        public int  o_launch;               /* What you need to launch it */
        public char o_packch;               /* What character it is in the pack */
        public string? o_damage;            /* Damage if used like sword */
        public string? o_hurldmg;           /* Damage if thrown */
        public int o_count;                 /* count for plural objects */
        public int o_which;                 /* Which object of a type it is */
        public int o_hplus;                 /* Plusses to hit */
        public int o_dplus;                 /* Plusses to damage */
        public int o_arm;                   /* Armor protection */
        public int o_flags;                 /* information about objects */
        public int o_group;                 /* group number for this object */
        public string? o_label;             /* Label for object */

        public ref int o_charges => ref o_arm;

        public ref int o_goldval => ref o_arm;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                if (IsPlayer)
                    return "Our hero!";

                if (IsItem)
                    return Program.Instance.inv_name(this, drop: false);

                if (IsMonster)
                    return Program.Instance.GetMonsterName(t_type);

                return "Unknown thing";
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool IsPlayer => (this == Program.Instance.player);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool IsMonster => IsMonsterIndex(t_type);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool IsItem => (o_type != 0);

        public THING Clone()
        {
            THING clone = new THING();

            clone.l_next     = l_next;
            clone.l_prev     = l_prev;

            clone.t_pos      = t_pos.Clone();
            clone.t_turn     = t_turn;
            clone.t_type     = t_type;
            clone.t_disguise = t_disguise;
            clone.t_oldch    = t_oldch;
            clone.t_dest     = t_dest;
            clone.t_flags    = t_flags;
            clone.t_stats    = t_stats.Clone();
            clone.t_room     = t_room;
            clone.t_pack     = t_pack;
            clone.t_reserved = t_reserved;

            clone.o_type     = o_type;
            clone.o_pos      = o_pos.Clone();
            clone.o_text     = o_text;
            clone.o_launch   = o_launch;
            clone.o_packch   = o_packch;
            clone.o_damage   = o_damage;
            clone.o_hurldmg  = o_hurldmg;
            clone.o_count    = o_count;
            clone.o_which    = o_which;
            clone.o_hplus    = o_hplus;
            clone.o_dplus    = o_dplus;
            clone.o_arm      = o_arm;
            clone.o_flags    = o_flags;
            clone.o_group    = o_group;
            clone.o_label    = o_label;

            return clone;
        }
    }

    /*
     * describe a place on the level map
     */
    class PLACE
    {
        public char p_ch;
        public byte p_flags;
        public THING? p_monst;
    }

    /*
     * Array containing information on all the various types of monsters
     */
    public class monster
    {
        public monster (string name, int carry, int flags, stats stats)
        {
            m_name = name;
            m_carry = carry;
            m_flags = flags;
            m_stats = stats;
        }

        public monster(monster other)
        {
            m_name  = other.m_name;
            m_carry = other.m_carry;
            m_flags = other.m_flags;
            m_stats = other.m_stats;
        }

        public string m_name { get; }       /* What to call the monster */
        public int m_carry { get; }         /* Probability of carrying something */
        public int m_flags { get; }         /* things about the monster */
        public stats m_stats;               /* Initial stats */
    };

    const int MAXDAEMONS = 20;

    class delayed_action
    {
        public delayed_action(Action<int> func, int arg, int time, int type)
        {
            d_func = func;
            d_arg = arg;
            d_time = time;
            d_type = type;
        }

        public int d_type { get; }
        public Action<int> d_func { get; }
        public int d_arg { get; }
        public int d_time { get; set; }
    }

    class STONE
    {
        public STONE(string name, int value)
        {
            st_name = name;
            st_value = value;
        }

        public string st_name;
        public int    st_value;
    }
}
