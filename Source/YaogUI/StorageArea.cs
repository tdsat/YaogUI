using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using FairyGUI;
using HarmonyLib;
using JetBrains.Annotations;
using XiaWorld.UI.InGame;
using XiaWorld;
using UnityEngine;

namespace YaogUI
{
    public class StoragePreset
    {
        public string Name;
        public string Priority = "Normal";
        public List<g_emItemLable> Labels = new List<g_emItemLable>();
        public List<string> Qualities = new List<string> { "Poor", "Common", "Excellent", "Exquisite", "None" };
        public List<string> Elements = new List<string> { "None", "Metal", "Wood", "Water", "Fire", "Earth" };
        public float[] Tier = { 0f, 12f };
        public bool onlyFSItem;
        public bool CanSale;
        public bool onlyBigFish;

        public static bool GetTruthyValue([CanBeNull] string value)
        {
            if (value == null) return false;
            if (value.Length == 0) return false;
            return value == "true" || value == "1" || value == "yes";
        }

        public void ApplyToArea(AreaStorage area)
        {
            // Clear everything
            area.ExcludeItemLable.Clear();
            area.IncludeItemQ = new bool[5];
            // Original has 7 elements for some reason, so we keep it that way
            area.IncludeElement = new bool[7];
            area.Priority = 1;
            //Items - There has to be a better way to do this :/
            foreach (g_emItemLable label in Enum.GetValues(typeof(g_emItemLable)))
            {
                if (Labels.Contains(label)) continue;
                area.ExcludeItemLable.Add((int) label);
            }
            // Priority
            if (!string.IsNullOrEmpty(Priority))
            {
                switch (Priority.ToLower())
                {
                    case "high":
                    case "2":
                        area.Priority = 2;
                        break;
                    case "low":
                    case "0":
                        area.Priority = 0;
                        break;
                    default:
                        area.Priority = 1;
                        break;
                }
            }
            // Quality
            area.IncludeItemQ[0] = Qualities.Contains("Poor");
            area.IncludeItemQ[1] = Qualities.Contains("Common");
            area.IncludeItemQ[2] = Qualities.Contains("Excellent");
            area.IncludeItemQ[3] = Qualities.Contains("Exquisite");
            area.IncludeItemQ[4] = Qualities.Contains("None");
            // Tier
            // Ok so tiers are a bit weird. Min tier's actual value is X-1 from what you see in the UI
            // So a UI Tier on is actually 0. So when we set a min tier of 6, we need to subtract 1 from
            // that to reflect what happens in game. We could require min to start from 0 in the XML, but
            // that's bad UX and a bit confusing, so we handle it here
            float min = Math.Min(12f, Math.Max(1, Tier[0])) - 1;
            float max = Math.Min(12f, Math.Max(1, Tier[1]));
            area.IncludeItemRate = new Vector2(min, max);
            // Elements
            area.IncludeElement[0] = Elements.Contains("None");
            area.IncludeElement[1] = Elements.Contains("Metal");
            area.IncludeElement[2] = Elements.Contains("Wood");
            area.IncludeElement[3] = Elements.Contains("Water");
            area.IncludeElement[4] = Elements.Contains("Fire");
            area.IncludeElement[5] = Elements.Contains("Earth");
            // Misc
            area.onlyBigFish = onlyBigFish;
            area.CanSale = CanSale;
            area.onlyFSItem = onlyFSItem;
        }
    }
    public static class StorageAreaHelper
    {
        public static readonly string defaultPresetXMLName = "default_presets.xml";
        public static readonly string userPresetXMLName = "custom_presets.xml";
        
        public static Wnd_StorageArea window;
        public static List<StoragePreset> presets = new List<StoragePreset>();

        public static List<StoragePreset> LoadPresets(string filename)
        {
            var loaded = new List<StoragePreset>();
            var xmlDoc = XmlLoader.ReadXmlFile(filename);
            if (xmlDoc == null)
            {
                throw new Exception(filename + " not found or failed to load");
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ns", "YaogUI");

            var root = xmlDoc.SelectSingleNode("//ns:Presets", nsmgr);
            if (root == null) throw new Exception(filename + " failed to load or malformed");

            var index = 0;
            foreach (XmlNode presetNode in root.SelectNodes("ns:Preset", nsmgr))
            {
                // Names should be unique but...
                ++index;
                var originalName = presetNode.Attributes["name"]?.Value;
                string name = originalName;
                if (originalName == null) name = $"Preset {index}";
                else
                {
                    if (loaded.Select(p => p.Name).Contains(originalName))
                    {
                        name = $"{originalName} {index}";
                    }
                }
                
                var preset = new StoragePreset
                {
                    Name = name,
                    Priority = presetNode.Attributes["priority"]?.Value,
                    CanSale = StoragePreset.GetTruthyValue(presetNode.Attributes["CanSale"]?.Value),
                    onlyFSItem = StoragePreset.GetTruthyValue(presetNode.Attributes["onlyFSItem"]?.Value),
                    onlyBigFish = StoragePreset.GetTruthyValue(presetNode.Attributes["onlyBigFish"]?.Value)
                };
                
                // Parse Kinds (Items categories)
                XmlNode kindNode = presetNode.SelectSingleNode("ns:Kind", nsmgr);
                if (kindNode != null)
                {
                    foreach (XmlNode labelNode in kindNode.SelectNodes("ns:Label", nsmgr))
                    {
                        preset.Labels.Add((g_emItemLable) Enum.Parse(typeof(g_emItemLable), labelNode.InnerText));
                    }
                }
                else
                {
                    foreach (g_emItemLable label in Enum.GetValues(typeof(g_emItemLable)))
                    {
                        preset.Labels.Add(label);
                    }
                }

                XmlNode qualityNode = presetNode.SelectSingleNode("ns:Quality", nsmgr);
                if (qualityNode != null)
                {
                    preset.Qualities.Clear();
                    foreach (XmlNode labelNode in qualityNode.SelectNodes("ns:Label", nsmgr))
                    {
                        preset.Qualities.Add(labelNode.InnerText);
                    }
                }

                // Parse Tier settings
                XmlNode tierNode = presetNode.SelectSingleNode("ns:Tier", nsmgr);
                if (tierNode != null)
                {
                    var minAttr = tierNode.Attributes["min"];
                    if (minAttr != null)
                    {
                        float.TryParse(minAttr.Value, out preset.Tier[0]);
                    }

                    var maxAttr = tierNode.Attributes["max"];
                    if (maxAttr != null)
                    {
                        float.TryParse(maxAttr.Value, out preset.Tier[1]);
                    }
                }

                // Parse Element settings
                XmlNode elementNode = presetNode.SelectSingleNode("ns:Elements", nsmgr);
                if (elementNode != null)
                {
                    preset.Elements.Clear();
                    foreach (XmlNode labelNode in elementNode.SelectNodes("ns:Label", nsmgr))
                    {
                        preset.Elements.Add(labelNode.InnerText);
                    }
                }

                loaded.Add(preset);
            }
            Main.Debug("Loaded " + loaded.Count + " presets");

            return loaded;
        }

        public static AreaStorage area =>
            Traverse.Create(Wnd_StorageArea.Instance).Field("area").GetValue<AreaStorage>();

        public static GButton[] buttons =>
            Traverse.Create(Wnd_StorageArea.Instance).Field("bnts").GetValue<GButton[]>();

        public static UI_WindowStorage UI => Wnd_StorageArea.Instance.UIInfo;

        public static UI_Checkbox[] qualityCheckboxList =
        {
            UI.m_nq0,
            UI.m_nq1,
            UI.m_nq2,
            UI.m_nq3,
            UI.m_nq4
        };

        public static void ToggleAll()
        {
            var toggle = (UI.m_n25.GetChildAt(0) as UI_Item_Storage).m_title.selected;
            SetAllItems(!toggle);
        }

        public static void SetAllItems(bool selected)
        {
            for (g_emItemKind groupIndex = g_emItemKind.None; groupIndex < g_emItemKind.Count; ++groupIndex)
            {
                var group =
                    UI.m_n25.GetChildAt((int)groupIndex) as UI_Item_Storage;
                group.m_title.selected = selected;
                group.m_title.onClick.Call();
            }
        }

        public static void ClearAllElements()
        {
            foreach (var button in buttons)
            {
                button.selected = false;
                button.onChanged.Call();
            }
        }

        public static void ClearAllQuality()
        {
            area.IncludeItemQ = new bool[5];
            foreach (var uiCheckbox in qualityCheckboxList)
            {
                uiCheckbox.selected = false;
            }
        }
    }

    [HarmonyPatch(typeof(Wnd_StorageArea), "OnShowUpdate")]
    public static class InitWndStorageAreaPatch
    {
        public static void Prefix(Wnd_StorageArea __instance)
        {
            StorageAreaHelper.window = __instance;
        }
    }
    
    [HarmonyPatch(typeof(Wnd_StorageArea), "OnInit")]
    public static class AddCtrlModifiersToClickHandler // This class name makes no sense...
    {
        public static void Postfix(Wnd_StorageArea __instance)
        {
            try
            {
                var UI = __instance.UIInfo;
                var buttons = Traverse.Create(Wnd_StorageArea.Instance).Field("bnts").GetValue<GButton[]>();

                // Clear all button
                var toggleItems = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
                toggleItems.name = "YaogUI.ToggleItems";
                toggleItems.text = TFMgr.Get("全部切换");
                //Trying to centre this thing is the hardest part...
                toggleItems.x = UI.m_n25.x + (UI.m_n25.width - toggleItems.width) / 2;
                toggleItems.y = UI.m_n25.y;
                toggleItems.visible = true;
                toggleItems.onClick.Add(StorageAreaHelper.ToggleAll);
                UI.AddChild(toggleItems);

                // Ctrl+Click disables all elements except for the selected one
                for (var elementIdx = 0; elementIdx < buttons.Length; elementIdx++)
                {
                    var gButton = buttons[elementIdx];
                    gButton.onClick.Add(e =>
                    {
                        if (!e.inputEvent.ctrl) return;
                        StorageAreaHelper.ClearAllElements();
                        gButton.selected = true;
                        gButton.onChanged.Call();
                    });
                }

                // Ctrl+Click disables all quality except for the selected one
                for (int i = 0; i < StorageAreaHelper.qualityCheckboxList.Length; i++)
                {
                    var checkbox = StorageAreaHelper.qualityCheckboxList[i];
                    checkbox.onClick.Add(e =>
                    {
                        if (!e.inputEvent.ctrl) return;
                        StorageAreaHelper.ClearAllQuality();
                        checkbox.selected = true;
                    });
                }
            }
            catch (Exception e)
            {
                Main.Debug(e.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(Wnd_StorageArea), "OnInit")]
    public static class FilterPresets
    {
        public static void Postfix(Wnd_StorageArea __instance)
        {
            try
            {
                if (StorageAreaHelper.presets.Count == 0)
                {
                    Main.Debug("Loading presets...");
                    LoadPresets();
                }
                
                var UI = StorageAreaHelper.UI;
                var presetDropdown = CreatePresetDropdown();
                
                var reloadPresets = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
                reloadPresets.text = "Reload";
                reloadPresets.tooltips = TFMgr.Get("从 XML 重新加载预设");
                reloadPresets.x = presetDropdown.x + presetDropdown.width + 10;
                reloadPresets.y = presetDropdown.y;
                reloadPresets.onClick.Add(ReloadPresets);
     
                UI.AddChild(reloadPresets);
            }
            catch (Exception e)
            {
                Main.Debug(e.ToString());
            }
        }
        public static void ApplyPreset(StoragePreset preset)
        {
            Main.Debug("Applying preset " + preset.Name);
            preset.ApplyToArea(StorageAreaHelper.area);
            StorageAreaHelper.window.ShowOrUpdate();
        }

        public static void LoadPresets()
        {
            StorageAreaHelper.presets.Clear();
            try
            {
                var userPresets = StorageAreaHelper.LoadPresets(StorageAreaHelper.userPresetXMLName);
                var defaultPresets = StorageAreaHelper.LoadPresets(StorageAreaHelper.defaultPresetXMLName);
                StorageAreaHelper.presets = userPresets.Concat(defaultPresets).ToList();
            }
            catch (Exception e)
            {
                Main.Debug("User preset not found, loading default preset instead." + e);
                StorageAreaHelper.presets = StorageAreaHelper.LoadPresets(StorageAreaHelper.defaultPresetXMLName);
            }
        }
        
        public static void ReloadPresets()
        {
            LoadPresets();
            CreatePresetDropdown();
        }

        public static UI_ComboBox CreatePresetDropdown()
        {
            
            var UI = StorageAreaHelper.UI;
            // Creating this combobox is a pain in the ass...
            var dropdown = UI_ComboBox.CreateInstance();
            dropdown.name = "YaogUI.PresetDropdown";
            dropdown.x = UI.m_n25.x + (UI.m_n25.width - dropdown.width);
            dropdown.y = UI.m_n25.y;
            // Values
            dropdown.values = new string[StorageAreaHelper.presets.Count + 1];
            dropdown.items = new string[StorageAreaHelper.presets.Count + 1];
            dropdown.items[0] = TFMgr.Get("选择预设");
            dropdown.values[0] = "-1";
            dropdown.value = "-1";

            for (var i = 0; i < StorageAreaHelper.presets.Count; i++)
            {
                dropdown.values[i + 1] = i.ToString();
                dropdown.items[i + 1] = StorageAreaHelper.presets[i].Name;
            }
                
            dropdown.onChanged.Add(e =>
            {
                int.TryParse(dropdown.value, out var index);
                if (index == -1) return;
                ApplyPreset(StorageAreaHelper.presets[index]);
                dropdown.value = "-1";
            });
            
            // Remove the old one if it exists. Not sure if there's a better way to do this...
            if (UI.GetChild("YaogUI.PresetDropdown") != null) 
                UI.RemoveChild(UI.GetChild("YaogUI.PresetDropdown"), true);
            
            UI.AddChild(dropdown);
            dropdown.dropdown.minWidth = 200;
            dropdown.UpdateDropdownList();

            return dropdown;
        }
    }
 
}