using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

using RogueSharp;

namespace RogueSharp;
internal partial class Program
{
    private const int DAEMON = -1;

    List<delayed_action> d_list = new();

    /// <summary>
    /// Find a particular slot in the table
    /// </summary>
    delayed_action? find_slot(Action<int> func)
    {
        return d_list.FirstOrDefault(slot => slot.d_func == func);
    }

    /// <summary>
    /// Start a daemon, takes a function.
    /// </summary>
    void start_daemon(Action<int> func, int arg, int type)
    {
        fuse(func, arg, DAEMON, type);
    }

    /// <summary>
    /// Remove a daemon from the list
    /// </summary>
    void kill_daemon(Action<int> func)
    {
        for (int i = 0; i < d_list.Count; i++)
        {
            if (d_list[i].d_func == func)
            {
                d_list.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Run all the daemons that are active with the current flag,
    /// passing the argument to the function.
    /// </summary>
    void do_daemons(int flag)
    {
        /*
         * Loop through the devil list
         */
        foreach (delayed_action dev in d_list)
        {
            /*
             * Executing each one, giving it the proper arguments
             */
            if (dev.d_type == flag && dev.d_time == DAEMON)
                dev.d_func(dev.d_arg);
        }
    }

    /// <summary>
    /// Start a fuse to go off in a certain number of turns
    /// </summary>
    void fuse(Action<int> func, int arg, int time, int type)
    {
        delayed_action dev = new(func, arg, time, type);
        d_list.Add(dev);
    }

    /// <summary>
    /// Increase the time until a fuse goes off
    /// </summary>
    void lengthen(Action<int> func, int xtime)
    {
        if (find_slot(func) is delayed_action wire)
            wire.d_time += xtime;
    }

    /// <summary>
    /// Put out a fuse
    /// </summary>
    void extinguish(Action<int> func)
    {
        kill_daemon(func);
    }

    /*
     * do_fuses:
     *   Decrement counters and start needed fuses
     */
    void do_fuses(int flag)
    {
        /*
         * Step though the list
         */
        for (int i = 0; i < d_list.Count; i++)
        {
            delayed_action wire = d_list[i];

            /*
             * Decrementing counters and starting things we want.  We also need
             * to remove the fuse from the list once it has gone off.
             */
            if (flag == wire.d_type && wire.d_time > 0 && --wire.d_time == 0)
            {
                wire.d_func(wire.d_arg);
                d_list.RemoveAt(i);
            }
        }
    }
}
