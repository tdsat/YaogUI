using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld.UI.InGame;
using System.Collections.Generic;
using XiaWorld;

namespace YaogUI
{
	public static class TradeWindowFields
	{
		// Field Names (should replace with actual fields)
		public static readonly string SellSearchInput = "YaogUI.SellSearchInput";
		public static readonly string BuySearchInput = "YaogUI.BuySearchInput";
		public static readonly string CategoryPanel = "YaogUI.CategoryPanel";

		public static List<string> ignoreItemsList = new List<string>();
		public static TradePriceDef priceDef;
		public static ITradeItemData iData;
		public static bool ignoreWorthlessItems = false;
	}

	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnShowUpdate")]
	public static class AddQuickCategoryListToTradeWindow
	{
		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			try
			{
				UI_TradeCategoryList categoryPanel;

				var tradeWindow = __instance;
				if (tradeWindow.UIInfo.GetChild(TradeWindowFields.CategoryPanel) == null)
				{
					categoryPanel = UI_TradeCategoryList.CreateInstance();
					categoryPanel.name = TradeWindowFields.CategoryPanel;
					tradeWindow.UIInfo.AddChild(categoryPanel);
				}
				else
				{
					categoryPanel = (UI_TradeCategoryList)tradeWindow.UIInfo.GetChild(TradeWindowFields.CategoryPanel);
				}
				tradeWindow.UIInfo.GetChild(TradeWindowFields.SellSearchInput).visible = true;
				tradeWindow.UIInfo.GetChild(TradeWindowFields.BuySearchInput).visible = true;
				categoryPanel.visible = true;
				var sellItemList = tradeWindow.UIInfo.m_rightitem;

				var categoryList = (GList)categoryPanel.GetChild("list");
				var items = sellItemList.GetChildren();
				categoryList.RemoveChildrenToPool();

				// Add link to spirit stone
				var btn = (GButton)categoryList.AddItemFromPool();
				btn.title = TFMgr.Get("灵石");
				btn.height = 30;
				btn.onClick.Set(() => sellItemList.ScrollToView(0, true, true));
				foreach (UI_TradeItem item in items)
				{
					var index = sellItemList.GetChildIndex(item);
					if (item.name == "ItemType")
					{
						btn = (GButton)categoryList.AddItemFromPool();
						btn.title = item.m_typename.text;
						btn.height = 30;
						btn.onClick.Set(() => sellItemList.ScrollToView(index, true, true));
					}
				}
				categoryPanel.x = sellItemList.x + sellItemList.width;
				categoryPanel.y = sellItemList.y - 60;
				categoryPanel.height = categoryList.numItems * 30 + 65;
                categoryPanel.m_hideWorthlessCheckbox.onClick.Add(e => {
					TradeWindowFields.ignoreWorthlessItems = ((GButton)e.sender).selected;
					AddTradeWindowSellItemSearch.FilterSellList(); 
				});

				// Build a local cache of worthless items.
				var saleList = Traverse.Create(tradeWindow).Field("saleList").GetValue<TradeSaleList>();
				var rightTree = Traverse.Create(saleList).Field("rightTree").GetValue<TreeView>();
				var root = rightTree.root;
				for (int i = 0; i < root.numChildren; i++)
				{
					TreeNode folder = root.GetChildAt(i);
					for (int j = 0; j < folder.numChildren; j++)
					{
						TreeNode itemNode = folder.GetChildAt(j);
						TradeSaleList.NodeData nodeData = saleList.TNode2NodeData(itemNode);
						var itemName = itemNode.data as string;
						TradePrice salePrice = TradeWindowFields.priceDef.GetItemPrice(nodeData.ItemName, nodeData.Rate).SalePrice;
						var finalPrice = TradeWindowFields.iData.ScaleSalePrice(salePrice.Value, nodeData.ItemName);
						if (finalPrice < 1)
						{
							TradeWindowFields.ignoreItemsList.Add(nodeData.ItemName);
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
	public static class AddTradeWindowSellItemSearch
	{
		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			try
			{
				var tradeWindow = __instance;
				var searchInput = UI_ClearableInput.CreateInstance();
                var clearSearchBtn = searchInput.m_clearButton;
                searchInput.name = TradeWindowFields.SellSearchInput;

                var list = tradeWindow.UIInfo.m_rightitem;
				list.foldInvisibleItems = true;

				searchInput.x = list.x - 10;
				searchInput.y = list.y - 40;
				searchInput.width = list.width;

                searchInput.onKeyDown.Add(e => { FilterSellList(); });
                tradeWindow.onRemovedFromStage.Add(e => ClearSellSearch());
                clearSearchBtn.onClick.Add(e => ClearSellSearch());
                tradeWindow.UIInfo.m_n51.onClickItem.Add(e => ClearSellSearch());

                tradeWindow.UIInfo.AddChild(searchInput);
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}

		public static void FilterSellList()
		{
			var tradeWindow		= Wnd_SchoolTrade.Instance;
			var list			= tradeWindow.UIInfo.m_rightitem;
            var items			= list.GetChildren();
            var searchText		= tradeWindow.UIInfo.GetChild(TradeWindowFields.SellSearchInput).text;
            var categoryPanel	= (UI_TradeCategoryList)tradeWindow.UIInfo.GetChild(TradeWindowFields.CategoryPanel);
			var ignoreWorthlessItems = categoryPanel.m_hideWorthlessCheckbox.selected;

            // Meh... this can be simplified but w/e
            var callbacks = new List<Func<UI_TradeItem, bool>>
            {
                item => item.m_itemname.text.ToLower().Contains(searchText.ToLower())
            };
            if (ignoreWorthlessItems)
            {
                callbacks.Add(item => !TradeWindowFields.ignoreItemsList.Contains(item.name));
            }

            foreach (UI_TradeItem item in items)
            {
                item.visible = callbacks.TrueForAll(x => x(item));
            }
        }

		private static void ClearSellSearch()
		{
			var searchField = Wnd_SchoolTrade.Instance.UIInfo.GetChild(TradeWindowFields.SellSearchInput);
            searchField.text = "";
            FilterSellList();
        }

	}

	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnInit")]
	public static class AddTradeWindowBuyItemSearch
	{
		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			try
			{
				var tradeWindow = __instance;
				var searchInput = UI_ClearableInput.CreateInstance();
				var clearSearchBtn = searchInput.m_clearButton;
				searchInput.name = TradeWindowFields.BuySearchInput;

				var list = tradeWindow.UIInfo.m_leftitem;
				list.foldInvisibleItems = true;

				searchInput.x = list.x - 10;
				searchInput.y = list.y - 40;
				searchInput.width = list.width;

				searchInput.onKeyDown.Add(e => FilterBuyList());
				tradeWindow.onRemovedFromStage.Add(e => ClearBuySearch());
				clearSearchBtn.onClick.Add(e => ClearBuySearch());
				// Search again when switching schools since items have changed
				tradeWindow.UIInfo.m_n51.onClickItem.Add(e => FilterBuyList());

				tradeWindow.UIInfo.AddChild(searchInput);
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}

		public static void FilterBuyList()
		{
			var tradeWindow = Wnd_SchoolTrade.Instance;
			var list = tradeWindow.UIInfo.m_leftitem;
			var items = list.GetChildren();
			var searchText = tradeWindow.UIInfo.GetChild(TradeWindowFields.BuySearchInput).text;
			Main.Debug(searchText);

			foreach (UI_TradeItem item in items)
			{
				item.visible = item.m_typename.text == "ItemType" || item.m_itemname.text.ToLower().Contains(searchText.ToLower());
			}
		}

		private static void ClearBuySearch(string searchText = "")
		{
			var searchField = Wnd_SchoolTrade.Instance.UIInfo.GetChild(TradeWindowFields.BuySearchInput);
			searchField.text = searchText;
			FilterBuyList();
		}

	}

	// Needed to get the values of priceDef and iData. Can't simply Traverse because they seem to be null
	// when we need/use them
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "ShowSchool")]
	public static class StorePriceDefForSchoolTrade
	{
		public static void Prefix(int school)
		{
			if (TradeWnd.HasTradeArea())
			{
				SchoolTradeDef schoolDef = TradeMgr.Instance.GetSchoolTradeDef(school);
				TradeWindowFields.priceDef = TradeMgr.Instance.GetPriceDef(schoolDef.Price);
				TradeWindowFields.iData = TradeMgr.Instance.GetSchoolTrade(school);
			}
		}
	}

	// Needed to get the values of priceDef and iData. Can't simply Traverse because they seem to be null
	// when we need/use them
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "ShowWalkTrader")]
	public static class StorePriceDefForTrader
	{
		public static void Prefix(string walker, Npc npc = null)
		{
			if (TradeWnd.HasTradeArea())
			{
				TradeWalkDef tradeDef = TradeMgr.Instance.GetWalkTradeDef(null);
				TradeWindowFields.priceDef = TradeMgr.Instance.GetPriceDef(tradeDef.Price);
				TradeWindowFields.iData = TradeMgr.Instance.WalkTrader.GetTrader(walker);
			}
		}
	}

	// Needed so that components get hidden when user accepts trade. There might be a better way to acheive this
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "__selectyes")]
	public static class HideTradeComponents
	{
		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			if (__instance.UIInfo.m_state.selectedIndex == 1)
            {
				__instance.UIInfo.GetChild(TradeWindowFields.CategoryPanel).visible = false;
				__instance.UIInfo.GetChild(TradeWindowFields.SellSearchInput).visible = false;
				__instance.UIInfo.GetChild(TradeWindowFields.BuySearchInput).visible = false;
			}

		}
	}
}
