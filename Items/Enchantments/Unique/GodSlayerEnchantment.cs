﻿using System.Collections.Generic;
using Terraria.ID;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Effects;
using androLib.Items;
using androLib.Common.Utility;
using Terraria.ModLoader;

namespace WeaponEnchantments.Items.Enchantments.Unique
{
	public abstract class GodSlayerEnchantment : Enchantment
	{
		public override int StrengthGroup => 7;
		public override void GetMyStats() {
			Effects = new() {
				new GodSlayer(@base: EnchantmentStrengthData)
			};

			AllowedList = new Dictionary<EItemType, float>() {
				{ EItemType.Weapons, 1f }
			};
		}
		public override string Artist => "Zorutan";
		public override string ArtModifiedBy => null;
		public override string Designer => "andro951";
	}
	[Autoload(false)]
	public class GodSlayerEnchantmentBasic : GodSlayerEnchantment
	{
		public override SellCondition SellCondition => SellCondition.PostPlantera;
		public override List<DropData> NpcAIDrops => new() {
			new(NPCAIStyleID.BiomeMimic)
		};
	}
	[Autoload(false)]
	public class GodSlayerEnchantmentCommon : GodSlayerEnchantment { }
	[Autoload(false)]
	public class GodSlayerEnchantmentRare : GodSlayerEnchantment { }
	[Autoload(false)]
	public class GodSlayerEnchantmentEpic : GodSlayerEnchantment { }
	[Autoload(false)]
	public class GodSlayerEnchantmentLegendary : GodSlayerEnchantment { }

}
