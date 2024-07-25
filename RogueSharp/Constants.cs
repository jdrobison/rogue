using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RogueSharp.Things;

namespace RogueSharp;

internal partial class Program
{
    //#define NOOP(x) (x += 0)
    private char CCHAR(char ch) => ch;

    /*
     * Maximum number of different things
     */
    const int MAXROOMS    = 9;
    const int MAXTHINGS   = 9;
    const int MAXOBJ      = 9;
    const int MAXPACK     = 23;
    const int MAXTRAPS    = 10;
    const int AMULETLEVEL = 26;
    const int NUMTHINGS   = 7;      /* number of types of things */
    const int MAXPASS     = 13;     /* upper limit on number of passages */
    const int MAXLINES    = 32;     /* maximum number of screen lines used */
    const int MAXCOLS     = 80;     /* maximum number of screen columns used */
    const int NUMLINES    = 24;
    const int NUMCOLS     = 80;
    const int STATLINE    = NUMLINES - 1;
    const int BORE_LEVEL  = 50;

    /*
     * return values for get functions
     */
    const int NORM  = 0;    /* normal exit */
    const int QUIT  = 1;    /* quit option setting */
    const int MINUS = 2;    /* back up one option */

    /*
     * inventory types
     */
    const int INV_OVER  = 0;
    const int INV_SLOW  = 1;
    const int INV_CLEAR = 2;

    /*
     * All the fun defines
     */
    //const int when        break;case
    //const int otherwise    break;default
    //const int until(expr)    while(!(expr))
    THING? next(THING ptr) => ptr.l_next;
    THING? prev(THING ptr) => ptr.l_prev;

    char winat(int y, int x)
    {
        if (moat(y, x) is THING thing)
            return thing.t_disguise;
            
        return chat(y, x);
    }
    
    bool ce(coord a, coord b) => (a == b);
    
    ref coord hero => ref player.t_pos;

    ref stats pstats => ref player.t_stats;

    ref THING? pack => ref player.t_pack;

    ref room proom => ref player.t_room;

    ref int max_hp => ref pstats.s_maxhp;

    int max(int a, int b) => Math.Max(a, b);

    bool on(THING thing, int flag) => (thing.t_flags & flag) != 0;

    int GOLDCALC => rnd(50 + (10 * level)) + 2;

    bool ISRING(int h, int r) => (cur_ring[h]?.o_which == r);

    bool ISWEARING(int r) => (ISRING(LEFT, r) || ISRING(RIGHT, r));

    bool ISMULT(int type) => (type is POTION or SCROLL or FOOD);

    PLACE INDEX(int y, int x) => places[x << (5 + y)];

    PLACE place_at(int y, int x) => INDEX(y, x);

    char chat(int y, int x) => place_at(y, x).p_ch;
    void set_chat(int y, int x, char value) => place_at(y, x).p_ch = value;

    byte flat(int y, int x)  => place_at(y,x).p_flags;
    void set_flat(int y, int x, byte value) => place_at(y,x).p_flags = value;
    
    THING? moat(int y, int x) => place_at(y,x).p_monst;
    void set_moat(int y, int x, THING? value) => place_at(y,x).p_monst = value;

    //const int unc(cp)        (cp).y, (cp).x

    [Conditional("MASTER")]
    void debug(string fmt, params object[] args)
    {
        if (wizard)
            msg(fmt, args);
    }

    /*
     * things that appear on the screens
     */
    const char PASSAGE  = '#';
    const char DOOR     = '+';
    const char FLOOR    = '.';
    const char PLAYER   = '@';
    const char TRAP     = '^';
    const char STAIRS   = '%';
    const char GOLD     = '*';
    const char POTION   = '!';
    const char SCROLL   = '?';
    const char MAGIC    = '$';
    const char FOOD     = ':';
    const char WEAPON   = ')';
    const char ARMOR    = ']';
    const char AMULET   = ',';
    const char RING     = '=';
    const char STICK    = '/';
    const int  CALLABLE = -1;
    const char R_OR_S   = unchecked((char) -2);

/*
 * Various constants
 */
    int BEARTIME   => spread(3);
    int SLEEPTIME  => spread(5);
    int HOLDTIME   => spread(2);
    int WANDERTIME => spread(70);
    int BEFORE     => spread(1);
    int AFTER      => spread(2);

    const int HEALTIME    = 30;
    const int HUHDURATION = 20;
    const int SEEDURATION = 850;
    const int HUNGERTIME  = 1300;
    const int MORETIME    = 150;
    const int STOMACHSIZE = 2000;
    const int STARVETIME  = 850;
    const char ESCAPE     = (char) 27;
    const int LEFT        = 0;
    const int RIGHT       = 1;
    const int BOLT_LENGTH = 6;
    const int LAMPDIST    = 3;

#if MASTER
#if PASSWD
    const string PASSWD = "mTBellIQOsLNA";
#endif
#endif

    /*
     * Save against things
     */
    const int VS_POISON       = 0x00;
    const int VS_PARALYZATION = 0x00;
    const int VS_DEATH        = 0x00;
    const int VS_BREATH       = 0x02;
    const int VS_MAGIC        = 0x03;

    /*
     * Various flag bits
     */
    /* flags for rooms */
    const int ISDARK = 0x01;            /* room is dark */
    const int ISGONE = 0x02;            /* room is gone (a corridor) */
    const int ISMAZE = 0x04;            /* room is gone (a corridor) */

    /* flags for objects */
    const int ISCURSED = 0x0001;        /* object is cursed */
    const int ISKNOW   = 0x0002;        /* player knows details about the object */
    const int ISMISL   = 0x0004;        /* object is a missile type */
    const int ISMANY   = 0x0010;        /* object comes in groups */
//  const int ISFOUND  = 0x0020;        /* ...is used for both objects and creatures */
    const int ISPROT   = 0x0040;        /* armor is permanently protected */

    /* flags for creatures */
    const int CANHUH   = 0x0000001;     /* creature can confuse */
    const int CANSEE   = 0x0000002;     /* creature can see invisible creatures */
    const int ISBLIND  = 0x0000004;     /* creature is blind */
    const int ISCANC   = 0x0000010;     /* creature has special qualities cancelled */
    const int ISLEVIT  = 0x0000010;     /* hero is levitating */
    const int ISFOUND  = 0x0000020;     /* creature has been seen (used for objects) */
    const int ISGREED  = 0x0000040;     /* creature runs to protect gold */
    const int ISHASTE  = 0x0000100;     /* creature has been hastened */
    const int ISTARGET = 0x0000200;     /* creature is the target of an 'f' command */
    const int ISHELD   = 0x0000400;     /* creature has been held */
    const int ISHUH    = 0x0001000;     /* creature is confused */
    const int ISINVIS  = 0x0002000;     /* creature is invisible */
    const int ISMEAN   = 0x0004000;     /* creature can wake when player enters room */
    const int ISHALU   = 0x0004000;     /* hero is on acid trip */
    const int ISREGEN  = 0x0010000;     /* creature can regenerate */
    const int ISRUN    = 0x0020000;     /* creature is running at the player */
    const int SEEMONST = 0x0040000;     /* hero can detect unseen monsters */
    const int ISFLY    = 0x0040000;     /* creature can fly */
    const int ISSLOW   = 0x0100000;     /* creature has been slowed */

    /*
     * Flags for level map
     */
    const byte F_PASS    = 0x80;         /* is a passageway */
    const byte F_SEEN    = 0x40;         /* have seen this spot before */
    const byte F_DROPPED = 0x20;         /* object was dropped here */
    const byte F_LOCKED  = 0x20;         /* door is locked */
    const byte F_REAL    = 0x10;         /* what you see is what you get */
    const byte F_PNUM    = 0x0f;         /* passage number mask */
    const byte F_TMASK   = 0x07;         /* trap number mask */

    /*
     * Trap types
     */
    const int T_DOOR  = 0;
    const int T_ARROW = 1;
    const int T_SLEEP = 2;
    const int T_BEAR  = 3;
    const int T_TELEP = 4;
    const int T_DART  = 5;
    const int T_RUST  = 6;
    const int T_MYST  = 7;
    const int NTRAPS  = 8;

    /*
     * Potion types
     */
    const int P_CONFUSE  = 0;
    const int P_LSD      = 1;
    const int P_POISON   = 2;
    const int P_STRENGTH = 3;
    const int P_SEEINVIS = 4;
    const int P_HEALING  = 5;
    const int P_MFIND    = 6;
    const int P_TFIND    = 7;
    const int P_RAISE    = 8;
    const int P_XHEAL    = 9;
    const int P_HASTE    = 10;
    const int P_RESTORE  = 11;
    const int P_BLIND    = 12;
    const int P_LEVIT    = 13;
    const int MAXPOTIONS = 14;

    /*
     * Scroll types
     */
    const int S_CONFUSE   = 0;
    const int S_MAP       = 1;
    const int S_HOLD      = 2;
    const int S_SLEEP     = 3;
    const int S_ARMOR     = 4;
    const int S_ID_POTION = 5;
    const int S_ID_SCROLL = 6;
    const int S_ID_WEAPON = 7;
    const int S_ID_ARMOR  = 8;
    const int S_ID_R_OR_S = 9;
    const int S_SCARE     = 10;
    const int S_FDET      = 11;
    const int S_TELEP     = 12;
    const int S_ENCH      = 13;
    const int S_CREATE    = 14;
    const int S_REMOVE    = 15;
    const int S_AGGR      = 16;
    const int S_PROTECT   = 17;
    const int MAXSCROLLS  = 18;

    /*
     * Weapon types
     */
    const int MACE       = 0;
    const int SWORD      = 1;
    const int BOW        = 2;
    const int ARROW      = 3;
    const int DAGGER     = 4;
    const int TWOSWORD   = 5;
    const int DART       = 6;
    const int SHIRAKEN   = 7;
    const int SPEAR      = 8;
    const int FLAME      = 9;    /* fake entry for dragon breath (ick) */
    const int MAXWEAPONS = 9;    /* this should equal FLAME */

    /*
     * Armor types
     */
    const int LEATHER         = 0;
    const int RING_MAIL       = 1;
    const int STUDDED_LEATHER = 2;
    const int SCALE_MAIL      = 3;
    const int CHAIN_MAIL      = 4;
    const int SPLINT_MAIL     = 5;
    const int BANDED_MAIL     = 6;
    const int PLATE_MAIL      = 7;
    const int MAXARMORS       = 8;

    /*
     * Ring types
     */
    const int R_PROTECT  = 0;
    const int R_ADDSTR   = 1;
    const int R_SUSTSTR  = 2;
    const int R_SEARCH   = 3;
    const int R_SEEINVIS = 4;
    const int R_NOP      = 5;
    const int R_AGGR     = 6;
    const int R_ADDHIT   = 7;
    const int R_ADDDAM   = 8;
    const int R_REGEN    = 9;
    const int R_DIGEST   = 10;
    const int R_TELEPORT = 11;
    const int R_STEALTH  = 12;
    const int R_SUSTARM  = 13;
    const int MAXRINGS   = 14;

    /*
     * Rod/Wand/Staff types
     */
    const int WS_LIGHT     = 0;
    const int WS_INVIS     = 1;
    const int WS_ELECT     = 2;
    const int WS_FIRE      = 3;
    const int WS_COLD      = 4;
    const int WS_POLYMORPH = 5;
    const int WS_MISSILE   = 6;
    const int WS_HASTE_M   = 7;
    const int WS_SLOW_M    = 8;
    const int WS_DRAIN     = 9;
    const int WS_NOP       = 10;
    const int WS_TELAWAY   = 11;
    const int WS_TELTO     = 12;
    const int WS_CANCEL    = 13;
    const int MAXSTICKS    = 14;
}
