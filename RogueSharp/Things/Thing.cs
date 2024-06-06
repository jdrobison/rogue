namespace RogueSharp.Things;

/// <summary>
/// An object (item, monster, or person) that can be placed on the map
/// </summary>
internal abstract class Thing
{
    public Coordinate Position { get; set; }
}
