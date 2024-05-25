using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KokoLib.Emitters;
using KokoLib;
using KokoLib.Nets;
using Terraria;
using Terraria.ID;
using WeaponEnchantments.Common.Utility;
using WeaponEnchantments.Common.Globals;
using WeaponEnchantments.Debuffs;
using Terraria.ModLoader;
using WeaponEnchantments.UI;
using androLib.Common.Utility;
using androLib.Common.Globals;
using System.IO;

namespace WeaponEnchantments.ModLib.KokoLib
{
	public interface INetMethods
	{
		public void NetStrikeNPC(NPC npc, int damage, bool crit);
		public void NetDebuffs(NPC npc, int damage, float amaterasuStrength, Dictionary<short, int> debuffs, HashSet<short> dontDissableImmunitiy);
		public void NetActivateOneForAll(Dictionary<NPC, (int, bool)> oneForAllNPCDictionary);
		public void NetAddNPCValue(NPC npc, int value);
		public void NetResetWarReduction(NPC npc);
		public void NetOfferChestItems(SortedDictionary<int, SortedSet<short>> chestItems);
		public void NetResetEnchantedItemInChest(int chestNum, short index);
		public void NetAnglerQuestSwap();
		public void SyncCursedNPCData(NPCNetInfoCursedNPC npc);
		public void SyncCursedEssenceCount(int cursedEssenceCount);
	}
	public class NetManager : ModHandler<INetMethods>, INetMethods
	{
		public override INetMethods Handler => this;
		public void NetStrikeNPC(NPC npc, int damage, bool crit) {
			WEGlobalNPC.StrikeNPC(npc, damage, crit);
			if (Main.netMode == NetmodeID.Server) {
				Net.IgnoreClient = WhoAmI;
				Net<INetMethods>.Proxy.NetStrikeNPC(npc, damage, crit);
			}
		}
		public void NetDebuffs(NPC target, int damage, float amaterasuStrength, Dictionary<short, int> debuffs, HashSet<short> dontDissableImmunitiy) {
			target.HandleOnHitNPCBuffs(damage, amaterasuStrength, debuffs, dontDissableImmunitiy);

			if (Main.netMode == NetmodeID.Server) {
				Net.IgnoreClient = WhoAmI;
				Net<INetMethods>.Proxy.NetDebuffs(target, damage, amaterasuStrength, debuffs, dontDissableImmunitiy);
			}
		}
		public void NetActivateOneForAll(Dictionary<NPC, (int, bool)> oneForAllNPCDictionary) {
			foreach (NPC npc in oneForAllNPCDictionary.Keys) {
				WEGlobalNPC.StrikeNPC(npc, oneForAllNPCDictionary[npc].Item1, oneForAllNPCDictionary[npc].Item2);
			}

			if (Main.netMode == NetmodeID.Server) {
				Net.IgnoreClient = WhoAmI;
				Net<INetMethods>.Proxy.NetActivateOneForAll(oneForAllNPCDictionary);
			}
		}
		public void NetAddNPCValue(NPC npc, int value) {
			npc.AddValue(value);

			if (Main.netMode == NetmodeID.Server) {
				Net.IgnoreClient = WhoAmI;
				Net<INetMethods>.Proxy.NetAddNPCValue(npc, value);
			}
		}

		public void NetResetWarReduction(NPC npc) {
			if (!npc.TryGetWEGlobalNPC(out WEGlobalNPC weGlobalNPC))
				weGlobalNPC.ResetWarReduction();

			if (Main.netMode == NetmodeID.Server) {
				Net.IgnoreClient = WhoAmI;
				Net<INetMethods>.Proxy.NetResetWarReduction(npc);
			}
		}

		public void NetOfferChestItems(SortedDictionary<int, SortedSet<short>> chestItems) {
			if (Main.netMode == NetmodeID.Server)
				EnchantingTableUI.OfferChestItems(chestItems);
		}

		public void NetResetEnchantedItemInChest(int chestNum, short index) {
			if (Main.netMode == NetmodeID.Server)
				EnchantedItemStaticMethods.ResetEnchantedItemInChestFromNet(chestNum, index);
		}

		public void NetAnglerQuestSwap() {
			if (Main.netMode == NetmodeID.Server) {
				Main.AnglerQuestSwap();
				Net.IgnoreClient = WhoAmI;
				Net<INetMethods>.Proxy.NetAnglerQuestSwap();
			}
			else {
				QuestFish.PrintAnglerQuest();
			}
		}

		public void SyncCursedNPCData(NPCNetInfoCursedNPC npc) {
			if (Main.netMode == NetmodeID.Server) {
				//npc.SetStatsFromInfo();
				Net<INetMethods>.Proxy.SyncCursedNPCData(npc);
			}
			else {
				npc.SetStatsFromInfoCursedNPC();
			}
		}

		public void SyncCursedEssenceCount(int cursedEssenceCount) {
			if (Main.netMode == NetmodeID.Server) {
				Net.IgnoreClient = WhoAmI;
				Net<INetMethods>.Proxy.SyncCursedEssenceCount(cursedEssenceCount);
			}

			CurseAttractionNPC.UpdateCursedEssenceCount(WhoAmI, cursedEssenceCount);
		}
	}
	public class NetFunctions {
		public static void SyncCursedNPCData(int npcWhoAmI) => Net<INetMethods>.Proxy.SyncCursedNPCData(new NPCNetInfoCursedNPC(Main.npc[npcWhoAmI]));

		public static void SynchCursedEssenceCount(int cursedEssenceCount) => Net<INetMethods>.Proxy.SyncCursedEssenceCount(cursedEssenceCount);
	}
	public struct NPCNetInfoCursedNPC {
		public bool cursed;
		public bool spawnedByBoss;
		public int curseIndex = -1;
		public int whoAmI;
		public int lifeMax;
		public float npcSlots;
		public int damage;
		public int defDamage;
		public NPCNetInfoCursedNPC(NPC npc) {
			if (npc.TryGetGlobalNPC(out CursedNPC cursedNPC)) {
				cursed = cursedNPC.Cursed;
				spawnedByBoss = cursedNPC.SpawnedByBoss;

				if (!cursed)
					return;

				curseIndex = cursedNPC.curseEffectIndex;
			}

			whoAmI = npc.whoAmI;
			lifeMax = npc.lifeMax;
			npcSlots = npc.npcSlots;
			damage = npc.damage;
			defDamage = npc.defDamage;
		}
		public NPCNetInfoCursedNPC(BinaryReader reader) {
			cursed = reader.ReadBoolean();
			spawnedByBoss = reader.ReadBoolean();
			if (!cursed)
				return;

			curseIndex = reader.ReadInt32();
			whoAmI = reader.ReadInt32();
			lifeMax = reader.ReadInt32();
			npcSlots = reader.ReadInt32();
			damage = reader.ReadInt32();
			defDamage = reader.ReadInt32();
		}
		public void Write(BinaryWriter writer) {
			writer.Write(cursed);
			writer.Write(spawnedByBoss);
			if (!cursed)
				return;

			writer.Write(curseIndex);
			writer.Write(whoAmI);
			writer.Write(lifeMax);
			writer.Write(npcSlots);
			writer.Write(damage);
			writer.Write(defDamage);
		}
		public void SetStatsFromInfoCursedNPC() {
			NPC npc = Main.npc[whoAmI];
			if (npc.TryGetGlobalNPC(out CursedNPC cursedNPC)) {
				cursedNPC.Cursed = cursed;
				cursedNPC.SpawnedByBoss = spawnedByBoss;
				if (!cursed)
					return;

				cursedNPC.curseEffectIndex = curseIndex;
			}

			npc.lifeMax = lifeMax;
			npc.npcSlots = npcSlots;
			npc.damage = damage;
			npc.defDamage = defDamage;
		}
	}
}