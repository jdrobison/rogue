using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Jabberwock : Monster
{
    public override char Appearance => 'J';

    public override string m_name => "jabberwock";

    public override int m_carry => 70;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.None;

    public override Stats m_stats { get; } = CreateStats(experience: 3000, level: 15, armor: 6, damage: "2x12/2x4");
}
