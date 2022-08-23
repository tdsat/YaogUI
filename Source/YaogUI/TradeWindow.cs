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
					categoryPanel = (GComponent)UIPackage.CreateObjectFromURL("ui://m5coew5edgsub6");
					categoryPanel.name = "YaogUI.CategoryList";
					tradeWindow.UIInfo.AddChild(categoryPanel);
				} else {
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

	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnInit")]
	public static class AddTradeWindowSellItemSearch
	{
		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			try
			{
				var tradeWindow = __instance;
				var searchInput = (GLabel)UIPackage.CreateObjectFromURL("ui://ncbwb41mv6072k");
				var clearSearchBtn = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv6076");
				searchInput.name = "YaogUI.SearchInput";
				clearSearchBtn.name = "YaogUI.ClearSearchInput";

				var list = tradeWindow.UIInfo.m_rightitem;
				list.foldInvisibleItems = true;

				searchInput.x = list.x - 10;
				searchInput.y = list.y - 40;
				searchInput.width = list.width - 40;
				clearSearchBtn.x = searchInput.x + searchInput.width;
				clearSearchBtn.y = searchInput.y - 1;
				clearSearchBtn.text = "Clear";
				searchInput.onKeyDown.Add(e => {
					var input = (InputTextField)e.initiator;
					filterList(
						list,
						item => item.m_typename.text == "ItemType" || item.m_itemname.text.ToLower().Contains(input.text.ToLower())
					);
				});
				tradeWindow.onRemovedFromStage.Add(e => clearSearch(list, searchInput));
				clearSearchBtn.onClick.Add(e => clearSearch(list, searchInput));

				tradeWindow.UIInfo.AddChild(searchInput);
				tradeWindow.UIInfo.AddChild(clearSearchBtn);
				tradeWindow.UIInfo.m_n51.onClickItem.Add(e => clearSearch(list, searchInput));
			}
			catch (Exception e)
			{
				KLog.Dbg("[YaogUI] error" + e.ToString(), new object[0]);
			}
		}
      
		private static void filterList(GList list, Func<UI_TradeItem, bool> searchCallback )
        {
			var items = list.GetChildren();
			foreach (UI_TradeItem item in items)
			{
				item.visible = searchCallback(item);
			}
		}

		private static void clearSearch(GList list, GLabel searchField)
        {
			searchField.text = "";
			filterList(list, item => true);
		}
	}
}
