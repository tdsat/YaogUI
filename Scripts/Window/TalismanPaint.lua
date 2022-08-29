local TalismanPaint = GameMain:GetMod("TalismanPaint")
local Wnd_FuPatinter = CS.Wnd_FuPatinter.Instance;

 local function AddSearchInput()
	local SearchInput = UIPackage.CreateObjectFromURL("ui://m5coew5eon84b7");
	TalismanPaint.SearchInput = SearchInput;
	SearchInput.name = 'YaogUI.TalismanSearch';
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
	local btn = TalismanPaint.SearchInput.m_clearButton;

	list.autoResizeItem = true
	list.defaultItem = 'ui://m5coew5esglfb2';
	list.lineGap = 2;
	list.width = 220;
	list.x = Wnd_FuPatinter.contentPane.x - 240;

	bg.width = 265;
	bg.x =  Wnd_FuPatinter.contentPane.x - 265;

	if input ~= nil then
		input.width = list.width + 10;
		input.x = list.x - 10;

		btn.onClick:Add(ClearSearchInput)
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


