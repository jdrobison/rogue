using RogueSharp.Enumerations;

namespace RogueSharp.Things;

internal class Room
{
    /// <summary>Upper left corner</summary>
    Coordinate r_pos;

    /// <summary>Size of room</summary>
    Coordinate r_max;

    /// <summary>Where the gold is</summary>
    Coordinate r_gold;

    /// <summary>How much the gold is worth</summary>
    int r_goldval;

    /// <summary>info about the room</summary>
    RoomFlags r_flags;

    /// <summary>Number of exits</summary>
    int r_nexits;

    /// <summary>Where the exits are</summary>
    Coordinate[] r_exit = new Coordinate[12];
}
