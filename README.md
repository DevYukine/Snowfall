# Snowfall

A Dalamud plugin for FINAL FANTASY XIV that simplifies mass materia management.

## Features

* **Mass Materia Retrieval**: Adds a "Retrieve All Materia" option to the inventory context menu, allowing you to remove all materia from an equipped item at once
* Smart menu display: The option only appears on items with materia slots that contain materia

## Usage

1. Open your inventory
2. Right-click on an equipped item that has materia melded to it
3. Select "Retrieve All Materia" from the context menu
4. All materia will be removed from the item automatically

## Installation

### Prerequisites


* XIVLauncher, FINAL FANTASY XIV, and Dalamud have all been installed and the game has been run with Dalamud at least once.
* XIVLauncher is installed to its default directories and configurations.
  * If a custom path is required for Dalamud's dev directory, it must be set with the `DALAMUD_HOME` environment variable.
* A .NET Core 8 SDK has been installed and configured, or is otherwise available. (In most cases, the IDE will take care of this.)

### Building

1. Open up `Snowfall.sln` in your C# editor of choice (likely [Visual Studio 2022](https://visualstudio.microsoft.com) or [JetBrains Rider](https://www.jetbrains.com/rider/)).
2. Build the solution. By default, this will build a `Debug` build, but you can switch to `Release` in your IDE.
3. The resulting plugin can be found at `Snowfall/bin/x64/Debug/Snowfall.dll` (or `Release` if appropriate.)

### Activating in-game

1. Launch the game and use `/xlsettings` in chat or `xlsettings` in the Dalamud Console to open up the Dalamud settings.
    * In here, go to `Experimental`, and add the full path to the `Snowfall.dll` to the list of Dev Plugin Locations.
2. Next, use `/xlplugins` (chat) or `xlplugins` (console) to open up the Plugin Installer.
    * In here, go to `Dev Tools > Installed Dev Plugins`, and the `Snowfall` should be visible. Enable it.
3. The plugin will now add the "Retrieve All Materia" option to your inventory context menu!

Note that you only need to add it to the Dev Plugin Locations once (Step 1); it is preserved afterwards. You can disable, enable, or load your plugin on startup through the Plugin Installer.

## Development

This plugin uses Dalamud's Context Menu API to extend the inventory context menu. The main logic is located in `Service/MenuGuiService.cs`.

For more information about developing Dalamud plugins, check out the [Dalamud Developer Docs](https://dalamud.dev).
