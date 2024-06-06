using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Aquator : Monster
{
    public override char Appearance => 'A';

    public override string m_name => "aquator";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 20, level: 5, armor: 2, damage: "0x0/0x0");
}
