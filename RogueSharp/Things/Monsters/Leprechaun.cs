using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Leprechaun : Monster
{
    public override char Appearance => 'L';

    public override string m_name => "leprechaun";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.None;

    public override Stats m_stats { get; } = CreateStats(experience: 10, level: 3, armor: 8, damage: "1x1");
}
