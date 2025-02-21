# cs2-blockmaker

**BlockMaker plugin to create and save blocks, mostly for HNS**

> block managing can be done within the building menu /buildmenu
> 
> hold USE button to grab block, look around to move, left and right click to change distance
> 
> hold RELOAD button and move your mouse to rotate the block

> [!CAUTION]
> **this project is not finished! any help would be very appreciated.**

<br>

**showcase:**<br>
[![showcase](https://img.youtube.com/vi/AEAEKhCErsw/hqdefault.jpg)](https://youtube.com/watch?v=AEAEKhCErsw)

<br>

## information:

### requirements

- [MetaMod](https://github.com/alliedmodders/metamod-source)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)
- [Block Maker Addon](https://steamcommunity.com/sharedfiles/filedetails/?id=3430295154)
- [ipsvn/ChaseMod](https://github.com/ipsvn/ChaseMod) (optional for gameplay)

<br>

> [!NOTE]
> thanks to [UgurhanK/BaseBuilder](https://github.com/UgurhanK/BaseBuilder) for the code base
>
> inspired by [BlockBuilder by x3ro](https://forums.alliedmods.net/showthread.php?t=258329)

<img src="https://github.com/user-attachments/assets/53e486cc-8da4-45ab-bc6e-eb38145aba36" height="200px"> <br>

<br>

## example config

```json
{
  "Settings": {
    "Prefix": "{purple}[BlockMaker]{default}",
    "Menu": "html",
    "Building": {
      "BuildMode": true,
      "BuildModeConfig": false,
      "AutoSave": false,
      "SaveTime": 300,
      "BlockGrabColor": "255,255,255,128",
      "BlockSizes": [
        { "Title": "Small", "Size": 0.5 },
        { "Title": "Normal", "Size": 1 },
        { "Title": "Large", "Size": 2 },
        { "Title": "X-Large", "Size": 3 }
      ]
    },
    "Blocks": {
      "DisableShadows": true,
      "CamouflageT": "characters/models/ctm_fbi/ctm_fbi.vmdl",
      "CamouflageCT": "characters/models/tm_leet/tm_leet_variantb.vmdl",
      "FireParticle": "particles/burning_fx/env_fire_medium.vpcf"
    },
    "Teleports": {
      "ForceAngles": false,
      "EntryModel": "models/blockmaker/teleport/model.vmdl",
      "EntryColor": "0,255,0,255",
      "ExitModel": "models/blockmaker/teleport/model.vmdl",
      "ExitColor": "255,0,0,255"
    }
  },
  "Commands": {
    "Admin": {
      "Permission": "@css/root",
      "BuildMode": "buildmode",
      "ManageBuilder": "builder,builders"
    },
    "Building": {
      "BuildMenu": "bm,buildmenu",
      "CreateBlock": "create",
      "DeleteBlock": "delete",
      "RotateBlock": "rotate",
      "PositionBlock": "position",
      "BlockType": "type",
      "BlockColor": "color",
      "CopyBlock": "copy",
      "ConvertBlock": "convert",
      "LockBlock": "lock",
      "SaveBlocks": "save",
      "Snapping": "snap",
      "Grid": "grid",
      "Noclip": "nc",
      "Godmode": "godmode",
      "TestBlock": "testblock"
    }
  },
  "Sounds": {
    "SoundEvents": "soundevents/blockmaker.vsndevts",
    "Blocks": {
      "Speed": { "Event": "speed", "Volume": 1 },
      "Camouflage": { "Event": "camouflage", "Volume": 1 },
      "Damage": { "Event": "damage", "Volume": 1 },
      "Health": { "Event": "health", "Volume": 1 },
      "Invincibility": { "Event": "invincibility", "Volume": 1 },
      "Nuke": { "Event": "nuke", "Volume": 1 },
      "Stealth": { "Event": "stealth", "Volume": 1 },
      "Teleport": { "Event": "teleport", "Volume": 1 }
    },
    "Building": {
      "Enabled": true,
      "Create": { "Event": "create", "Volume": 1 },
      "Delete": { "Event": "delete", "Volume": 1 },
      "Place": { "Event": "place", "Volume": 1 },
      "Rotate": { "Event": "rotate", "Volume": 1 },
      "Save": { "Event": "save", "Volume": 1 }
    }
  }
}
```

<br> <a href="https://ko-fi.com/exkludera" target="blank"><img src="https://cdn.ko-fi.com/cdn/kofi5.png" height="48px" alt="Buy Me a Coffee at ko-fi.com"></a>
