using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Zombie : Monster
{
    public override char Appearance => 'Z';

    public override string m_name => "zombie";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 6, level: 2, armor: 8, damage: "1x8");
}
