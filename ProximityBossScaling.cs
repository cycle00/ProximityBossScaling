using System;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace ProximityBossScaling {
	public class ProximityBossScaling : Mod {
		
		public override void Load() {
			IL_NPC.ScaleStats += ScaleStats_Hook;
			IL_NPC.NewNPC += NPCPosition_Hook;
			Logger.Info("Proximity Boss Scaling v1.1 has been loaded.");
		}

		private static void ScaleStats_Hook(ILContext il) {
			if (Main.netMode != 0) { // no point in doing any of this if player is in singleplayer
				try {
					ILCursor cursor = new ILCursor(il);
					ILLabel label = il.DefineLabel();
					cursor.GotoNext(i => i.MatchLdcI4(1));
					cursor.Index += 3; // place cursor right before the code which pushes the number of total players to the stack

					cursor.EmitLdarg0(); // push a the current NPC instance onto the stack since next function call requires the current instance
					cursor.EmitDelegate<Func<NPC, int>>(npc => {
						int num = 0;
						foreach (var player in Main.ActivePlayers) {
							if ((int)Math.Abs(npc.position.X - player.position.X) < (int)((double)NPC.sWidth * 2.1) && (int)Math.Abs(npc.position.Y - player.position.Y) < (int)((double)NPC.sHeight * 2.1)) {
								num++;
							}
						}
						return num;
					}); // push the detected number of players to the stack
					cursor.EmitBr(label); // skip the original code which pushes the total number of players to the stack
					
					cursor.GotoNext(i => i.MatchDup());
					cursor.MarkLabel(label);

#pragma warning disable CS0168
				} catch (Exception e) {
#pragma warning restore
					MonoModHooks.DumpIL(ModContent.GetInstance<ProximityBossScaling>(), il); // bugged in windows lmfao
				}
			}
		}

		private static void NPCPosition_Hook(ILContext il) { 
			if (Main.netMode != 0) {	
				// make the NPC get positioned before SetDefaults gets run so that ScaleStats has access to the NPC's spawn position
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

					// jump nonsense: jump to after SetDefaults -> execute all code until end of positioning 
					// -> jump to start of SetDefaults -> call SetDefaults -> jump to after positioning
#pragma warning disable CS0168
				} catch (Exception e) {
#pragma warning restore
					MonoModHooks.DumpIL(ModContent.GetInstance<ProximityBossScaling>(), il);
				}
			}
		}
	}
}
