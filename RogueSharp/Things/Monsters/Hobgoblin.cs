using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Hobgoblin : Monster
{
    public override char Appearance => 'H';

    public override string m_name => "hobgoblin";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 3, level: 1, armor: 5, damage: "1x8");
}
