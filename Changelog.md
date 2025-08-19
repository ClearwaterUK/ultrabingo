# v1.2.0

- **Added chat system.** Players in chat with other players in the same game with all-chat and team chat.
- **Added ability to join games in progress.** (The corresponding option needs to be enabled by the host in the lobby settings)
- Ongoing refactoring work to allow for better map pool selection and custom level selection.
- Made some changes to the startup flow, that should hopefully reduce the amount of instances where players will get unspecified errors on startup, preventing them from playing the mod.

# v.1.1.1
 
- Fixed non-whitelisted mods displaying incorrectly.
- Added [BetterWeaponHUDs](https://thunderstore.io/c/ultrakill/p/Jade_Harley/Better_Weapon_HUDs/) and [JadeLib](https://thunderstore.io/c/ultrakill/p/Jade_Harley/JadeLib/) (used by betterWeaponHUDs) to the whitelist.

# v.1.1.0

## Major changes:
  - **Added Domination gamemode.** Teams fight to control as many maps as possible before time runs out.
    - Added setting field for setting a time limit in a Domination gamemode (Only affects Domination gamemode).
  - **Added the ability to vote to reroll a map.**
  - **Removed style claim requirement due to lack of player usage**. All claim types in a bingo game will now be based on time only.
  - **Added Message of the Day section to the bingo main menu**. This allows for quick informing of updates, server changes and PSAs for known issues.
  - **Begun readding Angry map pools.** (Only a small handful of maps are available for now, more will be added over time as map makers upload their works.)
  - **Added special in-game ranks obtainable** by being a noteworthy member of the larger ULTRAKILL community (mapper, modder, speedrunner, etc), or by having supported the mod on Kofi. If you fit any of the criteria, feel free to apply [on the Discord](https://discord.gg/VyzFJwEWtJ) to receive your in-game role!
    - Developer: Developers and code contributors to Baphomet's Bingo.
    - Tester: All those who helped beta test the mod prior to public releases and gave feedback.
    - Donator: Those who have financially supported development efforts via Kofi.
    - Mapper: Those who contributed maps to the mod.
    - Legacy Mapper: Those who contributed maps **prior to the game's Ultra_Revamp update.**
    - Modder: Fellow mod makers in the ULTRAKILL scene.
    - Speedrunner: Recognised speedrunners in the ULTRAKILL scene.
    - New Blood: Reserved to Hakita & co, and other associates of New Blood Interactive.

## Changes:
- Visual pass to make UI elements more in line with UI changes from the latest game update.
- Added the ability to right click on a level in the bingo card on the pause menu, to ping the level for the rest of your team.
- Changed text on the pause menu to better indicate possible actions (left click to move to level, right click to ping map, etc.)
- Times to beat on a level will now be displayed directly in the pause card menu when highlighting it.
- When a player (re)claims a level, the new time requirement is now displayed in the HUD message that's broadcasted to all players.
- In the case of a connection error, the client will now try to reconnect up to 3 times before failing and leaving the game.
- Added [StyleToasts](https://thunderstore.io/c/ultrakill/p/The0x539/StyleToasts/) to the whitelist.


## Fixes:
- Implemented an FPS cap and VSync force enable when accessing the bingo main menu in the hopes of further reducing freezes & crashes related to UI elements failing to render. (This is an experimental change and may remain if it successfully mitigates problems.)
- Better handling of error flow if bad stuff happens.
- Fixed an issue where enabling cheats in the base game, then joining a bingo game, would still show finished levels in a bingo as having cheats used. (This was only a visual bug that would display a level as having cheats used when cheats are forcibly disabled in a bingo game.)
- Fixed games in the game browser always showing only 1 player when there may be more connected.
- Fixed bingo main menu buttons remaining locked if unable to host or join a game.
- Fixed being able to input non-alphanumeric characters in the join game input field.
- Fixed 1-1, 5-1 and 7-3 secret exits still being accessible via out-of-bounds clipping when "disable campaign alt exits" is enabled.
- Potentially fixed UI elements in the lobby screen randomly appearing invisible.

--- 

# v.1.0.6
- Gone straight to this version to resync with TS.
- Fixed the bingo pause menu and tab level stats not appearing.
- Fixed a small issue with the 0-2 alt exit prevention that was broken due to the game update.
- Fixed the divider on the bingo main menu not being symmetrical.
- Added Healthbars and HandPaint to the mod whitelist.

--- 

# v.1.0.3
- Critical fixes to make the mod work again after the ULTRA_REVAMP update
- Changed "Moving to" text from HUD message to the bingo pause card
- Added Encore level map pool
- Updated all official level thumbnails with their updated versions from the latest game version
- Removed all Angry map pools due to custom levels also breaking due to the game update

--- 

# v.1.0.2

- Added match browser
- Added ability to copy game id if hosting
- Better prevention against OOB skips to reach secret exits if disabling campaign alt exits is enabled
- When setting teams, the set team color now displays in the border instead of in the player text
- In-game bingo panel now displays correctly if weapon position is set to right instead of left/middle
- Fixed being able to spam start game button, which would cause the bingo grid to duplicate
- Fixed being able to spam host/join game buttons on the main menu, which would eventually cause a freeze
- Fixed being able to host/join a game and immediately return to menu before transitioned to main menu
- Fixed Take Back The Night not loading correctly due to incorrect ID on the server
- Fixed grid position being moved if the download of a level was started, then the player restarted the current level from the pause menu
- Properly changed reset button from Backspace to F10
- Prevent level switch if started right before a game ends
- Prevent nomo/noweap Angry setting applying in bingo gamemodes
- Prevent clicking on the button of the level we're currently in
- Prevent triggering campaign secret level ends via OOB
- Added Ultrakill Style Editor and USTManager to whitelist

- Added maps to map pool:
    - Angry Standard:
    - Inanis Power Station