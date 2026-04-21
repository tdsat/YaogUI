using FairyGUI;
using HarmonyLib;
using XiaWorld;
using System.Linq;

namespace YaogUI
{
	[HarmonyPatch(typeof(Wnd_SchoolTrade), "OnInit")]
	public static class TestT2
	{
		[HarmonyPostfix]
		public static void AddBalanceButton(Wnd_SchoolTrade __instance)
		{
			var balanceBtn = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
			balanceBtn.name = "YaogUI.BalanceTrade";
			balanceBtn.text = TFMgr.Get("平衡");
			balanceBtn.x = __instance.UIInfo.m_n51.x + __instance.UIInfo.m_n51.width + 10;
			balanceBtn.y = __instance.UIInfo.m_n51.y;
						
			var buyList = __instance.GetParts()[2] as TradeBuyList;
			var _iData = Traverse.Create(buyList).Field("_iData").GetValue<ITradeItemData>();
		
			// Hide the button if the trader doesn't have Spirit stones
			foreach (var tradeItem in _iData.GetTradeItems())
			{
				if (tradeItem.ItemName == "Item_LingStone")
				{
					__instance.UIInfo.AddChild(balanceBtn);
					// balanceBtn.visible = true;
					break;
				}
				balanceBtn.visible = false;
			}
			__instance.UIInfo.AddChild(balanceBtn);
		}

		public static void Postfix(Wnd_SchoolTrade __instance)
		{
			var npc = Traverse.Create(__instance).Field("iconNpc").GetValue<Npc>();
			Main.Debug($"OnInit: {npc.GetName()}");
			var buyList = __instance.GetParts()[2] as TradeBuyList;
			var leftTree = Traverse.Create(buyList).Field("leftTree").GetValue<TreeView>();
			var leftSelect = Traverse.Create(buyList).Field("leftSelect").GetValue<TreeView>();

			leftTree.onClickNode.Add(BalanceSheets);
			return;

			void BalanceSheets(EventContext context)
			{
				var node = context.data as TreeNode;
				var tradeItem = node.data2 as TradeItem;
				Main.Debug($"Item name: {tradeItem.ItemName}");
				if (!context.inputEvent.alt || tradeItem.ItemName != "Item_LingStone") return;
				var sellValue = Traverse.Create(__instance).Method("GetRightSelectValue").GetValue<TradePrice>();
				var buyValue = Traverse.Create(__instance).Method("GetLeftSelectValue").GetValue<TradePrice>();
				// These can be null if there are no items being bought/sold
				var sellPrice = sellValue?.Value ?? 0;
				var buyPrice = buyValue?.Value ?? 0;
				Main.Debug($"sellPrice price = {sellPrice}");
				Main.Debug($"buyPrice price = {buyPrice}");
				int difference = sellPrice - buyPrice;
				// We need to subtract one because there's already a click handler to move 1 spirit stone
				if (difference - 1 > 0)
				{
					Traverse.Create(buyList).Method("ToSelect", new[]
							{ typeof(TreeNode), typeof(TreeView), typeof(int) }, new object[] { node, leftSelect, difference - 1 })
						.GetValue();
				}
			}
		}
	}
}