using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Griffin : Monster
{
    public override char Appearance => 'G';

    public override string m_name => "griffin";

    public override int m_carry => 20;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean | MonsterFlags.IsFlying | MonsterFlags.IsRegen;

    public override Stats m_stats { get; } = CreateStats(experience: 2, level: 1, armor: 7, damage: "1x2");
}
