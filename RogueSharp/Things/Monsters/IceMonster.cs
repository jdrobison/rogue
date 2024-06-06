using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class IceMonster : Monster
{
    public override char Appearance => 'I';

    public override string m_name => "ice monster";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.None;

    public override Stats m_stats { get; } = CreateStats(experience: 5, level: 1, armor: 9, damage: "0x0");
}
