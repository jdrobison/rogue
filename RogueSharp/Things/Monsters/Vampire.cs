using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Vampire : Monster
{
    public override char Appearance => 'V';

    public override string m_name => "vampire";

    public override int m_carry => 20;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean | MonsterFlags.IsRegen;

    public override Stats m_stats { get; } = CreateStats(experience: 350, level: 8, armor: 2, damage: "1x10");
}
