﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using WeaponEnchantments.Common;
using WeaponEnchantments.Common.Globals;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Content.NPCs;
using WeaponEnchantments.Items;
using WeaponEnchantments.Items.Enchantments;
using WeaponEnchantments.Items.Enchantments.Unique;
using WeaponEnchantments.Items.Enchantments.Utility;
using WeaponEnchantments.Items.Utility;
using WeaponEnchantments.Tiles;
using static WeaponEnchantments.Common.Configs.ConfigValues;

namespace WeaponEnchantments
{
    public class WEModSystem : ModSystem
    {
        public static bool AltDown => Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt);
        public static bool ShiftDown => Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);
        internal static UserInterface weModSystemUI;
        internal static UserInterface mouseoverUIInterface;
        internal static UserInterface promptInterface;
        internal static byte versionUpdate = 0;
        public static bool PromptInterfaceActive => promptInterface?.CurrentState != null;
        public static int[] levelXps = new int[EnchantedItem.MAX_Level];
        private static bool favorited;
        public static int stolenItemToBeCleared = -1;
        public static List<string> updatedPlayerNames;
        public static SortedDictionary<ChestID, List<WeightedPair>> chestDrops = new SortedDictionary<ChestID, List<WeightedPair>>();

        private GameTime _lastUpdateUiGameTime;
        private bool dayTime = Main.dayTime;

        public override void OnModLoad() {
            if (!Main.dedServ) {
                weModSystemUI = new UserInterface();
                promptInterface = new UserInterface();
                mouseoverUIInterface = new UserInterface();
            }

            double previous = 0;
            double current;
            int l;
            for (l = 0; l < EnchantedItem.MAX_Level; l++) {
                current = previous * 1.23356622200537 + (l + 1) * 1000;
                previous = current;
                levelXps[l] = (int)current;
            }

            WEMod.playerSwapperModEnabled = ModLoader.HasMod("PlayerSwapper");
            if (WEMod.playerSwapperModEnabled)
                updatedPlayerNames = new List<string>();
        }
        public override void Unload() {
            if (!Main.dedServ) {
                weModSystemUI = null;
                mouseoverUIInterface = null;
                promptInterface = null;
            }
        }
        public override void PostDrawInterface(SpriteBatch spriteBatch) {
            WEPlayer wePlayer = Main.LocalPlayer.GetModPlayer<WEPlayer>();
            if (wePlayer.usingEnchantingTable) {
                //Disable Left Shift to Quick trash
                if (ItemSlot.Options.DisableLeftShiftTrashCan) {
                    wePlayer.disableLeftShiftTrashCan = ItemSlot.Options.DisableLeftShiftTrashCan;
                    ItemSlot.Options.DisableLeftShiftTrashCan = false;
                }

                Item itemInUI = wePlayer.ItemInUI();
                bool removedItem = false;
                bool addedItem = false;
                bool swappedItem = false;
                //Check if the itemSlot is empty because the item was just taken out and transfer the mods to the global item if so
                if (itemInUI.IsAir) {
                    if (wePlayer.itemInEnchantingTable)//If item WAS in the itemSlot but it is empty now,
                        removedItem = true;//Transfer items to global item and break the link between the global item and enchanting table itemSlots/enchantmentSlots

                    wePlayer.itemInEnchantingTable = false;//The itemSlot's PREVIOUS state is now empty(false)
                }
                else if (!wePlayer.itemInEnchantingTable) {//If itemSlot WAS empty but now has an item in it
                    //Check if itemSlot has item that was just placed there, copy the enchantments to the slots and link the slots to the global item
                    addedItem = true;
                    wePlayer.itemInEnchantingTable = true;//Set PREVIOUS state of itemSlot to having an item in it
                }
                else if (!wePlayer.itemBeingEnchanted.IsSameEnchantedItem(itemInUI)) {
                    swappedItem = true;
                }

                if (removedItem || swappedItem)
                    RemoveTableItem(wePlayer);

                if (addedItem || swappedItem) {
                    wePlayer.itemBeingEnchanted = wePlayer.ItemInUI();// Link the item in the table to the player so it can be updated after being taken out.
                    Item itemBeingEnchanted = wePlayer.itemBeingEnchanted;
                    favorited = itemBeingEnchanted.favorited;
                    itemBeingEnchanted.favorited = false;
                    if (itemBeingEnchanted.TryGetEnchantedItem(out EnchantedItem iBEGlobal)) {
                        iBEGlobal.inEnchantingTable = true;
                        wePlayer.previousInfusedItemName = iBEGlobal.infusedItemName;
                        if (iBEGlobal is EnchantedEquipItem enchantedEquipItem)
                            enchantedEquipItem.equippedInArmorSlot = false;
                    }

                    if (wePlayer.infusionConsumeItem != null && (EnchantedItemStaticMethods.IsWeaponItem(itemBeingEnchanted) || EnchantedItemStaticMethods.IsArmorItem(itemBeingEnchanted))) {
                        wePlayer.itemBeingEnchanted.TryInfuseItem(wePlayer.infusionConsumeItem);
                        wePlayer.enchantingTableUI.infusionButonText.SetText(TableTextID.Finalize.ToString().Lang(L_ID1.TableText));
                    }

                    if (wePlayer.ItemInUI().TryGetEnchantedItem(out EnchantedItem iGlobal)) {
                        for (int i = 0; i < EnchantingTable.maxEnchantments; i++) {
                            if (iGlobal.enchantments[i] != null) {//For each enchantment in the global item,
                                wePlayer.EnchantmentUISlot(i).Item = iGlobal.enchantments[i].Clone();//copy enchantments to the enchantmentSlots
                                wePlayer.enchantmentInEnchantingTable[i] = wePlayer.EnchantmentsModItem(i) != null;//Set PREVIOUS state of enchantmentSlot to has an item in it(true)
                                iGlobal.enchantments[i] = wePlayer.EnchantmentUISlot(i).Item;//Force link to enchantmentSlot just in case
                            }

                            iGlobal.enchantments[i] = wePlayer.EnchantmentUISlot(i).Item;//Link global item to the enchantmentSlots
                        }
                    }
                }

                itemInUI = wePlayer.ItemInUI();
                //Check if enchantments are added/removed from enchantmentSlots and re-link global item to enchantmentSlot
                for (int i = 0; i < EnchantingTable.maxEnchantments; i++) {
                    Item tableEnchantment = wePlayer.EnchantmentInUI(i);
                    Item itemEnchantment = new Item();
                    if (itemInUI.TryGetEnchantedItem(out EnchantedItem iGlobal))
                        itemEnchantment = iGlobal.enchantments[i];

                    if (tableEnchantment.IsAir) {
                        if (wePlayer.enchantmentInEnchantingTable[i]) {//if enchantmentSlot HAD an enchantment in it but it was just taken out,
                            //Force global item to re-link to the enchantmentSlot instead of following the enchantment just taken out
                            EnchantedItemStaticMethods.RemoveEnchantment(i);
                            //((Enchantment)itemEnchantment.ModItem).statsSet = false;
                            iGlobal.enchantments[i] = wePlayer.EnchantmentUISlot(i).Item;
                        }

                        wePlayer.enchantmentInEnchantingTable[i] = false;//Set PREVIOUS state of enchantmentSlot to empty(false)
                    }
                    else if (!itemEnchantment.IsAir && itemEnchantment.type != tableEnchantment.type) {
                        //If player swapped enchantments (without removing the previous one in the enchantmentSlot) Force global item to re-link to the enchantmentSlot instead of following the enchantment just taken out
                        EnchantedItemStaticMethods.RemoveEnchantment(i);
                        iGlobal.enchantments[i] = wePlayer.EnchantmentUISlot(i).Item;
                        EnchantedItemStaticMethods.ApplyEnchantment(i);
                    }
                    else if (!wePlayer.enchantmentInEnchantingTable[i]) {
                        //If it WAS empty but isn't now, re-link global item to enchantmentSlot just in case
                        wePlayer.enchantmentInEnchantingTable[i] = true;//Set PREVIOUS state of enchantmentSlot to has an item in it(true)
                        iGlobal.enchantments[i] = wePlayer.EnchantmentUISlot(i).Item;//Force link to enchantmentSlot just in case
                        EnchantedItemStaticMethods.ApplyEnchantment(i);
                    }
                }

                //If player is too far away, close the enchantment table
                if (!wePlayer.Player.IsInInteractionRangeToMultiTileHitbox(wePlayer.Player.chestX, wePlayer.Player.chestY) || wePlayer.Player.chest != -1 || !Main.playerInventory)
                    CloseWeaponEnchantmentUI();

                //Update cursor override
                if (ItemSlot.ShiftInUse) {
                    bool stop = false;
                    bool valid = false;
                    if (Main.mouseItem.IsAir && !Main.HoverItem.IsAir) {
                        for (int j = 0; j < EnchantingTable.maxItems; j++) {
                            if (wePlayer.enchantingTableUI.itemSlotUI[j].contains) {
                                stop = true;
                            }
                        }

                        for (int j = 0; j < EnchantingTable.maxEnchantments && !stop; j++) {
                            if (wePlayer.enchantingTableUI.enchantmentSlotUI[j].contains) {
                                stop = true;
                            }
                        }

                        for (int j = 0; j < EnchantingTable.maxEssenceItems && !stop; j++) {
                            if (wePlayer.enchantingTableUI.essenceSlotUI[j].contains) {
                                stop = true;
                            }
                        }

                        if (!stop)
                            valid = wePlayer.CheckShiftClickValid(ref Main.HoverItem);
                    }

                    if (!stop) {
                        if (!valid || Main.cursorOverride == 6) {
                            Main.cursorOverride = -1;
                        }
                    }
                }
            }

            //Fix for splitting stack of enchanted items in a chest
            if (wePlayer.Player.chest != -1) {
                int chest = wePlayer.Player.chest;
                if (Main.HoverItem.TryGetEnchantedItem()) {
                    Player player = wePlayer.Player;
                    Item[] inventory;
                    switch (chest) {
                        case -2:
                            inventory = player.bank.item;
                            break;
                        case -3:
                            inventory = player.bank2.item;
                            break;
                        case -4:
                            inventory = player.bank3.item;
                            break;
                        case -5:
                            inventory = player.bank4.item;
                            break;
                        default:
                            if (chest > -1) {
                                //Chest
                                inventory = Main.chest[chest].item;
                            }
                            else {
                                inventory = new Item[] { };
                            }

                            break;
                    }

                    for (int i = 0; i < inventory.Length; i++) {
                        ref Item item = ref inventory[i];
                        if (item.TryGetEnchantedItem(out EnchantedItem iGlobal)) {
                            if (item.IsSameEnchantedItem(Main.HoverItem) && item.stack == Main.HoverItem.stack) {
                                int stack = item.stack;
                                if (Main.mouseRight) {
                                    Item mouseItem = Main.mouseItem;
                                    int mouseItemStack = mouseItem.stack;
                                    int maxStack = mouseItem.maxStack;
                                    if (stack + mouseItemStack < maxStack) {
                                        Main.mouseItem.stack += stack;
                                        item = new Item();
                                    }
                                    else {
                                        Main.mouseItem.stack = maxStack;
                                        item.stack = stack + mouseItemStack - maxStack;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            //Calamity Reforge
            if (EnchantedItem.calamityReforged) {
                if (Main.reforgeItem.TryGetEnchantedItem()) {
                    //Calamity only
                    EnchantedItem.ReforgeItem(ref Main.reforgeItem, wePlayer.Player, true);
                }
                else {
                    //Calamity and AutoReforge
                    EnchantedItem.ReforgeItem(ref EnchantedItem.calamityAndAutoReforgePostReforgeItem, wePlayer.Player, true);
                }
            }

            //Fargos pirates that steal items
            if (stolenItemToBeCleared != -1 && Main.netMode != NetmodeID.MultiplayerClient) {
                Item itemToClear = Main.item[stolenItemToBeCleared];
                if (itemToClear != null && itemToClear.TryGetEnchantedItem(out EnchantedItem iGlobal)) {
                    iGlobal.lastValueBonus = 0;
                    iGlobal.prefix = -1;
                }

                stolenItemToBeCleared = -1;
            }

            //Player swapper
            if (WEMod.playerSwapperModEnabled && Main.netMode != NetmodeID.Server) {
                string playerName = wePlayer.Player.name;
                if (!updatedPlayerNames.Contains(playerName)) {
                    OldItemManager.ReplaceAllPlayerOldItems(wePlayer.Player);
                    updatedPlayerNames.Add(playerName);
                }
            }
        }
        public static void RemoveTableItem(WEPlayer wePlayer) {
            for (int i = 0; i < EnchantingTable.maxEnchantments; i++) {
                Item enchantmentInUI = wePlayer.EnchantmentInUI(i);
                //For each enchantment in the enchantmentSlots,
                if (enchantmentInUI != null) {
                    if (wePlayer.itemBeingEnchanted.TryGetEnchantedItem(out EnchantedItem iGlobal))
                        iGlobal.enchantments[i] = enchantmentInUI.Clone();//copy enchantments to the global item
                }

                wePlayer.EnchantmentUISlot(i).Item = new Item();//Delete enchantments still in enchantmentSlots(There were transfered to the global item)
                wePlayer.enchantmentInEnchantingTable[i] = false;//The enchantmentSlot's PREVIOUS state is now empty(false)
            }

            if (wePlayer.infusionConsumeItem != null) {
                if (!wePlayer.infusionConsumeItem.IsSameEnchantedItem(wePlayer.itemBeingEnchanted))
                    wePlayer.itemBeingEnchanted.TryInfuseItem(wePlayer.previousInfusedItemName, true);

                wePlayer.enchantingTableUI.infusionButonText.SetText(TableTextID.Cancel.ToString().Lang(L_ID1.TableText));
            }

            if (wePlayer.itemBeingEnchanted.TryGetEnchantedItem(out EnchantedItem iBEGlobal))
                iBEGlobal.inEnchantingTable = false;

            wePlayer.itemBeingEnchanted.favorited = favorited;
            wePlayer.itemBeingEnchanted = wePlayer.enchantingTableUI.itemSlotUI[0].Item;//Stop tracking the item that just left the itemSlot
        }
        public static void CloseWeaponEnchantmentUI(bool noSound = false) {
            WEPlayer wePlayer = Main.LocalPlayer.GetModPlayer<WEPlayer>();
            Item itemInUI = wePlayer.ItemInUI();
            if (itemInUI != null && !itemInUI.IsAir) {
                //Give item in table back to player
                wePlayer.ItemUISlot().Item = wePlayer.Player.GetItem(Main.myPlayer, itemInUI, GetItemSettings.LootAllSettings);

                //Clear item and enchantments from table
                itemInUI = wePlayer.ItemInUI();
                if (itemInUI.IsAir) {
                    RemoveTableItem(wePlayer);

                    wePlayer.enchantingTable.item[0] = new Item();
                    for (int i = 0; i < EnchantingTable.maxEnchantments; i++) {
                        wePlayer.enchantmentInEnchantingTable[i] = false;
                        wePlayer.enchantingTable.enchantmentItem[i] = new Item();
                        wePlayer.enchantingTableUI.enchantmentSlotUI[i].Item = new Item();
                    }
                }
            }

            wePlayer.itemBeingEnchanted = null;
            wePlayer.itemInEnchantingTable = false;
            wePlayer.usingEnchantingTable = false;
            if (wePlayer.Player.chest == -1) {
                if (!noSound)
                    SoundEngine.PlaySound(SoundID.MenuClose);
            }

            weModSystemUI.SetState(null);
            promptInterface.SetState(null);

            ItemSlot.Options.DisableLeftShiftTrashCan = wePlayer.disableLeftShiftTrashCan;
        }
        public static void OpenWeaponEnchantmentUI(bool noSound = false) {
            WEPlayer wePlayer = Main.LocalPlayer.GetModPlayer<WEPlayer>();
            wePlayer.usingEnchantingTable = true;
            if (!noSound)
                SoundEngine.PlaySound(SoundID.MenuOpen);

            UIState state = new UIState();
            state.Append(wePlayer.enchantingTableUI);
            weModSystemUI.SetState(state);
        }
        public static void QuickStackEssence() {
            bool transfered = false;
            WEPlayer wePlayer = Main.LocalPlayer.GetModPlayer<WEPlayer>();
            for (int j = 0; j < 50; j++) {
                if (wePlayer.Player.inventory[j].TryGetEnchantmentEssence(out EnchantmentEssence essence)) {
                    int tier = essence.EssenceTier;
                    int ammountToTransfer;
                    int startingStack = wePlayer.Player.inventory[j].stack;
                    if (wePlayer.enchantingTable.essenceItem[tier].IsAir) {
                        wePlayer.enchantingTable.essenceItem[tier] = wePlayer.Player.inventory[j].Clone();
                        wePlayer.Player.inventory[j] = new Item();
                        transfered = true;
                    }
                    else {
                        int maxStack = wePlayer.enchantingTable.essenceItem[tier].maxStack;
                        if (wePlayer.enchantingTable.essenceItem[tier].stack < maxStack) {
                            if (wePlayer.Player.inventory[j].stack + wePlayer.enchantingTable.essenceItem[tier].stack > maxStack) {
                                ammountToTransfer = maxStack - wePlayer.enchantingTable.essenceItem[tier].stack;
                            }
                            else {
                                ammountToTransfer = wePlayer.Player.inventory[j].stack;
                            }

                            wePlayer.enchantingTable.essenceItem[tier].stack += ammountToTransfer;
                            wePlayer.Player.inventory[j].stack -= ammountToTransfer;
                            transfered = true;
                        }
                    }

                    if (wePlayer.Player.inventory[j].stack == startingStack)
                        transfered = false;
                }
            }
            if (transfered)
                SoundEngine.PlaySound(SoundID.Grab);
        }
        public static bool AutoCraftEssence() {
            bool crafted = false;
            WEPlayer wePlayer = Main.LocalPlayer.GetModPlayer<WEPlayer>();
            for (int i = EnchantingTable.maxEssenceItems - 1; i > 0; i--) {
                if (wePlayer.enchantingTableUI.essenceSlotUI[i].Item.NullOrAir())
                    continue;

                int maxStack = wePlayer.enchantingTableUI.essenceSlotUI[i].Item.maxStack;
                if (wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack < maxStack) {
                    int ammountToTransfer;
                    if (wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack == 0 || (maxStack > wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack + (wePlayer.enchantingTableUI.essenceSlotUI[i - 1].Item.stack / 4))) {
                        ammountToTransfer = wePlayer.enchantingTableUI.essenceSlotUI[i - 1].Item.stack / 4;
                    }
                    else {
                        ammountToTransfer = maxStack - wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack;
                    }

                    if (ammountToTransfer > 0) {
                        wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack += ammountToTransfer;
                        wePlayer.enchantingTableUI.essenceSlotUI[i - 1].Item.stack -= ammountToTransfer * 4;
                        crafted = true;
                    }
                }
            }

            for (int i = 1; i < EnchantingTable.maxEssenceItems; i++) {
                if (wePlayer.enchantingTableUI.essenceSlotUI[i].Item.NullOrAir())
                    continue;

                int maxStack = wePlayer.enchantingTableUI.essenceSlotUI[i].Item.maxStack;
                if (wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack < maxStack) {
                    int ammountToTransfer;
                    if (wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack == 0 || (maxStack > wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack + (wePlayer.enchantingTableUI.essenceSlotUI[i - 1].Item.stack / 4))) {
                        ammountToTransfer = wePlayer.enchantingTableUI.essenceSlotUI[i - 1].Item.stack / 4;
                    }
                    else {
                        ammountToTransfer = maxStack - wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack;
                    }

                    if (ammountToTransfer > 0) {
                        wePlayer.enchantingTableUI.essenceSlotUI[i].Item.stack += ammountToTransfer;
                        wePlayer.enchantingTableUI.essenceSlotUI[i - 1].Item.stack -= ammountToTransfer * 4;
                        crafted = true;
                    }
                }
            }

            return crafted;
        }
        public override void PreSaveAndQuit() {
            WEPlayer wePlayer = Main.LocalPlayer.GetModPlayer<WEPlayer>();
            weModSystemUI.SetState(null);
            promptInterface.SetState(null);
            if (wePlayer.usingEnchantingTable) {
                CloseWeaponEnchantmentUI();
                wePlayer.enchantingTableUI.OnDeactivate();
            }
        }
        public override void UpdateUI(GameTime gameTime) {
            _lastUpdateUiGameTime = gameTime;
            if (weModSystemUI?.CurrentState != null) {
                weModSystemUI.Update(gameTime);
            }

            if (PromptInterfaceActive)
                promptInterface.Update(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Over"));
            if (index != -1) {
                layers.Insert
                (
                    ++index,
                    new LegacyGameInterfaceLayer
                    (
                        "WeaponEnchantments: Mouse Over",
                        delegate {
                            if (_lastUpdateUiGameTime != null && mouseoverUIInterface?.CurrentState != null) {
                                mouseoverUIInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                            }
                            return true;
                        },
                        InterfaceScaleType.UI
                     )
                );
            }

            index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (index != -1) {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "WeaponEnchantments: WeaponEnchantmentsUI",
                    delegate {
                        if (_lastUpdateUiGameTime != null && weModSystemUI?.CurrentState != null) {
                            weModSystemUI.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }

                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }

            index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (index != -1) {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "WeaponEnchantments: PromptUI",
                    delegate {
                        if (_lastUpdateUiGameTime != null && PromptInterfaceActive)
                            promptInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);

                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
        public override void AddRecipeGroups() {
            RecipeGroup group = new RecipeGroup(() => "Any Common Gem", new int[] {
                ItemID.Topaz,
                ItemID.Sapphire,
                ItemID.Ruby,
                ItemID.Emerald,
                ItemID.Amethyst
            });
            RecipeGroup.RegisterGroup("WeaponEnchantments:CommonGems", group);

            group = new RecipeGroup(() => "Any Rare Gem", new int[] {
                ItemID.Amber,
                ItemID.Diamond
            });
            RecipeGroup.RegisterGroup("WeaponEnchantments:RareGems", group);

            group = new RecipeGroup(() => "Workbenches", new int[] {
                ItemID.WorkBench,
                ItemID.BambooWorkbench,
                ItemID.BlueDungeonWorkBench,
                ItemID.BoneWorkBench,
                ItemID.BorealWoodWorkBench,
                ItemID.CactusWorkBench,
                ItemID.CrystalWorkbench,
                ItemID.DynastyWorkBench,
                ItemID.EbonwoodWorkBench,
                ItemID.FleshWorkBench,
                ItemID.FrozenWorkBench,
                ItemID.GlassWorkBench,
                ItemID.GoldenWorkbench,
                ItemID.GothicWorkBench,
                ItemID.GraniteWorkBench,
                ItemID.GreenDungeonWorkBench,
                ItemID.HoneyWorkBench,
                ItemID.LesionWorkbench,
                ItemID.LihzahrdWorkBench,
                ItemID.LivingWoodWorkBench,
                ItemID.MarbleWorkBench,
                ItemID.MartianWorkBench,
                ItemID.MeteoriteWorkBench,
                ItemID.MushroomWorkBench,
                ItemID.NebulaWorkbench,
                ItemID.ObsidianWorkBench,
                ItemID.PalmWoodWorkBench,
                ItemID.PearlwoodWorkBench,
                ItemID.PinkDungeonWorkBench,
                ItemID.PumpkinWorkBench,
                ItemID.RichMahoganyWorkBench,
                ItemID.SandstoneWorkbench,
                ItemID.ShadewoodWorkBench,
                ItemID.SkywareWorkbench,
                ItemID.SlimeWorkBench,
                ItemID.SolarWorkbench,
                ItemID.SpiderWorkbench,
                ItemID.SpookyWorkBench,
                ItemID.StardustWorkbench,
                ItemID.SteampunkWorkBench,
                ItemID.VortexWorkbench
            });
            RecipeGroup.RegisterGroup("WeaponEnchantments:Workbenches", group);

            group = new RecipeGroup(() => "Any Aligned Soul", new int[] {
                ItemID.SoulofLight,
                ItemID.SoulofNight
            });
            RecipeGroup.RegisterGroup("WeaponEnchantments:AlignedSoul", group);
        }
        public override void PostWorldGen() {
            for (int chestIndex = 0; chestIndex < 1000; chestIndex++) {
                Chest chest = Main.chest[chestIndex];
                if (chest == null)
                    continue;

                int itemsPlaced = 0;

                ChestID chestID = GetChestIDFromChest(chest);
                GetChestLoot(chestID, out List<WeightedPair> options, out float chance);

                if (chance <= 0f)
                    continue;

                if (options == null)
                    continue;

                for (int j = 0; j < 40 && itemsPlaced < chance; j++) {
                    if (chest.item[j].type != ItemID.None)
                        continue;

                    int type = options.GetOneFromWeightedList(chance);

                    if (type > 0) {
                        bool found = false;
                        for (int k = j; k >= 0; k--) {
                            if (chest.item[k].type == type && chest.item[k].stack < chest.item[k].maxStack) {
                                chest.item[k].stack++;
                                found = true;
                                j--;
                            }
                        }

                        if (!found)
                            chest.item[j] = new Item(type);
                    }

                    itemsPlaced++;
                }
            }
        }
        public static ChestID GetChestIDFromChest(Chest chest) {
            Tile tile = Main.tile[chest.x, chest.y];
            ushort tileType = tile.TileType;
            short tileFrameX = tile.TileFrameX;
            // If you look at the sprite for Chests by extracting Tiles_21.xnb, you'll see that the 12th chest is the Ice Chest.
            // Since we are counting from 0, this is where 11 comes from. 36 comes from the width of each tile including padding.
            switch (tileType) {
                case TileID.Containers:
                case TileID.FakeContainers:
                    return (ChestID)(tileFrameX / 36);
                case TileID.Containers2:
                case TileID.FakeContainers2:
                    return (ChestID)(tileFrameX / 36 + 100);
                default:
                    return ChestID.None;
            }
        }
        public static void GetChestLoot(ChestID chestID, out List<WeightedPair> itemTypes, out float chance) {
            chance = 0f;
            itemTypes = chestDrops.ContainsKey(chestID) ? chestDrops[chestID] : null;
            if (itemTypes == null)
                return;

            chance = ChestSpawnChance;
            if (itemTypes.Count == 1)
                chance *= itemTypes[0].Weight;

            switch (chestID) {
                case ChestID.Chest_Normal:
                    chance *= 0.7f;
                    //itemTypes.Add(ModContent.ItemType<DefenseEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<DamageEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<CriticalStrikeChanceEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<ReducedManaUsageEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<SizeEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<AmmoCostEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<AttackSpeedEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<PeaceEnchantmentBasic>());
                    break;
                case ChestID.Gold:
                    //itemTypes.Add(ModContent.ItemType<CriticalStrikeChanceEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<SpelunkerEnchantmentLegendary>());
                    //itemTypes.Add(ModContent.ItemType<DangerSenseEnchantmentLegendary>());
                    //itemTypes.Add(ModContent.ItemType<HunterEnchantmentLegendary>());
                    //itemTypes.Add(ModContent.ItemType<ObsidianSkinEnchantmentLegendary>());
                    //itemTypes.Add(ModContent.ItemType<AttackSpeedEnchantmentBasic>());
                    break;
                case ChestID.Gold_Locked:
                    //itemTypes.Add(ModContent.ItemType<AllForOneEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<OneForAllEnchantmentBasic>());
                    break;
                case ChestID.Shadow:
                case ChestID.Shadow_Locked:
                    chance *= 2f;
                    //itemTypes.Add(ModContent.ItemType<ArmorPenetrationEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<LifeStealEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<WarEnchantmentBasic>());
                    break;
                case ChestID.RichMahogany:
                    //itemTypes.Add(ModContent.ItemType<CriticalStrikeChanceEnchantmentBasic>());
                    break;
                case ChestID.Ivy:
                    //itemTypes.Add(ModContent.ItemType<CriticalStrikeChanceEnchantmentBasic>());
                    break;
                case ChestID.Frozen:
                    //itemTypes.Add(ModContent.ItemType<ReducedManaUsageEnchantmentBasic>());
                    break;
                case ChestID.LivingWood:
                    //itemTypes.Add(ModContent.ItemType<SizeEnchantmentBasic>());
                    break;
                case ChestID.Skyware:
                    //itemTypes.Add(ModContent.ItemType<AttackSpeedEnchantmentBasic>());
                    break;
                case ChestID.WebCovered:
                    //itemTypes.Add(ModContent.ItemType<AmmoCostEnchantmentBasic>());
                    break;
                case ChestID.Lihzahrd:
                    chance *= 2f;
                    //itemTypes.Add(ModContent.ItemType<ArmorPenetrationEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<LifeStealEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<AllForOneEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<OneForAllEnchantmentBasic>());
                    break;
                case ChestID.Water:
                    //itemTypes.Add(ModContent.ItemType<ReducedManaUsageEnchantmentBasic>());
                    break;
                case ChestID.Jungle_Dungeon:
                    chance = 1f;
                    //itemTypes.Add(ModContent.ItemType<Enchantment>());
                    break;
                case ChestID.Corruption_Dungeon:
                    chance = 1f;
                    //itemTypes.Add(ModContent.ItemType<Enchantment>());
                    break;
                case ChestID.Crimson_Dungeon:
                    chance = 1f;
                    //itemTypes.Add(ModContent.ItemType<Enchantment>());
                    break;
                case ChestID.Hallowed_Dungeon:
                    chance = 1f;
                    //itemTypes.Add(ModContent.ItemType<Enchantment>());
                    break;
                case ChestID.Ice_Dungeon:
                    chance = 1f;
                    //itemTypes.Add(ModContent.ItemType<Enchantment>());
                    break;
                case ChestID.Mushroom:
                    //itemTypes.Add(ModContent.ItemType<AmmoCostEnchantmentBasic>());
                    break;
                case ChestID.Granite:
                    //itemTypes.Add(ModContent.ItemType<AttackSpeedEnchantmentBasic>());
                    break;
                case ChestID.Marble:
                    //itemTypes.Add(ModContent.ItemType<AmmoCostEnchantmentBasic>());
                    break;
                case ChestID.Gold_DeadMans:
                    //itemTypes.Add(ModContent.ItemType<CriticalStrikeChanceEnchantmentBasic>());
                    //itemTypes.Add(ModContent.ItemType<SpelunkerEnchantmentLegendary>());
                    //itemTypes.Add(ModContent.ItemType<DangerSenseEnchantmentLegendary>());
                    //itemTypes.Add(ModContent.ItemType<HunterEnchantmentLegendary>());
                    //itemTypes.Add(ModContent.ItemType<ObsidianSkinEnchantmentLegendary>());
                    //itemTypes.Add(ModContent.ItemType<AttackSpeedEnchantmentBasic>());
                    break;
                case ChestID.SandStone:
                    //itemTypes.Add(ModContent.ItemType<AmmoCostEnchantmentBasic>());
                    break;
                case ChestID.Desert_Dungeon:
                    chance = 1f;
                    //itemTypes.Add(ModContent.ItemType<Enchantment>());
                    break;
            }
        }
        public override void LoadWorldData(TagCompound tag) {
            versionUpdate = tag.Get<byte>("versionUpdate");
            OldItemManager.versionUpdate = versionUpdate;
        }
        public override void SaveWorldData(TagCompound tag) {
            tag["versionUpdate"] = versionUpdate;
        }
        public override void PostUpdateTime() {
            if (Main.dayTime && !dayTime) {
                Witch.resetShop = true;

                //If player has a fishing pole in inventory with NpcContactAnglerEnchantment, tell them the new fishing quest.
                foreach (Item item in Main.LocalPlayer.inventory.Where(i => i.fishingPole > 0)) {
                    if (item.TryGetEnchantedItem(out EnchantedFishingPole enchantedFishingPole)) {
                        foreach(Enchantment enchantment in enchantedFishingPole.enchantments.Select(e => e.ModItem).OfType<Enchantment>()) {
                            if (enchantment is NpcContactAnglerEnchantment anglerEnchantment) {
                                int newQuestFish = Main.anglerQuestItemNetIDs[Main.anglerQuest];
                                Main.NewText($"The daily fishing quest has reset.  Your next quest is {ContentSamples.ItemsByType[newQuestFish].Name}.\n" +
                                    $"{Lang.AnglerQuestChat(false)}");
                            }
						}
                    }
                }
            }

            dayTime = Main.dayTime;
        }
    }
}