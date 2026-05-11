using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld.UI.InGame;
using System.Collections.Generic;
using XiaWorld;
using System.Linq;

namespace YaogUI
{
	public class TradeWindowSearch : UIMod
	{
		public static List<string> ignoreItemsList = new List<string>();
		public static TradePriceDef priceDef;
		public static UI_ClearableInput sellSearchInput;
		public static UI_ClearableInput buySearchInput;
		public static UI_TradeCategoryList categoryList;

		public static bool ignoreWorthlessItems;

		public static void CleanUp()
		{
			// There MUST be a better way to achieve the same result...
			if (sellSearchInput != null)
			{
				//Clear these events because there's weird case where they still trigger even if the input is not visible
				sellSearchInput.onKeyDown.Clear();
				sellSearchInput.visible = false;
				ClearBuySearch();
			}

			if (buySearchInput != null)
			{
				buySearchInput.onKeyDown.Clear();
				buySearchInput.visible = false;
				ClearSellSearch();
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
			var searchText = sellSearchInput.text.ToLower();

			// Meh... this can be simplified but w/e
			var callbacks = new List<Func<UI_TradeItem, bool>>
			{
				item => item.m_itemname.text.ToLower().Contains(searchText)
			};
			if (ignoreWorthlessItems)
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
			sellSearchInput.text = "";
			FilterSellList();
		}

		public static void FilterBuyList()
		{
			var tradeWindow = Wnd_SchoolTrade.Instance;
			var list = tradeWindow.UIInfo.m_leftitem;
			var items = list.GetChildren();
			var searchText = buySearchInput.text.ToLower();

			foreach (UI_TradeItem item in items)
			{
				item.visible = item.m_typename.text == "ItemType" ||
				               item.m_itemname.text.ToLower().Contains(searchText);
			}

			FilterSchoolList(tradeWindow);
		}

		// Gray-out schools that don't have the items matching the current search term
		public static void FilterSchoolList(Wnd_SchoolTrade tradeWindow)
		{
			var schoolList = tradeWindow.UIInfo.m_n51;
			if (schoolList == null || schoolList.GetChildren().Length == 0) return;

			var searchText = buySearchInput.text.ToLower();
			foreach (var btn in schoolList.GetChildren())
			{
				int school = (int)btn.data;
				var accessible = SchoolGlobleMgr.Instance.HasSchoolPower(school) &&
				                 SchoolGlobleMgr.Instance.GetSchoolPower(school).GiftCount > 0;
				var iData = (ITradeItemData)IManagerModule_LoopInterval<TradeMgr>.Instance.GetSchoolTrade(school);

				btn.grayed = !accessible
				             || !iData.GetTradeItems().Exists((TradeItem item) =>
					             item.GetDisplayName().ToLower().Contains(searchText));
			}
		}

		public static void ClearBuySearch()
		{
			buySearchInput.text = "";
			FilterBuyList();
		}

		
		[HarmonyPatch(typeof(Wnd_SchoolTrade))]
		public static class AddQuickCategoryListToTradeWindow
		{
			[HarmonyPatch(methodName: "OnShowUpdate")]
			[HarmonyPostfix]
			public static void UpdateCategoryListItems(Wnd_SchoolTrade __instance)
			{
				try
				{
					var categoryPanel = TradeWindowSearch.categoryList;

					sellSearchInput.visible = true;
					buySearchInput.visible = true;
					// Re-attach keydown events
					sellSearchInput.onKeyDown.Add(FilterSellList);
					buySearchInput.onKeyDown.Add(FilterBuyList);

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
						ignoreWorthlessItems = ((GButton)e.sender).selected;
						FilterSellList();
					});
					ignoreWorthlessItems = categoryPanel.m_hideWorthlessCheckbox.selected;
					ignoreItemsList.Clear();
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

							TradePrice tradeValue = priceDef
								.GetItemPrice(nodeData.ItemName, nodeData.Rate).SalePrice;
							var finalPrice = iData.ScaleSalePrice(tradeValue.Value, nodeData.ItemName);
							// Main.Debug($"Final price for {nodeData.ItemName} with value of {finalPrice}");

							if (finalPrice < 1)
							{
								ignoreItemsList.Add(nodeData.ItemName);
							}

							// While we're at it, also sort the items in each folder by name
							if (j > 0)
							{
								int k = j;
								do
								{
									TreeNode prevNode = folder.GetChildAt(--k);
									var prevItemName = prevNode.data as string;
									if (string.Compare(itemName, prevItemName) < 0)
										folder.SwapChildren(itemNode, prevNode);
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

			[HarmonyPatch(methodName: "OnHide")]
			[HarmonyPostfix]
			public static void OnHideCleanup()
			{
				CleanUp();
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
				categoryList = (UI_TradeCategoryList)GetOrAddChild(tradeWindow.UIInfo, UI_TradeCategoryList.CreateInstance(), "YaogUI.CategoryPanel");
				categoryList.visible = true;
				// Put it next to the sell list
				categoryList.x =
					tradeWindow.UIInfo.m_rightitem.x + tradeWindow.UIInfo.m_rightitem.width;
				categoryList.y = tradeWindow.UIInfo.m_rightitem.y - 60;
			}

			public static void AddTradeWindowSellItemSearch(Wnd_SchoolTrade tradeWindow)
			{
				sellSearchInput = (UI_ClearableInput)GetOrAddChild(tradeWindow.UIInfo, UI_ClearableInput.CreateInstance(), "YaogUI.SellSearchInput");
				var searchInput = sellSearchInput;
				var clearSearchBtn = searchInput.m_clearButton;

				var list = tradeWindow.UIInfo.m_rightitem;
				list.foldInvisibleItems = true;

				searchInput.x = list.x - 10;
				searchInput.y = list.y - 40;
				searchInput.width = list.width;

				searchInput.onKeyDown.Add(FilterSellList);
				clearSearchBtn.onClick.Add(ClearSellSearch);
				// Search again when switching schools since items have changed
				tradeWindow.UIInfo.m_n51.onClickItem.Add(ClearSellSearch);
			}

			public static void AddTradeWindowBuyItemSearch(Wnd_SchoolTrade tradeWindow)
			{
				buySearchInput = UI_ClearableInput.CreateInstance();
				buySearchInput = (UI_ClearableInput)GetOrAddChild(tradeWindow.UIInfo, UI_ClearableInput.CreateInstance(), "YaogUI.BuySearchInput");

				var searchInput = buySearchInput;
				var clearSearchBtn = searchInput.m_clearButton;

				var list = tradeWindow.UIInfo.m_leftitem;
				list.foldInvisibleItems = true;

				searchInput.x = list.x - 10;
				searchInput.y = list.y - 40;
				searchInput.width = list.width;

				searchInput.onKeyDown.Add(FilterBuyList);
				clearSearchBtn.onClick.Add(ClearBuySearch);
				// Search again when switching schools since items have changed
				tradeWindow.UIInfo.m_n51.onClickItem.Add(FilterBuyList);
			}
		}

		[HarmonyPatch]
		public static class StoreRequiredVariables
		{
			// Needed to get the values of priceDef. Can't simply Traverse because they seem to be null
			// when we need/use them
			[HarmonyPatch(typeof(Wnd_SchoolTrade), "ShowWalkTrader")]
			[HarmonyPrefix]
			public static void StorePriceDefForTrader(string walker, Npc npc = null)
			{
				if (TradeWnd.HasTradeArea())
				{
					TradeWalkDef tradeDef = TradeMgr.Instance.GetWalkTradeDef(null);
					priceDef = TradeMgr.Instance.GetPriceDef(tradeDef.Price);
				}
			}

			[HarmonyPatch(typeof(Wnd_SchoolTrade), "ShowSchool")]
			[HarmonyPrefix]
			public static void StorePriceDefForSchoolTrade(int school)
			{
				if (TradeWnd.HasTradeArea())
				{
					SchoolTradeDef schoolDef = TradeMgr.Instance.GetSchoolTradeDef(school);
					priceDef = TradeMgr.Instance.GetPriceDef(schoolDef.Price);
				}
			}

			// Needed so that components get hidden when user accepts trade. There might be a better way to achieve this
			[HarmonyPatch(typeof(Wnd_SchoolTrade), "__selectyes")]
			[HarmonyPostfix]
			public static void HideTradeComponents(Wnd_SchoolTrade __instance)
			{
				if (__instance.UIInfo.m_state.selectedIndex == 1)
				{
					CleanUp();
				}
			}
		}
	}
}