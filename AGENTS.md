# TinyIsland Agent Instructions

## Project Snapshot

- Unity project for a short survival-arcade prototype.
- Unity version: `6000.4.8f1` from `ProjectSettings/ProjectVersion.txt`.
- Render pipeline: URP.
- Core loop: gather driftwood, react to tides, build tower parts, climb, survive, escalate.

## Repository Layout

- Runtime code lives under `Assets/_Project/Code`.
- Scenes live under `Assets/_Project/Scenes`.
- Prefabs live under `Assets/_Project/Prefabs`.
- Project docs live under `Docs`.
- Generated IDE files such as `*.csproj` and `*.sln` are ignored and should not be committed.
- Keep Unity `.meta` files with any created, moved, or deleted assets.

## Unity Editing Rules

- Prefer changing C# scripts over hand-editing scene or prefab YAML.
- Only edit `.unity` or `.prefab` YAML when the change is small, well understood, and can be validated by diff.
- Do not rewrite scene or prefab files for unrelated changes.
- When adding serialized fields, choose conservative defaults so existing scene objects continue to work.
- Avoid relying on generated `.csproj` contents. Unity regenerates them; use temporary edits only for local compile checks when needed.
- If adding code that uses Unity UI, ensure `Assets/_Project/Code/TinyIsland.Runtime.asmdef` references the required assembly.

## Code Style

- Match the existing simple MonoBehaviour style.
- Keep gameplay behavior inside the relevant domain folder: `Player`, `Tower`, `Tide`, `Climbing`, `Hazards`, `Wood`, `UI`, etc.
- Use explicit serialized fields for tunable gameplay values.
- Prefer small components with clear ownership over broad manager classes.
- Keep comments sparse and only for non-obvious behavior.
- Default to ASCII in source files unless the file already uses non-ASCII text.

## Gameplay Rules

- Tower building is part-based. `TowerController` owns tower state and visual activation.
- Player interaction with building belongs in `PlayerTowerBuildInteractor`.
- Climbing/rhythm logic belongs in `ClimbingController` and related UI components.
- World-space player prompts should follow the player and face the camera.
- Screen-space HUD belongs in UI components, not in `OnGUI`, except as debug fallback.
- Preserve the current prototype scope: avoid new systems unless they directly support the requested loop.

## Verification

- After C# changes, run:

  ```bash
  dotnet build TinyIsland.slnx
  ```

- If Unity has not regenerated project files and `dotnet build` misses new scripts, open the project in Unity or temporarily add new source files to the ignored generated `.csproj` only for local validation.
- Do not commit generated `.csproj` changes.
- For Unity Editor validation, prefer batchmode when licensing allows:

  ```bash
  /Applications/Unity/Hub/Editor/6000.4.8f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath /Users/moushn_01/Projects/TinyIsland -logFile /tmp/TinyIslandUnity.log
  ```

- If batchmode fails because of licensing or environment restrictions, report that limitation clearly.

## Git Hygiene

- Do not revert user changes.
- Before editing, check `git status --short` when changes may overlap with the task.
- Keep unrelated cleanup out of feature changes.
- `AGENTS.md` is intended to be tracked. Use personal global guidance in `~/.codex/AGENTS.md` for local preferences that should not be committed.
