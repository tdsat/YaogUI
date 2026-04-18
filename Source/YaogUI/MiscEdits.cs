using HarmonyLib;
using System;
using FairyGUI;

namespace YaogUI
{
	[HarmonyPatch(typeof(Wnd_GameMain), "OnInit")]
	public static class DoubleMaterialRows
	{
		public static void Postfix(Wnd_GameMain __instance)
		{
			try
			{
				var materialList = __instance.UIInfo.m_MainList.m_StuffList;
				var listBg = __instance.UIInfo.m_MainList.m_n9;
				materialList.columnCount = 2;
				materialList.layout = ListLayoutType.FlowVertical;
				listBg.width = 115;
				materialList.x = listBg.x;
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}
	}
}