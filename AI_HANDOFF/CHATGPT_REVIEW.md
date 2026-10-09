# REVIEW BRIEF FOR CHATGPT (read this instead of the repo)

Report time: 2026-10-09, Milestone A (first real sprite art). Branch `claude/milestone-1-greybox`, code commit `e8b86bd` (reports commit follows; `git log -1` is truth). TESTED = run in Unity; WRITTEN = code only; PLANNED.
Goal: a polished hybrid classic-and-modern cartoon boss-fight game, Cuphead-inspired expressive animation, original designs.

## Latest screenshots (REAL renders of the running Unity game unless labeled)
Folder `AI_HANDOFF/SCREENSHOTS/` (README lists everything; `VISUAL_REVIEW.md` is the older critique of the placeholder art).
- **Before / after (same camera, same moment):** `MA_2026-10-09_A0_before-after-comparison.jpg` (placeholder shapes vs generated sprite)
- Sprite in the tutorial: `MA_..._A3_sprite-in-tutorial.jpg`; in the boss fight: `A4_sprite-in-boss-fight.jpg`; beside a boss attack warning: `A5_...`; shooting: `A6_...` (no shoot art yet, so it only stretches)
- Animation preview scene (world view only; its IMGUI panel is not captured): `A7_...`
- **Sprite sheet contact sheet (processed frames on the game's sky color, NOT a game render):** `MA_..._saiyan-idle_contact-sheet.jpg`; animated loop: `MA_..._saiyan-idle_preview.gif` (also NOT a game render)
- Clip of the sprite in the level (run/jump are procedural stretch only): `MA_..._video_sprite-idle-in-game.mp4`
- Earlier boss, attack, phase and UI stills and clips: `M1_2026-10-09_*`
- Source art from Codex (model sheets, pose test, 8 raw idle frames): `Art/Source/Saiyan/` (NOT game renders). Concept art for comparison: `References/ConceptArt/`.

## 1. What changed since the last report
Real art arrived: Codex (image generation) produced a model sheet, a 5-pose test and 8 idle frames after two review rounds from me (simplify, clean background, lock scarf/hair/gloves). I built the pipeline: `Tools/sprites/process_frames.py` (magenta key-out with edge un-mixing, colour matching across frames, bleed fix, feet pivot, 512 px export, contact sheet and GIF), automatic Unity import rules, a flipbook animator with frame events, `SaiyanSpriteVisual` (sprite art with the placeholder as fallback; F9 toggles in the level), and an Animation Preview scene (F10 on the main menu). Also tuning after the first human play: dash 0.26 s, boss 180 HP, faster rhythm.

## 2. What is working (TESTED)
EditMode 27/27, PlayMode 36/36. The idle loop plays in the game and the preview scene; feet sit on the floor via the pivot; the gameplay hitbox is unchanged (0.6 x 1.3) and independent of the 2.3-unit-tall art; facing left mirrors the drawing; the toggle restores the placeholder; the preview scene lists animations, loops, pauses, steps frames, changes speed/scale, shows hitbox and pivot. Real keyboard, gamepad and mouse input verified through virtual devices; the user confirmed the controls feel good in the earlier build.

## 3. What is not working / missing
- Only **idle** exists (8 frames, small breathing, one blink). Missing Saiyan animations: run, jump (takeoff, rise, fall, land), dash with smear, shoot, run-and-shoot, hurt, defeat, Super, victory. Until then the sprite slides with procedural squash/lean when moving; sprite art is therefore OFF by default (F9 turns it on).
- **No Cakezilla art** (preview shows "KingCakezilla (no art yet)"), no effects art (stars, dust, impacts, confetti): still procedural placeholders.
- Codex said, and I confirm: the idle motion is subtle; hair is more detailed than ideal for a long production run; scarf is a single ribbon but broad in places.
- No human judgment yet on how the sprite looks in motion in the game.

## 4. Decisions that need review
1. Detail level: the generated style is more detailed (hair strands, shading) than a classic cartoon. Keep it, or ask for simpler hair and bolder outlines before the 100+ frame animations? (Colour drift between frames was real and had to be corrected in processing; more detail means more drift risk.)
2. Is 8 fps for idle right, and should idle motion be exaggerated (bigger squash, bigger scarf and hair follow-through) for the Cuphead feel?
3. Sprite size: 2.33 units tall (hitbox 1.3). Larger or smaller?
4. Boss art route: separate animated parts (cut-out) vs full-frame sprite sheets; how to get the parts clean (generate each part separately)?
5. Colour matching to frame 00 is a correction step (histogram match), not redrawing: acceptable?

## 5. Relevant files
`Tools/sprites/process_frames.py`; `Assets/Scripts/Art/SpriteClips.cs` (ClipPlayer, SpriteLibrary, SpriteFlipbook), `AnimationPreview.cs`; `Assets/Scripts/Player/SaiyanSpriteVisual.cs`; `Assets/Scripts/Editor/SpriteImport.cs`; processed frames `Assets/Resources/Art/Saiyan/idle/` (+ `anim.json`, `library.json`); source `Art/Source/Saiyan/`; briefs `Docs/CODEX_SPRITE_BRIEF.md`, `Docs/CODEX_ROUND3_FEEDBACK.md`; plan `AI_HANDOFF/ART_PIPELINE_PLAN.md`.

## 6. Commit and branch
`claude/milestone-1-greybox` @ `e8b86bd`. Codex's own branch `codex/saiyan-model-sheet` @ `bae0269` holds only its first sheet; later rounds were delivered as files and imported by me.

## 7. Recommended next milestone
Milestone B (core player animation): Codex generates run (10-12 frames), jump takeoff/rise/fall/land, dash with separate smear frames, shoot (with the frame where the star leaves the hand marked for the animation event) and hurt, in batches of one animation at a time with a contact sheet each, using the locked idle as the reference. I process and wire each batch, then turn sprite art ON by default once run, jump, dash, shoot and hurt exist.

## 8. Questions where a second opinion helps
1. Looking at A0 and A4: does the generated Saiyan fit the concept art and the Cuphead-inspired target, and what would make the animation feel more expressive (anticipation frames, smears, exaggeration)?
2. Should hair be simplified for production, and how would you prompt that without changing the face?
3. Best way to get a consistent, large animated boss from an image generator: full-frame sequences, layered parts, or both?
4. Idle breathing is subtle: how much exaggeration is right for a kid-friendly arcade boss game?
