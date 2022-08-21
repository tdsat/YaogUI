using HarmonyLib;
using XiaWorld;

namespace YaogUI
{
	[HarmonyPatch(typeof(Npc), "ChangeToCorpse")]
	public static class ChangeVesselNameOnDeath
	{
		public static void Prefix(Npc __instance, bool isgod = false, bool nojiehui = false)
		{

			if (!__instance.IsCorpse && __instance.IsPuppet && !string.IsNullOrEmpty(__instance.Author) )
            {
				__instance.SetName(__instance.Author + "'s " + __instance.GetName());
			}
		}
	}
}
