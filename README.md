# cs2-blockmaker

**BlockMaker plugin to create and save blocks, mostly for HNS**

> block managing can be done within the building menu /buildmenu
> 
> USE button to hold block, look around to move, left and right click to change distance
> 
> RELOAD button to hold block, left click to rotate horizontal axis, right click to rotate vertical axis

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
- [BlockBuilder Assets](https://steamcommunity.com/sharedfiles/filedetails/?id=3299954847)
- [ipsvn/ChaseMod](https://github.com/ipsvn/ChaseMod) (optional for gameplay)
- [exkludera/cs2-teleports](https://github.com/exkludera/cs2-teleports) (optional for gameplay)

<br>

> [!NOTE]
> thanks to [UgurhanK/BaseBuilder](https://github.com/UgurhanK/BaseBuilder) for the code base
>
> inspired by [BlockBuilder by x3ro](https://forums.alliedmods.net/showthread.php?t=258329) (block models are from them also)

<img src="https://github.com/user-attachments/assets/53e486cc-8da4-45ab-bc6e-eb38145aba36" height="200px"> <br>

<br>

## example config

```json
{
  "Settings": {
    "Main": {
      "Prefix": "{purple}[BlockMaker]{default}",
      "MenuType": "html"
    },
    "Building": {
      "BuildMode": false,
      "BuildModeConfig": false,
      "BlockGrabColor": "255,255,255,128",
      "GridValues": [ 16, 32, 64, 128, 256 ],
      "RotationValues": [ 15, 30, 45, 60, 90, 120 ],
      "AutoSave": false,
      "SaveTime": 300
    },
    "Blocks": {
      "DisableShadows": true,
      "Bhop": {
        "Duration": 0.25,
        "Cooldown": 1.5
      },
      "Health": {
        "Cooldown": 0.5,
        "Value": 2
      },
      "Grenade": {
        "Cooldown": 60
      },
      "Gravity": {
        "Duration": 4,
        "Cooldown": 5,
        "Value": 0.4
      },
      "Frost": {
        "Cooldown": 60
      },
      "Flash": {
        "Cooldown": 60
      },
      "Fire": {
        "Duration": 5,
        "Cooldown": 5,
        "Value": 8
      },
      "Delay": {
        "Duration": 1,
        "Cooldown": 1.5
      },
      "Damage": {
        "Cooldown": 0.5,
        "Value": 5
      },
      "Stealth": {
        "Duration": 10,
        "Cooldown": 60
      },
      "Speed": {
        "Duration": 3,
        "Cooldown": 60,
        "Value": 2
      },
      "SpeedBoost": {
        "Value": 650
      },
      "Slap": {
        "Value": 2
      },
      "Random": {
        "Cooldown": 60
      },
      "Invincibility": {
        "Duration": 5,
        "Cooldown": 60
      },
      "Trampoline": {
        "Value": 500
      },
      "Camouflage": {
        "ModelT": "characters/models/ctm_fbi/ctm_fbi.vmdl",
        "ModelCT": "characters/models/tm_leet/tm_leet_variantb.vmdl",
        "Duration": 10,
        "Cooldown": 60
      }
    }
  },
  "Commands": {
    "Admin": {
      "Permission": "@css/root",
      "BuildMode": "buildmode,togglebuild",
      "ManageBuilder": "builder,togglebuilder,allowbuilder"
    },
    "Building": {
      "BuildMenu": "buildmenu,blockmenu,blocksmenu",
      "CreateBlock": "block,create,createblock,place,placeblock",
      "DeleteBlock": "delete,deleteblock,remove,removeblock",
      "RotateBlock": "rotate,rotateblock",
      "BlockType": "type, blocktype",
      "BlockColor": "color, blockcolor",
      "CopyBlock": "copy, copyblock",
      "ConvertBlock": "convert,convertblock,replace,replaceblock",
      "SaveBlocks": "save,saveblocks,saveblock",
      "Snapping": "snap,snapblock,blocksnap",
      "Grid": "grid,togglegrid",
      "Noclip": "nc,noclip",
      "Godmode": "god,godmode",
      "TestBlock": "testblock"
    }
  },
  "Sounds": {
    "Blocks": {
      "Speed": "sounds/bootsofspeed.vsnd",
      "Camouflage": "sounds/camouflage.vsnd",
      "Damage": "sounds/dmg.vsnd",
      "Health": "sounds/heartbeat.vsnd",
      "Invincibility": "sounds/invincibility.vsnd",
      "Nuke": "sounds/nuke.vsnd",
      "Stealth": "sounds/stealth.vsnd",
      "Teleport": "sounds/teleport.vsnd"
    },
    "Building": {
      "Enabled": true,
      "Create": "sounds/buttons/blip1.vsnd",
      "Delete": "sounds/buttons/blip2.vsnd",
      "Place": "sounds/buttons/latchunlocked2.vsnd",
      "Rotate": "sounds/buttons/button9.vsnd",
      "Save": "sounds/buttons/bell1.vsnd"
    }
  }
}
```

<br> <a href="https://ko-fi.com/exkludera" target="blank"><img src="https://cdn.ko-fi.com/cdn/kofi5.png" height="48px" alt="Buy Me a Coffee at ko-fi.com"></a>
