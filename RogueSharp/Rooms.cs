using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>position matrix for maze positions</summary>
    class SPOT
    {
        public int nexits;
        public bool used;
        public readonly coord[] exits = new coord[4];
    }

    const int GOLDGRP = 1;

    /// <summary>
    /// Create rooms and corridors with a connectivity graph
    /// </summary>
    void do_rooms()
    {
        int i;
        THING tp;
        int left_out;
        coord top;
        coord bsze;                /* maximum room size */
        coord mp;

        bsze.x = NUMCOLS / 3;
        bsze.y = NUMLINES / 3;

        /*
         * Clear things for a new level
         */
        foreach (room room in rooms)
        {
            room.r_goldval = 0;
            room.r_nexits = 0;
            room.r_flags = 0;
        }

        /*
         * Put the gone rooms, if any, on the level
         */
        left_out = rnd(4);
        for (i = 0; i<left_out; i++)
            rooms[rnd_room()].r_flags |= ISGONE;
        /*
         * dig and populate all the rooms on the level
         */
        for (i = 0; i<MAXROOMS; i++)
        {
            room room = rooms[i];

            /*
             * Find upper left corner of box that this room goes in
             */
            top.x = (i % 3) * bsze.x + 1;
            top.y = (i / 3) * bsze.y;
            if ((room.r_flags & ISGONE) != 0)
            {
                /*
                 * Place a gone room.  Make certain that there is a blank line
                 * for passage drawing.
                 */
                do
                {
                    room.r_pos.x = top.x + rnd(bsze.x - 2) + 1;
                    room.r_pos.y = top.y + rnd(bsze.y - 2) + 1;
                    room.r_max.x = -NUMCOLS;
                    room.r_max.y = -NUMLINES;
                } while (!((room.r_pos.y > 0) && (room.r_pos.y < NUMLINES-1)));
                continue;
            }
            /*
             * set room type
             */
            if (rnd(10) < level - 1)
            {
                room.r_flags |= ISDARK;      /* dark room */
                if (rnd(15) == 0)
                    room.r_flags = ISMAZE;       /* maze room */
            }
            /*
             * Find a place and size for a random room
             */
            if ((room.r_flags & ISMAZE) != 0)
            {
                room.r_max.x = bsze.x - 1;
                room.r_max.y = bsze.y - 1;
                if ((room.r_pos.x = top.x) == 1)
                    room.r_pos.x = 0;
                if ((room.r_pos.y = top.y) == 0)
                {
                    room.r_pos.y++;
                    room.r_max.y--;
                }
            }
            else
            {
                do
                {
                    room.r_max.x = rnd(bsze.x - 4) + 4;
                    room.r_max.y = rnd(bsze.y - 4) + 4;
                    room.r_pos.x = top.x + rnd(bsze.x - room.r_max.x);
                    room.r_pos.y = top.y + rnd(bsze.y - room.r_max.y);
                } while (room.r_pos.y == 0);
            }

            draw_room(room);
            /*
             * Put the gold in
             */
            if (rnd(2) == 0 && (!amulet || level >= max_level))
            {
                THING gold;

                gold = new_item();
                gold.o_goldval = room.r_goldval = GOLDCALC;
                find_floor(room, out room.r_gold, 0, false);
                gold.o_pos = room.r_gold;
                set_chat(room.r_gold.y, room.r_gold.x, GOLD);
                gold.o_flags = ISMANY;
                gold.o_group = GOLDGRP;
                gold.o_type = GOLD;
                attach(ref lvl_obj, gold);
            }
            /*
             * Put the monster in
             */
            if (rnd(100) < (room.r_goldval > 0 ? 80 : 25))
            {
                tp = new_item();
                find_floor(room, out mp, 0, true);
                new_monster(tp, randmonster(false), mp);
                give_pack(tp);
            }
        }
    }

    /// <summary>
    /// Draw a box around a room and lay down the floor for normal rooms; for maze rooms, draw maze.
    /// </summary>
    void draw_room(room room)
    {
        int y, x;

        if ((room.r_flags & ISMAZE) != 0)
        {
            do_maze(room);
        }
        else
        {
            vert(room, room.r_pos.x);              /* Draw left side */
            vert(room, room.r_pos.x + room.r_max.x - 1);    /* Draw right side */
            horiz(room, room.r_pos.y);             /* Draw top */
            horiz(room, room.r_pos.y + room.r_max.y - 1);   /* Draw bottom */

            /*
             * Put the floor down
             */
            for (y = room.r_pos.y + 1; y < room.r_pos.y + room.r_max.y - 1; y++)
                for (x = room.r_pos.x + 1; x < room.r_pos.x + room.r_max.x - 1; x++)
                    set_chat(y, x, FLOOR);
        }
    }

    /// <summary>
    /// Draw a vertical line
    /// </summary>
    void vert(room room, int startx)
    {
        int y;

        for (y = room.r_pos.y + 1; y <= room.r_max.y + room.r_pos.y - 1; y++)
            set_chat(y, startx, '|');
    }

    /// <summary>
    /// Draw a horizontal line
    /// </summary>
    void horiz(room room, int starty)
    {
        int x;

        for (x = room.r_pos.x; x <= room.r_pos.x + room.r_max.x - 1; x++)
            set_chat(starty, x, '-');
    }

    private int  Maxy, Maxx, Starty, Startx;

    private SPOT[,] maze = new SPOT[(NUMLINES/3)+1, (NUMCOLS/3)+1];
    private coord _do_maze_pos;
    private coord _dig_pos;
    private readonly coord[] _dig_del = { new(2, 0), new(-2, 0), new(0, 2), new(0, -2) };

    /// <summary>
    /// Dig a maze
    /// </summary>
    void do_maze(room room)
    {
        foreach (SPOT spot in maze)
        {
            spot.used = false;
            spot.nexits = 0;
        }

        Maxy = room.r_max.y;
        Maxx = room.r_max.x;
        Starty = room.r_pos.y;
        Startx = room.r_pos.x;
        int starty = rnd(room.r_max.y) / 2 * 2;
        int startx = rnd(room.r_max.x) / 2 * 2;
        _do_maze_pos.y = starty + Starty;
        _do_maze_pos.x = startx + Startx;
        putpass(_do_maze_pos);
        dig(starty, startx);
    }

    /// <summary>
    /// Dig out from around where we are now, if possible
    /// </summary>
    void dig(int y, int x)
    {
        int cnt, newy, newx, nexty = 0, nextx = 0;

        for (; ; )
        {
            cnt = 0;
            foreach (coord cp in _dig_del)
            {
                newy = y + cp.y;
                newx = x + cp.x;
                if (newy < 0 || newy > Maxy || newx < 0 || newx > Maxx)
                    continue;
                if ((flat(newy + Starty, newx + Startx) & F_PASS) != 0)
                    continue;
                if (rnd(++cnt) == 0)
                {
                    nexty = newy;
                    nextx = newx;
                }
            }
            if (cnt == 0)
                return;
            accnt_maze(y, x, nexty, nextx);
            accnt_maze(nexty, nextx, y, x);
            if (nexty == y)
            {
                _dig_pos.y = y + Starty;
                if (nextx - x < 0)
                    _dig_pos.x = nextx + Startx + 1;
                else
                    _dig_pos.x = nextx + Startx - 1;
            }
            else
            {
                _dig_pos.x = x + Startx;
                if (nexty - y < 0)
                    _dig_pos.y = nexty + Starty + 1;
                else
                    _dig_pos.y = nexty + Starty - 1;
            }
            putpass(_dig_pos);
            _dig_pos.y = nexty + Starty;
            _dig_pos.x = nextx + Startx;
            putpass(_dig_pos);
            dig(nexty, nextx);
        }
    }

    /// <summary>
    /// Account for maze exits
    /// </summary>
    void accnt_maze(int y, int x, int ny, int nx)
    {
        SPOT spot = maze[y, x];

        int i = 0;
        for (i = 0; i < spot.nexits; i++)
        {
            if (spot.exits[i].y == ny && spot.exits[i].x == nx)
                return;
        }

        spot.exits[i].y = ny;
        spot.exits[i].x = nx;
    }

    /// <summary>
    /// Pick a random spot in a room
    /// </summary>
    coord rnd_pos(room room)
    {
        return new coord
        {
            x = room.r_pos.x + rnd(room.r_max.x - 2) + 1,
            y = room.r_pos.y + rnd(room.r_max.y - 2) + 1,
        };
    }

    /// <summary>
    /// Find a valid floor spot in this room.  If rp is null, then pick a new room each time around the loop.
    /// </summary>
    bool find_floor(room? room, out coord coord, int limit, bool monst)
    {
        PLACE place;
        int cnt;
        char compchar = '\0';
        bool pickroom = (room == null);

        coord = new coord();

        if (!pickroom)
            compchar = (((room.r_flags & ISMAZE) != 0) ? PASSAGE : FLOOR);
        cnt = limit;
        for (; ; )
        {
            if (limit != 0 && cnt-- == 0)
                return false;
            if (pickroom)
            {
                room = rooms[rnd_room()];
                compchar = (((room.r_flags & ISMAZE) != 0) ? PASSAGE : FLOOR);
            }
            coord = rnd_pos(room);
            place = INDEX(coord.y, coord.x);
            if (monst)
            {
                if (place.p_monst == null && step_ok(place.p_ch))
                    return true;
            }
            else if (place.p_ch == compchar)
                return true;
        }
    }

    /// <summary>
    /// Code that is executed whenver you appear in a room
    /// </summary>
    void enter_room(coord cp)
    {
        THING? tp;
        int y, x;
        char ch;

        if (roomin(cp) is not room room)
            throw new ArgumentException($"Coordinate {cp} is not in a room", nameof(cp));

        proom = room;
        door_open(room);

        if (((room.r_flags & ISDARK) == 0) && !on(player, ISBLIND))
        {
            for (y = room.r_pos.y; y < room.r_max.y + room.r_pos.y; y++)
            {
                move(y, room.r_pos.x);
                for (x = room.r_pos.x; x < room.r_max.x + room.r_pos.x; x++)
                {
                    tp = moat(y, x);
                    ch = chat(y, x);
                    if (tp == null)
                        if (CCHAR(inch()) != ch)
                            addch(ch);
                        else
                            move(y, x + 1);
                    else
                    {
                        tp.t_oldch = ch;
                        if (!see_monst(tp))
                            if (on(player, SEEMONST))
                            {
                                standout();
                                addch(tp.t_disguise);
                                standend();
                            }
                            else
                                addch(ch);
                        else
                            addch(tp.t_disguise);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Code for when we exit a room
    /// </summary>
    void leave_room(coord cp)
    {
        PLACE place;
        room room;
        int y, x;
        char floor;
        char ch;
        room = proom;

        if ((room.r_flags & ISMAZE) != 0)
            return;

        if ((room.r_flags & ISGONE) != 0)
            floor = PASSAGE;
        else if (((room.r_flags & ISDARK) == 0) || on(player, ISBLIND))
            floor = FLOOR;
        else
            floor = ' ';

        proom = passages[flat(cp.y, cp.x) & F_PNUM];
        for (y = room.r_pos.y; y < room.r_max.y + room.r_pos.y; y++)
        {
            for (x = room.r_pos.x; x < room.r_max.x + room.r_pos.x; x++)
            {
                move(y, x);
                switch (ch = CCHAR(inch()))
                {
                    case FLOOR:
                        if (floor == ' ' && ch != ' ')
                            addch(' ');
                        break;
                    default:
                        /*
                         * to check for monster, we have to strip out
                         * standout bit
                         */
                        if (Char.IsAscii(ch) && Char.IsUpper(ch))
                        {
                            if (on(player, SEEMONST))
                            {
                                standout();
                                addch(ch);
                                standend();
                                break;
                            }
                            place = INDEX(y, x);
                            addch(place.p_ch == DOOR ? DOOR : floor);
                        }
                        break;
                }
            }
        }

        door_open(room);
    }
}
