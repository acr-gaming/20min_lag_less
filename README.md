
# Lag Less Mod

## [Youtube How To Install Video](https://www.youtube.com/watch?v=h4udIjLuEUg)



# Dev Stuff

## 1. Make lib folder and COPY dlls from game folder
```
--->root
    ---> .vscode
    ---> lib
        -->Assembly-CSharp-nstrip.dll
        -->Unity.InputSystem.dll
    --> Plugin.cs
    --> lagless20min.csproj
    ...
```
Unity.InputSystem.dll and Assembly-CSharp are in 

`C:\Program Files (x86)\Steam\steamapps\common\20MinuteTillDawn\MinutesTillDawn_Data\Managed` 

**probably, depends where your steam is installed.

## 2. Download NStrip and move NStrip.exe into lib folder
https://github.com/BepInEx/NStrip


## 3. Make private stuff public
Run 

`NStrip.exe -p -cg Assembly-CSharp.dll`

in terminal in lib folder. Then you can delete the original `Assembly-CSharp.dll` you copied over.

## 4. Enable/Disable Dev Stuff

ATM `Plugins.cs` has `LLConstants` that control turning some stuff off and on. If you enable dev `k` spawns experience and `j` will give you a bunch of upgrades designed to be laggy for testing.


