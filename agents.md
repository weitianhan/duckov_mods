# Duckov Client Mod (VS Code + Codex) 开发准备指南（agents.md）

本项目用于开发《Escape from Duckov》的**客户端插件 Mod**（C# DLL）。  
目标：使用 **VS Code + Codex** 完成开发、构建，并将产物自动部署到游戏的 Mods 目录以便测试。

---

## 0. 关键约束（务必先读）

- 本 Mod 入口类必须满足官方加载约定：
  - `info.ini` 里的 `name=MyMod`
  - DLL 文件名为 `MyMod.dll`
  - 入口类为：`MyMod.ModBehaviour`（命名空间与 `name` 一致，类名为 `ModBehaviour`）
- 项目 TargetFramework 必须为 **.NET Standard 2.1**
- 需要引用游戏安装目录下的 Managed DLL（你提供的路径）：
  - `C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Managed`

---

## 1. 安装与环境准备（一次性）

### 1.1 安装 .NET SDK
需要能在命令行运行 `dotnet`。

- 安装 **.NET SDK（建议 8.x 或更新）**
- 验证（PowerShell / CMD）：
  - `dotnet --info`

> 注意：项目 TargetFramework 是 netstandard2.1，但使用较新的 SDK 构建通常没问题。

### 1.2 安装 VS Code 与必要扩展
VS Code 扩展建议：

- **C# Dev Kit**（或至少 C# / OmniSharp）
- **.NET Install Tool**（可选，方便补齐环境）
- **EditorConfig**（可选，统一格式）
- 你用于协作开发的 **Codex** 扩展（按你自己的安装方式即可）

### 1.3 确认游戏安装路径与 Mods 目录
假设游戏安装在 Steam 默认目录：

- 游戏根目录：
  - `C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov`
- Managed DLL 目录（你提供）：
  - `C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Managed`
- Mods 目录（用于本地测试部署）：
  - `C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Mods`

如果 `Duckov_Data\Mods` 不存在，可以手动创建。

---

## 2. 仓库结构建议（标准化）

建议项目结构如下：

```
repo-root/
  src/
    MyMod/
      MyMod.csproj
      ModBehaviour.cs
  mod_package/
    info.ini
    preview.png (可选)
  .vscode/
    tasks.json
    settings.json (可选)
  agents.md
  README.md
```

- `src/MyMod`：C# 源码与 csproj
- `mod_package`：发布包所需静态文件（`info.ini` / `preview.png`）
- 构建后自动生成 `MyMod.dll` 并复制到游戏 Mods 目录

---

## 3. 创建 C# 项目（命令行）

在仓库根目录执行（PowerShell）：

```powershell
mkdir -Force src\MyMod | Out-Null
cd src\MyMod
dotnet new classlib -n MyMod
```

然后修改 `src/MyMod/MyMod.csproj`：

- TargetFramework 设为 `netstandard2.1`
- 关闭隐式 using（官方示例常见做法）
- 引用游戏 Managed DLL

---

## 4. MyMod.csproj 模板（关键：引用 Managed DLL + 自动部署）

将 `src/MyMod/MyMod.csproj` 改成类似下面这样（可直接用）：

说明：
- `DuckovManagedDir` 指向你提供的 Managed 目录
- `DuckovModsDir` 用于自动部署
- 引用项里给了常见的 Unity + 游戏程序集示例；如果你本机 DLL 名称不同，按实际文件名调整。

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>

    <!-- 建议关闭：避免 netstandard2.1 下出现额外 using 行为差异 -->
    <ImplicitUsings>false</ImplicitUsings>

    <!-- 游戏安装路径：按需修改 -->
    <DuckovManagedDir>C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Managed</DuckovManagedDir>
    <DuckovModsDir>C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Mods</DuckovModsDir>

    <!-- 构建输出 -->
    <AssemblyName>MyMod</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!-- Unity 常见依赖 -->
    <Reference Include="UnityEngine">
      <HintPath>$(DuckovManagedDir)\UnityEngine.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>$(DuckovManagedDir)\UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>

    <!-- 游戏 / Modding API（根据实际 DLL 名称调整）
         如果你的目录里不是 Duckov.dll/Duckov.Modding.dll，而是 TeamSoda.* 等，
         就把 HintPath 改成对应文件名。 -->
    <Reference Include="Duckov">
      <HintPath>$(DuckovManagedDir)\Duckov.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Duckov.Modding">
      <HintPath>$(DuckovManagedDir)\Duckov.Modding.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <!-- 构建后自动部署到 Mods/MyMod/ -->
  <Target Name="DeployToDuckovMods" AfterTargets="Build">
    <PropertyGroup>
      <DeployDir>$(DuckovModsDir)\MyMod</DeployDir>
    </PropertyGroup>

    <MakeDir Directories="$(DeployDir)" />

    <!-- 复制 DLL -->
    <Copy SourceFiles="$(TargetDir)MyMod.dll" DestinationFolder="$(DeployDir)" />

    <!-- 复制 info.ini / preview.png（如果存在） -->
    <Copy SourceFiles="..\..\mod_package\info.ini" DestinationFolder="$(DeployDir)" Condition="Exists('..\..\mod_package\info.ini')" />
    <Copy SourceFiles="..\..\mod_package\preview.png" DestinationFolder="$(DeployDir)" Condition="Exists('..\..\mod_package\preview.png')" />
  </Target>

</Project>
```

### 4.1 常见问题：找不到 Duckov.dll / Duckov.Modding.dll
不同版本/发布方式下，DLL 名称可能不一样（例如 `TeamSoda.*` 或其它命名）。

处理方式：
1. 打开 `C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Managed`
2. 确认实际存在的 DLL 名称
3. 按实际文件名修改 `<HintPath>...dll</HintPath>`
4. 缺哪个加哪个：报 `CS0246/CS0234` 时补引用

---

## 5. 编写入口类（ModBehaviour）

新建 `src/MyMod/ModBehaviour.cs`：

```csharp
using UnityEngine;

namespace MyMod
{
    // 入口类名必须是 ModBehaviour；并且命名空间必须与 info.ini 的 name 一致
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private void Start()
        {
            Debug.Log("[MyMod] ModBehaviour.Start() loaded!");
        }

        private void Update()
        {
            // 注意：不要在 Update 里疯狂打印日志
        }
    }
}
```

> 如果你发现基类命名空间不同（例如不是 `Duckov.Modding.ModBehaviour`），以你本机 Managed DLL 中的定义为准。

---

## 6. 编写 info.ini（必须）

在 `mod_package/info.ini` 创建：

```ini
name=MyMod
displayName=My Mod
description=My first Duckov client mod (VS Code + Codex).
publishedFileId=
tags=
```

注意：
- `name=MyMod` 必须与：
  - DLL 文件名：`MyMod.dll`
  - 命名空间：`namespace MyMod`
  - 入口类：`MyMod.ModBehaviour`
  保持一致。

---

## 7. VS Code 一键构建/部署

### 7.1 tasks.json
创建 `.vscode/tasks.json`：

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build: MyMod (deploy to Duckov Mods)",
      "type": "shell",
      "command": "dotnet",
      "args": [
        "build",
        "${workspaceFolder}/src/MyMod/MyMod.csproj",
        "-c",
        "Debug"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

然后在 VS Code：
- `Ctrl+Shift+P` → `Tasks: Run Task` → 选择 `build: MyMod (deploy to Duckov Mods)`

构建完成后应自动部署到：
- `...\Duckov_Data\Mods\MyMod\MyMod.dll`
- `...\Duckov_Data\Mods\MyMod\info.ini`

---

## 8. 运行与验证（本地测试流程）

1. 运行一次构建任务，确认 Mods 目录下出现：
   - `MyMod.dll`
   - `info.ini`
2. 启动游戏
3. 在游戏的 Mods 菜单（如果有）启用该 Mod（或游戏启动自动加载）
4. 查看游戏日志/控制台输出，验证出现：
   - `[MyMod] ModBehaviour.Start() loaded!`

> 如果你不知道日志在哪里：  
> - 在游戏目录内搜索最近修改的 `Player.log` / `output_log.txt` / `log` 等文件  
> - 或用 Windows 搜索 “Player.log”

---

## 9. Codex 协作建议（让它更稳定地产出可构建代码）

建议你在 Codex 会话里明确这些规则：

- 目标框架：`netstandard2.1`
- 只做官方客户端插件（不引入 BepInEx，除非你明确需要）
- 不引入不必要依赖（Harmony 如非必须不加）
- 输出必须能 `dotnet build` 通过
- 任何新增引用都必须在 csproj 明确写 `<Reference HintPath=...>`

---

## 10. 常见坑位清单（快速自查）

- [ ] `info.ini` 的 `name` 与 DLL 名称、命名空间一致
- [ ] TargetFramework 是 `netstandard2.1`
- [ ] `<ImplicitUsings>false</ImplicitUsings>` 已设置
- [ ] 游戏路径含空格，csproj HintPath 必须完整且正确
- [ ] 引用 DLL 的 `<Private>false</Private>`（避免把 Unity DLL 复制进输出目录）
- [ ] Mods 目录存在：`Duckov_Data\Mods\MyMod\`
- [ ] Update 里不要频繁 `Debug.Log`（性能与日志爆炸）

---

## 11. 最小命令（只记这个也能跑）

在仓库根目录：

```powershell
dotnet build .\src\MyMod\MyMod.csproj -c Debug
```

构建后自动部署到游戏 Mods 目录（由 csproj 的 `DeployToDuckovMods` target 完成）。
