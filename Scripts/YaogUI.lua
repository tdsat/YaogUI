local YaogUI = GameMain:GetMod("YaogUI");--Register a new mod first

function YaogUI:OnBeforeInit()
  xlua.private_accessible(CS.XLua.LuaEnv)
  xlua.private_accessible(CS.XLua.ObjectTranslator)
  local thisData = CS.ModsMgr.Instance:FindMod("YaogUI", nil, true)
  if thisData == nil then return end;
  local thisPath = thisData.Path
  local mllFile = CS.System.IO.Path.Combine(thisPath, "YaogUI.dll")
  local asm = CS.System.Reflection.Assembly.LoadFrom(mllFile)
  CS.XiaWorld.LuaMgr.Instance.Env.translator.assemblies:Add(asm)
  CS.YaogUI.Main.Patch()
end

function YaogUI:OnInit()
	print("YaogUI Initiated");
	local tbEventMod = GameMain:GetMod("_Event")

	-- Clean messages on load
	local list = ThingMgr.NpcList
	for _, npc in pairs(list) do
		if npc.IsPlayerThing and npc.AtG then
			self:ClearMessageIfHealthy(npc);
		end
	end
	tbEventMod:RegisterEvent(g_emEvent.NpcHealthChanged, self.ClearMessageIfHealthy, self)
end

function YaogUI:OnEnter()
	print("YaogUI Entered");
end

function FilterList(list, needle, searchTextCb)
	local Items = list:GetChildren();
	needle = needle:lower();

	for i = 0, Items.Length - 1 do
		local Item = Items[i]
		local searchText = searchTextCb(Item);
		if searchText:lower():find(needle) ~= nil then
			Item.visible = true
		else
			Item.visible = false
		end
	end
end

function YaogUI:ClearMessageIfHealthy(npc, objs)
	if npc == nil or npc.IsPlayerThing == false or npc.AtG == false or npc.IsPuppet then return end;
	local bdDamageItems = npc.PropertyMgr.BodyData.m_DamageID;
	local doRemoveMesages = true;
	if (bdDamageItems.Count > 0) then
		for _, dmg in pairs(bdDamageItems) do
			if dmg.def.Trend ~= 0.0 then
				-- We check all bodyparts. If at least one is still healing, then we don't remove the message
				doRemoveMesages = false;
			end
		end
		if doRemoveMesages then
			MessageMgr:RemoveMessage(50, {npc}); -- X : hurt message
			MessageMgr:RemoveMessage(51, {npc}); -- X : Injury deteriorating messages
			-- print('[YaogUI]Removed message for ' .. npc.Name);
		end
	else
		MessageMgr:RemoveMessage(50, {npc}); -- X : hurt message
		MessageMgr:RemoveMessage(51, {npc}); -- X : Injury deteriorating messages
		-- print('[YaogUI]No damage found for ' .. npc.Name);
	end
end