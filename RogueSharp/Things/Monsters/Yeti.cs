using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Yeti : Monster
{
    public override char Appearance => 'Y';

    public override string m_name => "yeti";

    public override int m_carry => 30;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.None;

    public override Stats m_stats { get; } = CreateStats(experience: 50, level: 4, armor: 6, damage: "1x6/1x6");
}
