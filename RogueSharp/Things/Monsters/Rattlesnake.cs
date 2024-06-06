using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Rattlesnake : Monster
{
    public override char Appearance => 'R';

    public override string m_name => "rattlesnake";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 9, level: 2, armor: 3, damage: "1x6");
}
