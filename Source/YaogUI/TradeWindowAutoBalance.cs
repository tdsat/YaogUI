using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld;
using System.Linq;

namespace YaogUI
{
	public static class AutoBalance
	{
		public static GButton balanceRightBtn;
		public static GButton balanceLeftBtn;

		public static void BalanceTradeNodes(TreeView tradeList, TreeView selectList, TreeNode spiritStoneNode)
		{
			// var tradeItem = node.data2 as TradeItem;
			var sellValue = Traverse.Create(Wnd_SchoolTrade.Instance).Method("GetRightSelectValue").GetValue<TradePrice>();
			var buyValue = Traverse.Create(Wnd_SchoolTrade.Instance).Method("GetLeftSelectValue").GetValue<TradePrice>();
			// These can be null if there are no items being bought/sold
			var sellPrice = sellValue?.Value ?? 0;
			var buyPrice = buyValue?.Value ?? 0;
			int difference = sellPrice - buyPrice;
			Main.Debug($"Difference: {difference}");
			if (difference != 0)
			{
				Traverse.Create(tradeList).Method("ToSelect", new[]
						{ typeof(TreeNode), typeof(TreeView), typeof(int) }, new object[] { spiritStoneNode, selectList, difference})
					.GetValue();
			}
		}
	}

	[HarmonyDebug]
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnInit")]
	public static class AutoBalance_Wnd_SchoolTrade_OnInit
	{
		[HarmonyPostfix]
		public static void AddBalanceButtons(Wnd_SchoolTrade __instance)
		{
			try
			{
				var UI = __instance.UIInfo;
				
				var balanceRight = AutoBalance.balanceRightBtn ?? (GButton) UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				balanceRight.name = "YaogUI.BalanceRight";
				balanceRight.text = TFMgr.Get("平衡");
				balanceRight.tooltips = TFMgr.Get("点击使用灵石平衡交易。");

				balanceRight.x = 180;
				balanceRight.y = 65;
				AutoBalance.balanceRightBtn = balanceRight;

				var balanceLeft = AutoBalance.balanceLeftBtn ?? (GButton) UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				balanceLeft.name = "YaogUI.BalanceLeft";
				balanceLeft.text = balanceRight.text;
				balanceLeft.tooltips = balanceRight.tooltips;
				balanceLeft.x = UI.width - 250;
				balanceLeft.y = 65;
				AutoBalance.balanceLeftBtn = balanceLeft;

				//MLL Hack
				if (UI.GetChild("YaogUI.BalanceLeft") != null)
					UI.RemoveChild(UI.GetChild("YaogUI.BalanceLeft"));
				if (UI.GetChild("YaogUI.BalanceRight") != null)
					UI.RemoveChild(UI.GetChild("YaogUI.BalanceRight"));

				UI.RemoveChild(balanceLeft);
				UI.RemoveChild(balanceRight);
				
				UI.AddChild(balanceRight);
				UI.AddChild(balanceLeft);
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}
	}
	[HarmonyDebug]
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnShowUpdate")]
	public static class AutoBalance_Wnd_SchoolTrade_OnShowOrUpdate
	{
		[HarmonyPostfix]
		public static void AddAutoBalanceFunctionality(Wnd_SchoolTrade __instance)
		{
			try
			{
				var b1 = (GButton) __instance.UIInfo.GetChild("YaogUI.BalanceLeft");
				var b2 = (GButton) __instance.UIInfo.GetChild("YaogUI.BalanceRight");
				
				foreach (var balanceButton in new[] {b1, b2})
				{
					Main.Debug($"Going through {balanceButton.name}");
					// Maaan this is so stupid...
					TreeView tradeList;
					TreeView tradeSelect;
					if (balanceButton == AutoBalance.balanceRightBtn)
					{
						// Balance Sell List
						var list = __instance.GetParts()[3] as TradeSaleList;
						tradeList = Traverse.Create(list).Field("rightTree").GetValue<TreeView>();
						tradeSelect = Traverse.Create(list).Field("rightSelect").GetValue<TreeView>();
					}
					else
					{
						// Balance Buy List
						var list = __instance.GetParts()[2] as TradeBuyList;
						tradeList = Traverse.Create(list).Field("leftTree").GetValue<TreeView>();
						tradeSelect = Traverse.Create(list).Field("leftSelect").GetValue<TreeView>();
					}

					// Find spirit stone node
					TreeNode spiritStoneNode = null;
					Main.Debug($"Found {tradeList.list.numItems} for {balanceButton.name}");
					for (int i = 0; i < tradeList.list.numItems; i++)
					{
						var node = tradeList.list.GetChildAt(i).data as TreeNode;
						if (node.data2 is TradeItem tradeItem && tradeItem.ItemName == "Item_LingStone")
						{
							spiritStoneNode = node;
							break;
						}
					}

					if (spiritStoneNode == null)
					{
						balanceButton.enabled = false;
						balanceButton.tooltips = TFMgr.Get("点击使用灵石平衡交易。");
						return;
					}

					balanceButton.enabled = true;
					balanceButton.tooltips = TFMgr.Get("点击使用灵石平衡交易。");

					Main.Debug("Adding on click handler");
					balanceButton.onClick.Add(e =>
					{
						Main.Debug("Balance this!");
						AutoBalance.BalanceTradeNodes(tradeList, tradeSelect, spiritStoneNode);
					});
					
					// __instance.UIInfo.RemoveChild(balanceButton);
					// __instance.UIInfo.AddChild(balanceButton);
				}
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}
	}

	// public static void Postfix(Wnd_SchoolTrade __instance)
	// {
	// 	// var npc = Traverse.Create(__instance).Field("iconNpc").GetValue<Npc>();
	// 	// Main.Debug($"OnInit: {npc.GetName()}");
	// 	var buyList = __instance.GetParts()[2] as TradeBuyList;
	// 	var leftTree = Traverse.Create(buyList).Field("leftTree").GetValue<TreeView>();
	// 	var leftSelect = Traverse.Create(buyList).Field("leftSelect").GetValue<TreeView>();
	//
	// 	leftTree.onClickNode.Add(BalanceSheets);
	// 	return;
	//
	// 	void BalanceSheets(EventContext context)
	// 	{
	// 		var node = context.data as TreeNode;
	// 		var tradeItem = node.data2 as TradeItem;
	// 		Main.Debug($"Item name: {tradeItem.ItemName}");
	// 		if (!context.inputEvent.alt || tradeItem.ItemName != "Item_LingStone") return;
	// 		var sellValue = Traverse.Create(__instance).Method("GetRightSelectValue").GetValue<TradePrice>();
	// 		var buyValue = Traverse.Create(__instance).Method("GetLeftSelectValue").GetValue<TradePrice>();
	// 		// These can be null if there are no items being bought/sold
	// 		var sellPrice = sellValue?.Value ?? 0;
	// 		var buyPrice = buyValue?.Value ?? 0;
	// 		Main.Debug($"sellPrice price = {sellPrice}");
	// 		Main.Debug($"buyPrice price = {buyPrice}");
	// 		int difference = sellPrice - buyPrice;
	// 		// We need to subtract one because there's already a click handler to move 1 spirit stone
	// 		if (difference - 1 > 0)
	// 		{
	// 			Traverse.Create(buyList).Method("ToSelect", new[]
	// 					{ typeof(TreeNode), typeof(TreeView), typeof(int) }, new object[] { node, leftSelect, difference - 1 })
	// 				.GetValue();
	// 		}
	// 	}
	// }
}