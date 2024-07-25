/*
 * Score file structure
 *
 * @(#)score.h	4.6 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */
namespace RogueSharp;

internal partial class Program
{
    struct SCORE
    {
        public uint sc_uid;
        public int sc_score;
        public uint sc_flags;
        public ushort sc_monster;
        public string? sc_name;
        public int sc_level;
        public uint sc_time;
    };
}
