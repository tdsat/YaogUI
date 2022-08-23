using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld.UI.InGame;

namespace YaogUI
{
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnShowUpdate")]
	public static class AddQuickCategoryListToTradeWindow
	{
		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			try
			{
				GComponent categoryPanel;

				var tradeWindow = __instance;
				if (tradeWindow.UIInfo.GetChild("YaogUI.CategoryList") == null)
				{
					KLog.Dbg("[YaogUI] Panel not exist,adding");
					categoryPanel = (GComponent)UIPackage.CreateObjectFromURL("ui://m5coew5edgsub6");
					categoryPanel.name = "YaogUI.CategoryList";
					tradeWindow.UIInfo.AddChild(categoryPanel);
				} else {
					KLog.Dbg("[YaogUI] Panel already exists");
					categoryPanel = (GComponent)tradeWindow.UIInfo.GetChild("YaogUI.CategoryList");
				}
				var sellItemList = tradeWindow.UIInfo.m_rightitem;

                var categoryList = (GList)categoryPanel.GetChild("list");
                var items = sellItemList.GetChildren();
                categoryList.RemoveChildrenToPool();
                foreach (UI_TradeItem item in items)
                {
					var index = sellItemList.GetChildIndex(item);
					if (item.name == "ItemType")
                    {
                        var btn = (GButton)categoryList.AddItemFromPool();
                        btn.title = item.m_typename.text;
                        btn.height = 30;
                        btn.onClick.Set(() => sellItemList.ScrollToView(index, true, true));
                    }
                }
				categoryPanel.x = sellItemList.x + sellItemList.width;
				categoryPanel.y = sellItemList.y - 60;
				categoryPanel.height = categoryList.numItems * 30 + 50;
            }
			catch (Exception e)
			{
				KLog.Dbg("[YaogUI] error" + e.ToString(), new object[0]);
			}
		}
	}
}
