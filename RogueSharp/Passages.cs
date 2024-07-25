/*
 * Draw the connecting passages
 *
 * @(#)passages.c	4.22 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */
using System.Diagnostics;

using RogueSharp.Helpers;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    class rdes
    {
        public rdes(int[] connAsInt)
        {
            if (connAsInt.Length != MAXROOMS)
                throw new ArgumentException($"connAsInt must have length equal to MAXROOMS {MAXROOMS}", nameof(connAsInt));

            for (int i = 0; i < connAsInt.Length; i++)
            {
                conn[i] = Convert.ToBoolean(connAsInt[i]);
            }
        }

        public readonly bool[] conn   = new bool[MAXROOMS];     /* possible to connect to room i? */
        public readonly bool[] isconn = new bool[MAXROOMS];     /* connection been made to room i? */
        public bool            ingraph;                         /* this room in graph already? */
    }

    rdes[] _passage_descriptions =
    {
        new ([ 0, 1, 0, 1, 0, 0, 0, 0, 0 ]),
        new ([ 1, 0, 1, 0, 1, 0, 0, 0, 0 ]),
        new ([ 0, 1, 0, 0, 0, 1, 0, 0, 0 ]),
        new ([ 1, 0, 0, 0, 1, 0, 1, 0, 0 ]),
        new ([ 0, 1, 0, 1, 0, 1, 0, 1, 0 ]),
        new ([ 0, 0, 1, 0, 1, 0, 0, 0, 1 ]),
        new ([ 0, 0, 0, 1, 0, 0, 0, 1, 0 ]),
        new ([ 0, 0, 0, 0, 1, 0, 1, 0, 1 ]),
        new ([ 0, 0, 0, 0, 0, 1, 0, 1, 0 ]),
    };

    /// <summary>
    /// Draw all the passages on a level.
    /// </summary>
    void do_passages()
    {
        /*
         * reinitialize room graph description
         */
        foreach (var rdes in _passage_descriptions)
        {
            for (int j = 0; j<MAXROOMS; j++)
                rdes.isconn[j] = false;

            rdes.ingraph = false;
        }

        /*
         * starting with one room, connect it to a random adjacent room and
         * then pick a new room to start with.
         */
        int roomcount = 1;
        rdes r1 = _passage_descriptions[rnd(MAXROOMS)];
        r1.ingraph = true;
        do
        {
            rdes? r2 = null;

            /*
             * find a room to connect with
             */
            int j = 0;
            for (int i = 0; i<MAXROOMS; i++)
            {
                if (r1.conn[i] && !_passage_descriptions[i].ingraph && rnd(++j) == 0)
                    r2 = _passage_descriptions[i];
            }
            /*
             * if no adjacent rooms are outside the graph, pick a new room
             * to look from
             */
            if (j == 0)
            {
                do
                {
                    r1 = _passage_descriptions[rnd(MAXROOMS)];
                } while (!r1.ingraph);
            }
            /*
             * otherwise, connect new room to the graph, and draw a tunnel
             * to it
             */
            else if (r2 is not null)
            {
                int i;

                r2.ingraph = true;
                i = _passage_descriptions.IndexOf(r1);
                j = _passage_descriptions.IndexOf(r2);
                conn(i, j);
                r1.isconn[j] = true;
                r2.isconn[i] = true;
                roomcount++;
            }
            else
            {
                Debug.Fail("Why didn't we find a second room to connect with?");
            }
        } while (roomcount < MAXROOMS);

        /*
         * attempt to add passages to the graph a random number of times so
         * that there isn't always just one unique passage through it.
         */
        for (roomcount = rnd(5); roomcount > 0; roomcount--)
        {
            rdes? r2 = null;

            r1 = _passage_descriptions[rnd(MAXROOMS)];  /* a random room to look from */

            /*
             * find an adjacent room not already connected
             */
            int j = 0;
            for (int i = 0; i < MAXROOMS; i++)
            {
                if (r1.conn[i] && !r1.isconn[i] && rnd(++j) == 0)
                    r2 = _passage_descriptions[i];
            }

            /*
             * if there is one, connect it and look for the next added
             * passage
             */
            if ((j != 0) && r2 is not null)
            {
                int i;

                i = _passage_descriptions.IndexOf(r1);
                j = _passage_descriptions.IndexOf(r2);
                conn(i, j);
                r1.isconn[j] = true;
                r2.isconn[i] = true;
            }
            else
            {
                Debug.Fail("Why didn't we find a second room to connect with?");
            }
        }
        passnum();
    }

    private coord _conn_del, _conn_curr, _conn_turn_delta, _conn_spos, _conn_epos;

    /// <summary>
    /// Draw a corridor from a room in a certain direction.
    /// </summary>
    void conn(int r1, int r2)
    {
        room rpf;
        room rpt;
        int rmt;
        int distance = 0, turn_spot, turn_distance = 0;
        int rm;
        char direc;

        if (r1 < r2)
        {
            rm = r1;
            if (r1 + 1 == r2)
                direc = 'r';
            else
                direc = 'd';
        }
        else
        {
            rm = r2;
            if (r2 + 1 == r1)
                direc = 'r';
            else
                direc = 'd';
        }
        rpf = rooms[rm];
        /*
         * Set up the movement variables, in two cases:
         * first drawing one down.
         */
        if (direc == 'd')
        {
            rmt = rm + 3;               /* room # of dest */
            rpt = rooms[rmt];           /* room pointer of dest */
            _conn_del.x = 0;              /* direction of move */
            _conn_del.y = 1;
            _conn_spos.x = rpf.r_pos.x;          /* start of move */
            _conn_spos.y = rpf.r_pos.y;
            _conn_epos.x = rpt.r_pos.x;          /* end of move */
            _conn_epos.y = rpt.r_pos.y;
            if ((rpf.r_flags & ISGONE) == 0)       /* if not gone pick door pos */
            {
                do
                {
                    _conn_spos.x = rpf.r_pos.x + rnd(rpf.r_max.x - 2) + 1;
                    _conn_spos.y = rpf.r_pos.y + rpf.r_max.y - 1;
                } while (((rpf.r_flags & ISMAZE) != 0) && ((flat(_conn_spos.y, _conn_spos.x) & F_PASS) == 0));
            }
            if ((rpt.r_flags & ISGONE) == 0)
            {
                do
                {
                    _conn_epos.x = rpt.r_pos.x + rnd(rpt.r_max.x - 2) + 1;
                } while (((rpt.r_flags & ISMAZE) != 0) && ((flat(_conn_epos.y, _conn_epos.x) & F_PASS) == 0));
            }
            distance = Math.Abs(_conn_spos.y - _conn_epos.y) - 1;    /* distance to move */
            _conn_turn_delta.y = 0;           /* direction to turn */
            _conn_turn_delta.x = (_conn_spos.x < _conn_epos.x ? 1 : -1);
            turn_distance = Math.Abs(_conn_spos.x - _conn_epos.x);   /* how far to turn */
        }
        else if (direc == 'r')          /* setup for moving right */
        {
            rmt = rm + 1;
            rpt = rooms[rmt];
            _conn_del.x = 1;
            _conn_del.y = 0;
            _conn_spos.x = rpf.r_pos.x;
            _conn_spos.y = rpf.r_pos.y;
            _conn_epos.x = rpt.r_pos.x;
            _conn_epos.y = rpt.r_pos.y;
            if ((rpf.r_flags & ISGONE) == 0)
            {
                do
                {
                    _conn_spos.x = rpf.r_pos.x + rpf.r_max.x - 1;
                    _conn_spos.y = rpf.r_pos.y + rnd(rpf.r_max.y - 2) + 1;
                } while (((rpf.r_flags & ISMAZE) != 0) && ((flat(_conn_spos.y, _conn_spos.x) & F_PASS) == 0));
            }
            if ((rpt.r_flags & ISGONE) == 0)
            {
                do
                {
                    _conn_epos.y = rpt.r_pos.y + rnd(rpt.r_max.y - 2) + 1;
                } while (((rpt.r_flags & ISMAZE) != 0) && ((flat(_conn_epos.y, _conn_epos.x) & F_PASS) == 0));
            }
            distance = Math.Abs(_conn_spos.x - _conn_epos.x) - 1;
            _conn_turn_delta.y = (_conn_spos.y < _conn_epos.y ? 1 : -1);
            _conn_turn_delta.x = 0;
            turn_distance = Math.Abs(_conn_spos.y - _conn_epos.y);
        }
#if MASTER
        else
        {
            debug("error in connection tables");
            rpt = rpf;
        }
#endif

        turn_spot = rnd(distance - 1) + 1;      /* where turn starts */

        /*
         * Draw in the doors on either side of the passage or just put #'s
         * if the rooms are gone.
         */
        if ((rpf.r_flags & ISGONE) == 0)
            door(rpf, _conn_spos);
        else
            putpass(_conn_spos);
        if ((rpt.r_flags & ISGONE) == 0)
            door(rpt, _conn_epos);
        else
            putpass(_conn_epos);
        /*
         * Get ready to move...
         */
        _conn_curr.x = _conn_spos.x;
        _conn_curr.y = _conn_spos.y;
        while (distance > 0)
        {
            /*
             * Move to new position
             */
            _conn_curr.x += _conn_del.x;
            _conn_curr.y += _conn_del.y;
            /*
             * Check if we are at the turn place, if so do the turn
             */
            if (distance == turn_spot)
            {
                while (turn_distance-- != 0)
                {
                    putpass(_conn_curr);
                    _conn_curr.x += _conn_turn_delta.x;
                    _conn_curr.y += _conn_turn_delta.y;
                }
            }
            /*
             * Continue digging along
             */
            putpass(_conn_curr);
            distance--;
        }
        _conn_curr.x += _conn_del.x;
        _conn_curr.y += _conn_del.y;
        if (!ce(_conn_curr, _conn_epos))
            msg("warning, connectivity problem on this level");
    }

    /// <summary>
    /// add a passage character or secret passage here
    /// </summary>
    void putpass(coord cp)
    {
        PLACE place;

        place = INDEX(cp.y, cp.x);
        place.p_flags |= F_PASS;
        if (rnd(10) + 1 < level && rnd(40) == 0)
            place.p_flags &= unchecked((byte) ~F_REAL);
        else
            place.p_ch = PASSAGE;
    }

    /// <summary>
    /// Add a door or possibly a secret door.  Also enters the door in the exits array of the room.
    /// </summary>
    void door(room room, coord pos)
    {
        room.r_exit[room.r_nexits++] = pos;

        if ((room.r_flags & ISMAZE) != 0)
            return;

        PLACE place = INDEX(pos.y, pos.x);
        if (rnd(10) + 1 < level && rnd(5) == 0)
        {
            if (pos.y == room.r_pos.y || pos.y == room.r_pos.y + room.r_max.y - 1)
                place.p_ch = '-';
            else
                place.p_ch = '|';
            place.p_flags &= unchecked((byte) ~F_REAL);
        }
        else
            place.p_ch = DOOR;
    }

#if MASTER
    /// <summary>
    /// Add the passages to the current window (wizard command)
    /// </summary>
    void add_pass()
    {
        for (int y = 1; y < NUMLINES - 1; y++)
        {
            for (int x = 0; x < NUMCOLS; x++)
            {
                PLACE place = INDEX(y, x);

                if (( (place.p_flags & F_PASS) != 0) || place.p_ch == DOOR ||
                    (((place.p_flags & F_REAL) == 0) && (place.p_ch == '|' || place.p_ch == '-')))
                {
                    char ch = place.p_ch;
                    if ((place.p_flags & F_PASS) != 0)
                        ch = PASSAGE;
                    place.p_flags |= F_SEEN;
                    move(y, x);
                    if (place.p_monst != null)
                        place.p_monst.t_oldch = place.p_ch;
                    else if ((place.p_flags & F_REAL) != 0)
                        addch(ch);
                    else
                    {
                        standout();
                        addch(((place.p_flags & F_PASS) != 0) ? PASSAGE : DOOR);
                        standend();
                    }
                }
            }
        }
    }
#endif

    private int _passages_pnum;
    private bool _passages_newpnum;

    /// <summary>
    /// Assign a number to each passageway
    /// </summary>
    void passnum()
    {
        _passages_pnum = 0;
        _passages_newpnum = false;

        foreach (room room in passages)
        {
            room.r_nexits = 0;
        }

        foreach (room room in rooms)
        {
            for (int i = 0; i < room.r_nexits; i++)
            {
                _passages_newpnum = true;
                numpass(room.r_exit[i].y, room.r_exit[i].x);
            }
        }
    }

    /// <summary>
    /// Number a passageway square and its brethren
    /// </summary>
    void numpass(int y, int x)
    {
        byte flags;
        room room;
        char ch;

        if (x >= NUMCOLS || x < 0 || y >= NUMLINES || y <= 0)
            return;
        flags = flat(y, x);
        if ((flags & F_PNUM) != 0)
            return;
        if (_passages_newpnum)
        {
            _passages_pnum++;
            _passages_newpnum = false;
        }
        /*
         * check to see if it is a door or secret door, i.e., a new exit,
         * or a numerable type of place
         */
        if ((ch = chat(y, x)) == DOOR ||
            (((flags & F_REAL) == 0) && (ch == '|' || ch == '-')))
        {
            room = passages[_passages_pnum];
            room.r_exit[room.r_nexits].y = y;
            room.r_exit[room.r_nexits++].x = x;
        }
        else if ((flags & F_PASS) == 0)
            return;

        flags |= (byte) _passages_pnum;
        set_flat(y, x, flags);

        /*
         * recurse on the surrounding places
         */
        numpass(y + 1, x);
        numpass(y - 1, x);
        numpass(y, x + 1);
        numpass(y, x - 1);
    }
}
