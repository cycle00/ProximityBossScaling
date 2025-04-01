using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace ProximityBossScaling {
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class ProximityBossScaling : Mod {
		
		public override void Load() {
			IL_NPC.ScaleStats += ScaleStats_Hook;
			IL_NPC.NewNPC += NPCPosition_Hook;
			Logger.Info("Proximity Boss Scaling v0.1 has been loaded.");
		}

		private static void ScaleStats_Hook(ILContext il) {
			try {
				ILCursor cursor = new ILCursor(il);
				ILLabel label = il.DefineLabel();
				cursor.GotoNext(i => i.MatchLdcI4(1));
				cursor.Index += 3;

				cursor.EmitLdarg0();
				cursor.EmitDelegate<Func<NPC, int>>(npc => {
					int num = 0;
					foreach (var player in Main.ActivePlayers) {
						if ((int)Math.Abs(npc.position.X - player.position.X) < (int)((double)NPC.sWidth * 2.1) && (int)Math.Abs(npc.position.Y - player.position.Y) < (int)((double)NPC.sHeight * 2.1)) {
							num++;
						}
					}
					return num;
				});
				cursor.EmitBr(label);
				
				cursor.GotoNext(i => i.MatchDup());
				cursor.MarkLabel(label);

#pragma warning disable CS0168
			} catch (Exception e) {
#pragma warning restore
				MonoModHooks.DumpIL(ModContent.GetInstance<ProximityBossScaling>(), il); // bugged in windows lmfao
			}
		}

		private static void NPCPosition_Hook(ILContext il) {
			try {
				ILCursor cursor = new ILCursor(il);
				ILLabel beforeSetDefaults = cursor.DefineLabel();
				ILLabel afterSetDefaults = cursor.DefineLabel();
				ILLabel afterPosition = cursor.DefineLabel();

				cursor.GotoNext(i => i.MatchStelemRef());
				cursor.Index++;
				cursor.EmitBr(afterSetDefaults);
				cursor.MarkLabel(beforeSetDefaults);

				cursor.Index += 8;
				cursor.EmitBr(afterPosition);
				cursor.MarkLabel(afterSetDefaults);

				cursor.Index += 34;
				cursor.EmitBr(beforeSetDefaults);
				cursor.MarkLabel(afterPosition);

#pragma warning disable CS0168
			} catch (Exception e) {
#pragma warning restore
				MonoModHooks.DumpIL(ModContent.GetInstance<ProximityBossScaling>(), il);
			}
		}
	}
}
