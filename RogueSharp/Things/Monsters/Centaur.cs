using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Centaur : Monster
{
    public override char Appearance => 'C';

    public override string m_name => "centaur";

    public override int m_carry => 15;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.None;

    public override Stats m_stats { get; } = CreateStats(experience: 17, level: 4, armor: 4, damage: "1x2/1x5/1x5");
}
