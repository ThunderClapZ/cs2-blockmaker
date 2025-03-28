# cs2-blockmaker

**BlockMaker plugin to create and save blocks, mostly for HNS**

> block managing can be done within the building menu /bm
> 
> hold USE button to grab block, look around to move, left and right click to change distance
> 
> hold RELOAD button and move your mouse to rotate the block

<br>

| Video Showcase |
|-------|
| [![showcase](https://img.youtube.com/vi/IEcDrD1sUSc/hqdefault.jpg)](https://youtube.com/watch?v=IEcDrD1sUSc) |

<br>

## information:

### requirements

- [MetaMod](https://github.com/alliedmodders/metamod-source)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)
- [Block Maker Addon](https://steamcommunity.com/sharedfiles/filedetails/?id=3430295154)
- [CS2MenuManager](https://github.com/schwarper/CS2MenuManager)
- [ChaseMod](https://github.com/ipsvn/ChaseMod) (optional for gameplay)

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
    "Prefix": "{purple}BlockMaker {grey}|",
    "MenuType": "CenterHtmlMenu",
    "Building": {
      "BuildMode": {
        "Enable": true,
        "Config": false
      },
      "AutoSave": {
        "Enable": true,
        "Timer": 300
      },
      "Grab": {
        "Render": true,
        "RenderColor": "255,255,255,128",
        "Beams": true,
        "BeamsColor": "255,255,255,64"
      },
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
      "Entry": {
        "Model": "models/blockmaker/teleport/model.vmdl",
        "Color": "0,255,0,255"
      },
      "Exit": {
        "Model": "models/blockmaker/teleport/model.vmdl",
        "Color": "255,0,0,255"
      }
    }
  },
  "Commands": {
    "Admin": {
      "Permission": [ "@css/root" ],
      "BuildMode": [ "buildmode" ],
      "ManageBuilder": [ "builder", "builders" ],
      "ResetProperties": [ "resetproperties" ]
    },
    "Building": {
      "BuildMenu": [ "bm", "buildmenu" ],
      "CreateBlock": [ "create" ],
      "DeleteBlock": [ "delete" ],
      "RotateBlock": [ "rotate" ],
      "PositionBlock": [ "position" ],
      "BlockType": [ "type" ],
      "BlockColor": [ "color" ],
      "CopyBlock": [ "copy" ],
      "ConvertBlock": [ "convert" ],
      "LockBlock": [ "lock" ],
      "SaveBlocks": [ "save" ],
      "Snapping": [ "snap" ],
      "Grid": [ "grid" ],
      "Noclip": [ "nc" ],
      "Godmode": [ "godmode" ],
      "TestBlock": [ "testblock" ]
    }
  },
  "Sounds": {
    "SoundEvents": "soundevents/blockmaker.vsndevts",
    "Blocks": {
      "Speed": "bm_speed",
      "Camouflage": "bm_camouflage",
      "Damage": "bm_damage",
      "Fire": "bm_fire",
      "Health": "bm_health",
      "Invincibility": "bm_invincibility",
      "Nuke": "bm_nuke",
      "Stealth": "bm_stealth",
      "Teleport": "bm_teleport"
    },
    "Building": {
      "Enabled": true,
      "Create": "bm_create",
      "Delete": "bm_delete",
      "Place": "bm_place",
      "Rotate": "bm_rotate",
      "Save": "bm_save"
    }
  }
}
```

<br> <a href="https://ko-fi.com/exkludera" target="blank"><img src="https://cdn.ko-fi.com/cdn/kofi5.png" height="48px" alt="Buy Me a Coffee at ko-fi.com"></a>
