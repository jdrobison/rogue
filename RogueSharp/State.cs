/*
    state.c - Portable Rogue Save State Code

    Copyright (C) 1999, 2000, 2005 Nicholas J. Kisseberth
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions
    are met:
    1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
    2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.
    3. Neither the name(s) of the author(s) nor the names of other contributors
        may be used to endorse or promote products derived from this software
        without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE AUTHOR(S) AND CONTRIBUTORS ``AS IS'' AND
    ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
    ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR(S) OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
    OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
    HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
    LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
    OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
    SUCH DAMAGE.
*/
using RogueSharp.Helpers;

using static RogueSharp.Helpers.CursesHelper;

using str_t = System.UInt32;

namespace RogueSharp;

internal partial class Program
{
    /************************************************************************/
    /* Save State Code                                                      */
    /************************************************************************/
    const uint RSID_STATS        = 0xABCD0001;
    const uint RSID_THING        = 0xABCD0002;
    const uint RSID_THING_NULL   = 0xDEAD0002;
    const uint RSID_OBJECT       = 0xABCD0003;
    const uint RSID_MAGICITEMS   = 0xABCD0004;
    const uint RSID_KNOWS        = 0xABCD0005;
    const uint RSID_GUESSES      = 0xABCD0006;
    const uint RSID_OBJECTLIST   = 0xABCD0007;
    const uint RSID_BAGOBJECT    = 0xABCD0008;
    const uint RSID_MONSTERLIST  = 0xABCD0009;
    const uint RSID_MONSTERSTATS = 0xABCD000A;
    const uint RSID_MONSTERS     = 0xABCD000B;
    const uint RSID_TRAP         = 0xABCD000C;
    const uint RSID_WINDOW       = 0xABCD000D;
    const uint RSID_DAEMONS      = 0xABCD000E;
    const uint RSID_IWEAPS       = 0xABCD000F;
    const uint RSID_IARMOR       = 0xABCD0010;
    const uint RSID_SPELLS       = 0xABCD0011;
    const uint RSID_ILIST        = 0xABCD0012;
    const uint RSID_HLIST        = 0xABCD0013;
    const uint RSID_DEATHTYPE    = 0xABCD0014;
    const uint RSID_CTYPES       = 0xABCD0015;
    const uint RSID_COORDLIST    = 0xABCD0016;
    const uint RSID_ROOMS        = 0xABCD0017;

    bool READSTAT => (format_error || read_error);
    bool WRITESTAT => (write_error);

    bool read_error   = false;
    bool write_error  = false;
    bool format_error = false;

    byte[] EnsureLittleEndian(byte[] bytes)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return bytes;
    }

    bool rs_write(BinaryWriter writer, byte[] bytes)
    {
        if (write_error)
            return WRITESTAT;

        encwrite(writer, bytes);

        return WRITESTAT;
    }

    bool rs_read(BinaryReader reader, out byte[] bytes, int size)
    {
        bytes = Array.Empty<byte>();

        if (read_error || format_error)
            return READSTAT;

        bytes = encread(reader, size);

        return READSTAT;
    }

    bool rs_write_int(BinaryWriter writer, int value)
    {
        if (write_error)
            return WRITESTAT;

        byte[] bytes = BitConverter.GetBytes(value);
        return rs_write(writer, EnsureLittleEndian(bytes));
    }

    bool rs_read_int(BinaryReader reader, out int value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read(reader, out byte[] bytes, sizeof(int));
        value = BitConverter.ToInt32(EnsureLittleEndian(bytes));

        return READSTAT;
    }

    bool rs_write_byte(BinaryWriter writer, byte value)
    {
        if (write_error)
            return WRITESTAT;

        byte[] bytes = [ value ];

        return rs_write(writer, bytes);
    }

    bool rs_read_byte(BinaryReader reader, out byte value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read(reader, out byte[] bytes, sizeof(byte));
        value = bytes[0];

        return READSTAT;
    }

    bool rs_write_char(BinaryWriter writer, char value)
    {
        if (write_error)
            return WRITESTAT;

        byte[] bytes = BitConverter.GetBytes(value);
        return rs_write(writer, EnsureLittleEndian(bytes));
    }

    bool rs_read_char(BinaryReader reader, out char value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read(reader, out byte[] bytes, sizeof(char));
        value = BitConverter.ToChar(EnsureLittleEndian(bytes));

        return READSTAT;
    }

#if false
    int
    rs_write_chars(BinaryWriter writer, char* c, int count)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_int(writer, count);
        rs_write(writer, c, count);

        return WRITESTAT;
    }

    int
    rs_read_chars(BinaryReader reader, char* i, int count)
    {
        int value = 0;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, &value);

        if (value != count)
            format_error = true;

        rs_read(reader, i, count);

        return READSTAT;
    }
#endif

    bool rs_write_ints(BinaryWriter writer, int[] values)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_int(writer, values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            rs_write_int(writer, values[i]);
        }

        return WRITESTAT;
    }

    bool rs_read_ints(BinaryReader reader, int[] values)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int count);

        if (count != values.Length)
            format_error = true;

        for (int i = 0; i < count; i++)
        {
            rs_read_int(reader, out values[i]);
        }

        return READSTAT;
    }

    bool rs_write_boolean(BinaryWriter writer, bool value)
    {
        if (write_error)
            return WRITESTAT;

        byte[] bytes = BitConverter.GetBytes(value);
        return rs_write(writer, EnsureLittleEndian(bytes));
    }

    bool rs_read_boolean(BinaryReader reader, out bool value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read(reader, out byte[] bytes, sizeof(bool));
        value = BitConverter.ToBoolean(EnsureLittleEndian(bytes));

        return READSTAT;
    }

    bool rs_write_booleans(BinaryWriter writer, bool[] values)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_int(writer, values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            rs_write_boolean(writer, values[i]);
        }

        return WRITESTAT;
    }

    bool rs_read_booleans(BinaryReader reader, bool[] values)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int count);

        if (count != values.Length)
            format_error = true;

        for (int i = 0; i < count; i++)
        {
            rs_read_boolean(reader, out values[i]);
        }

        return READSTAT;
    }

    bool rs_write_short(BinaryWriter writer, short value)
    {
        if (write_error)
            return WRITESTAT;

        byte[] bytes = BitConverter.GetBytes(value);
        return rs_write(writer, EnsureLittleEndian(bytes));
    }

    bool rs_read_short(BinaryReader reader, out short value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read(reader, out byte[] bytes, sizeof(int));
        value = BitConverter.ToInt16(EnsureLittleEndian(bytes));

        return READSTAT;
    }

    bool rs_write_shorts(BinaryWriter writer, short[] values)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_int(writer, values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            rs_write_short(writer, values[i]);
        }

        return WRITESTAT;
    }

    bool rs_read_shorts(BinaryReader reader, short[] values)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int count);

        if (count != values.Length)
            format_error = true;

        for (int i = 0; i < count; i++)
        {
            rs_read_short(reader, out values[i]);
        }

        return READSTAT;
    }

    bool rs_write_ushort(BinaryWriter writer, ushort value)
    {
        if (write_error)
            return WRITESTAT;

        byte[] bytes = BitConverter.GetBytes(value);
        return rs_write(writer, EnsureLittleEndian(bytes));
    }

    bool rs_read_ushort(BinaryReader reader, out ushort value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read(reader, out byte[] bytes, sizeof(int));
        value = BitConverter.ToUInt16(EnsureLittleEndian(bytes));

        return READSTAT;
    }

    bool rs_write_uint(BinaryWriter writer, uint value)
    {
        if (write_error)
            return WRITESTAT;

        byte[] bytes = BitConverter.GetBytes(value);
        return rs_write(writer, EnsureLittleEndian(bytes));
    }

    bool rs_read_uint(BinaryReader reader, out uint value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read(reader, out byte[] bytes, sizeof(int));
        value = BitConverter.ToUInt32(EnsureLittleEndian(bytes));

        return READSTAT;
    }

    bool rs_write_marker(BinaryWriter writer, uint id)
    {
        return rs_write_uint(writer, id);
    }

    bool rs_read_marker(BinaryReader reader, uint expectedId)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_uint(reader, out uint id);
        if (expectedId != id)
            format_error = true;

        return READSTAT;
    }

    /******************************************************************************/

    const int NullStringLength = -1;

    bool rs_write_string(BinaryWriter writer, string value)
    {
        if (write_error)
            return WRITESTAT;

        byte[] bytes = DefaultEncoding.GetBytes(value);

        rs_write_int(writer, bytes.Length);
        return rs_write(writer, bytes);
    }

    bool rs_read_string(BinaryReader reader, out string value)
    {
        value = string.Empty;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int length);
        rs_read(reader, out byte[] bytes, length);
        value = DefaultEncoding.GetString(bytes);

        return READSTAT;
    }

    bool rs_write_nullable_string(BinaryWriter writer, string? value)
    {
        if (write_error)
            return WRITESTAT;

        if (value == null)
        {
            rs_write_int(writer, NullStringLength);
            return WRITESTAT;
        }

        byte[] bytes = DefaultEncoding.GetBytes(value);

        rs_write_int(writer, bytes.Length);
        return rs_write(writer, bytes);
    }

    bool rs_read_nullable_string(BinaryReader reader, out string? value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int length);
        if (length == NullStringLength)
            return READSTAT;

        rs_read(reader, out byte[] bytes, length);
        value = DefaultEncoding.GetString(bytes);

        return READSTAT;
    }

#if false
    int
    rs_read_new_string(BinaryReader reader, char** s)
    {
        int len=0;
        char *buf=0;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, &len);

        if (len == 0)
            buf = null;
        else
        {
            buf = malloc(len);

            if (buf == null)
                read_error = true;
        }

        rs_read_chars(reader, buf, len);

        *s = buf;

        return READSTAT;
    }
#endif

    bool rs_write_strings(BinaryWriter writer, string[] values)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_int(writer, values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            rs_write_string(writer, values[i]);
        }

        return WRITESTAT;
    }

    bool rs_read_strings(BinaryReader reader, string?[] values)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int count);

        if (count != values.Length)
            format_error = true;

        for (int i = 0; i < count; i++)
        {
            rs_read_string(reader, out values[i]);
        }

        return READSTAT;
    }

#if false
    int
    rs_read_new_strings(BinaryReader reader, char** s, int count)
    {
        int n     = 0;
        int value = 0;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, &value);

        if (value != count)
            format_error = true;

        for (n = 0; n < count; n++)
            if (rs_read_new_string(reader, &s[n]) != 0)
                break;

        return READSTAT;
    }
#endif

    bool rs_write_string_index(BinaryWriter writer, string[] master, string str)
    {
        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < master.Length; i++)
        {
            if (str == master[i])
                return rs_write_int(writer, i);
        }

        return rs_write_int(writer, -1);
    }

    bool rs_read_string_index(BinaryReader reader, string[] master, out string str)
    {
        str = string.Empty;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int i);

        if (i >= master.Length)
            format_error = true;
        else if (i >= 0)
            str = master[i];

        return READSTAT;
    }

    bool rs_write_str_t(BinaryWriter writer, str_t value)
    {
        if (write_error)
            return WRITESTAT;

        return rs_write_uint(writer, value);
    }

    bool rs_read_str_t(BinaryReader reader, out str_t value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        return rs_read_uint(reader, out value);
    }

    bool rs_write_coord(BinaryWriter writer, coord value)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_int(writer, value.x);
        rs_write_int(writer, value.y);

        return WRITESTAT;
    }

    bool rs_read_coord(BinaryReader reader, out coord value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        coord c;
        rs_read_int(reader, out c.x);
        rs_read_int(reader, out c.y);

        if (!READSTAT)
        {
            value.x = c.x;
            value.y = c.y;
        }

        return READSTAT;
    }

    bool rs_write_window(BinaryWriter writer, CursesWindow win)
    {
        if (write_error)
            return WRITESTAT;

        int width  = win.Width;
        int height = win.Height;

        rs_write_marker(writer, RSID_WINDOW);
        rs_write_int(writer, height);
        rs_write_int(writer, width);

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                rs_write_int(writer, mvwinch(win, row, col));
            }
        }

        return WRITESTAT;
    }

    bool rs_read_window(BinaryReader reader, CursesWindow win)
    {
        if (read_error || format_error)
            return READSTAT;

        int width  = win.Width;
        int height = win.Height;

        rs_read_marker(reader, RSID_WINDOW);
        rs_read_int(reader, out int maxlines);
        rs_read_int(reader, out int maxcols);

        for (int row = 0; row < maxlines; row++)
        {
            for (int col = 0; col < maxcols; col++)
            {
                if (rs_read_int(reader, out int value))
                    return READSTAT;

                if ((row < height) && (col < width))
                    mvwaddch(win, row, col, (char) value);
            }
        }

        return READSTAT;
    }

    /******************************************************************************/

    THING? get_list_item(THING? l, int i)
    {
        for (int count = 0; l != null; count++, l = l.l_next)
        {
            if (count == i)
                return l;
        }

        return null;
    }

    int find_list_ptr(THING? l, THING? ptr)
    {
        for (int i = 0; l != null; i++, l = l.l_next)
        {
            if (l == ptr)
                return i;
        }

        return -1;
    }

    int list_size(THING? l)
    {
        int count;

        for (count = 0; l != null; count++, l = l.l_next)
            ;

        return (count);
    }

    /******************************************************************************/

    bool rs_write_stats(BinaryWriter writer, stats value)
    {
        if (write_error)
            return (WRITESTAT);

        rs_write_marker(writer, RSID_STATS);
        rs_write_str_t(writer, value.s_str);
        rs_write_int(writer, value.s_exp);
        rs_write_int(writer, value.s_lvl);
        rs_write_int(writer, value.s_arm);
        rs_write_int(writer, value.s_hpt);
        rs_write_string(writer, value.s_dmg);
        rs_write_int(writer, value.s_maxhp);

        return (WRITESTAT);
    }

    bool rs_read_stats(BinaryReader reader, out stats value)
    {
        value = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read_marker(reader, RSID_STATS);
        rs_read_str_t(reader, out value.s_str);
        rs_read_int(reader, out value.s_exp);
        rs_read_int(reader, out value.s_lvl);
        rs_read_int(reader, out value.s_arm);
        rs_read_int(reader, out value.s_hpt);
        rs_read_string(reader, out value.s_dmg);
        rs_read_int(reader, out value.s_maxhp);

        return READSTAT;
    }

    bool rs_write_stone_index(BinaryWriter writer, STONE[] master, string value)
    {
        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < master.Length; i++)
        {
            if (value == master[i].st_name)
            {
                rs_write_int(writer, i);
                return WRITESTAT;
            }
        }

        rs_write_int(writer, -1);

        return WRITESTAT;
    }

    bool rs_read_stone_index(BinaryReader reader, STONE[] master, out string value)
    {
        value = string.Empty;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int i);

        if (i >= master.Length)
            format_error = true;
        else if (i >= 0)
            value = master[i].st_name;

        return READSTAT;
    }

    bool rs_write_scrolls(BinaryWriter writer)
    {
        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < MAXSCROLLS; i++)
        {
            rs_write_string(writer, s_names[i]);
        }

        return READSTAT;
    }

    bool rs_read_scrolls(BinaryReader reader)
    {
        if (read_error || format_error)
            return READSTAT;

        for (int i = 0; i < MAXSCROLLS; i++)
        {
            rs_read_string(reader, out s_names[i]);
        }

        return READSTAT;
    }

    bool rs_write_potions(BinaryWriter writer)
    {
        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < MAXPOTIONS; i++)
        {
            rs_write_string_index(writer, rainbow, p_colors[i]);
        }

        return WRITESTAT;
    }

    bool rs_read_potions(BinaryReader reader)
    {

        if (read_error || format_error)
            return READSTAT;

        for (int i = 0; i < MAXPOTIONS; i++)
        {
            rs_read_string_index(reader, rainbow, out p_colors[i]);
        }

        return READSTAT;
    }

    bool rs_write_rings(BinaryWriter writer)
    {
        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < MAXRINGS; i++)
        {
            rs_write_stone_index(writer, stones, r_stones[i]);
        }

        return WRITESTAT;
    }

    bool rs_read_rings(BinaryReader reader)
    {
        if (read_error || format_error)
            return READSTAT;

        for (int i = 0; i < MAXRINGS; i++)
        {
            rs_read_stone_index(reader, stones, out r_stones[i]);
        }

        return READSTAT;
    }

    bool rs_write_sticks(BinaryWriter writer)
    {
        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < MAXSTICKS; i++)
        {
            if (ws_type[i] == "staff")
            {
                rs_write_int(writer, 0);
                rs_write_string_index(writer, wood, ws_made[i]);
            }
            else
            {
                rs_write_int(writer, 1);
                rs_write_string_index(writer, metal, ws_made[i]);
            }
        }

        return WRITESTAT;
    }

    bool rs_read_sticks(BinaryReader reader)
    {
        if (read_error || format_error)
            return READSTAT;

        for (int i = 0; i < MAXSTICKS; i++)
        {
            rs_read_int(reader, out int list);

            if (list == 0)
            {
                rs_read_string_index(reader, wood, out ws_made[i]);
                ws_type[i] = "staff";
            }
            else
            {
                rs_read_string_index(reader, metal, out ws_made[i]);
                ws_type[i] = "wand";
            }
        }

        return READSTAT;
    }

    bool rs_write_daemons(BinaryWriter writer, List<delayed_action> d_list)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_marker(writer, RSID_DAEMONS);
        rs_write_int(writer, d_list.Count);

        foreach (delayed_action daemon in d_list)
        {
            int func;

            if (daemon.d_func == noop)
                func = 0;
            else if (daemon.d_func == rollwand)
                func = 1;
            else if (daemon.d_func == doctor)
                func = 2;
            else if (daemon.d_func == stomach)
                func = 3;
            else if (daemon.d_func == runners)
                func = 4;
            else if (daemon.d_func == swander)
                func = 5;
            else if (daemon.d_func == nohaste)
                func = 6;
            else if (daemon.d_func == unconfuse)
                func = 7;
            else if (daemon.d_func == unsee)
                func = 8;
            else if (daemon.d_func == sight)
                func = 9;
            else
                func = -1;

            rs_write_int(writer, daemon.d_type);
            rs_write_int(writer, func);
            rs_write_int(writer, daemon.d_arg);
            rs_write_int(writer, daemon.d_time);
        }

        return WRITESTAT;
    }

    bool rs_read_daemons(BinaryReader reader, out List<delayed_action> d_list)
    {
        d_list = new List<delayed_action>();

        if (read_error || format_error)
            return READSTAT;

        rs_read_marker(reader, RSID_DAEMONS);
        rs_read_int(reader, out int count);

        d_list.EnsureCapacity(count);

        for (int i = 0; i < count; i++)
        {
            rs_read_int(reader, out int type);
            rs_read_int(reader, out int funcNumber);
            rs_read_int(reader, out int arg);
            rs_read_int(reader, out int time);

            Action<int> func = funcNumber switch
            {
                0 => noop,
                1 => rollwand,
                2 => doctor,
                3 => stomach,
                4 => runners,
                5 => swander,
                6 => nohaste,
                7 => unconfuse,
                8 => unsee,
                9 => sight,
                _ => noop,
            };

            if (func == noop)
                type = arg = time = 0;

            d_list.Add(new delayed_action(func, arg, time, type));
        }

        return READSTAT;
    }

    bool rs_write_obj_info(BinaryWriter writer, obj_info[] items)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_marker(writer, RSID_MAGICITEMS);
        rs_write_int(writer, items.Length);

        for (int i = 0; i < items.Length; i++)
        {
            rs_write_string(writer, items[i].oi_name);
            rs_write_int(writer, items[i].oi_prob);
            rs_write_int(writer, items[i].oi_worth);
            rs_write_nullable_string(writer, items[i].oi_guess);
            rs_write_boolean(writer, items[i].oi_know);
        }

        return WRITESTAT;
    }

    bool rs_read_obj_info(BinaryReader reader, obj_info[] items)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_marker(reader, RSID_MAGICITEMS);

        rs_read_int(reader, out int value);

        if (value > items.Length)
            format_error = true;

        for (int i = 0; i < value; i++)
        {
            rs_read_string(reader, out string name);
            rs_read_int(reader, out int prob);
            rs_read_int(reader, out int worth);

            items[i] = new obj_info(name, prob, worth);

            rs_read_nullable_string(reader, out items[i].oi_guess);
            rs_read_boolean(reader, out items[i].oi_know);
        }

        return READSTAT;
    }

    bool rs_write_key(BinaryWriter writer, ConsoleKeyInfo key)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_char(writer, key.KeyChar);
        rs_write_int(writer, (int) key.Key);
        rs_write_boolean(writer, key.Modifiers.HasFlag(ConsoleModifiers.Shift));
        rs_write_boolean(writer, key.Modifiers.HasFlag(ConsoleModifiers.Alt));
        rs_write_boolean(writer, key.Modifiers.HasFlag(ConsoleModifiers.Control));

        return WRITESTAT;
    }

    bool rs_read_key(BinaryReader reader, out ConsoleKeyInfo key)
    {
        key = new ConsoleKeyInfo();

        if (read_error || format_error)
            return READSTAT;

        rs_read_char(reader, out char keyChar);
        rs_read_int(reader, out int consoleKey);
        rs_read_boolean(reader, out bool shift);
        rs_read_boolean(reader, out bool alt);
        rs_read_boolean(reader, out bool control);

        key = new ConsoleKeyInfo(keyChar, (ConsoleKey) consoleKey, shift, alt, control);

        return READSTAT;
    }

    bool rs_write_room(BinaryWriter writer, room room)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_coord(writer, room.r_pos);
        rs_write_coord(writer, room.r_max);
        rs_write_coord(writer, room.r_gold);
        rs_write_int(writer, room.r_goldval);
        rs_write_short(writer, room.r_flags);
        rs_write_int(writer, room.r_nexits);
        rs_write_coord(writer, room.r_exit[0]);
        rs_write_coord(writer, room.r_exit[1]);
        rs_write_coord(writer, room.r_exit[2]);
        rs_write_coord(writer, room.r_exit[3]);
        rs_write_coord(writer, room.r_exit[4]);
        rs_write_coord(writer, room.r_exit[5]);
        rs_write_coord(writer, room.r_exit[6]);
        rs_write_coord(writer, room.r_exit[7]);
        rs_write_coord(writer, room.r_exit[8]);
        rs_write_coord(writer, room.r_exit[9]);
        rs_write_coord(writer, room.r_exit[10]);
        rs_write_coord(writer, room.r_exit[11]);

        return WRITESTAT;
    }

    bool rs_read_room(BinaryReader reader, out room room)
    {
        room = new room();

        if (read_error || format_error)
            return READSTAT;

        rs_read_coord(reader, out room.r_pos);
        rs_read_coord(reader, out room.r_max);
        rs_read_coord(reader, out room.r_gold);
        rs_read_int(reader, out room.r_goldval);
        rs_read_short(reader, out room.r_flags);
        rs_read_int(reader, out room.r_nexits);
        rs_read_coord(reader, out room.r_exit[0]);
        rs_read_coord(reader, out room.r_exit[1]);
        rs_read_coord(reader, out room.r_exit[2]);
        rs_read_coord(reader, out room.r_exit[3]);
        rs_read_coord(reader, out room.r_exit[4]);
        rs_read_coord(reader, out room.r_exit[5]);
        rs_read_coord(reader, out room.r_exit[6]);
        rs_read_coord(reader, out room.r_exit[7]);
        rs_read_coord(reader, out room.r_exit[8]);
        rs_read_coord(reader, out room.r_exit[9]);
        rs_read_coord(reader, out room.r_exit[10]);
        rs_read_coord(reader, out room.r_exit[11]);

        return READSTAT;
    }

    bool rs_write_rooms(BinaryWriter writer, room[] rooms)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_int(writer, rooms.Length);

        for (int i = 0; i < rooms.Length; i++)
        {
            rs_write_room(writer, rooms[i]);
        }

        return WRITESTAT;
    }

    bool rs_read_rooms(BinaryReader reader, room[] rooms)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int count);

        if (count > rooms.Length)
            format_error = true;

        for (int i = 0; i < count; i++)
        {
            rs_read_room(reader, out rooms[i]);
        }

        return READSTAT;
    }

    bool rs_write_room_reference(BinaryWriter writer, room room)
    {
        int roomIndex = -1;

        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == room)
            {
                roomIndex = i;
                break;
            }
        }

        rs_write_int(writer, roomIndex);

        return WRITESTAT;
    }

    bool rs_read_room_reference(BinaryReader reader, out room room)
    {
        room = rooms[0];

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int i);
        room = rooms[i];

        return READSTAT;
    }

    bool rs_write_monsters(BinaryWriter writer, monster[] monsters)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_marker(writer, RSID_MONSTERS);
        rs_write_int(writer, monsters.Length);

        for (int i = 0; i < monsters.Length; i++)
        {
            rs_write_stats(writer, monsters[i].m_stats);
        }

        return WRITESTAT;
    }

    bool rs_read_monsters(BinaryReader reader, monster[] monsters)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_marker(reader, RSID_MONSTERS);

        rs_read_int(reader, out int count);

        if (count != monsters.Length)
            format_error = true;

        for (int i = 0; i < count; i++)
        {
            rs_read_stats(reader, out monsters[i].m_stats);
        }

        return READSTAT;
    }

    bool rs_write_object(BinaryWriter writer, THING o)
    {
        if (write_error)
            return WRITESTAT;

        // TODO: write data for creatures/player?

        rs_write_marker(writer, RSID_OBJECT);
        rs_write_int(writer, o.o_type);
        rs_write_coord(writer, o.o_pos);
        rs_write_int(writer, o.o_launch);
        rs_write_char(writer, o.o_packch);
        rs_write_nullable_string(writer, o.o_damage);
        rs_write_nullable_string(writer, o.o_hurldmg);
        rs_write_int(writer, o.o_count);
        rs_write_int(writer, o.o_which);
        rs_write_int(writer, o.o_hplus);
        rs_write_int(writer, o.o_dplus);
        rs_write_int(writer, o.o_arm);
        rs_write_int(writer, o.o_flags);
        rs_write_int(writer, o.o_group);
        rs_write_nullable_string(writer, o.o_label);

        return WRITESTAT;
    }

    bool rs_read_object(BinaryReader reader, out THING o)
    {
        o = new THING();

        if (read_error || format_error)
            return READSTAT;

        // TODO: read data for creatures/player?

        rs_read_marker(reader, RSID_OBJECT);
        rs_read_int(reader, out o.o_type);
        rs_read_coord(reader, out o.o_pos);
        rs_read_int(reader, out o.o_launch);
        rs_read_char(reader, out o.o_packch);
        rs_read_nullable_string(reader, out o.o_damage);
        rs_read_nullable_string(reader, out o.o_hurldmg);
        rs_read_int(reader, out o.o_count);
        rs_read_int(reader, out o.o_which);
        rs_read_int(reader, out o.o_hplus);
        rs_read_int(reader, out o.o_dplus);
        rs_read_int(reader, out o.o_arm);
        rs_read_int(reader, out o.o_flags);
        rs_read_int(reader, out o.o_group);
        rs_read_nullable_string(reader, out o.o_label);

        return READSTAT;
    }

    bool rs_write_object_list(BinaryWriter writer, THING? l)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_marker(writer, RSID_OBJECTLIST);
        rs_write_int(writer, list_size(l));

        for (; l != null; l = l.l_next)
        {
            rs_write_object(writer, l);
        }

        return WRITESTAT;
    }

    bool rs_read_object_list(BinaryReader reader, out THING? list)
    {
        list = default;

        THING? o = null, previous = null;

        if (read_error || format_error)
            return READSTAT;

        rs_read_marker(reader, RSID_OBJECTLIST);
        rs_read_int(reader, out int cnt);

        for (int i = 0; i < cnt; i++)
        {
            rs_read_object(reader, out o);
            o.l_prev = previous;
            list ??= o;

            if (previous != null)
                previous.l_next = o;

            previous = o;
        }

        return READSTAT;
    }

    bool rs_write_object_reference(BinaryWriter writer, THING? list, THING? item)
    {
        if (write_error)
            return WRITESTAT;

        int i = find_list_ptr(list, item);

        return rs_write_int(writer, i);
    }

    bool rs_read_object_reference(BinaryReader reader, THING? list, out THING? item)
    {
        item = default;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int i);

        item = get_list_item(list, i);

        return READSTAT;
    }

    int find_room_coord(room[] rooms, coord pos)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].r_gold == pos)
                return i;
        }

        return -1;
    }

    int find_thing_coord(THING? things, coord pos)
    {
        int i = 0;

        for (THING? thing = things; thing != null; thing = thing.l_next, i++)
        {
            if (pos == thing.t_pos)
                return i;
        }

        return -1;
    }

    int find_object_coord(THING? objects, coord pos)
    {
        int i = 0;

        for (THING? o = objects; o != null; o = o.l_next, i++)
        {
            if (pos == o.o_pos)
                return i;
        }

        return -1;
    }

    bool rs_write_thing(BinaryWriter writer, THING? t)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_marker(writer, RSID_THING);

        if (t == null)
        {
            rs_write_int(writer, 0);
            return WRITESTAT;
        }

        rs_write_int(writer, 1);
        rs_write_coord(writer, t.t_pos);
        rs_write_boolean(writer, t.t_turn);
        rs_write_char(writer, t.t_type);
        rs_write_char(writer, t.t_disguise);
        rs_write_char(writer, t.t_oldch);

        /* 
            t_dest can be:
            0,0: null
            0,1: location of hero
            1,i: location of a thing (monster)
            2,i: location of an object
            3,i: location of gold in a room

            We need to remember what we are chasing rather than 
            the current location of what we are chasing.
        */

        if (t.t_dest == hero)
        {
            rs_write_int(writer, 0);
            rs_write_int(writer, 1);
        }
        //else if (t.t_dest == null)
        //{
        //    rs_write_int(writer, 0);
        //    rs_write_int(writer, 0);
        //}
        else
        {
            int i = find_thing_coord(mlist, t.t_dest);

            if (i >= 0)
            {
                rs_write_int(writer, 1);
                rs_write_int(writer, i);
            }
            else
            {
                i = find_object_coord(lvl_obj, t.t_dest);

                if (i >= 0)
                {
                    rs_write_int(writer, 2);
                    rs_write_int(writer, i);
                }
                else
                {
                    i = find_room_coord(rooms, t.t_dest);

                    if (i >= 0)
                    {
                        rs_write_int(writer, 3);
                        rs_write_int(writer, i);
                    }
                    else
                    {
                        rs_write_int(writer, 0);
                        rs_write_int(writer, 1); /* chase the hero anyway */
                    }
                }
            }
        }

        rs_write_int(writer, t.t_flags);
        rs_write_stats(writer, t.t_stats);
        rs_write_room_reference(writer, t.t_room);
        rs_write_object_list(writer, t.t_pack);

        return WRITESTAT;
    }

    bool rs_read_thing(BinaryReader reader, out THING t)
    {
        t = new THING();

        if (read_error || format_error)
            return READSTAT;

        rs_read_marker(reader, RSID_THING);
        rs_read_int(reader, out int index);

        if (index == 0)
            return READSTAT;

        rs_read_coord(reader, out t.t_pos);
        rs_read_boolean(reader, out t.t_turn);
        rs_read_char(reader, out t.t_type);
        rs_read_char(reader, out t.t_disguise);
        rs_read_char(reader, out t.t_oldch);

        /* 
            t_dest can be (listid,index):
            0,0: null
            0,1: location of hero
            1,i: location of a thing (monster)
            2,i: location of an object
            3,i: location of gold in a room

            We need to remember what we are chasing rather than 
            the current location of what we are chasing.
        */

        rs_read_int(reader, out int listid);
        rs_read_int(reader, out index);
        t.t_reserved = -1;

        if (listid == 0) /* hero or null */
        {
            if (index == 1)
                t.t_dest = hero;
            else
                t.t_dest = default;
        }
        else if (listid == 1) /* monster/thing */
        {
            t.t_dest     = default;
            t.t_reserved = index;
        }
        else if (listid == 2) /* object */
        {
            THING? item = get_list_item(lvl_obj, index);

            if (item != null)
                t.t_dest = item.o_pos;
        }
        else if (listid == 3) /* gold */
        {
            t.t_dest = rooms[index].r_gold;
        }
        else
        {
            t.t_dest = default;
        }

        rs_read_int(reader, out t.t_flags);
        rs_read_stats(reader, out t.t_stats);
        rs_read_room_reference(reader, out t.t_room);
        rs_read_object_list(reader, out t.t_pack);

        return READSTAT;
    }

    void rs_fix_thing(THING t)
    {
        if (t.t_reserved < 0)
            return;

        THING? item = get_list_item(mlist, t.t_reserved);

        if (item != null)
            t.t_dest = item.t_pos;
    }

    bool rs_write_thing_list(BinaryWriter writer, THING? l)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_marker(writer, RSID_MONSTERLIST);

        int cnt = list_size(l);

        rs_write_int(writer, cnt);

        if (cnt < 1)
            return WRITESTAT;

        while (l != null)
        {
            rs_write_thing(writer, l);
            l = l.l_next;
        }

        return WRITESTAT;
    }

    bool rs_read_thing_list(BinaryReader reader, out THING? list)
    {
        list = null;

        THING? t = null, previous = null;

        if (read_error || format_error)
            return READSTAT;

        rs_read_marker(reader, RSID_MONSTERLIST);
        rs_read_int(reader, out int cnt);

        for (int i = 0; i < cnt; i++)
        {
            rs_read_thing(reader, out t);
            t.l_prev = previous;
            list ??= t;

            if (previous != null)
                previous.l_next = t;

            previous = t;
        }

        return READSTAT;
    }

    void rs_fix_thing_list(THING? list)
    {
        for (THING? item = list; item != null; item = item.l_next)
        {
            rs_fix_thing(item);
        }
    }

    bool rs_write_thing_reference(BinaryWriter writer, THING? list, THING? item)
    {
        if (write_error)
            return WRITESTAT;

        int i = find_list_ptr(list, item);

        return rs_write_int(writer, i);
    }

    bool rs_read_thing_reference(BinaryReader reader, THING? list, out THING? item)
    {
        item = null;

        if (read_error || format_error)
            return READSTAT;

        rs_read_int(reader, out int i);

        if (i >= 0)
            item = get_list_item(list, i);

        return READSTAT;
    }

    bool rs_write_thing_references(BinaryWriter writer, THING? list, THING?[] items)
    {
        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < items.Length; i++)
        {
            rs_write_thing_reference(writer, list, items[i]);
        }

        return WRITESTAT;
    }

    bool rs_read_thing_references(BinaryReader reader, THING? list, THING?[] items)
    {
        if (read_error || format_error)
            return READSTAT;

        for (int i = 0; i < items.Length; i++)
        {
            rs_read_thing_reference(reader, list, out items[i]);
        }

        return WRITESTAT;
    }

    bool rs_write_places(BinaryWriter writer, PLACE[] places)
    {
        if (write_error)
            return WRITESTAT;

        for (int i = 0; i < places.Length; i++)
        {
            rs_write_char(writer, places[i].p_ch);
            rs_write_byte(writer, places[i].p_flags);
            rs_write_thing_reference(writer, mlist, places[i].p_monst);
        }

        return WRITESTAT;
    }

    bool rs_read_places(BinaryReader reader, PLACE[] places)
    {
        int i = 0;

        if (read_error || format_error)
            return READSTAT;

        for (i = 0; i < places.Length; i++)
        {
            rs_read_char(reader, out places[i].p_ch);
            rs_read_byte(reader, out places[i].p_flags);
            rs_read_thing_reference(reader, mlist, out places[i].p_monst);
        }

        return READSTAT;
    }

    bool rs_save_file(BinaryWriter writer)
    {
        if (write_error)
            return WRITESTAT;

        rs_write_boolean(writer, after);                 /* 1  */    /* extern.c */
        rs_write_boolean(writer, again);                 /* 2  */
        rs_write_boolean(writer, noscore);               /* 3  */
        rs_write_boolean(writer, seenstairs);            /* 4  */
        rs_write_boolean(writer, amulet);                /* 5  */
        rs_write_boolean(writer, door_stop);             /* 6  */
        rs_write_boolean(writer, fight_flush);           /* 7  */
        rs_write_boolean(writer, firstmove);             /* 8  */
        rs_write_boolean(writer, got_ltc);               /* 9  */
        rs_write_boolean(writer, has_hit);               /* 10 */
        rs_write_boolean(writer, in_shell);              /* 11 */
        rs_write_boolean(writer, inv_describe);          /* 12 */
        rs_write_boolean(writer, jump);                  /* 13 */
        rs_write_boolean(writer, kamikaze);              /* 14 */
        rs_write_boolean(writer, lower_msg);             /* 15 */
        rs_write_boolean(writer, move_on);               /* 16 */
        rs_write_boolean(writer, msg_esc);               /* 17 */
        rs_write_boolean(writer, passgo);                /* 18 */
        rs_write_boolean(writer, playing);               /* 19 */
        rs_write_boolean(writer, q_comm);                /* 20 */
        rs_write_boolean(writer, running);               /* 21 */
        rs_write_boolean(writer, save_msg);              /* 22 */
        rs_write_boolean(writer, see_floor);             /* 23 */
        rs_write_boolean(writer, stat_msg);              /* 24 */
        rs_write_boolean(writer, terse);                 /* 25 */
        rs_write_boolean(writer, to_death);              /* 26 */
        rs_write_boolean(writer, tombstone);             /* 27 */
#if MASTER
        rs_write_boolean(writer, wizard);                /* 28 */
#else
        rs_write_boolean(writer, false);                 /* 28 */
#endif
        rs_write_booleans(writer, pack_used);            /* 29 */
        rs_write_key(writer, dirKey);
        rs_write_string(writer, file_name);
        rs_write_string(writer, huh);
        rs_write_potions(writer);
        //rs_write_chars(writer, prbuf, 2*MAXSTR);
        rs_write_rings(writer);
        rs_write_string(writer, release);
        rs_write_key(writer, runKey);
        rs_write_scrolls(writer);
        rs_write_char(writer, take);
        rs_write_string(writer, whoami);
        rs_write_sticks(writer);
        rs_write_int(writer, orig_dsusp);
        rs_write_string(writer, fruit);
        rs_write_string(writer, home);
        rs_write_strings(writer, inv_t_name);
        rs_write_key(writer, l_last_commKey);
        rs_write_key(writer, l_last_dirKey);
        rs_write_key(writer, last_commKey);
        rs_write_key(writer, last_dirKey);
        rs_write_strings(writer, tr_name);
        rs_write_int(writer, n_objs);
        rs_write_int(writer, ntraps);
        rs_write_int(writer, hungry_state);
        rs_write_int(writer, inpack);
        rs_write_int(writer, inv_type);
        rs_write_int(writer, level);
        rs_write_int(writer, max_level);
        rs_write_int(writer, mpos);
        rs_write_int(writer, no_food);
        rs_write_ints(writer, a_class);
        rs_write_int(writer, count);
        rs_write_int(writer, food_left);
        rs_write_int(writer, lastscore);
        rs_write_int(writer, no_command);
        rs_write_int(writer, no_move);
        rs_write_int(writer, purse);
        rs_write_int(writer, quiet);
        rs_write_int(writer, vf_hit);
        rs_write_int(writer, dnum);
        rs_write_int(writer, seed);
        rs_write_ints(writer, e_levels);
        rs_write_coord(writer, delta);
        rs_write_coord(writer, oldpos);
        rs_write_coord(writer, stairs);

        rs_write_thing(writer, player);
        rs_write_object_reference(writer, player.t_pack, cur_armor);
        rs_write_object_reference(writer, player.t_pack, cur_ring[0]);
        rs_write_object_reference(writer, player.t_pack, cur_ring[1]);
        rs_write_object_reference(writer, player.t_pack, cur_weapon);
        rs_write_object_reference(writer, player.t_pack, l_last_pick);
        rs_write_object_reference(writer, player.t_pack, last_pick);

        rs_write_object_list(writer, lvl_obj);
        rs_write_thing_list(writer, mlist);

        rs_write_places(writer, places);

        rs_write_stats(writer, max_stats);
        rs_write_rooms(writer, rooms);
        rs_write_room_reference(writer, oldroom);
        rs_write_rooms(writer, passages);

        rs_write_monsters(writer, monsters);
        rs_write_obj_info(writer, things);
        rs_write_obj_info(writer, arm_info);
        rs_write_obj_info(writer, pot_info);
        rs_write_obj_info(writer, ring_info);
        rs_write_obj_info(writer, scr_info);
        rs_write_obj_info(writer, weap_info);
        rs_write_obj_info(writer, ws_info);

        rs_write_daemons(writer, d_list);            /* 5.4-daemon.c */
//# if MASTER
//        rs_write_int(writer, total);                          /* 5.4-list.c   */
//#else
//        rs_write_int(writer, 0);
//#endif
        rs_write_int(writer, rollwand_between);              /* 5.4-daemons.c*/
        rs_write_coord(writer, _move_nh);                    /* 5.4-move.c    */
        rs_write_int(writer, group);                         /* 5.4-weapons.c */

        rs_write_window(writer, stdscr);

        return WRITESTAT;
    }

    bool rs_restore_file(BinaryReader reader)
    {
        if (read_error || format_error)
            return READSTAT;

        rs_read_boolean(reader, out after);               /* 1  */    /* extern.c */
        rs_read_boolean(reader, out again);               /* 2  */
        rs_read_boolean(reader, out noscore);             /* 3  */
        rs_read_boolean(reader, out seenstairs);          /* 4  */
        rs_read_boolean(reader, out amulet);              /* 5  */
        rs_read_boolean(reader, out door_stop);           /* 6  */
        rs_read_boolean(reader, out fight_flush);         /* 7  */
        rs_read_boolean(reader, out firstmove);           /* 8  */
        rs_read_boolean(reader, out got_ltc);             /* 9  */
        rs_read_boolean(reader, out has_hit);             /* 10 */
        rs_read_boolean(reader, out in_shell);            /* 11 */
        rs_read_boolean(reader, out inv_describe);        /* 12 */
        rs_read_boolean(reader, out jump);                /* 13 */
        rs_read_boolean(reader, out kamikaze);            /* 14 */
        rs_read_boolean(reader, out lower_msg);           /* 15 */
        rs_read_boolean(reader, out move_on);             /* 16 */
        rs_read_boolean(reader, out msg_esc);             /* 17 */
        rs_read_boolean(reader, out passgo);              /* 18 */
        rs_read_boolean(reader, out playing);             /* 19 */
        rs_read_boolean(reader, out q_comm);              /* 20 */
        rs_read_boolean(reader, out running);             /* 21 */
        rs_read_boolean(reader, out save_msg);            /* 22 */
        rs_read_boolean(reader, out see_floor);           /* 23 */
        rs_read_boolean(reader, out stat_msg);            /* 24 */
        rs_read_boolean(reader, out terse);               /* 25 */
        rs_read_boolean(reader, out to_death);            /* 26 */
        rs_read_boolean(reader, out tombstone);           /* 27 */
# if MASTER
        rs_read_boolean(reader, out wizard);              /* 28 */
#else
        rs_read_boolean(reader, out dummyint);            /* 28 */
#endif
        rs_read_booleans(reader, pack_used);              /* 29 */
        rs_read_key(reader, out dirKey);
        rs_read_string(reader, out file_name);
        rs_read_string(reader, out huh);
        rs_read_potions(reader);
        //rs_read_chars(reader, prbuf, 2*MAXSTR);
        rs_read_rings(reader);
        rs_read_string(reader, out release);
        rs_read_key(reader, out runKey);
        rs_read_scrolls(reader);
        rs_read_char(reader, out take);
        rs_read_string(reader, out whoami);
        rs_read_sticks(reader);
        rs_read_int(reader, out orig_dsusp);
        rs_read_string(reader, out fruit);
        rs_read_string(reader, out home);
        rs_read_strings(reader, inv_t_name);
        rs_read_key(reader, out l_last_commKey);
        rs_read_key(reader, out l_last_dirKey);
        rs_read_key(reader, out last_commKey);
        rs_read_key(reader, out last_dirKey);
        rs_read_strings(reader, tr_name);
        rs_read_int(reader, out n_objs);
        rs_read_int(reader, out ntraps);
        rs_read_int(reader, out hungry_state);
        rs_read_int(reader, out inpack);
        rs_read_int(reader, out inv_type);
        rs_read_int(reader, out level);
        rs_read_int(reader, out max_level);
        rs_read_int(reader, out mpos);
        rs_read_int(reader, out no_food);
        rs_read_ints(reader, a_class);
        rs_read_int(reader, out count);
        rs_read_int(reader, out food_left);
        rs_read_int(reader, out lastscore);
        rs_read_int(reader, out no_command);
        rs_read_int(reader, out no_move);
        rs_read_int(reader, out purse);
        rs_read_int(reader, out quiet);
        rs_read_int(reader, out vf_hit);
        rs_read_int(reader, out dnum);
        rs_read_int(reader, out seed);
        rs_read_ints(reader, e_levels);
        rs_read_coord(reader, out delta);
        rs_read_coord(reader, out oldpos);
        rs_read_coord(reader, out stairs);

        rs_read_thing(reader, out player);
        rs_read_object_reference(reader, player.t_pack, out cur_armor);
        rs_read_object_reference(reader, player.t_pack, out cur_ring[0]);
        rs_read_object_reference(reader, player.t_pack, out cur_ring[1]);
        rs_read_object_reference(reader, player.t_pack, out cur_weapon);
        rs_read_object_reference(reader, player.t_pack, out l_last_pick);
        rs_read_object_reference(reader, player.t_pack, out last_pick);

        rs_read_object_list(reader, out lvl_obj);
        rs_read_thing_list(reader, out mlist);
        rs_fix_thing(player);
        rs_fix_thing_list(mlist);

        rs_read_places(reader, places);

        rs_read_stats(reader, out max_stats);
        rs_read_rooms(reader, rooms);
        rs_read_room_reference(reader, out oldroom);
        rs_read_rooms(reader, passages);

        rs_read_monsters(reader, monsters);
        rs_read_obj_info(reader, things);
        rs_read_obj_info(reader, arm_info);
        rs_read_obj_info(reader, pot_info);
        rs_read_obj_info(reader, ring_info);
        rs_read_obj_info(reader, scr_info);
        rs_read_obj_info(reader, weap_info);
        rs_read_obj_info(reader, ws_info);

        rs_read_daemons(reader, out d_list);                   /* 5.4-daemon.c     */
        //rs_read_int(reader, &dummyint);  /* total */            /* 5.4-list.c    */
        rs_read_int(reader, out rollwand_between);                /* 5.4-daemons.c    */
        rs_read_coord(reader, out _move_nh);                      /* 5.4-move.c       */
        rs_read_int(reader, out group);                            /* 5.4-weapons.c    */

        rs_read_window(reader, stdscr);

        return READSTAT;
    }
}
