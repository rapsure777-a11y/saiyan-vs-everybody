# ART AND AUDIO ASSETS

## Usable inside Unity right now
**No production art exists.** Everything on screen is generated in code by `Assets/Scripts/Core/PlaceholderArt.cs` (outlined circles, rounded rectangles, stars, hearts, gradients) and assembled into Saiyan (`Player/PlayerVisual.cs`), Cakezilla, scenery. WRITTEN, never rendered yet.

## Visual references (not game-ready)
- `References/ConceptArt/01_Game_Key_Art.png`: title, mood, palette, other bosses (pirate octopus, arcade cabinet), platformer world.
- `References/ConceptArt/02_Saiyan_vs_King_Cakezilla.png`: main target for Saiyan, Cakezilla (layered pink cake, red nose, giant white gloves, candle crown) and the candy kingdom.
- These are posters: no separated layers, no sprite sheets, no rig. They are committed to the private repo only as reference.
- `References/PrivatePhoto/`: the child's private reference photo lives only on the owner's PC, is git-ignored, never loaded by the game, never built, never uploaded. Not copied into this project by Claude.

## Missing art
Saiyan sprite sheets and animations (run, idle, jump, fall, land, dash, shoot, hurt, victory), Cakezilla layers and animations (idle, telegraph per attack, slam, spit, hurt, 3 phases including Supreme, defeat), cupcake/donut/blob/puddle/shockwave sprites, Frosting Fields background layers and tiles, title/menu art, UI frames, logo (a logo exists only inside the poster).

## Temporary assets that must be replaced
All placeholder shapes above; generated placeholder sound tones (`Core/AudioHooks.cs`); the built-in font used by UI.

## Audio
No music, no voice, no recorded SFX. Placeholder tones exist per cue (jump, shoot, dash, hit, super, telegraph, slam, splat...). Real audio plugs in through `AudioHooks.Register` or `Resources/Audio/<CueName>`.

## Suggested pipeline for real art
Give each character as separated PNG layers or a sprite sheet (transparent background, one pose per frame, consistent scale); import to `Assets/Art`; swap `PlayerVisual` for an Animator-driven rig. Gameplay code does not depend on the placeholder visuals.

## Update 2026-10-09 (Milestone 1)
- Screenshots and clips of the current placeholder visuals (real renders): `AI_HANDOFF/SCREENSHOTS/` (index in its README; critique against the concept art in `VISUAL_REVIEW.md`).
- Usable in Unity now: only procedural placeholders (`PlaceholderArt.cs`, `PlayerVisual.cs`, `BossVisual.cs`, `ArenaBuilder.cs`). They render correctly and are what the screenshots show. No sprite sheets, animation clips, music or recorded SFX exist.
- Still temporary and to be replaced: Saiyan, King Cakezilla (and his Supreme form), cupcakes/blobs/puddles/shockwave, all Frosting Fields art, UI frames and fonts, the placeholder tones in `AudioHooks.cs`.

## Update 2026-10-09 (Milestone A): first real art
GENUINELY GENERATED (by Codex, image generation from the two concept images; no private photo): Saiyan model sheet v1 and a simplified sheet, 5-pose test, 8 idle frames. Raw and reference files: `Art/Source/Saiyan/` (reference/ and idle/). Processed, imported and playing in Unity: **Saiyan idle** (`Assets/Resources/Art/Saiyan/idle/`, 512 px frames, 200 ppu, pivot at the feet, 8 fps loop). Usable inside Unity: YES (verified by tests and screenshots). Still placeholders: every other Saiyan animation, all of King Cakezilla, all effects, all backgrounds, all UI art, all audio. The placeholder Saiyan stays as the fallback and as the default until the core animation set exists.
