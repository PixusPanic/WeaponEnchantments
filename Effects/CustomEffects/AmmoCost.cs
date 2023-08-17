﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEnchantments.Common;
using WeaponEnchantments.Common.Utility;
using static WeaponEnchantments.WEPlayer;
using androLib.Common.Utility;

namespace WeaponEnchantments.Effects
{
	public class AmmoCost : StatEffect, INonVanillaStat
    {
        public AmmoCost(DifficultyStrength additive = null, DifficultyStrength multiplicative = null, DifficultyStrength flat = null, DifficultyStrength @base = null) : base(additive, multiplicative, flat, @base) {

		}
		public AmmoCost(EStatModifier eStatModifier) : base(eStatModifier) { }
		public override EnchantmentEffect Clone() {
			return new AmmoCost(EStatModifier.Clone());
		}

		public override int DisplayNameNum => EffectStrength >= 0f ? 1 : 2;//1 is Chance not to consume.  2 is Increased Ammo cost
		public override EnchantmentStat statName => EnchantmentStat.AmmoCost;
		public override IEnumerable<object> TooltipArgs => new object[] { base.Tooltip };
		public override string Tooltip => StandardTooltip;
		public override string TooltipValue => (EffectStrength < 0f ? EffectStrength * -1 : EffectStrength).PercentString();
	}
}
