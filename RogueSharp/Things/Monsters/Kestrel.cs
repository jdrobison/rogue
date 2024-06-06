using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Kestrel : Monster
{
    public override char Appearance => 'K';

    public override string m_name => "kestrel";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean | MonsterFlags.IsFlying;

    public override Stats m_stats { get; } = CreateStats(experience: 1, level: 1, armor: 7, damage: "1x4");
}
