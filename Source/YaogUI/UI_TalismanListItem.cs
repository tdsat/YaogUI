/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace YaogUI
{
    public partial class UI_TalismanListItem : GButton
    {
        public GImage m_background;
        public const string URL = "ui://m5coew5esglfb2";

        public static UI_TalismanListItem CreateInstance()
        {
            return (UI_TalismanListItem)UIPackage.CreateObject("YaogUI", "TalismanListItem");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_background = (GImage)GetChildAt(0);
        }
    }
}