using androLib;
using androLib.Common.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Effects;

namespace WeaponEnchantments.Common.Globals
{
	public class WEGlobalItem : GlobalItem
	{
		public override void GrabRange(Item item, Player player, ref int grabRange) {
			if (player?.TryGetModPlayer(out WEPlayer wePlayer) == true) {
				if (wePlayer.CheckEnchantmentStats(EnchantmentStat.PickupRange, out float add))
					grabRange = (int)Math.Round((float)grabRange + add);
			}
		}
		public override bool GrabStyle(Item item, Player player) {
			if (player?.TryGetModPlayer(out WEPlayer wePlayer) == true && item.playerIndexTheItemIsReservedFor == player.whoAmI) {
				if (wePlayer.CheckEnchantmentStats(EnchantmentStat.ItemAttractionAndPickupSpeed, out float mult, 1f)) {
					return VectorMath.CalculateHomingVelocity(
						() => player.Center,
						(Vector2 targetCenter) => !player.TryGetModPlayer(out StoragePlayer storagePlayer) || targetCenter != storagePlayer.CenterBeforeMoveUpdate ? player.velocity : Vector2.Zero,
						() => Math.Max(item.velocity.Length(), player.velocity.Length() * 1.1f) * (mult - 1f),//Make sure it's base speed is at least 10% faster than the player.
						() => item.Center,
						(Vector2 velocity) => { item.position += velocity; return; },
						(Vector2 center) => { item.Center = center; return; }
					);
				}

				return false;
			}

			return false;
		}
	}
}
