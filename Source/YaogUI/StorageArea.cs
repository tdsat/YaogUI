using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld.UI.InGame;
using System.Collections.Generic;
using XiaWorld;
using XiaWorld.ThingStep;
using System.Linq;
using KTV;
using UnityEngine;

namespace YaogUI
{
	[HarmonyPatch(typeof(Wnd_StorageArea), "OnInit")]
	public static class TestShit
	{
		public static void Postfix(Wnd_StorageArea __instance)
		{
			try
			{
				var UI = __instance.UIInfo;
				var area = Traverse.Create(__instance).Field("area").GetValue<AreaStorage>();
				
				//UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				var clearAll = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				clearAll.name = "YaogUI.ClearAll";
				clearAll.visible = true;
				clearAll.text = TFMgr.Get("全部清除");

				// Place buttons
				clearAll.x = UI.width - 50;
				clearAll.y = 50;

				var clearItems = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				clearAll.name = "YaogUI.ClearItems";
				clearAll.visible = true;
				clearAll.text = TFMgr.Get("明确");

				clearItems.x = clearAll.x;
				clearItems.y = clearAll.y + 30;

				// Add callbacks
				clearAll.onClick.Add(delegate(EventContext context)
				{
					Main.Debug("All");
					for (g_emItemKind groupIndex = g_emItemKind.None; groupIndex < g_emItemKind.Count; ++groupIndex)
					{
						UI_Item_Storage group = UI.m_n25.GetChildAt((int) groupIndex) as UI_Item_Storage;
						group.m_title.selected = false;
						group.m_title.onClick.Call();
					}
				});

				clearItems.onClick.Add((e) =>
				{
					Main.Debug("Items");
				});

				UI.AddChild(clearAll);
				UI.AddChild(clearItems);
				
				// for (g_emItemKind itemKind = g_emItemKind.None; itemKind < g_emItemKind.Count; itemKind++)
				// {
				// 	UI_Item_Storage group = UI.m_n25.GetChildAt((int) itemKind) as UI_Item_Storage;
				// 	group.m_title.onClick.Add(delegate (EventContext context)
				// 	{
				// 		Main.Debug("Clicked button");
				// 		if (context.inputEvent.ctrl)
				// 		{
				// 			Main.Debug("Control was pressed");
				// 		}
				// 	});
				// }
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}
	}
}
