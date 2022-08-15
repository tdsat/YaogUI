local TalismanPaint = GameMain:GetMod("TalismanPaint")
local Wnd_FuPatinter = CS.Wnd_FuPatinter.Instance;

 local function AddSearchInput()
	local SearchInput = UIPackage.CreateObjectFromURL("ui://ncbwb41mv6072k");
	TalismanPaint.SearchInput = SearchInput;
	SearchInput.name = 'SearchInput';
	Wnd_FuPatinter.contentPane:AddChild(SearchInput);
	TalismanPaint.TalismanList = Wnd_FuPatinter.contentPane.m_n25;
	--  So that invisible itens take no space
	TalismanPaint.TalismanList.foldInvisibleItems = true;
	SearchInput.x = SearchInput.x - 110;
	SearchInput.y = SearchInput.y + 10;
	SearchInput.width = 120;
	SearchInput.onKeyDown:Add(FilterTalismanList);
	-- Clear search on window close
	Wnd_FuPatinter.onRemovedFromStage:Add(ClearSearchInput)
end

local function ChangeList()
	local list = TalismanPaint.TalismanList;
	local bg = Wnd_FuPatinter.contentPane.m_n61;
	local input = TalismanPaint.SearchInput;
	local btn = UIPackage.CreateObjectFromURL("ui://ncbwb41mv6076");

	list.autoResizeItem = true
	list.defaultItem = 'ui://m5coew5esglfb2';
	list.lineGap = 2;
	list.width = 220;
	list.x = Wnd_FuPatinter.contentPane.x - 240;

	bg.width = 265;
	bg.x =  Wnd_FuPatinter.contentPane.x - 265;

	if input ~= nil then
		input.x = Wnd_FuPatinter.contentPane.x - 260;

 		btn.text = 'Clear';
		btn.x = input.x + 200
		btn.y = input.y - 2;
		btn.onClick:Add(ClearSearchInput)
		Wnd_FuPatinter.contentPane:AddChild(btn);
		input.width = 260 - btn.width;
	end
end

function ClearSearchInput()
	TalismanPaint.SearchInput.text = '';
	FilterTalismanList();
end

function TalismanPaint:OnEnter()
	Wnd_FuPatinter = CS.Wnd_FuPatinter.Instance;
	Wnd_FuPatinter.onPositionChanged:Add(function()
		AddSearchInput();
		ChangeList();
		Wnd_FuPatinter.onPositionChanged:Clear();
	end);
end

function FilterTalismanList()
	FilterList(
		TalismanPaint.TalismanList,
		TalismanPaint.SearchInput.text,
		function(Talisman) return Talisman.title .. Talisman.tooltips; end
	)
end


