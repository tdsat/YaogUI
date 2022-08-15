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