using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Unicorn : Monster
{
    public override char Appearance => 'U';

    public override string m_name => "black unicorn";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 190, level: 7, armor: -2, damage: "1x9/1x9/2x9");
}
