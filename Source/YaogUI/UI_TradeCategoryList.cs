/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace YaogUI
{
    public partial class UI_TradeCategoryList : GComponent
    {
        public GImage m_bg;
        public GImage m_bg_title;
        public GRichTextField m_title;
        public GList m_list;
        public GButton m_hideWorthlessCheckbox;
        public const string URL = "ui://m5coew5edgsub6";

        public static UI_TradeCategoryList CreateInstance()
        {
            return (UI_TradeCategoryList)UIPackage.CreateObject("YaogUI", "TradeCategoryList");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GImage)GetChildAt(0);
            m_bg_title = (GImage)GetChildAt(1);
            m_title = (GRichTextField)GetChildAt(2);
            m_list = (GList)GetChildAt(3);
            m_hideWorthlessCheckbox = (GButton)GetChildAt(4);
        }
    }
}