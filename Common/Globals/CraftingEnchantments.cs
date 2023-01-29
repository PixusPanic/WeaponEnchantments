﻿using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Items;

namespace WeaponEnchantments.Common.Globals
{
	public class CraftingEnchantments : GlobalItem
	{
		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
            //Bars created from uncrafting Containments
			switch (entity.type) {
                case ItemID.SilverBar:
                case ItemID.TungstenBar:
                case ItemID.GoldBar:
                case ItemID.PlatinumBar:
                case ItemID.DemoniteBar:
                case ItemID.CrimtaneBar:
                    return true;
			}

			if(entity.ModItem == null)
                return false;

            ModItem modItem = entity.ModItem;
            return modItem is Enchantment or EnchantmentEssenceBasic;
		}

		public override void OnCreate(Item item, ItemCreationContext context) {
            if (context is RecipeCreationContext recipeCreationContext) {
                if (recipeCreationContext.ConsumedItems == null)
                    return;

                Dictionary<int, int> otherCraftedItems = new();
                foreach (Item consumedItem in recipeCreationContext.ConsumedItems) {
                    otherCraftedItems.AddOrCombine(GetOtherCraftedItems(item, consumedItem));
                }

                foreach(KeyValuePair<int, int> pair in otherCraftedItems) {
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("Crafting"), pair.Key, pair.Value);
                }
            }
        }

        public static Dictionary<int, int> GetOtherCraftedItems(Item item,  Item consumedItem) {
            Dictionary<int, int> dict = new();
            if (consumedItem.ModItem is Enchantment consumedEnchantment) {
                int newSize;
                if (item.ModItem is Enchantment enchantment) {
                    newSize = enchantment.EnchantmentTier;
                }
                else {
                    newSize = 0;
                }
                int size = consumedEnchantment.EnchantmentTier;
                if (newSize > size) {
                    if (size < 2) {
                        dict.AddOrCombine((ContainmentItem.IDs[size], 1));
                    }
                    else if (size == 3) {
                        dict.AddOrCombine((ItemID.Topaz, 2));
                    }
                }
                else {
                    if (size == 4) {
                        dict.AddOrCombine((ItemID.Amber, 1));
                    }
                    else if (size == 3) {
                        dict.AddOrCombine((ItemID.Topaz, 2));
                    }

                    if (size >= 2) {
                        if (newSize < 2)
                            dict.AddOrCombine((ContainmentItem.IDs[2], 1));
                    }
                    else if (size < 2) {
                        dict.AddOrCombine((ContainmentItem.IDs[size], 1));
                    }

                    //Essence
                    int essenceNumber = consumedEnchantment.Utility ? 5 : 10;
                    for (int k = newSize + 1; k <= size; k++) {
                        dict.AddOrCombine((EnchantmentEssence.IDs[k], essenceNumber));
                    }
                }
            }
            else if (consumedItem.ModItem is ContainmentItem containment) {
                if (containment.tier == 2 && item.type == ContainmentItem.barIDs[0, 2])
                    dict.AddOrCombine((ItemID.Topaz, 4));
            }

            return dict;
        }
	}
}
