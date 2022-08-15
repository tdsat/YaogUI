using HarmonyLib;
using XiaWorld;
using XiaWorld.UI.InGame;
using System;

namespace YaogUI
{
	[HarmonyPatch(typeof(Wnd_QuickCityWindow), "OnShowUpdate")]
	public static class AddFaithToSectPowerWindow
	{
		public static void Postfix(Wnd_QuickCityWindow __instance)
		{
			try
            {
				var list = __instance.UIInfo.m_n3;
				var items = list.GetChildren();
				foreach (UI_QuickCityItem item in items)
				{
					item.height = 130;
					item.m_n141.height = 130;
				}
			} catch (Exception e)
            {
				KLog.Dbg("[YaogUI] error" + e.ToString(), new object[0]);
			}
		}
	}

	// Add the actual progress bar and values. We piggy-back on the UpdateRCK function to avoid any unnecessary checks
	[HarmonyPatch(typeof(Wnd_QuickCityWindow), "UpdateRCK")]
	public static class AddFaithToSectPowerWindow2
	{
		public static void Postfix(Wnd_QuickCityWindow __instance, OutspreadMgr.Region region, UI_QuickCityItem cityItem)
		{
			try
			{
				var curFaith = region.Faith;
				var maxFaith = region.MaxFaith;
				UI_XinYangSlider faithString;
				
				faithString = (UI_XinYangSlider)cityItem.GetChild("YaogUI.FaithString");
				if (faithString is null)
                {
					faithString = UI_XinYangSlider.CreateInstance();
					faithString.name = "YaogUI.FaithString";
					cityItem.AddChild(faithString);
				}


				faithString.m_n43.visible = false; // 'Region' text bg
				faithString.m_n40.visible = false; // Separator
				faithString.y = 105;
				faithString.x = 20;

				var height = 10;
				faithString.m_n41.height = height;
				faithString.m_n41.y = 7;
				faithString.m_n41.alpha = .5f;

				faithString.m_n38.height = height;
				faithString.m_n38.m_bar.height = height;
				faithString.m_n38.m_n0.height = height;


				faithString.m_n39.height = height;
				faithString.m_n39.m_bar.height = height;
				faithString.m_n39.m_n0.height = height;
				
				faithString.m_n38.value = 0.0;
				faithString.m_n39.value = 0.0;

				float faithPerc = (float)region.Faith / (float)region.MaxFaith;
				if (region.Faith >= 0)
				{
					faithString.m_n39.value = (double)(faithPerc * 100f);
				}
				else
				{
					faithPerc *= -1;
					faithString.m_n39.value = (double)(faithPerc * 100f);
				}
				faithString.m_text.text = TFMgr.Get("信仰值");
				faithString.m_n42.text = string.Format("{0}/{1}", region.Faith, region.MaxFaith);
			}
			catch (Exception e)
			{
				KLog.Dbg("[YaogUI] error" + e.ToString(), new object[0]);
			}
		}
	}
}
