using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft;

using WINDOW = RogueSharp.Helpers.CursesWindow;

namespace RogueSharp;

internal partial class Program
{
    private bool after;                 /* true if we want after daemons */
    private bool again;                 /* Repeating the last command */
    private bool noscore;               /* Was a wizard sometime */
    private bool seenstairs;            /* Have seen the stairs (for lsd) */
    private bool amulet;                /* He found the amulet */
    private bool door_stop;             /* Stop running when we pass a door */
    private bool fight_flush;           /* true if toilet input */
    private bool firstmove;             /* First move after setting door_stop */
    private bool got_ltc;               /* We have gotten the local tty chars */
    private bool has_hit;               /* Has a "hit" message pending in msg */
    private bool in_shell;              /* true if executing a shell */
    private bool inv_describe = true;   /* Say which way items are being used */
    private bool jump;                  /* Show running as series of jumps */
    private bool kamikaze;              /* to_death really to DEATH */
    private bool lower_msg;             /* Messages should start w/lower case */
    private bool move_on;               /* Next move shouldn't pick up items */
    private bool msg_esc;               /* Check for ESC from msg's --More-- */
    private bool passgo;                /* Follow passages */
    private bool playing = true;        /* true until he quits */
    private bool q_comm;                /* Are we executing a 'Q' command? */
    private bool running;               /* true if player is running */
    private bool save_msg = true;       /* Remember last msg */
    private bool see_floor = true;      /* Show the lamp illuminated floor */
    private bool stat_msg;              /* Should status() print as a msg() */
    private bool terse;                 /* true if we should be short */
    private bool to_death;              /* Fighting is to the death! */
    private bool tombstone = true;      /* Print out tombstone at end */
#if MASTER
    private bool wizard;                /* true if allows wizard commands */
#endif
    private bool[] pack_used = new bool[26];         /* Is the character used in the pack? */

    private ConsoleKeyInfo dirKey;                          /* Direction from last get_dir() call */
    private string file_name;                               /* Save file name */
    private string huh;                                     /* The last message printed */
    private string[] p_colors = new string[MAXPOTIONS];     /* Colors of the potions */
//  private string prbuf;                                   /* buffer for sprintfs */
    private string[] r_stones = new string[MAXRINGS];       /* Stone settings of the rings */
    private ConsoleKeyInfo runKey;                          /* Direction player is running */
    private string[] s_names = new string[MAXSCROLLS];      /* Names of the scrolls */
    private char take;                                      /* Thing she is taking */
    private string whoami;                                  /* Name of player */
    private string[] ws_made = new string[MAXSTICKS];       /* What sticks are made of */
    private string[] ws_type = new string[MAXSTICKS];       /* Is it a wand or a staff */
    private int  orig_dsusp;                                /* Original dsusp char */
    private string fruit = "slime-mold";                    /* Favorite fruit */
    private string home;                                    /* User's home directory */
    private string[] inv_t_name = 
    {
        "Overwrite",
        "Slow",
        "Clear"
    };
    private ConsoleKeyInfo l_last_commKey;                  /* Last last_comm */
    private ConsoleKeyInfo l_last_dirKey;                   /* Last last_dirKey */
    private ConsoleKeyInfo last_commKey;                    /* Last command typed */
    private ConsoleKeyInfo last_dirKey;                     /* Last direction given */
    private string[] tr_name =                              /* Names of the traps */
    {
        "a trapdoor",
        "an arrow trap",
        "a sleeping gas trap",
        "a beartrap",
        "a teleport trap",
        "a poison dart trap",
        "a rust trap",
        "a mysterious trap"
    };

    private int n_objs;                         /* # items listed in inventory() call */
    private int ntraps;                         /* Number of traps on this level */
    private int hungry_state = 0;               /* How hungry is he */
    private int inpack = 0;                     /* Number of things in pack */
    private int inv_type = 0;                   /* Type of inventory to use */
    private int level = 1;                      /* What level she is on */
    private int max_hit;                        /* Max damage done to her in to_death */
    private int max_level;                      /* Deepest player has gone */
    private int mpos = 0;                       /* Where cursor is on top line */
    private int no_food = 0;                    /* Number of levels without food */
    private int[] a_class = new int[MAXARMORS]  /* Armor class for each armor type */
    {
        8,    /* LEATHER */
        7,    /* RING_MAIL */
        7,    /* STUDDED_LEATHER */
        6,    /* SCALE_MAIL */
        5,    /* CHAIN_MAIL */
        4,    /* SPLINT_MAIL */
        4,    /* BANDED_MAIL */
        3,    /* PLATE_MAIL */
    };

    private int count = 0;              /* Number of times to repeat command */
    private FileStream? scoreboard;     /* File descriptor for score file */
    private int food_left;              /* Amount of food in hero's stomach */
    private int lastscore = -1;         /* Score before this turn */
    private int no_command = 0;         /* Number of turns asleep */
    private int no_move = 0;            /* Number of turns held in place */
    private int purse = 0;              /* How much gold he has */
    private int quiet = 0;              /* Number of quiet turns */
    private int vf_hit = 0;             /* Number of time flytrap has hit */

    private int dnum;                   /* Dungeon number */
    private int seed;                   /* Random number seed */
    private int[] e_levels = 
    {
             10,
             20,
             40,
             80,
            160,
            320,
            640,
           1300,
           2600,
           5200,
          13000,
          26000,
          50000,
         100000,
         200000,
         400000,
         800000,
        2000000,
        4000000,
        8000000,
              0,
    };

    private coord delta;                        /* Change indicated to get_dir() */
    private coord oldpos;                       /* Position before last look() call */
    private coord stairs;                       /* Location of staircase */

    private PLACE[] places = new PLACE[MAXLINES*MAXCOLS];     /* level map */

    private THING? cur_armor;                   /* What he is wearing */
    private THING?[] cur_ring = new THING[2];   /* Which rings are being worn */
    private THING? cur_weapon;                  /* Which weapon he is weilding */
    private THING? l_last_pick;                 /* Last last_pick */
    private THING? last_pick;                   /* Last object picked in get_item() */
    private THING? lvl_obj;                     /* List of objects on this level */
    private THING? mlist;                       /* List of monsters on the level */
    public THING player = new();                /* His stats */
    /* restart of game */

    private WINDOW hw;                          /* used as a scratch window */

    private static readonly stats INIT_STATS = new(16, 0, 1, 10, 12, "1x4", 12);

    private stats max_stats = new(INIT_STATS);          /* The maximum for the player */

    private room oldrp;                                 /* Roomin(&oldpos) */
    private room[] rooms = new room[MAXROOMS];          /* One for each room -- A level */
    private room[] passages = new room[MAXPASS]         /* One for each passage */
    {
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
        new room() { r_flags = ISGONE | ISDARK },
    };

    private monster[] monsters =
    {
        /*  Name          Carry  Flags                          str,  exp, lvl, amr, hpt, dmg */
        new("aquator",        0, ISMEAN,               new stats(10,   20,   5,   2,   1, "0x0/0x0")),
        new("bat",            0, ISFLY,                new stats(10,    1,   1,   3,   1, "1x2")),
        new("centaur",       15, 0,                    new stats(10,   17,   4,   4,   1, "1x2/1x5/1x5")),
        new("dragon",       100, ISMEAN,               new stats(10, 5000,  10,  -1,   1, "1x8/1x8/3x10")),
        new("emu",            0, ISMEAN,               new stats(10,    2,   1,   7,   1, "1x2")),
        // NOTE: the flytrap damage is %%% so that xstr won't merge this string with others, since it is written on in the program
        new("venus flytrap",  0, ISMEAN,               new stats(10,   80,   8,   3,   1, "%%%x0")),
        new("griffin",       20, ISMEAN|ISFLY|ISREGEN, new stats(10, 2000,  13,   2,   1, "4x3/3x5")),
        new("hobgoblin",      0, ISMEAN,               new stats(10,    3,   1,   5,   1, "1x8")),
        new("ice monster",    0, 0,                    new stats(10,    5,   1,   9,   1, "0x0")),
        new("jabberwock",    70, 0,                    new stats(10, 3000,  15,   6,   1, "2x12/2x4")),
        new("kestrel",        0, ISMEAN|ISFLY,         new stats(10,    1,   1,   7,   1, "1x4")),
        new("leprechaun",     0, 0,                    new stats(10,   10,   3,   8,   1, "1x1")),
        new("medusa",        40, ISMEAN,               new stats(10,  200,   8,   2,   1, "3x4/3x4/2x5")),
        new("nymph",        100, 0,                    new stats(10,   37,   3,   9,   1, "0x0")),
        new("orc",           15, ISGREED,              new stats(10,    5,   1,   6,   1, "1x8")),
        new("phantom",        0, ISINVIS,              new stats(10,  120,   8,   3,   1, "4x4")),
        new("quagga",         0, ISMEAN,               new stats(10,   15,   3,   3,   1, "1x5/1x5")),
        new("rattlesnake",    0, ISMEAN,               new stats(10,    9,   2,   3,   1, "1x6")),
        new("snake",          0, ISMEAN,               new stats(10,    2,   1,   5,   1, "1x3")),
        new("troll",         50, ISREGEN|ISMEAN,       new stats(10,  120,   6,   4,   1, "1x8/1x8/2x6")),
        new("black unicorn",  0, ISMEAN,               new stats(10,  190,   7,  -2,   1, "1x9/1x9/2x9")),
        new("vampire",       20, ISREGEN|ISMEAN,       new stats(10,  350,   8,   1,   1, "1x10")),
        new("wraith",         0, 0,                    new stats(10,   55,   5,   4,   1, "1x6")),
        new("xeroc",         30, 0,                    new stats(10,  100,   7,   7,   1, "4x4")),
        new("yeti",          30, 0,                    new stats(10,   50,   4,   6,   1, "1x6/1x6")),
        new("zombie",         0, ISMEAN,               new stats(10,    6,   2,   8,   1, "1x8")),
    };

    public static bool IsMonsterIndex(char ch) => (ch >= 'A') && (ch <= 'Z');

    public static int GetMonsterIndex(char ch)
    {
        Requires.Range(IsMonsterIndex(ch), nameof(ch));
        return ch - 'A';
    }

    public monster GetMonster(char ch) => monsters[GetMonsterIndex(ch)];

    public string GetMonsterName(char ch) => GetMonster(ch).m_name;

    private obj_info[] things = new obj_info[NUMTHINGS]
    {
        new("potion", 26),
        new("scroll", 36),
        new("food",   16),
        new("weapon",  7),
        new("armor",   7),
        new("ring",    4),
        new("stick",   4),
    };

    obj_info[] arm_info = new obj_info[MAXARMORS]
    {
        new("leather armor",                20,  20),
        new("ring mail",                    15,  25),
        new("studded leather armor",        15,  20),
        new("scale mail",                   13,  30),
        new("chain mail",                   12,  75),
        new("splint mail",                  10,  80),
        new("banded mail",                  10,  90),
        new("plate mail",                    5, 150),
    };

    obj_info[] pot_info = new obj_info[MAXPOTIONS]
    {
        new("confusion",                     7,   5),
        new("hallucination",                 8,   5),
        new("poison",                        8,   5),
        new("gain strength",                13, 150),
        new("see invisible",                 3, 100),
        new("healing",                      13, 130),
        new("monster detection",             6, 130),
        new("magic detection",               6, 105),
        new("raise level",                   2, 250),
        new("extra healing",                 5, 200),
        new("haste self",                    5, 190),
        new("restore strength",             13, 130),
        new("blindness",                     5,   5),
        new("levitation",                    6,  75),
    };

    obj_info[] ring_info = new obj_info[MAXRINGS]
    {
        new("protection",                    9, 400),
        new("add strength",                  9, 400),
        new("sustain strength",              5, 280),
        new("searching",                    10, 420),
        new("see invisible",                10, 310),
        new("adornment",                     1,  10),
        new("aggravate monster",            10,  10),
        new("dexterity",                     8, 440),
        new("increase damage",               8, 400),
        new("regeneration",                  4, 460),
        new("slow digestion",                9, 240),
        new("teleportation",                 5,  30),
        new("stealth",                       7, 470),
        new("maintain armor",                5, 380),
    };

    obj_info[] scr_info = new obj_info[MAXSCROLLS]
    {
        new("monster confusion",             7, 140),
        new("magic mapping",                 4, 150),
        new("hold monster",                  2, 180),
        new("sleep",                         3,   5),
        new("enchant armor",                 7, 160),
        new("identify potion",              10,  80),
        new("identify scroll",              10,  80),
        new("identify weapon",               6,  80),
        new("identify armor",                7, 100),
        new("identify ring, wand or staff", 10, 115),
        new("scare monster",                 3, 200),
        new("food detection",                2,  60),
        new("teleportation",                 5, 165),
        new("enchant weapon",                8, 150),
        new("create monster",                4,  75),
        new("remove curse",                  7, 105),
        new("aggravate monsters",            3,  20),
        new("protect armor",                 2, 250),
    };

    obj_info[] weap_info = new obj_info[MAXWEAPONS + 1]
    {
        new("mace",                         11,   8),
        new("long sword",                   11,  15),
        new("short bow",                    12,  15),
        new("arrow",                        12,   1),
        new("dagger",                        8,   3),
        new("two handed sword",             10,  75),
        new("dart",                         12,   2),
        new("shuriken",                     12,   5),
        new("spear",                        12,   5),
        new("(fake dragon's breath)",        0,   0),   // DO NOT REMOVE: fake entry for dragon's breath
    };

    obj_info[] ws_info = new obj_info[MAXSTICKS]
    {
        new("light",                        12, 250),
        new("invisibility",                  6,   5),
        new("lightning",                     3, 330),
        new("fire",                          3, 330),
        new("cold",                          3, 330),
        new("polymorph",                    15, 310),
        new("magic missile",                10, 170),
        new("haste monster",                10,   5),
        new("slow monster",                 11, 350),
        new("drain life",                    9, 300),
        new("nothing",                       1,   5),
        new("teleport away",                 6, 340),
        new("teleport to",                   6,  50),
        new("cancellation",                  5, 280),
    };

    static char CTRL(char c) => (char)(c & 0x37);

    h_list[] helpstr =
    {
        new('?',       "    prints help",                            true),
        new('/',       "    identify object",                        true),
        new('h',       "    left",                                   true),
        new('j',       "    down",                                   true),
        new('k',       "    up",                                     true),
        new('l',       "    right",                                  true),
        new('y',       "    up & left",                              true),
        new('u',       "    up & right",                             true),
        new('b',       "    down & left",                            true),
        new('n',       "    down & right",                           true),
        new('H',       "    run left",                               false),
        new('J',       "    run down",                               false),
        new('K',       "    run up",                                 false),
        new('L',       "    run right",                              false),
        new('Y',       "    run up & left",                          false),
        new('U',       "    run up & right",                         false),
        new('B',       "    run down & left",                        false),
        new('N',       "    run down & right",                       false),
        new(CTRL('H'), "    run left until adjacent",                false),
        new(CTRL('J'), "    run down until adjacent",                false),
        new(CTRL('K'), "    run up until adjacent",                  false),
        new(CTRL('L'), "    run right until adjacent",               false),
        new(CTRL('Y'), "    run up & left until adjacent",           false),
        new(CTRL('U'), "    run up & right until adjacent",          false),
        new(CTRL('B'), "    run down & left until adjacent",         false),
        new(CTRL('N'), "    run down & right until adjacent",        false),
        new('\0',      "    <SHIFT><dir>: run that way",             true),
        new('\0',      "    <CTRL><dir>: run till adjacent",         true),
        new('f',       "<dir>    fight till death or near death",    true),
        new('t',       "<dir>    throw something",                   true),
        new('m',       "<dir>    move onto without picking up",      true),
        new('z',       "<dir>    zap a wand in a direction",         true),
        new('^',       "<dir>    identify trap type",                true),
        new('s',       "    search for trap/secret door",            true),
        new('>',       "    go down a staircase",                    true),
        new('<',       "    go up a staircase",                      true),
        new('.',       "    rest for a turn",                        true),
        new(',',       "    pick something up",                      true),
        new('i',       "    inventory",                              true),
        new('I',       "    inventory single item",                  true),
        new('q',       "    quaff potion",                           true),
        new('r',       "    read scroll",                            true),
        new('e',       "    eat food",                               true),
        new('w',       "    wield a weapon",                         true),
        new('W',       "    wear armor",                             true),
        new('T',       "    take armor off",                         true),
        new('P',       "    put on ring",                            true),
        new('R',       "    remove ring",                            true),
        new('d',       "    drop object",                            true),
        new('c',       "    call object",                            true),
        new('a',       "    repeat last command",                    true),
        new(')',       "    print current weapon",                   true),
        new(']',       "    print current armor",                    true),
        new('=',       "    print current rings",                    true),
        new('@',       "    print current stats",                    true),
        new('D',       "    recall what's been discovered",          true),
        new('o',       "    examine/set options",                    true),
        new(CTRL('R'), "    redraw screen",                          true),
        new(CTRL('P'), "    repeat last message",                    true),
        new(ESCAPE,    "    cancel command",                         true),
        new('S',       "    save game",                              true),
        new('Q',       "    quit",                                   true),
        new('!',       "    shell escape",                           true),
        new('F',       "<dir>    fight till either of you dies",     true),
        new('v',       "    print version number",                   true),
        new('\0',      "",                                           false),
    };
}
