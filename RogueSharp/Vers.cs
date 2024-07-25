/*
 * Version number.  Whenever a new version number is desired, use sccs
 * to get vers.c.  encstr is declared here to force it to be loaded
 * before the version number, and therefore not to be written in saved
 * games.
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
    private string release = "5.4.4";
    private string version = "rogue (rogueforge) 09/05/07";

    // original C-style string: "\300k||`\251Y.'\305\321\201+\277~r\"]\240_\223=1\341)\222\212\241t;\t$\270\314/<#\201\254"
    private static readonly byte[] encstr =
    [
        0xc0,
        (byte) 'k',
        (byte) '|',
        (byte) '|',
        (byte) '`',
        0xa9,
        (byte) 'Y',
        (byte) '.',
        (byte) '\'',
        0xc5,
        0xd1,
        0x81,
        (byte) '+',
        0xbf,
        (byte) '~',
        (byte) 'r',
        (byte) '"',
        (byte) ']',
        0xa0,
        (byte) '_',
        0x93,
        (byte) '=',
        (byte) '1',
        0xe1,
        (byte) ')',
        0x92,
        0x8a,
        0xa1,
        (byte) 't',
        (byte) ';',
        (byte) '\t',
        (byte) '$',
        0xb8,
        0xcc,
        (byte) '/',
        (byte) '<',
        (byte) '#',
        0x81,
        0xac
    ];

    // original C-style string: "\355kl{+\204\255\313idJ\361\214=4:\311\271\341wK<\312\321\213,,7\271/Rk%\b\312\f\246"
    private static readonly byte[] statlist =
    [
        0xed,
        (byte) 'k',
        (byte) 'l',
        (byte) '{',
        (byte) '+',
        0x84,
        0xad,
        0xcb,
        (byte) 'i',
        (byte) 'd',
        (byte) 'J',
        0xf1,
        0x8c,
        (byte) '=',
        (byte) '4',
        (byte) ':',
        0xc9,
        0xb9,
        0xe1,
        (byte) 'w',
        (byte) 'K',
        (byte) '<',
        0xca,
        0xd1,
        0x8b,
        (byte) ',',
        (byte) ',',
        (byte) '7',
        0xb9,
        (byte) '/',
        (byte) 'R',
        (byte) 'k',
        (byte) '%',
        (byte) '\b',
        0xca,
        (byte) '\f',
        0xa6
    ];
}
