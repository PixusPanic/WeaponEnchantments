﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Localization;

namespace WeaponEnchantments.Items
{
    public class PowerBooster : WEModItem
    {
        public static int ID;
        public override string Texture => (GetType().Namespace + ".Sprites." + Name).Replace('.', '/');
        public override DropRestrictionsID DropRestrictionsID => DropRestrictionsID.HardModeBosses;
        public override List<WikiTypeID> WikiItemTypes => new() { WikiTypeID.PowerBooster };
        public override bool ConfigOnlyDrop => true;
        public override int CreativeItemSacrifice => 1;
		public override string LocalizationTooltip => 
            "Use this while the item you want to boost is in an Enchantment Table to raise its base level by 10.\n" +
			"(Shift left click from your inventory or left click on item in the table with this on your cursor.)\n" +
			"This item will be returned if the boosted item is offered.";

        public override string Artist => "andro951";
        public override string Designer => "andro951";
        public override void SetStaticDefaults() {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
            ItemID.Sets.ItemIconPulse[Item.type] = true;
            ItemID.Sets.ItemNoGravity[Item.type] = true;

            base.SetStaticDefaults();
        }
        public override void SetDefaults() {
            Item.value = 500000;
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.Orange;
        }
        public override void PostUpdate() {
            Lighting.AddLight(Item.Center, Color.WhiteSmoke.ToVector3() * 0.55f * Main.essScale); // Makes this item glow when thrown out of inventory.
        }
        public override void AddRecipes() {
            ID = Item.type;
        }
    }
}
