using HarmonyLib;
using System;
using System.Collections.Generic;
using FairyGUI;
using XiaWorld;
using XiaWorld.UI.InGame;

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
				var thingDefList = ThingMgr.Instance.GetBuildingAllStuff(def3.Name);
				if (thingDefList == null || thingDefList.Count <= 0)
				{
					listBg.visible = false;
					return;
				}

				// This is some shit code but I can't be bothered tbh...
				listBg.visible = true;
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

	[HarmonyPatch]
	public static class ProduceShortcuts
	{
		[HarmonyPatch(typeof(Wnd_BuildingProduce), "__clickSelectItem")]
		[HarmonyPostfix]
		public static void HandleShortcuts(Wnd_BuildingProduce __instance, EventContext context)
		{
			BuildingThing building = Traverse.Create(__instance).Field("Building").GetValue<BuildingThing>();
		
			var count = 1;
			if (context.inputEvent.shift)
			{
				count = 10;
			} else if (context.inputEvent.ctrl)
			{
				count = 50;
			}
		
			if (building.ProduceMachine == null) return;
			var index = building.ProduceMachine.m_lisProduceList?.Count - 1;
			
			building.ProduceMachine.LoopTask(index ?? 0, count);
		}

		[HarmonyPatch(typeof(Wnd_BuildingProduce), "UpdateSelectList")]
		[HarmonyPostfix]
		public static void UpdateTooltips(Wnd_BuildingProduce __instance)
		{
			List<BuildingProduce.BuildingProduceData> lisProduceMenu = Traverse.Create(__instance).Field("Building")
				.GetValue<BuildingThing>().ProduceMachine.m_lisProduceMenu;
			for (int index = 0; index < lisProduceMenu.Count; ++index)
			{
				var button = __instance.UIInfo.m_SelectList.GetChildAt(index);
				button.tooltips += "[color=#9e6404]\n\nShift：x10\nCtrl：x50[/color]";
			}
		}
	}
}