# Duckov Client Mod: Lean Collaboration Guide

## 1. Goal
- Build a client mod DLL for Escape from Duckov.
- Develop with VS Code + Codex.
- `dotnet build` should auto-deploy to the game's Mods folder.

## 2. Hard Rules
- `info.ini` must contain: `name=MyMod`
- Output DLL name must be: `MyMod.dll`
- Entry type must be: `MyMod.ModBehaviour`
- Target framework must be: `netstandard2.1`
- Use official client mod loading only (no BepInEx/Harmony unless explicitly requested).

## 3. Current Repo Layout
```
repo-root/
  src/MyMod/
    MyMod.csproj
    ModBehaviour.cs
  mod_package/
    info.ini
  .vscode/
    tasks.json
  DisLikeAll.sln
```

## 4. Verified Local Paths
- Managed DLLs:
  - `C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Managed`
- Mods deploy target:
  - `C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Mods`
- Game log:
  - `C:\Users\weitianhan\AppData\LocalLow\TeamSoda\Duckov\Player.log`

## 5. Dependency Rules (for current game build)
- Keep explicit `<Reference + HintPath>` entries in `MyMod.csproj`.
- Required references:
  - `UnityEngine.dll`
  - `UnityEngine.CoreModule.dll`
  - `UnityEngine.InputLegacyModule.dll`
  - `TeamSoda.Duckov.Core.dll`
- Keep `<Private>false</Private>` on game/Unity references.

## 6. Build and Log Commands
- Build + deploy:
```powershell
dotnet build .\src\MyMod\MyMod.csproj -c Debug
```
- Tail logs:
```powershell
Get-Content "C:\Users\weitianhan\AppData\LocalLow\TeamSoda\Duckov\Player.log" -Wait -Tail 80
```

## 7. Minimal Development Workflow
For every change:
1. Ensure `dotnet build` passes.
2. Ensure DLL is updated in `Mods\MyMod`.
3. Verify behavior in `Player.log`.

General coding notes:
- Prefer existing game APIs first; use reflection only when no public API exists.
- Do not spam logs inside `Update()`.
