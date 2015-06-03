# Cities Skylines: Scrollable Toolbar
[![Build status](https://ci.appveyor.com/api/projects/status/u7ied8k0aacca1um/branch/master?svg=true)](https://ci.appveyor.com/project/Archomeda/csl-scrollable-toolbar/branch/master)

Are you destroying your left mouse button because of all that clicking on the
right arrow in the toolbar, just to get to that road piece that's way near the
end? Not anymore! This mod enables mouse wheel scrolling in the toolbar. You can
now save your precious mouse and your precious time.

This mod for Cities Skylines patches the toolbar UI in such a way, that
scrolling with your mouse becomes possible. It doesn't matter where you hover
with your mouse, as long as it's on the toolbar. It should be compatible with
most, if not all, mods out here. If a mod happens to use the same toolbar UI as
the game itself (like Traffic++), then this mod will work for the UI of that
mod too.

## Features
- Patches the vanilla toolbar panels to be scrollable with the mouse wheel
- Adds a small button in the top right corner of the toolbar that allows you to
  toggle between normal width and extended width.
  - Note however, that this feature is automatically disabled partly or
    completely if another mod changes the toolbar. Please consult the notes
    below.

## Custom configuration settings
For more information about customizing your configuration settings, go to
[CONFIGURATION.md](CONFIGURATION.md).

## Installation
Go to the
[Steam Workshop](http://steamcommunity.com/sharedfiles/filedetails/?id=451700838)
and subscribe to the mod, it will install automatically. This will also keep it
updated with newer releases. If you want to do it manually, you can clone this
repo, compile the code yourself and place the DLL file in your mods folder.

## Compatibility
This mod is based on version 1.1.0 of Cities Skylines, and it is not guaranteed
that it will work on later versions. I'll try to keep it updated when newer
versions are released however.

If a newer version of Cities Skylines breaks your game, don't worry. This mod
should not break your saves (as far as I know). Just disable the mod for the
time being until an update of this mod is released.

### Mods
This mod should be compatible with all mods, as long as they don't change the
following stuff:
- Detouring of `UIInput.MouseHandler.ProcessInput()`
- Changing the panels in the toolbar in such a way that this mod cannot find
  the containers it has to patch anymore (`UIScrollablePanel` in `TSContainer`)

*Special support:*
- Traffic++ (automatically, since it uses the same UI as the game itself)

*Limited support:*
- Enhanced Build Panel
  - Only the panels that Enhanced Build Panel does not overwrite, are supported
    by Scrollable Toolbar.
  - You might see the toggle button show up for a second sometimes when it's not
    supposed to be there, but it's nothing to worry about.
- Sapphire skins
  - The feature to toggle the toolbar width is disabled. I cannot guarantee that
    it works for every skin out there (and skins can drastically change the
    layout!).

Even though Scrollable Toolbar has limited features with these mods, Scrollable
Toolbar should work fine together with those mods.

## Contributing
I'm open for any contributions you can make. If you find a bug, create an issue
here on GitHub. GitHub is very nice with maintaining a list of issues.
Submitting a bug report on the Steam Workshop is also appreciated, but it might
take a little longer for me to respond, because I prefer GitHub. If you know C#,
you can try to fix it yourself and submit a pull request.

### Compilation Notes
Please note that setting up your development environment is a bit different from
the Cities Skylines wiki. Instead of hardcoding various dependencies in the
solution file, you have to specify the path yourself as a user configuration
that will not be pushed to the repo. In order to do this, after you have opened
the project in Visual Studio, go to the project settings, reference paths, and
add your `SteamApps/Common/Cities_Skylines/Cities_Data/Managed` folder there.

Also, please refrain from adding those hardcoded references in the solution
file if you want to submit a pull request.

