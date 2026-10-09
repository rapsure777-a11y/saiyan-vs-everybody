# Brief for Codex: Saiyan sprite art (step 0: model sheet, then idle animation)

You are producing **original 2D cartoon sprite art** for the Unity game *Saiyan vs. Everybody!* (repo `rapsure777-a11y/saiyan-vs-everybody`). Claude handles all Unity integration; **you produce the images only**. Read this whole file first. Work on a new branch named `codex/saiyan-model-sheet` (never commit to `main` or `claude/*` branches).

## Quality target
A polished hybrid classic-and-modern cartoon look with the expressive, hand-animated feel associated with Cuphead, but **original characters and artwork** (no traced or copied Cuphead, Crash or other copyrighted designs). Clean ink outline, flat cel colors with simple 2-3 tone shading, strong readable silhouettes.

## Visual source of truth (read these images first)
- `References/ConceptArt/02_Saiyan_vs_King_Cakezilla.png`: main reference for Saiyan's face, proportions and costume.
- `References/ConceptArt/01_Game_Key_Art.png`: additional views of Saiyan, mood and palette.
These are posters, not sprites. Do not just cut the character out and stretch it: draw/generate real new poses.

## Character: Saiyan (preserve exactly)
Youthful cartoon boy; short tousled dark brown hair; large expressive blue eyes; confident, friendly, determined face; blue hoodie with a small gold crown emblem on the chest; big white cartoon gloves; oversized blue-and-white sneakers; dark cargo pants; a flowing blue scarf that trails behind. Gameplay view: side view facing **right**.

## HARD RULES
1. **Never use, request, upload, or commit the private photo** (`References/PrivatePhoto/` is git-ignored and must stay that way). Use only the two concept images above.
2. **Do not purchase services or incur API charges.** If your environment has no image-generation capability, say so plainly and stop; **do not substitute programmatically drawn geometric shapes** and call them sprites.
3. Do not modify anything under `Assets/Scripts`, `Assets/Tests`, `ProjectSettings` or `Packages`. Only add files under `Art/Source/` and the report file named below.
4. Do not claim an image works until you have looked at it. Reject and redo frames with: changing faces, different outfit or colors, altered hair, extra or missing fingers, flickering outlines, background remnants, misalignment, or changing proportions.

## Deliverable 1: character model sheet (do this first, then stop for review)
One image `Art/Source/Saiyan/model_sheet.png` on a flat plain background containing: side view facing right (the gameplay pose, relaxed standing), front view, three-quarter view, and 6 face expressions (smirk, shout, hurt, happy, determined, surprised). Same costume in every view. Also `Art/Source/Saiyan/model_sheet_notes.md` listing the exact colors you used (hex) for hair, skin, eyes, hoodie, gold emblem, gloves, shoes, pants, scarf.
**Stop after Deliverable 1 and wait for approval** (the user will review it). Do not generate animation frames from an unapproved sheet.

## Deliverable 2 (only after the sheet is approved): `idle` animation, 8 frames
Loop-friendly (frame 07 leads back into frame 00): gentle breathing, slight body bob with squash and stretch, hair tuft and scarf follow-through, glove sway, and a blink on one frame. Keep the feet planted in the same spot in every frame.
- Files: `Art/Source/Saiyan/idle/saiyan_idle_00.png` ... `saiyan_idle_07.png`.
- Each frame: **1024x1024 PNG**, character facing right, full body visible with padding, **same camera distance and same character height in every frame**, feet near the bottom center, nothing cropped.
- **Background:** transparent if your tool supports it; otherwise flat pure magenta `#FF00FF` (no gradients, no shadows, no scenery). Do not use magenta anywhere in the character.
- Also produce `Art/Source/Saiyan/idle/contact_sheet.png`: all 8 frames in one row on a neutral gray background, so a reviewer can judge consistency in one look.

## Naming and layout for later animations (so Claude's importer works unchanged)
`Art/Source/Saiyan/<animation>/saiyan_<animation>_<NN>.png`, two-digit zero-based frame numbers in play order. Extra effect frames: `saiyan_<animation>_smear_<NN>.png`. Animations requested later (frame targets): idle 8-12, run 10-12, jump takeoff 4-6, rise/fall 4-6, landing 4-6, dash with smear 6-8, shoot 6-8, run-and-shoot 8-12, hurt 5-7, defeat 10-14, super 12-16, victory 12-16.

## Report (required)
Commit your files and add `Art/Source/Saiyan/REPORT.md` stating: what tool or method generated the images, whether any frame was edited by hand or by script, which frames you rejected and why, and anything you could not do. Push the branch and tell the user the branch name and the last commit SHA. Be explicit about what is genuinely generated art versus anything placeholder.


---

# ROUND 2 (after Claude's review of the first model sheet, 2026-10-09)

The first sheet (`Art/Source/Saiyan/model_sheet.png`, commit bae0269 on `codex/saiyan-model-sheet`) is a strong design: costume, scarf, crown emblem, gloves, big blue-and-white shoes, hair and the six expressions are all on-model and read clearly. Thank you. Keep this character. Two things must change before we animate, and one thing must be tested first.

## A. Simplify for animation (production model sheet)
Hundreds of frames will have to stay consistent, so the drawing needs to be cheaper to repeat exactly:
- **Thicker, more uniform ink outline** (the classic cartoon look); fewer thin hair strands (draw the hair as about 8-10 bold tufts with a consistent shape); flat cel shading with at most two tones (base + one shadow), no gradients, no fabric-texture noise, no dirt/scuffs on the pants or shoes.
- Keep the same face design, proportions, colors and costume. Keep the scarf, crown emblem (chest and shoes), cargo pockets.
- The hurt expression's cheek scratch is fine as an optional mark; do not add other marks.

## B. Clean background
Transparent PNG with **no ground shadow**, or flat pure magenta `#FF00FF` with no shadow, no gradient, no cream tint. Anything the pipeline has to remove by guesswork risks eating the outline.

## C. Pose test BEFORE the idle animation (stop for review again)
Produce `Art/Source/Saiyan/pose_test.png` (or one PNG per pose), side view facing right, **same scale, same outline, same colors as the simplified sheet**, on the clean background:
1. mid-stride running pose, 2. jump rising pose (arms up, scarf streaming), 3. shooting pose (front glove thrust forward, a small star leaving it), 4. dash pose (leaning forward, with one speed-smear version), 5. hurt pose (recoiling).
Purpose: prove the character survives motion without the face, hands, shoes or hair drifting. Do not start the 8-frame idle until the pose test is reviewed.

## D. Report
Update `Art/Source/Saiyan/REPORT.md` (method, edits, rejected outputs, remaining flaws) and push the same branch. If a pose comes out inconsistent, regenerate it instead of keeping it.
