## v0.5 (23/04/2026)
- (Trade Window) Added buttons to balance the trade values using spirit stones (if available)
- (Trade Window) Fix issue where trade search stoped working after a trade was accepted
- (Trade Window) Fix issue where some of the new elements would not disappear property when a trade was accepted
- (Misc) Fix issue in material select where you couldn't see all available material because the list wouldn't scroll
- (Talisman) Talisman search is auto-focused when you open it
Also added some extra code to avoid issues with ModLoaderLite.

## v0.4.1 (19/04/2026)
- (Storage Filters) User presets no overwrite default ones
- Fix release bugs

## v0.4 (18/04/2026)
- (Storage Filters) Added Storage area presets functionality
- (Storage Filters) Ctrl+Click on the Element/Quality options has a different effect
- (Misc) Auto-select NPC during certain interactions if there's only one option
- (Misc) Doubled the columns when selecting building material
- (Trade Window) Fixed search not working in the latest game version

## v0.3 (03/09/2022)
 - Added `ignoreWorthlessItems` checkbox to the trade window.
 - Make sure Harmony patches are only applied once
   - This should fix an issue where the search functionality of the trade window would not work after reloading
- Sort sold items alphabetically
- Add ability to search the items sold by the trader too
- Replace clearable search fields with custom component
  - Now also generate component code
- Remove some useless components from the FairyGUI project
- Added changelog :) 