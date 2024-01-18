﻿using System.Collections.Generic;
using Terraria.ModLoader;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Effects;

namespace WeaponEnchantments.Items.Enchantments.Utility {
    public abstract class FishingPowerEnchantment : Enchantment {
		public override int StrengthGroup => 4;
		public override void GetMyStats() {
            Effects = new() {
                new FishingPower(@base: EnchantmentStrengthData)
            };

            AllowedList = new Dictionary<EItemType, float>() {
                { EItemType.Armor, 1f },
                { EItemType.Accessories, 1f },
                { EItemType.FishingPoles, 1f }
            };
        }

        public override string ShortTooltip => GetShortTooltip(sign: true, percent: false, multiply100: false);
        public override string Artist => "andro951";
        public override string ArtModifiedBy => null;
        public override string Designer => "andro951";
    }
    public class FishingPowerEnchantmentBasic : FishingPowerEnchantment
    {
        public override SortedDictionary<ChestID, float> ChestDrops => new() {
            { ChestID.Water, 1f }
        };
        public override List<WeightedPair> CrateDrops => new() {
            new(CrateID.Wooden),
            new(CrateID.Pearlwood_WoodenHard)
        };
    }
    public class FishingPowerEnchantmentCommon : FishingPowerEnchantment { }
    public class FishingPowerEnchantmentRare : FishingPowerEnchantment { }
    public class FishingPowerEnchantmentEpic : FishingPowerEnchantment { }
    public class FishingPowerEnchantmentLegendary : FishingPowerEnchantment { }

}
