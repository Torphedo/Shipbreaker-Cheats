This is a mod for Hardspace: Shipbreaker to enable some debugging features and cheats. Extract the release into your game's root folder and overwrite files when prompted to install. You can see all of the edits I've made by looking at the changes in [this commit](https://github.com/Torphedo/Shipbreaker-Cheats/commit/c40cda6b727d09995cb1d36b32882b85ea0ce254).

# In-game Features

These features can be used and/or toggled while the game is running.
Press F10 to see a list of keybinds. This list will be different in RACE mode and Career mode. The keybinds that F10 shows you are all the mod keybinds that you can use in that mode.
- F1: Refill Oxygen (Career mode only)
- F2: Refill fuel, heal all HP and suit damage (Career mode only)
- F3: Disable all UI (any game-mode)
- F4: Toggle FPS counter (any game-mode)
- F5: "Mega Cutter" (Career mode only, details below)
- F6: Increase Certification level by 1
- F7: Toggle debug wireframe rendering (any game-mode)
- F10: Show this keybind list
- Left Arrow: Pause time (Career mode only)
- Right Arrow: Reset game speed to 1x (Career mode only)
- Up Arrow: Increase game speed by 0.1x (Career mode only)
- Down Arrow: Decrease game speed by 0.1x (Career mode only)

When you increase your certification, you must return to the HAB to see changes.
The "Mega Cutter" was a name created by BBI. The Mega Cutter has no animation, particle effect, or range limit. It acts as a massive splitsaw beam about as wide as the player. It can go through unlimited layers of material, and ignores cut grade (It doesn't affect posters, shipping containers, and ECU panels for some reason).

# Config Options

A few features couldn't be changed during gameplay for various technical reasons. As a workaround, I used BBI's great config file system to add these to a config file. The mod currently uses `config.ini`in the game's root folder. Change options in this file, then boot/reboot the game to see changes.

- GodMode: Disables O2 drain, prevents the Cutter from damaging themselves or their suit
- TroutMode: Stops the Cutter from being pushed by rushing air during violent decompressions
- NoClipMode: Allows the Cutter to pass through any solid object
- FreeUpgrades: Makes every upgrade for all tools cost 0 LT
- InfDurability: Stops all tools from losing durability
- InfTethers: Sets the tether count to infinity
- InfHeat: Stops the Stinger and Splitsaw from building up any heat
