/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace YaogUI
{
    public partial class UI_ClearableInput : GLabel
    {
        public Controller m_grayed;
        public GButton m_clearButton;
        public const string URL = "ui://m5coew5eon84b7";

        public static UI_ClearableInput CreateInstance()
        {
            return (UI_ClearableInput)UIPackage.CreateObject("YaogUI", "ClearableInput");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_grayed = GetControllerAt(0);
            m_clearButton = (GButton)GetChildAt(2);
        }
    }
}