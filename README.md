# YaogUI

A UI mod for Amazing Cultivation Simulator. Currently in very early stages of development.

This is mostly intended for the english version of the game, but most of the features _should_ work in the chinese version too.

Releases can be downloaded from [NexusMods](https://www.nexusmods.com/amazingcultivationsimulator/mods/9/)

# Features

### Trade Window
- Ability to search user items
- Category bookmarks to make it easier to jump between item categories
- Sold items are sorted alphabetically
- Added a checkbox to exclude worthless items from the list
  - By worthless I mean items that have a trade value of 0, not useless.

### Talisman Drawing Window
- Ability to search when crafting item
- Talisman list also has a new look

### Other
- When a vessel dies, it name changes to '[Owner]'s Vessel' to make it easier to identify who owns it
- 'X : Hurt' and 'X : injury deteriorating' messages get cleaned if the affected pawn get healed
  - Experimental - Any feedback appreciated

# Development

The `Source` folder (hopefully) contains everything related to this mod. You can safely remove that folder if you are not interested in development.
 
- `Source\YaogUI` contains the Visual Studio solution and the code for the Harmony patches
- `Source\FairyProject` contains any new UI elements using [Fairy GUI](https://fairygui.com)

# TODO

This is a non-exhaustive list that I plan to work on. I can't give any promises for any of those since I'm not sure how to do most of those. I would appreciate any tips or PRs!

 - [X] Add a button to quickly send a disciple to camp/adventure in the 'Power Management' window
 - [X] Automatically remove 'X is hurt' message if the target is no longer hurt
 - [ ] Ability to save/favorite talismans
	-  Need to figure out how saves work
 - [X] Search in trade window
 - [ ] Pin common trade items to top of trade window (like Spirit Stones/Crystals etc)
 - [ ] Improve the inner disciples spell list
 - [ ] Make features optional through the UI
 - [ ] Add more options to the sidebar message configuration (eg "Ignore 'There is a fire' messages" etc)

 There are other changes that I would like to implement, like the ability to move items between storage spaces, but that is beyond my abilities for now.

 Also, new ideas are welcome but I can't promise anything.

 Feedback is always welcome!
