﻿using System.Collections.Generic;
using Terraria.ID;
using WeaponEnchantments.Common;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Effects;
using androLib.Items;
using androLib.Common.Utility;
using Terraria.ModLoader;

namespace WeaponEnchantments.Items.Enchantments
{
	public abstract class DamageEnchantment : Enchantment
	{
		public override int LowestCraftableTier => 0;
		public override void GetMyStats() {
			Effects = new() {
				new DamageAfterDefenses(EnchantmentStrengthData)
			};
		}

		public override string ShortTooltip => GetShortTooltip(sign: true);
		public override string Artist => "Zorutan";
		public override string ArtModifiedBy => null;
		public override string Designer => "andro951";
	}
	[Autoload(false)]
	public class DamageEnchantmentBasic : DamageEnchantment
	{
		public override SellCondition SellCondition => SellCondition.Always;
		public override List<DropData> NpcDropTypes => new() {
			new(NPCID.KingSlime)
		};
		public override List<DropData> NpcAIDrops => new() {
			new(NPCAIStyleID.Slime)
		};
		public override List<DropData> ChestDrops => new() {
			new(ChestID.Chest_Normal)
		};
		public override List<DropData> CrateDrops => new() {
			new(CrateID.Wooden, 0.5f),
			new(CrateID.Pearlwood_WoodenHard, 0.5f)
		};
	}
	[Autoload(false)]
	public class DamageEnchantmentCommon : DamageEnchantment { }
	[Autoload(false)]
	public class DamageEnchantmentRare : DamageEnchantment { }
	[Autoload(false)]
	public class DamageEnchantmentEpic : DamageEnchantment { }
	[Autoload(false)]
	public class DamageEnchantmentLegendary : DamageEnchantment { }

}
