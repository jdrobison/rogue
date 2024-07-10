using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSharp;

internal partial class Program
{
    /// <summary>
    /// Takes an item out of whatever linked list it might be in
    /// </summary>
    void detach(ref THING? list, THING item)
    {
        if (list != null)
        { 
            if (list == item)
                list = next(item);
            if (prev(item) != null)
                item.l_prev!.l_next = next(item);
            if (next(item) != null)
                item.l_next!.l_prev = prev(item);
        }

        item.l_next = null;
        item.l_prev = null;
    }

    /// <summary>
    /// Add an item to the head of a list
    /// </summary>
    bool attach([NotNullWhen(true)] ref THING? list, THING item)
    {
        if (list != null)
        {
            item.l_next = list;
            list.l_prev = item;
            item.l_prev = null;
        }
        else
        {
            item.l_next = null;
            item.l_prev = null;
        }

        list = item;
        return true;
    }

    /// <summary>
    /// Throw the whole blamed thing away
    /// </summary>
    void _free_list(ref THING? ptr)
    {
        while (ptr != null)
        {
            THING item = ptr;
            ptr = next(item);
            discard(item);
        }
    }

    /// <summary>
    /// Free up an item
    /// </summary>
    void discard(THING item)
    {
        // nothing to do
    }

    /// <summary>
    /// Get a new item
    /// </summary>
    THING new_item() => new THING();
}
