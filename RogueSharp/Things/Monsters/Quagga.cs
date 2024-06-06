using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Quagga : Monster
{
    public override char Appearance => 'Q';

    public override string m_name => "quagga";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 15, level: 3, armor: 3, damage: "1x5/1x5");
}
