using System.Collections.Generic;
using HarmonyLib;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace YaogUI
{
	[HarmonyPatch]
	public static class SubSpiritCrafting
	{
		[HarmonyPatch(typeof(Wnd_FabaoHeLianWindow), "SetHelianBtn")]
		[HarmonyPostfix]
		public static void UpdateEsotericaTooltip(Wnd_FabaoHeLianWindow __instance, ref List<UI_FabaoHelianBtn> __result, FabaoHelianData item)
		{
			if (item.kind != HelianKind.Esoterica) return;
			
			foreach (var btn in __result)
			{
				string manualName = (string)btn.data2;
				EsotericaData sysEsoterica = EsotericaMgr.Instance.GetSysEsoterica(manualName);
				EsotericaDef esotericaTemplate = EsotericaMgr.Instance.GetEsotericaTemplate(sysEsoterica.TID);
				string str1 = !string.IsNullOrEmpty(GameDefine.GetEsotericaTypeStr(esotericaTemplate.Type)) ? string.Format(TFMgr.Get("\n分类:{0}"), GameDefine.GetEsotericaTypeStr(esotericaTemplate.Type)) : null;
				string str2 = $"[{esotericaTemplate.DisplayName ?? sysEsoterica.DisplayName}]{str1}\n" + string.Format(TFMgr.Get("提高道行:{0}\n"), sysEsoterica.Difficulty) + string.Format(TFMgr.Get("需求境界：{0}\n"), GameDefine.GongStageLevelTxt[sysEsoterica.GLevel]);
				btn.tooltips = string.Format("{2}\n{0}{1}", !string.IsNullOrEmpty(esotericaTemplate.Desc) ? esotericaTemplate.Desc + "\n" : (object) string.Empty, GameDefine.GetFixedStory(null, esotericaTemplate.GetEffectDesc(sysEsoterica)), str2);
			}
		}
	}
}