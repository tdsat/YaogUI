using System;
using FairyGUI;
using HarmonyLib;
using XiaWorld.UI.InGame;
using System.Collections.Generic;
using XiaWorld;
using XiaWorld.ThingStep;
using System.Linq;
using KTV;
using UnityEngine;

namespace YaogUI
{
    public static class StorageAreaHelper
    {
        public static AreaStorage area =>
            Traverse.Create(Wnd_StorageArea.Instance).Field("area").GetValue<AreaStorage>();
        public static GButton[] buttons => Traverse.Create(Wnd_StorageArea.Instance).Field("bnts").GetValue<GButton[]>();

        public static UI_WindowStorage UI => Wnd_StorageArea.Instance.UIInfo;

        public static UI_Checkbox[] elementCheckboxList =
        {
            UI.m_n58,
            UI.m_n43,
            UI.m_n46,
            UI.m_n49,
            UI.m_n52,
            UI.m_n55
        };

        public static UI_Checkbox[] qualityCheckboxList =
        {
            UI.m_nq0,
            UI.m_nq1,
            UI.m_nq2,
            UI.m_nq3,
            UI.m_nq4,
        };

    }

    [HarmonyPatch(typeof(Wnd_StorageArea), "OnInit")]
    public static class TestShit
    {
        public static void Postfix(Wnd_StorageArea __instance)
        {
            try
            {
                var UI = __instance.UIInfo;
                var buttons = Traverse.Create(Wnd_StorageArea.Instance).Field("bnts").GetValue<GButton[]>();

                // Clear all button
                var clearAll = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
                clearAll.name = "YaogUI.ClearAll";
                clearAll.text = TFMgr.Get("全部清除");
                clearAll.x = UI.m_n25.x + (UI.m_n25.width - clearAll.width)/2; //Trying to centre this thing is the hardest part...
                clearAll.y = UI.m_n25.y;
                clearAll.visible = true;
                clearAll.onClick.Add(ClearAllItems);
                UI.AddChild(clearAll);

                // Ctrl+Click disables all elements except for the selected one
                for (var elementIdx = 0; elementIdx < buttons.Length; elementIdx++)
                {
                    var gButton = buttons[elementIdx];
                    var index = elementIdx;
                    gButton.onClick.Add((e) =>
                    {
                        if (!e.inputEvent.ctrl) return;
                        ClearAllElements();
                        gButton.selected = true;
                        gButton.onChanged.Call();
                        Main.Debug(index.ToString());
                    });
                }
                
                // Ctrl+Click disables all quality except for the selected one
                for (int i = 0; i < StorageAreaHelper.qualityCheckboxList.Length; i++)
                {
                    var checkbox = StorageAreaHelper.qualityCheckboxList[i];
                    checkbox.onClick.Add((e) =>
                    {
                        if (!e.inputEvent.ctrl) return;
                        ClearAllQuality();
                        checkbox.selected = true;
                    });
                }
            }
            catch (Exception e)
            {
                Main.Debug(e.ToString());
            }
        }

        public static void ClearAllItems()
        {
            for (g_emItemKind groupIndex = g_emItemKind.None; groupIndex < g_emItemKind.Count; ++groupIndex)
            {
                var group =
                    Wnd_StorageArea.Instance.UIInfo.m_n25.GetChildAt((int)groupIndex) as UI_Item_Storage;
                group.m_title.selected = false;
                group.m_title.onClick.Call();
            }
        }

        public static void ClearAllElements()
        {
            foreach (var button in StorageAreaHelper.buttons)
            {
                button.selected = false;
                button.onChanged.Call();
            }
        }

        public static void ClearAllQuality()
        {
            StorageAreaHelper.area.IncludeItemQ = new bool[5];
            foreach (var uiCheckbox in StorageAreaHelper.qualityCheckboxList)
            {
                uiCheckbox.selected = false;
            }
        }
    }
}