/*
 * save and restore routines
 *
 * @(#)save.c	4.33 (Berkeley) 06/01/83
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using static RogueSharp.Helpers.CursesHelper;

namespace RogueSharp;

internal partial class Program
{
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;

    /// <summary>
    /// Implement the "save game" command
    /// </summary>
    void save_game()
    {
        int c;
        string buf = string.Empty;
        FileStream? stream = null;
        BinaryWriter? writer = null;

        try
        {
            /*
             * get file name
             */
            mpos = 0;
over:
            if (file_name[0] != '\0')
            {
                for (; ; )
                {
                    msg("save file (%s)? ", file_name);
                    c = readchar().KeyChar;
                    mpos = 0;
                    if (c == ESCAPE)
                    {
                        msg("");
                        return;
                    }
                    else if (c == 'n' || c == 'N' || c == 'y' || c == 'Y')
                        break;
                    else
                        msg("please answer Y or N");
                }

                if (c == 'y' || c == 'Y')
                {
                    addstr("Yes\n");
                    refresh();
                    buf = file_name;
                }
            }

            do
            {
                if (buf.Length == 0)
                {
                    mpos = 0;
                    msg("file name: ");
                    if (get_str(out buf, stdscr) == QUIT)
                    {
                        msg("");
                        return;
                    }
                }

                mpos = 0;

                /*
                 * test to see if the file exists
                 */
                if (File.Exists(buf))
                {
                    for (; ; )
                    {
                        msg("File exists.  Do you wish to overwrite it?");
                        mpos = 0;

                        if ((c = readchar().KeyChar) == ESCAPE)
                        {
                            msg("");
                            return;
                        }

                        if (c == 'y' || c == 'Y')
                            break;
                        else if (c == 'n' || c == 'N')
                            goto over;
                        else
                            msg("Please answer Y or N");
                    }

                    msg("file name: %s", buf);
                    File.Delete(file_name);
                }

                file_name = buf;

                try
                {
                    stream = new FileStream(file_name, FileMode.Create, FileAccess.Write, FileShare.None);
                    writer = new BinaryWriter(stream);
                }
                catch (Exception ex)
                {
                    mpos = 0;
                    msg(ex.Message);
                }
            } while (writer == null);

            save_file(writer);
        }
        finally
        {
            writer?.Dispose();
            stream?.Dispose();
        }
    }

#if false
    /// <summary>
    /// Automatically save a file.  This is used if a HUP signal is received
    /// </summary>
    void auto_save(int sig)
    {
        FILE *savef;
        NOOP(sig);

        md_ignoreallsignals();
        if (file_name[0] != '\0' && ((savef = fopen(file_name, "w")) != null ||
        (md_unlink_open_file(file_name, savef) >= 0 && (savef = fopen(file_name, "w")) != null)))
            save_file(savef);
        exit(0);
    }
#endif

    /// <summary>
    /// Write the saved game on the file
    /// </summary>
    void save_file(BinaryWriter writer)
    {
        //char buf[80];
        //mvcur(0, COLS - 1, LINES - 1, 0);
        //putchar('\n');
        //endwin();
        //resetltchars();
        //md_chmod(file_name, 0400);
        rs_write_string(writer, version);
        rs_write_int(writer, LINES);
        rs_write_int(writer, COLS);
        rs_save_file(writer);
        writer.Flush();
        //fclose(writer);
        //exit(0);
    }

    /// <summary>
    /// Restore a saved game from a file with elaborate checks for file integrity from cheaters
    /// </summary>
    bool restore(string file)
    {
        try
        {
            if (file == "-r")
                file = file_name;

            //md_tstphold();

            using (FileStream stream = new(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new(stream, DefaultEncoding))
            {
                bool symlink = is_symlink(file);

                rs_read_string(reader, out string? buf);
                if (buf != version)
                {
                    Console.Write("Sorry, saved game is out of date.\n");
                    return false;
                }

                rs_read_int(reader, out int lines);
                rs_read_int(reader, out int cols);

                if (lines > LINES)
                {
                    endwin();
                    Console.Write("Sorry, original game was played on a screen with {0} lines.\n", lines);
                    Console.Write("Current screen only has {0} lines. Unable to restore game\n", LINES);
                    return (false);
                }

                if (cols > COLS)
                {
                    endwin();
                    Console.Write("Sorry, original game was played on a screen with {0} columns.\n", cols);
                    Console.Write("Current screen only has {0} columns. Unable to restore game\n", COLS);
                    return (false);
                }

                initscr();                          /* Start up cursor package */
                keypad(stdscr, true);

                hw = newwin(LINES, COLS, 0, 0);
                //setup();

                rs_restore_file(reader);

                mpos = 0;
                clearok(stdscr, true);

                /*
                 * defeat multiple restarting from the same place
                 */
#if MASTER
                if (!wizard)
#endif
                    if (symlink)
                    {
                        endwin();
                        Console.Write("\nCannot restore from a linked file\n");
                        return false;
                    }

                if (pstats.s_hpt <= 0)
                {
                    endwin();
                    Console.Write("\n\"He's dead, Jim\"\n");
                    return false;
                }

                //md_tstpresume();

                //environ = envp;
                file_name = file;
                clearok(curscr, true);
                //srand(md_getpid());
                msg("file name: %s", file);

                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

        // --- local method ---
        static bool is_symlink(string path)
        {
            FileInfo file = new FileInfo(path);
            return file.LinkTarget != null;
        }
    }

    /// <summary>
    /// Perform an encrypted write
    /// </summary>
    void encwrite(BinaryWriter writer, byte[] bytes)
    {
        Salt salt = new();

        for (int i = 0; i < bytes.Length; i++)
        {
            writer.Write(encrypt(bytes[i], i, ref salt));
        }

#if false
        char *e1, *e2, fb;
        int temp;
        extern char statlist[];
        size_t o_size = size;
        e1 = encstr;
        e2 = statlist;
        fb = 0;

        while (size)
        {
            if (putc(*s++ ^ *e1 ^ *e2 ^ fb, outf) == EOF)
                break;

            temp = *e1++;
            fb = fb + ((char) (temp * *e2++));
            if (*e1 == '\0')
                e1 = encstr;
            if (*e2 == '\0')
                e2 = statlist;
            size--;
        }

        return (o_size - size);
#endif
    }

    /// <summary>
    /// Perform an encrypted read
    /// </summary>
    byte[] encread(BinaryReader reader, int size)
    {
        byte[] bytes = new byte[size];
        encread(reader, bytes);

        return bytes;
    }

    /// <summary>
    /// Perform an encrypted read
    /// </summary>
    void encread(BinaryReader reader, byte[] bytes)
    {
        Salt salt = new();

        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = reader.ReadByte();
            bytes[i] = decrypt(b, i, ref salt);
        }

#if false
        char *e1, *e2, fb;
        int temp;
        size_t read_size;
        extern char statlist[];

        fb = 0;

        if ((read_size = fread(s, 1, size, reader)) == 0 || read_size == -1)
            return (read_size);

        e1 = encstr;
        e2 = statlist;

        while (size--)
        {
            *s++ ^= *e1 ^ *e2 ^ fb;
            temp = *e1++;
            fb = fb + (char) (temp * *e2++);
            if (*e1 == '\0')
                e1 = encstr;
            if (*e2 == '\0')
                e2 = statlist;
        }

        return (read_size);
#endif
    }

    /// <summary>
    /// Encrypts a byte
    /// </summary>
    byte encrypt(byte b, int i, ref Salt salt)
    {
        byte e = encstr[i % encstr.Length];
        byte s = statlist[i % statlist.Length];
        byte fb = salt.GetSaltedValue(e, s);

        return (byte) (b ^ e ^ s ^ fb);
    }

    /// <summary>
    /// Encrypts a byte
    /// </summary>
    byte decrypt(byte b, int i, ref Salt salt) => encrypt(b, i, ref salt);

    struct Salt
    {
        private byte salt;

        public byte GetSaltedValue(byte b1, byte b2)
        {
            return salt = unchecked((byte)(salt + (byte)(b1 * b2)));
        }
    }

    /// <summary>
    /// Read in the score file
    /// </summary>
    void rd_score(SCORE[] top_ten)
    {
        if (scoreboard == null)
            return;

        scoreboard.Position = 0;

        using (BinaryReader reader = new(scoreboard, DefaultEncoding, leaveOpen: true))
        {
            for (int i = 0; i < top_ten.Length; i++)
            {
                rs_read_string(reader, out top_ten[i].sc_name);
                rs_read_uint  (reader, out top_ten[i].sc_uid);
                rs_read_int   (reader, out top_ten[i].sc_score);
                rs_read_uint  (reader, out top_ten[i].sc_flags);
                rs_read_ushort(reader, out top_ten[i].sc_monster);
                rs_read_int   (reader, out top_ten[i].sc_level);
                rs_read_uint  (reader, out top_ten[i].sc_time);
            }
        }

        scoreboard.Position = 0;
    }

    /// <summary>
    /// Read in the score file
    /// </summary>
    void wr_score(SCORE[] top_ten)
    {
        if (scoreboard == null)
            return;

        scoreboard.Position = 0;

        using (BinaryWriter writer = new(scoreboard, DefaultEncoding, leaveOpen: true))
        {
            for (int i = 0; i < top_ten.Length; i++)
            {
                rs_write_string(writer, top_ten[i].sc_name ?? "Unknown");
                rs_write_uint  (writer, top_ten[i].sc_uid);
                rs_write_int   (writer, top_ten[i].sc_score);
                rs_write_uint  (writer, top_ten[i].sc_flags);
                rs_write_ushort(writer, top_ten[i].sc_monster);
                rs_write_int   (writer, top_ten[i].sc_level);
                rs_write_uint  (writer, top_ten[i].sc_time);
            }
        }

        scoreboard.Position = 0;
    }
}
