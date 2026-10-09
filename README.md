# Saiyan vs. Everybody!

Original, family-friendly 2D boss-fight platformer for Windows PC (Unity 6000.3.9f1, URP, Input System). Vertical slice: Frosting Fields tutorial and King Cakezilla (phase 1 attacks playable; phases 2 and 3 pending). Placeholder art, placeholder audio.

Status and honest test results: `AI_HANDOFF/PROJECT_STATUS.md` and `TEST_REPORT.md`. Real in-game screenshots and clips: `AI_HANDOFF/SCREENSHOTS/`. Design: `AI_HANDOFF/GAME_DESIGN.md`. Original brief: `Docs/BUILD_BRIEF.md`.

## Open and run
1. Install Unity 6000.3.9f1. Open this folder as a project (the first import takes a few minutes).
2. Menu `Saiyan > Setup Project` (recreates the URP asset, input actions, tuning asset, scenes and build settings). Open `Assets/Scenes/MainMenu.unity` and press Play.
3. Windows build: `Saiyan > Build Windows Player` (writes `Builds/Windows/SaiyanVsEverybody.exe`; not yet built or tested).
4. Headless tests: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools\unity-run.ps1 -Tests EditMode` (or `PlayMode`).

## Controls
Keyboard: A/D or arrows move, Space jump (hold for higher), J shoot, K dash, L Super, Esc pause.
Gamepad: stick or d-pad move, A jump, X shoot, B or RB dash, Y Super, Start pause.
Bindings live in `Assets/Resources/Input/SaiyanControls.inputactions`.

## Tuning
Player: `PlayerTuning` on `PlayerController` (inspector). Boss: `Assets/Settings/KingCakezillaTuning.asset` (health, phase lines, every attack's timing and size, hazard cap).

## Private reference photo
`References/PrivatePhoto/` is git-ignored. The photo is never loaded by the game, never built and never uploaded. Keep it only on the local PC.
