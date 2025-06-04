# DarkAgesPatcher


**DOOM: The Dark Ages patcher for modding purposes.**

This tool patches the DOOM: The Dark Ages game executable for modding purposes. The patches are defined in a "patch definitions file" that is automatically downloaded and updated from the update server specified in the configuration file.

**Official modern DOOM modding Discord server:** https://discord.gg/W9t4nZa

### Features

 - User friendly GUI to make patching the game easy.
 - Backs up the game executable before applying patches (optional)
 - Automatic updates for the patch definitions.
 - Can be used through the command-line, no need for the GUI.

### Command-line usage

This tool can also be used through the command-line without using the GUI. The following arguments can be used:

```
--update - Checks for updates and downloads them if available
--patch <file path> - Patches the given game executable
```

### Update server

The update server can be configured in the configuration file of the application by changing the "UpdateServer" key. The specified server must serve the following files:

 - DarkAgesPatcher.def
   - File containing the patch definitions. This is the file that is downloaded.
 - DarkAgesPatcher.md5
   - File containing the MD5 checksum of the patch definitions file above. This file must be a one-liner with no line breaks. This is the file used to check for updates.
   
The default update server specified in the configuration file is hosted by myself and I will keep the patch definitions updated.

### Patch definitions file

An example of a patch definitions file and it's syntax:

```
# game build definitions
# id=executable name:md5 checksum
steam=DOOMTheDarkAges.exe:7ea73e0ee1a2066dc43502930ededced

# patches
# patch=description:compatible build ids (comma separated):offset:hex patch

# skip data checksum checks (by emoose)
# unrestrict cvars & console commands (by SunBeam, ported by emoose)
patch=unrestrict cvars & console commands:pattern:steam:084C8B0EBA01:084C8B0EBA00
```
