using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Troll : Monster
{
    public override char Appearance => 'T';

    public override string m_name => "troll";

    public override int m_carry => 50;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean | MonsterFlags.IsRegen;

    public override Stats m_stats { get; } = CreateStats(experience: 120, level: 6, armor: 4, damage: "1x8/1x8/2x6");
}
