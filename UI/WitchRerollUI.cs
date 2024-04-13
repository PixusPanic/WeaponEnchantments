using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;
using WeaponEnchantments.Common.Globals;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Content.NPCs;
using WeaponEnchantments.Items;
using androLib.Common.Utility;
using androLib.Common.Globals;
using androLib.UI;
using androLib;

namespace WeaponEnchantments.UI
{
	public class DrawnUIData {
		public int cost;
		public bool hoveringReroll;
	}
	public class WitchRerollUI
	{
		private static int GetUI_ID(int id) => MasterUIManager.GetUI_ID(id, WE_UI_ID.Witch_UITypeID);

		private static bool talkingToWitch = false;
		private static DrawnUIData drawnUIData = new();
		public static void PostDrawInterface(SpriteBatch spriteBatch) {
			//Witch Re-roll ItemSlot
			if (Witch.rerollUI || Witch.cursesUI) {
				int talkNPC = Main.LocalPlayer.talkNPC;
				talkingToWitch = talkNPC >= 0 && Main.npc[talkNPC].ModFullName() == $"{WEMod.ModName}/{typeof(Witch).Name}";
				if (!talkingToWitch) {
					//Not talking to the Witch
					Witch.rerollUI = false;
					Witch.cursesUI = false;
					if (!Witch.witchUI_Item.NullOrAir())
						StorageManager.TryReturnItemToPlayer(ref Witch.witchUI_Item, Main.LocalPlayer, true);
				}
				else {
					//Talking to the Witch
					drawnUIData = new();

					#region Determine Text

					if (Witch.mouseRerollEnchantment) {
						if (Witch.rerollScale < 1f) {
							Witch.rerollScale += 0.02f;
						}
					}
					else if (Witch.rerollScale > 1f) {
						Witch.rerollScale -= 0.02f;
					}

					int num56 = 50;
					int num57 = 270;
					string text = "";
					if (!Witch.witchUI_Item.NullOrAir()) {
						if (Witch.rerollUI) {
							text = Lang.inter[46].Value + ": ";//Cost:
							int cost = 100000;
							if (Main.LocalPlayer.discountAvailable)
								cost *= (int)((double)cost * 0.8);

							cost = (int)((double)cost * Main.LocalPlayer.currentShoppingSettings.PriceAdjustment);

							string text2 = "";
							int num59 = 0;
							int num60 = 0;
							int num61 = 0;
							int num62 = 0;
							int num63 = cost;
							if (num63 < 1)
								num63 = 1;

							if (num63 >= 1000000) {
								num59 = num63 / 1000000;
								num63 -= num59 * 1000000;
							}

							if (num63 >= 10000) {
								num60 = num63 / 10000;
								num63 -= num60 * 10000;
							}

							if (num63 >= 100) {
								num61 = num63 / 100;
								num63 -= num61 * 100;
							}

							if (num63 >= 1)
								num62 = num63;

							if (num59 > 0)
								text2 = text2 + "[c/" + Colors.AlphaDarken(Colors.CoinPlatinum).Hex3() + ":" + num59 + " " + Lang.inter[15].Value + "] ";

							if (num60 > 0)
								text2 = text2 + "[c/" + Colors.AlphaDarken(Colors.CoinGold).Hex3() + ":" + num60 + " " + Lang.inter[16].Value + "] ";

							if (num61 > 0)
								text2 = text2 + "[c/" + Colors.AlphaDarken(Colors.CoinSilver).Hex3() + ":" + num61 + " " + Lang.inter[17].Value + "] ";

							if (num62 > 0)
								text2 = text2 + "[c/" + Colors.AlphaDarken(Colors.CoinCopper).Hex3() + ":" + num62 + " " + Lang.inter[18].Value + "] ";

							ItemSlot.DrawSavings(spriteBatch, num56 + 130, Main.instance.invBottom, horizontal: true);
							ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text2, new Vector2((float)(num56 + 50) + FontAssets.MouseText.Value.MeasureString(text).X, num57), Microsoft.Xna.Framework.Color.White, 0f, Vector2.Zero, Vector2.One);

							int num64 = num56 + 70;
							int num65 = num57 + 40;
							bool mouseHoveringReroll = MasterUIManager.NoUIBeingHovered && Main.mouseX > num64 - 15 && Main.mouseX < num64 + 15 && Main.mouseY > num65 - 15 && Main.mouseY < num65 + 15 && !PlayerInput.IgnoreMouseInterface;
							Texture2D reforgeHammerIconTexture = TextureAssets.Reforge[0].Value;
							if (mouseHoveringReroll) {
								MasterUIManager.UIBeingHovered = GetUI_ID(WE_UI_ID.WitchReroll);
								reforgeHammerIconTexture = TextureAssets.Reforge[1].Value;
							}

							spriteBatch.Draw(reforgeHammerIconTexture, new Vector2(num64, num65), null, Color.White, 0f, reforgeHammerIconTexture.Size() / 2f, Witch.rerollScale, SpriteEffects.None, 0f);
							UILinkPointNavigator.SetPosition(304, new Vector2(num64, num65) + reforgeHammerIconTexture.Size() / 4f);

							drawnUIData.hoveringReroll = mouseHoveringReroll;
							drawnUIData.cost = cost;
						}
						else if (Witch.cursesUI) {
							if (Witch.witchUI_Item?.ModItem is Enchantment enchantmentInUI) {
								if (enchantmentInUI.CanBeCursed) {
									//Draw Savings for Cursed Essence   TODO
									//Draw cursed essence cost          TODO


									text = Lang.inter[46].Value + ": ";//Cost:
								}
							}
							
						}
					}
					else {
						if (Witch.rerollUI) {
							text = GameMessageTextID.PlaceHereReRoll.ToString().Lang_WE(L_ID1.GameMessages);//Place an enchantment here to re-roll
						}
						else if (Witch.cursesUI) {
							text = GameMessageTextID.PlaceHereCurse.ToString().Lang_WE(L_ID1.GameMessages);//Place an enchantment here to apply a curse
						}
					}

					#endregion

					//Add Enchantment effects to text
					if (Witch.witchUI_Item?.ModItem is Enchantment enchantment) {
						text += "\n";
						IEnumerable<string> tooltips = enchantment.GetEffectsTooltips().Select(t => t.Item1);
						foreach (string tooltip in tooltips) {
							text += $"\n{tooltip}";
						}
					}

					//Draw Text
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(num56 + 50, num57), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One);
					
					//Item slot click interactions
					if (MasterUIManager.MouseHoveringItemSlot(num56, num57, GetUI_ID(WE_UI_ID.WitchReroll))) {
						if (Witch.rerollUI) {
							if (Main.mouseItem.NullOrAir() || Main.mouseItem?.ModItem is Enchantment enchantmentMouseItem && enchantmentMouseItem.IsRerollable) {
								MasterUIManager.ItemSlotClickInteractions(ref Witch.witchUI_Item);
							}
						}
						else if (Witch.cursesUI) {
							if (Main.mouseItem.NullOrAir() || Main.mouseItem?.ModItem is Enchantment enchantmentMouseItem && enchantmentMouseItem.CanBeCursed) {
								MasterUIManager.ItemSlotClickInteractions(ref Witch.witchUI_Item);
							}
						}
					}

					//Draw Item slot
					MasterUIManager.DrawItemSlot(spriteBatch, Witch.witchUI_Item, num56, num57);
				}
			}
			else {
				talkingToWitch = false;
			}
		}
		public static void UpdateInterface() {
			if (!talkingToWitch)
				return;

			if (Witch.rerollUI) {
				if (!Witch.witchUI_Item.NullOrAir()) {
					//Mouse hovering reroll?
					if (drawnUIData.hoveringReroll) {
						int cost = drawnUIData.cost;
						Main.hoverItemName = GameMessageTextID.ReRoll.ToString().Lang_WE(L_ID1.GameMessages);//Re-roll
						if (!Witch.mouseRerollEnchantment)
							SoundEngine.PlaySound(SoundID.MenuTick);

						Witch.mouseRerollEnchantment = true;
						Main.LocalPlayer.mouseInterface = true;

						if (Main.mouseLeftRelease && Main.mouseLeft && Main.LocalPlayer.CanAfford(cost) && Witch.witchUI_Item?.ModItem is Enchantment enchantment && enchantment.IsRerollable) {
							Main.LocalPlayer.BuyItem(cost);
							enchantment.ReRollStats();
							Witch.witchUI_Item.position.X = Main.LocalPlayer.position.X + (float)(Main.LocalPlayer.width / 2) - (float)(Witch.witchUI_Item.width / 2);
							Witch.witchUI_Item.position.Y = Main.LocalPlayer.position.Y + (float)(Main.LocalPlayer.height / 2) - (float)(Witch.witchUI_Item.height / 2);

							//Todo Popup text for this
							//PopupText.NewText(PopupTextContext.ItemReforge, Witch.rerollItem, Witch.rerollItem.stack, noStack: true);
							SoundEngine.PlaySound(SoundID.Item37);
						}
					}
					else {
						Witch.mouseRerollEnchantment = false;
					}
				}
			}
			else if (Witch.cursesUI) {

			}
		}
	}
}
