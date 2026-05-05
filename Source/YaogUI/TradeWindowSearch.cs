using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld.UI.InGame;
using System.Collections.Generic;
using XiaWorld;
using System.Linq;

namespace YaogUI
{
	public static class TradeWindowSearch
	{
		public static List<string> ignoreItemsList = new List<string>();
		public static TradePriceDef priceDef;
		public static UI_ClearableInput sellSearchInput;
		public static UI_ClearableInput buySearchInput;
		public static UI_TradeCategoryList categoryList;

		public static bool ignoreWorthlessItems;

		public static void CleanUp()
		{
			if (sellSearchInput != null)
			{
				//Clear these events because there's weird case where they still trigger even if the input is not visible
				sellSearchInput.onKeyDown.Clear();
				sellSearchInput.visible = false;
			}

			if (buySearchInput != null)
			{
				buySearchInput.onKeyDown.Clear();
				buySearchInput.visible = false;
			}

			if (categoryList != null)
			{
				categoryList.visible = false;
			}

			// Not sure if it's a good idea doing this here. Will keep an eye...
			if (AutoBalance.balanceLeftBtn != null)
				AutoBalance.balanceLeftBtn.visible = false;
			if (AutoBalance.balanceRightBtn != null)
				AutoBalance.balanceRightBtn.visible = false;
		}

		public static void FilterSellList()
		{
			var tradeWindow = Wnd_SchoolTrade.Instance;
			var list = tradeWindow.UIInfo.m_rightitem;
			var items = list.GetChildren();
			var searchText = TradeWindowSearch.sellSearchInput.text.ToLower();

			// Meh... this can be simplified but w/e
			var callbacks = new List<Func<UI_TradeItem, bool>>
			{
				item => item.m_itemname.text.ToLower().Contains(searchText)
			};
			if (TradeWindowSearch.ignoreWorthlessItems)
			{
				callbacks.Add(item => !TradeWindowSearch.ignoreItemsList.Contains(item.name));
			}

			foreach (UI_TradeItem item in items)
			{
				item.visible = callbacks.TrueForAll(x => x(item));
			}
		}

		public static void ClearSellSearch()
		{
			var searchField = TradeWindowSearch.sellSearchInput;
			searchField.text = "";
			FilterSellList();
		}

		public static void FilterBuyList()
		{
			var tradeWindow = Wnd_SchoolTrade.Instance;
			var list = tradeWindow.UIInfo.m_leftitem;
			var items = list.GetChildren();
			var searchText = TradeWindowSearch.buySearchInput.text.ToLower();

			foreach (UI_TradeItem item in items)
			{
				item.visible = item.m_typename.text == "ItemType" ||
				               item.m_itemname.text.ToLower().Contains(searchText);
			}
		}

		public static void ClearBuySearch()
		{
			TradeWindowSearch.buySearchInput.text = "";
			FilterBuyList();
		}
	}

	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnShowUpdate")]
	public static class AddQuickCategoryListToTradeWindow
	{
		[HarmonyPostfix]
		public static void UpdateCategoryListItems(Wnd_SchoolTrade __instance)
		{
			try
			{
				var categoryPanel = TradeWindowSearch.categoryList;

				TradeWindowSearch.sellSearchInput.visible = true;
				TradeWindowSearch.buySearchInput.visible = true;
				// Re-attach keydown events
				TradeWindowSearch.sellSearchInput.onKeyDown.Add(TradeWindowSearch.FilterSellList);
				TradeWindowSearch.buySearchInput.onKeyDown.Add(TradeWindowSearch.FilterBuyList);
				
				categoryPanel.visible = true;
				var sellItemList = __instance.UIInfo.m_rightitem;

				var categoryList = categoryPanel.m_list;
				var items = sellItemList.GetChildren();
				categoryList.RemoveChildrenToPool();
				// Add link to spirit stone
				var btn = (GButton)categoryList.AddItemFromPool();
				btn.title = TFMgr.Get("灵石");
				btn.height = 30;
				btn.onClick.Add(() => sellItemList.ScrollToView(0, true, true));
				foreach (var o in items)
				{
					var item = (UI_TradeItem)o;
					if (item == null || item.name != "ItemType") continue;
					var index = sellItemList.GetChildIndex(item);
					btn = (GButton)categoryList.AddItemFromPool();
					btn.title = item.m_typename.text;
					btn.height = 30;
					btn.onClick.Set(() => sellItemList.ScrollToView(index, true, true));
				}

				categoryPanel.height = categoryList.numItems * 30 + 65;
				categoryPanel.m_hideWorthlessCheckbox.onClick.Add(e =>
				{
					TradeWindowSearch.ignoreWorthlessItems = ((GButton)e.sender).selected;
					TradeWindowSearch.FilterSellList();
				});
				TradeWindowSearch.ignoreWorthlessItems = categoryPanel.m_hideWorthlessCheckbox.selected;
				TradeWindowSearch.ignoreItemsList.Clear();
				// Build a local cache of worthless items.
				var saleList = __instance.GetParts()[3] as TradeSaleList;
				var rightTree = Traverse.Create(saleList).Field("rightTree").GetValue<TreeView>();
				var root = rightTree.root;
				var iData = Traverse.Create(__instance).Field("_iData").GetValue<ITradeItemData>();

				for (int i = 0; i < root.numChildren; i++)
				{
					var folder = root.GetChildAt(i);
					for (int j = 0; j < folder.numChildren; j++)
					{
						TreeNode itemNode = folder.GetChildAt(j);
						TradeSaleList.NodeData nodeData = saleList.TNode2NodeData(itemNode);
						var itemName = itemNode.data as string;

						TradePrice tradeValue = TradeWindowSearch.priceDef
							.GetItemPrice(nodeData.ItemName, nodeData.Rate).SalePrice;
						var finalPrice = iData.ScaleSalePrice(tradeValue.Value, nodeData.ItemName);
						// Main.Debug($"Final price for {nodeData.ItemName} with value of {finalPrice}");

						if (finalPrice < 1)
						{
							TradeWindowSearch.ignoreItemsList.Add(nodeData.ItemName);
						}

						// While we're at it, also sort the items in each folder by name
						if (j > 0)
						{
							int k = j;
							do
							{
								TreeNode prevNode = folder.GetChildAt(--k);
								var prevItemName = prevNode.data as string;
								if (string.Compare(itemName, prevItemName) < 0) folder.SwapChildren(itemNode, prevNode);
							} while (k > 0);
						}
					}
				}
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}
	}

	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnInit")]
	public static class AddSearchFields
	{
		[HarmonyPostfix]
		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			try
			{
				AddTradeWindowSellItemSearch(__instance);
				AddTradeWindowBuyItemSearch(__instance);
				AddCategoryPanel(__instance);
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}

		public static void AddCategoryPanel(Wnd_SchoolTrade tradeWindow)
		{
			TradeWindowSearch.categoryList = UI_TradeCategoryList.CreateInstance();
			TradeWindowSearch.categoryList.name = "YaogUI.CategoryPanel";
			TradeWindowSearch.categoryList.visible = true;
			tradeWindow.UIInfo.AddChild(TradeWindowSearch.categoryList);
			// Put it next to the sell list
			TradeWindowSearch.categoryList.x = tradeWindow.UIInfo.m_rightitem.x + tradeWindow.UIInfo.m_rightitem.width;
			TradeWindowSearch.categoryList.y = tradeWindow.UIInfo.m_rightitem.y - 60;
		}


		public static void AddTradeWindowSellItemSearch(Wnd_SchoolTrade tradeWindow)
		{
			TradeWindowSearch.sellSearchInput = UI_ClearableInput.CreateInstance();
			TradeWindowSearch.sellSearchInput.name = "YaogUI.SellSearchInput";
			var searchInput = TradeWindowSearch.sellSearchInput;
			var clearSearchBtn = searchInput.m_clearButton;

			var list = tradeWindow.UIInfo.m_rightitem;
			list.foldInvisibleItems = true;

			searchInput.x = list.x - 10;
			searchInput.y = list.y - 40;
			searchInput.width = list.width;

			searchInput.onKeyDown.Add(TradeWindowSearch.FilterSellList);
			clearSearchBtn.onClick.Add(TradeWindowSearch.ClearSellSearch);
			// Search again when switching schools since items have changed
			tradeWindow.UIInfo.m_n51.onClickItem.Add(TradeWindowSearch.ClearSellSearch);

			tradeWindow.UIInfo.AddChild(searchInput);
		}

		public static void AddTradeWindowBuyItemSearch(Wnd_SchoolTrade tradeWindow)
		{
			TradeWindowSearch.buySearchInput = UI_ClearableInput.CreateInstance();
			TradeWindowSearch.buySearchInput.name = "YaogUI.BuySearchInput";
			var searchInput = TradeWindowSearch.buySearchInput;
			var clearSearchBtn = searchInput.m_clearButton;

			var list = tradeWindow.UIInfo.m_leftitem;
			list.foldInvisibleItems = true;

			searchInput.x = list.x - 10;
			searchInput.y = list.y - 40;
			searchInput.width = list.width;

			searchInput.onKeyDown.Add(TradeWindowSearch.FilterBuyList);
			clearSearchBtn.onClick.Add(TradeWindowSearch.ClearBuySearch);
			// Search again when switching schools since items have changed
			tradeWindow.UIInfo.m_n51.onClickItem.Add(TradeWindowSearch.FilterBuyList);

			tradeWindow.UIInfo.AddChild(searchInput);
		}
	}

	// Needed to get the values of priceDef. Can't simply Traverse because they seem to be null
	// when we need/use them
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "ShowSchool")]
	public static class StorePriceDefForSchoolTrade
	{
		public static void Prefix(int school)
		{
			if (TradeWnd.HasTradeArea())
			{
				SchoolTradeDef schoolDef = TradeMgr.Instance.GetSchoolTradeDef(school);
				TradeWindowSearch.priceDef = TradeMgr.Instance.GetPriceDef(schoolDef.Price);
			}
		}
	}

	// Needed to get the values of priceDef. Can't simply Traverse because they seem to be null
	// when we need/use them
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "ShowWalkTrader")]
	public static class StorePriceDefForTrader
	{
		public static void Prefix(string walker, Npc npc = null)
		{
			if (TradeWnd.HasTradeArea())
			{
				TradeWalkDef tradeDef = TradeMgr.Instance.GetWalkTradeDef(null);
				TradeWindowSearch.priceDef = TradeMgr.Instance.GetPriceDef(tradeDef.Price);
			}
		}
	}

	// Needed so that components get hidden when user accepts trade. There might be a better way to achieve this
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "__selectyes")]
	public static class HideTradeComponents
	{
		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			if (__instance.UIInfo.m_state.selectedIndex == 1)
			{
				TradeWindowSearch.CleanUp();
			}
		}
	}
}