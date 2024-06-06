﻿using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Snake : Monster
{
    public override char Appearance => 'S';

    public override string m_name => "snake";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    public override Stats m_stats { get; } = CreateStats(experience: 2, level: 1, armor: 5, damage: "1x3");
}