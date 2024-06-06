using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Dragon : Monster
{
    public override char Appearance => 'D';

    public override string m_name => "dragon";

    public override int m_carry => 100;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 5000, level: 10, armor: -1, damage: "1x8/1x8/3x10");
}
