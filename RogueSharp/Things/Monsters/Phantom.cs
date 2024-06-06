using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Phantom : Monster
{
    public override char Appearance => 'P';

    public override string m_name => "phantom";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsInvisible;

    public override Stats m_stats { get; } = CreateStats(experience: 120, level: 8, armor: 3, damage: "4x4");
}
