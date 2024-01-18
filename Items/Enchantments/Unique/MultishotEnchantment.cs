﻿using System.Collections.Generic;
using Terraria.ID;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Effects;

namespace WeaponEnchantments.Items.Enchantments.Unique
{
	public abstract class MultishotEnchantment : Enchantment
	{
		public override int StrengthGroup => 8;
		public override int DamageClassSpecific => (int)DamageClassID.Ranged;
		public override void GetMyStats() {
			Effects = new() {
				new Multishot(@base: EnchantmentStrengthData)
			};

			AllowedList = new Dictionary<EItemType, float>() {
				{ EItemType.Weapons, 1f }
			};
		}
		public override string Artist => "Zorutan";
		public override string ArtModifiedBy => null;
		public override string Designer => "andro951";
	}
	public class MultishotEnchantmentBasic : MultishotEnchantment
	{
		public override SellCondition SellCondition => SellCondition.PostPlantera;
		public override List<WeightedPair> NpcAIDrops => new() {
			new(NPCAIStyleID.BiomeMimic)
		};
	}
	public class MultishotEnchantmentCommon : MultishotEnchantment { }
	public class MultishotEnchantmentRare : MultishotEnchantment { }
	public class MultishotEnchantmentEpic : MultishotEnchantment { }
	public class MultishotEnchantmentLegendary : MultishotEnchantment { }

}
