using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Nymph : Monster
{
    public override char Appearance => 'N';

    public override string m_name => "nymph";

    public override int m_carry => 100;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.None;

    public override Stats m_stats { get; } = CreateStats(experience: 37, level: 3, armor: 9, damage: "0x0");
}
