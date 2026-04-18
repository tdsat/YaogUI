using HarmonyLib;
using System;
using FairyGUI;

namespace YaogUI
{
	[HarmonyPatch(typeof(Wnd_GameMain), "OnInit")]
	public static class DoubleMaterialRows
	{
		public static void Postfix(Wnd_GameMain __instance)
		{
			try
			{
				var materialList = __instance.UIInfo.m_MainList.m_StuffList;
				var listBg = __instance.UIInfo.m_MainList.m_n9;
				materialList.columnCount = 2;
				materialList.layout = ListLayoutType.FlowVertical;
				listBg.width = 115;
				materialList.x = listBg.x;
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}
	}
	
	[HarmonyPatch(typeof(Wnd_SelectNpc), "OnShowUpdate")]
	public static class AutoSelectSingleNpc
	{
		public static void Postfix(Wnd_SelectNpc __instance)
		{
			// When picking an NPC for an action, if there's only one option, auto-select it
			// This is to help with some annoying interaction where it doesn't really matter
			// who you pick (like the Stella puzzles or when inspecting decorations)
			if (__instance.UIInfo.m_n25.numItems == 1)
			{
				__instance.UIInfo.m_n25.GetChildAt(0).onClick.Call();
			}
		}
	}
}