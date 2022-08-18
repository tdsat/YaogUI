using HarmonyLib;
using XiaWorld;
using XiaWorld.UI.InGame;
using System;
using FairyGUI;
using System.Globalization;

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
					item.height = 140;
					item.m_n141.height = 140;
				}
			}
			catch (Exception e)
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
				faithString.y = 110;
				faithString.x = 10;

				var height = 10;
				var width = 70;

				//Background
				faithString.m_n41.height = height;

				faithString.m_n41.width = width * 2;
				faithString.m_n41.y = 7;
				faithString.m_n41.alpha = .5f;

				// Right (negative)
				faithString.m_n38.height = height;
				faithString.m_n38.m_bar.height = height;
				faithString.m_n38.m_n0.height = height;
				faithString.m_n38.y = 9;

                faithString.m_n38.maxWidth = width;
                faithString.m_n38.m_bar.maxWidth = width;
                faithString.m_n38.m_n0.maxWidth = width;

                // Left (positive)
                faithString.m_n39.height = height;
				faithString.m_n39.m_bar.height = height;
				faithString.m_n39.m_n0.height = height;

                faithString.m_n39.maxWidth = width;
                faithString.m_n39.m_bar.maxWidth = width;
                faithString.m_n39.m_n0.maxWidth = width;

                faithString.m_n38.value = 0.0;
				faithString.m_n39.value = 0.0;
				
				float faithPerc = Math.Abs((float)region.Faith / region.MaxFaith) * 100f;
				if (region.Faith >= 0)
				{
					faithString.m_n39.value = faithPerc;
				}
				else
				{
					faithString.m_n38.value = faithPerc;
				}

				faithString.m_n41.x = width - 10;
				faithString.m_n39.x = width * 2 - 10;
				faithString.m_n38.x = width - 30;

				faithString.m_text.text = TFMgr.Get("信仰值");
				var nfi = new NumberFormatInfo { NumberGroupSeparator = "." };
                faithString.m_n42.text = string.Format("{0} ({1}%)", region.Faith.ToString("#,###", nfi), (int)faithPerc);
				AddAdventureButton(region, cityItem);
				AddCampButton(region, cityItem);
			}
			catch (Exception e)
			{
				KLog.Dbg("[YaogUI] error" + e.ToString(), new object[0]);
			}
		}

		static void AddAdventureButton(OutspreadMgr.Region region, GComponent cityItem)
		{
			if (!(cityItem.GetChild("YaogUI.Adventure") is null)) return;

			OutspreadRegionDef regionDef = IManagerModule_LoopInterval<OutspreadMgr>.Instance.GetRegionDef(region.RegionName);
			PlaceDef placeDef = PlacesMgr.Instance.GetPlaceDef(regionDef.ClickPlace);
				
			var adventureBtn = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
			adventureBtn.name = "YaogUI.Adventure";
			adventureBtn.x = 220;
			adventureBtn.y = 111;
			adventureBtn.height = 20;
			adventureBtn.width = 85;
			adventureBtn.text = TFMgr.Get("历练");
			adventureBtn.grayed = !SchoolMgr.Instance.Created;
			adventureBtn.data2 = 0;
			adventureBtn.data = placeDef.Name;

			adventureBtn.onClick.Add(new EventCallback1(PubShowGoSelect));
			cityItem.AddChild(adventureBtn);
		}

		static void AddCampButton(OutspreadMgr.Region region, GComponent cityItem)
		{
			if (!(cityItem.GetChild("YaogUI.Camp") is null)) return;

			OutspreadRegionDef regionDef = IManagerModule_LoopInterval<OutspreadMgr>.Instance.GetRegionDef(region.RegionName);
			PlaceDef placeDef = PlacesMgr.Instance.GetPlaceDef(regionDef.ClickPlace);

			var campButton = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
			campButton.name = "YaogUI.Camp";
			campButton.x = 310;
			campButton.y = 111;
			campButton.height = 20;
			campButton.width = 55;
			campButton.text = TFMgr.Get("驻扎");
			campButton.grayed = !SchoolMgr.Instance.Created;
			campButton.data2 = 1;
			campButton.data = placeDef.Name;

			campButton.onClick.Add(new EventCallback1(PubShowGoSelect));
			cityItem.AddChild(campButton);
		}

		public static void PubShowGoSelect(EventContext eventContext)
        {
			OverrideShowGoSelect.PubShowGoSelect(Wnd_World.Instance, eventContext);
		}
	}
	[HarmonyPatch]
	public class OverrideShowGoSelect
	{
		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Wnd_World), "ShowGoSelect")]
		public static void PubShowGoSelect(Wnd_World __instance, EventContext context)
		{
			// Nothing to do, used just to get access to ShowGoSelect since it's a private method
		}
	}
}