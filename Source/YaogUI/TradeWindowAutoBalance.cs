using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld;
using JetBrains.Annotations;
using UnityEngine;

namespace YaogUI
{
	public static class AutoBalance
	{
		public static bool initialized = false;
		public static GButton balanceRightBtn;
		public static GButton balanceLeftBtn;

		public static void BalanceTradeNodes(EventContext context)
		{
			var balanceButton = (GButton)context.sender;
			// Maaan this is so stupid...
			TradeSaleList saleList = null;
			TradeBuyList buyList = null;
			TreeView itemTree;
			TreeView tradeSelect;

			// Figure out which list we're balancing based on the button pressed. Need different handling due to 
			// different types/field names
			if (balanceButton.name == "YaogUI.BalanceRight")
			{
				// Balance Sell List
				saleList = (TradeSaleList)Wnd_SchoolTrade.Instance.GetParts()[3];
				itemTree = Traverse.Create(saleList).Field("rightTree").GetValue<TreeView>();
				tradeSelect = Traverse.Create(saleList).Field("rightSelect").GetValue<TreeView>();
			}
			else
			{
				// Balance Buy List
				buyList = (TradeBuyList)Wnd_SchoolTrade.Instance.GetParts()[2];
				itemTree = Traverse.Create(buyList).Field("leftTree").GetValue<TreeView>();
				tradeSelect = Traverse.Create(buyList).Field("leftSelect").GetValue<TreeView>();
			}

			// Find spirit stone node
			TreeNode spiritStoneNode = GetSpiritStonNode(itemTree);
			if (spiritStoneNode == null)
			{
				balanceButton.tooltips = TFMgr.Get("未找到灵石");
				return;
			}

			var sellValue = Traverse.Create(Wnd_SchoolTrade.Instance).Method("GetRightSelectValue")
				.GetValue<TradePrice>();
			var buyValue = Traverse.Create(Wnd_SchoolTrade.Instance).Method("GetLeftSelectValue")
				.GetValue<TradePrice>();

			// We need to take price scaling into account when calculating the amount of spirit stones we need to transfer
			var scale = Traverse.Create(Wnd_SchoolTrade.Instance).Field("_parser").Method("GetSaleScale")
				.GetValue<float>();

			// These can be null if there are no items being bought/sold
			var offerAmount = sellValue?.Value ?? 0;
			var askingPrice = buyValue?.Value ?? 0;

			var offerValue = offerAmount * scale;
			var finalOffer = Mathf.CeilToInt(offerValue);

			int difference = finalOffer - askingPrice;
			var toTransfer = Mathf.CeilToInt(askingPrice / scale - offerAmount);

			if (difference == 0) return;
			if (saleList != null)
			{
				Traverse.Create(saleList).Method("ToSelect", new[]
							{ typeof(TreeNode), typeof(TreeView), typeof(int) },
						new object[] { spiritStoneNode, tradeSelect, toTransfer })
					.GetValue();
				saleList.ValueChange();
			}

			if (buyList != null)
			{
				Traverse.Create(buyList).Method("ToSelect", new[]
							{ typeof(TreeNode), typeof(TreeView), typeof(int) },
						new object[] { spiritStoneNode, tradeSelect, toTransfer * -1 })
					.GetValue();
				buyList.ValueChange();
			}
		}

		[CanBeNull]
		public static TreeNode GetSpiritStonNode(TreeView itemTree)
		{
			for (int i = 0; i < itemTree.list.numChildren; i++)
			{
				var node = (TreeNode)itemTree.list.GetChildAt(i).data;
				if (node.data2 != null)
				{
					// node.data2 is either a string or a TradeItem. There's probably a function that finds it
					// but I have no idea which so we do it by hand. 
					if (node.data2 is TradeItem v)
					{
						if (v.ItemName == "Item_LingStone") return node;
					}
					else
					{
						if ((string)node.data2 == "Item_LingStone") return node;
					}
				}
			}

			return null;
		}
	}

	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnInit")]
	public static class AutoBalance_Wnd_SchoolTrade_OnInit
	{
		[HarmonyPostfix]
		public static void AddBalanceButtons(Wnd_SchoolTrade __instance)
		{
			if (AutoBalance.initialized) return; //Another attempt to avoid MLL. Fuck this shit honestly
			try
			{
				var UI = __instance.UIInfo;

				var balanceRight = AutoBalance.balanceRightBtn ??
				                   (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				balanceRight.name = "YaogUI.BalanceRight";
				balanceRight.text = TFMgr.Get("平衡");
				balanceRight.tooltips = TFMgr.Get("点击使用灵石平衡交易。");
				balanceRight.x = UI.width - 250;
				balanceRight.y = 65;
				AutoBalance.balanceRightBtn = balanceRight;

				var balanceLeft = AutoBalance.balanceLeftBtn ??
				                  (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				balanceLeft.name = "YaogUI.BalanceLeft";
				balanceLeft.text = balanceRight.text;
				balanceLeft.tooltips = balanceRight.tooltips;
				balanceLeft.x = 180;
				balanceLeft.y = 65;
				AutoBalance.balanceLeftBtn = balanceLeft;

				//MLL Hacks...
				if (UI.GetChild("YaogUI.BalanceLeft") != null)
					UI.RemoveChild(UI.GetChild("YaogUI.BalanceLeft"));
				if (UI.GetChild("YaogUI.BalanceRight") != null)
					UI.RemoveChild(UI.GetChild("YaogUI.BalanceRight"));
				UI.RemoveChild(balanceLeft);
				UI.RemoveChild(balanceRight);

				balanceRight.onClick.Add(AutoBalance.BalanceTradeNodes);
				balanceLeft.onClick.Add(AutoBalance.BalanceTradeNodes);
				UI.AddChild(balanceRight);
				UI.AddChild(balanceLeft);
				AutoBalance.initialized = true;
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}
	}

	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnShowUpdate")]
	public static class AutoBalance_Wnd_SchoolTrade_OnShowOrUpdate
	{
		[HarmonyPostfix]
		[HarmonyPriority(Priority.LowerThanNormal)]
		public static void MakeButtonsVisible(Wnd_SchoolTrade __instance)
		{
			AutoBalance.balanceLeftBtn.visible = true;
			AutoBalance.balanceRightBtn.visible = true;
		}

		[HarmonyPostfix]
		public static void AltClickToBalance(Wnd_SchoolTrade __instance)
		{
			var buyList = __instance.GetParts()[2] as TradeBuyList;
			var leftTree = Traverse.Create(buyList).Field("leftTree").GetValue<TreeView>();
			var leftSelect = Traverse.Create(buyList).Field("leftSelect").GetValue<TreeView>();

			leftTree.onClickNode.Add(BalanceSheets);
			return;

			void BalanceSheets(EventContext context)
			{
				var node = context.data as TreeNode;
				var tradeItem = node.data2 as TradeItem;
				if (!context.inputEvent.alt || tradeItem?.ItemName != "Item_LingStone") return;

				var sellValue = Traverse.Create(__instance).Method("GetRightSelectValue").GetValue<TradePrice>();
				var buyValue = Traverse.Create(__instance).Method("GetLeftSelectValue").GetValue<TradePrice>();
				// These can be null if there are no items being bought/sold
				var sellPrice = sellValue?.Value ?? 0;
				var buyPrice = buyValue?.Value ?? 0;
				int difference = sellPrice - buyPrice;
				// We need to subtract one because there's already a click handler to move 1 spirit stone
				if (difference - 1 > 0)
				{
					Traverse.Create(buyList).Method("ToSelect", new[]
								{ typeof(TreeNode), typeof(TreeView), typeof(int) },
							new object[] { node, leftSelect, difference - 1 })
						.GetValue();
				}
			}
		}
	}
}