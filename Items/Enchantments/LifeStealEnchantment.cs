﻿using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Effects;

namespace WeaponEnchantments.Items.Enchantments {
    public abstract class LifeStealEnchantment : Enchantment {
        public override float ScalePercent => 0.8f;
        public override bool Max1 => true;
        public override float CapacityCostMultiplier => 2f;
		public override int StrengthGroup => 5;
        public override void GetMyStats() {
            Effects = new() {
                new LifeSteal(@base: EnchantmentStrengthData)
            };

            AllowedList = new Dictionary<EItemType, float>() {
                { EItemType.Weapons, 1f }
            };
        }
		public override string Artist => "Zorutan";
        public override string ArtModifiedBy => null;
        public override string Designer => "andro951";
        public override string WikiDescription => 
            $"Life Steal from an enchantment is not exactly the same as vanilla lifesteal.  Vanilla lifesteal calculates the " +
			$"amount of healing you will receive and truncates any decimal.  Lifesteal from an enchantment stores the decimal and " +
			$"adds it to your heal amount the next time you use lifesteal.  (Note: This remainder is reset to zero if you reach " +
			$"full health)  This makes lifesteal from enchantments valuable on any weapon because you can heal some regardless of " +
			$"the damage per hit.  Lifesteal from enchantments utilizes the same lifesteal limiting as vanilla does.  This " +
			$"limit is very unlikely to be reached unless you are using a weapon at least as powerful as the Zenith.  At that point, " +
			$"you may reach the limit.  If you do exceed the limit, you will gain zero life from lifesteal util it has recovered.  " +
			$"The amount the limit value is affected can be adjusted in the config.  (Note: the config option does not affect any " +
			$"source of lifesteal besides lifesteal from enchantments.)  Life steal gained from minions is reduced by half. (I may " +
			$"ban it or reduce it more on minions at some point in the future.  Haven't decided yet.)  Vanilla lifesteal allows you " +
			$"heal from lifesteal when already at full health (wasting the life steal pool for no reason).  Enchantments do not do this.  " +
			$"Additionally, life steal from enchantments will not over heal you past full health which would also waste the pool.  " +
			$"The moon lord's Moon Leach debuff normally prevents all lifesteal.  I personally don't like mechanics that completely " +
			$"turn off effects like this, so life steal from enchantments is reduced by 50% from this debuff instead.";
    }
    public class LifeStealEnchantmentBasic : LifeStealEnchantment
    {
        public override SellCondition SellCondition => SellCondition.PostEaterOfWorldsOrBrainOfCthulhu;
        public override List<WeightedPair> NpcDropTypes => new() {
            new(NPCID.WallofFlesh)
        };
        public override List<WeightedPair> NpcAIDrops => new() {
            new(NPCAIStyleID.Vulture),
            new(NPCAIStyleID.TheHungry),
            new(NPCAIStyleID.Creeper)
        };
        public override SortedDictionary<ChestID, float> ChestDrops => new() {
            { ChestID.Shadow, 0.1f },
            { ChestID.Shadow_Locked, 0.1f }
        };
        public override List<WeightedPair> CrateDrops => new() {
            new(CrateID.Obsidian_LockBox, 0.05f),
            new(CrateID.Crimson, 0.5f),
            new(CrateID.Hematic_CrimsonHard, 0.5f)
        };
    }
    public class LifeStealEnchantmentCommon : LifeStealEnchantment { }
    public class LifeStealEnchantmentRare : LifeStealEnchantment { }
    public class LifeStealEnchantmentEpic : LifeStealEnchantment { }
    public class LifeStealEnchantmentLegendary : LifeStealEnchantment { }

}
