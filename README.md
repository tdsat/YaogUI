# YaogUI

A UI mod for Amazing Cultivation Simulator. Currently, in the early stages of development.

This is mostly intended for the English version of the game, but most of the features _should_ work in the Chinese version too.

Releases can be downloaded from [NexusMods](https://www.nexusmods.com/amazingcultivationsimulator/mods/9/)

# Features

### Trade Window
- Ability to search user items
- Category bookmarks to make it easier to jump between item categories
- Sold items are sorted alphabetically
- Added a checkbox to exclude worthless items from the list
  - By worthless I mean items that have a trade value of 0, not useless.

##### How to add custom presets
You can add custom presets by creating a `custom_presets.xml` and adding your own. You can use the `storage_presets_sample.xml`
as a reference since it contains all available options. 

The simplest way to add custom presets is to copy the `default_presets.xml` and rename it to `custom_presets.xml`

> Please note that custom presets will overwrite default ones if they have the same name

### Talisman Drawing Window
- Ability to search when crafting item
- Talisman list also has a new look

### Storage Filter Window
- Button to toggle all item filters on/off
- When you Ctrl+Click on the Element/Quality options, it disables all other options
- Ability to create/load filter presets
  - You can create custom presets by changing the `custom_presets.xml`

### Other
- When a vessel dies, it name changes to '[Owner]'s Vessel' to make it easier to identify who owns it
- 'X : Hurt' and 'X : injury deteriorating' messages get cleaned if the affected pawn get healed
- Doubled the material selection columns when building
- During adventures, when the NPC selection dialog appears, if there's only one NPC in the map they are auto-selected

# Development

> NOTE : The build project is no longer guaranteed to work out-of-box. I don't use Windows anymore and can't be bothered to test/fix it
> It _should_ work, but might need some changes WRT to required assemblies etc.

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
 - [ ] Add more options to the sidebar message configuration (e.g. "Ignore 'There is a fire' messages" etc)

 There are other changes that I would like to implement, like the ability to move items between storage spaces, but that is beyond my abilities for now.

 Also, new ideas are welcome, but I can't promise anything.

 Feedback is always welcome!
