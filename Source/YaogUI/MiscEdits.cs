using HarmonyLib;
using System;
using FairyGUI;
using XiaWorld;

namespace YaogUI
{
	[HarmonyPatch(typeof(Wnd_GameMain), "UpdateStuffList")]
	public static class DoubleMaterialRows
	{
		public static void Prefix(Wnd_GameMain __instance, GButton gButton, UIMainMenuListDef_Data data)
		{
			try
			{
				var materialList = __instance.UIInfo.m_MainList.m_StuffList;
				var listBg = __instance.UIInfo.m_MainList.m_n9;
				materialList.layout = ListLayoutType.FlowHorizontal;
				
				ThingDef def3 = ThingMgr.Instance.GetDef(g_emThingType.Building, data.ObjName);
				if (def3.Building.BeMade.CostItems != null && def3.Building.BeMade.CostItems.Count > 0)
				{ // Dual-material thing
					listBg.width = 220;
					materialList.width = 255;
					materialList.columnGap = 65;
				}
				else
				{
					listBg.width = 120;
					materialList.width = 130;
					materialList.columnGap = 0;
				}
				materialList.x = listBg.x + 5;
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