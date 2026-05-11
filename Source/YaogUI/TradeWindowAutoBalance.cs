using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld;
using JetBrains.Annotations;
using UnityEngine;

namespace YaogUI
{
	public class AutoBalance : UIMod
	{
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

			var offerValue = Mathf.CeilToInt(offerAmount * scale);
			int difference = offerValue - askingPrice;

			if (difference == 0) return;
			if (saleList != null)
			{
				var toTransfer = Mathf.CeilToInt(askingPrice / scale - offerAmount);
				
				Traverse.Create(saleList).Method("ToSelect", new[]
							{ typeof(TreeNode), typeof(TreeView), typeof(int) },
						new object[] { spiritStoneNode, tradeSelect, toTransfer })
					.GetValue();
				saleList.ValueChange();
			}

			if (buyList != null)
			{
				// Main.Debug($"Asking price {askingPrice} - Offer amount {offerAmount} - Difference {difference} - ToTransfer {difference}");
				Traverse.Create(buyList).Method("ToSelect", new[]
							{ typeof(TreeNode), typeof(TreeView), typeof(int) },
						new object[] { spiritStoneNode, tradeSelect, difference })
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
		
		
		[HarmonyPatch]
		public static class AutoBalance_Wnd_SchoolTrade_OnInit
		{
			[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnInit")]
			[HarmonyPostfix]
			public static void AddBalanceButtons(Wnd_SchoolTrade __instance)
			{
				try
				{
					var UI = __instance.UIInfo;

					balanceRightBtn = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
					balanceRightBtn = (GButton)GetOrAddChild(UI, balanceRightBtn, "YaogUI.BalanceRight");
					balanceRightBtn.text = TFMgr.Get("平衡");
					balanceRightBtn.tooltips = TFMgr.Get("点击使用灵石平衡交易。");
					balanceRightBtn.x = UI.width - 250;
					balanceRightBtn.y = 65;

					balanceLeftBtn = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
					balanceLeftBtn = (GButton)GetOrAddChild(UI, balanceLeftBtn, "YaogUI.BalanceLeft");
				
					balanceLeftBtn.name = "YaogUI.BalanceLeft";
					balanceLeftBtn.text = balanceRightBtn.text;
					balanceLeftBtn.tooltips = balanceRightBtn.tooltips;
					balanceLeftBtn.x = 180;
					balanceLeftBtn.y = 65;

					UI.RemoveChild(balanceLeftBtn);
					UI.RemoveChild(balanceRightBtn);

					balanceRightBtn.onClick.Add(BalanceTradeNodes);
					balanceLeftBtn.onClick.Add(BalanceTradeNodes);
					// UI.AddChild(balanceRightBtn);
					// UI.AddChild(balanceLeftBtn);
				}
				catch (Exception e)
				{
					Main.Debug(e.ToString());
				}
			}
		
			[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnShowUpdate")]
			[HarmonyPriority(Priority.LowerThanNormal)]
			public static void MakeButtonsVisible(Wnd_SchoolTrade __instance)
			{
				balanceLeftBtn.visible = true;
				balanceRightBtn.visible = true;
			}
		}
	}
}