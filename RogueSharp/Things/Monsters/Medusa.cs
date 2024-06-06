using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Medusa : Monster
{
    public override char Appearance => 'M';

    public override string m_name => "medusa";

    public override int m_carry => 40;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 200, level: 8, armor: 2, damage: "3x4/3x4/2x5");
}
