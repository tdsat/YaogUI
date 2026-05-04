using System;
using HarmonyLib;
using XiaWorld.UI.InGame;

namespace YaogUI
{
	public static class SchoolGiftSearch
	{
		public static UI_ClearableInput giftInputSearch;

		public static void FilterSellList()
		{
			var tradeWindow = Wnd_SchoolGiveGift.Instance;
			var list = tradeWindow.UIInfo.m_rightitem;
			var items = list.GetChildren();
			var searchText = giftInputSearch.text;

			foreach (UI_TradeItem item in items)
			{
				item.visible = item.m_itemname.text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1;
			}
		}

		public static void ClearSellSearch()
		{
			var searchField = giftInputSearch;
			searchField.text = "";
			FilterSellList();
		}

		[HarmonyPatch]
		public static class AddGiftSearch
		{
			[HarmonyPatch(typeof(Wnd_SchoolGiveGift), "OnInit")]
			[HarmonyPostfix]
			public static void Postfix(Wnd_SchoolGiveGift __instance)
			{
				giftInputSearch = UI_ClearableInput.CreateInstance();
				giftInputSearch.name = "YaogUI.GiftSearchInput";
				var searchInput = giftInputSearch;
				var clearSearchBtn = searchInput.m_clearButton;

				var list = __instance.UIInfo.m_rightitem;
				list.foldInvisibleItems = true;

				searchInput.x = list.x + 5;
				searchInput.y = list.y - 70;
				searchInput.width = list.width;

				searchInput.onKeyDown.Add(FilterSellList);
				clearSearchBtn.onClick.Add(ClearSellSearch);

				__instance.UIInfo.AddChild(searchInput);
				searchInput.GetTextField().RequestFocus();
			}
			
			[HarmonyPatch(typeof(Wnd_SchoolGiveGift), "ClickYes")]
			[HarmonyPostfix]
			public static void HideSearchField2(Wnd_SchoolGiveGift __instance)
			{
				if (!Traverse.Create(__instance).Field("saleList").Method("IsSelectEmpty").GetValue<bool>())
					giftInputSearch.visible = false;
			}
		
			[HarmonyPatch(typeof(Wnd_SchoolGiveGift), "OnShowUpdate")]
			[HarmonyPostfix]
			public static void ShowField(Wnd_SchoolGiveGift __instance)
			{
				giftInputSearch.visible = true;
				giftInputSearch.GetTextField().RequestFocus();
			}
		}
	}
}