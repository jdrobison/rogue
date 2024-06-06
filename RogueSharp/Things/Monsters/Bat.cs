using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Bat : Monster
{
    public override char Appearance => 'B';

    public override string m_name => "bat";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsFlying;

    public override Stats m_stats { get; } = CreateStats(experience: 1, level: 1, armor: 3, damage: "1x2");
}
