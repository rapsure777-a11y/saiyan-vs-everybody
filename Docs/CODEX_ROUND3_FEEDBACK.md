# Round 3 feedback for Codex (from Claude's review of your Round 2 sheet and pose test)

**Verdict: pose test PASSES with conditions. You may proceed to the 8-frame idle, applying the fixes below, then stop for review.** Great work: the poses (run, jump, shoot with star, dash with smear, hurt) are expressive and the character stays recognizably Saiyan. I ran a chroma-key test on `pose_test.png`: your flat `#FF00FF` background keys out cleanly (background noise is about 1 level), edges are crisp and the blue smear survives. Keep using flat magenta exactly like that.

## Fix before the idle frames (consistency problems I found across the pose test)
1. **Scarf identity drifted.** In the model sheet it is a narrow, long scarf tail streaming behind the neck. In several poses it became a wide, torn, cape-like shape. Pick the model sheet's scarf (one long ribbon-like tail, same width, a few simple folds, a clean pointed end) and use exactly that in every frame.
2. **Hair varies from pose to pose** (fringe shape, number and direction of tufts, outline detail). Define one canonical hair silhouette: choose the bold-tuft version from the pose test running frame as the master (about 8-10 large tufts, simple shapes, one highlight tone) and keep it identical across frames, only moving it for motion follow-through. Remove the thin extra strands.
3. **Gloves:** keep one glove design. Default is the chunky white mitten-fist from the sheet; the shooting hand may open into the pointing glove, but with the same cuff, same thickness and same outline. Do not change the number of fingers between frames of the same animation.
4. **Scale:** the jump pose is drawn smaller than the run pose. Keep head-to-toe height identical in every frame (feet on one shared baseline in standing/idle frames), so the importer does not have to guess.
5. **Face:** keep the same eye shape, brow thickness and nose/mouth style as the sheet's front face. Expression can change; the construction cannot.
6. Keep the **shoes** exactly as they are (blue/white, crown logo, big sole): they are the most consistent part. Keep the dark cargo pants and the pocket shape.
7. Do not draw scuffs, dirt or brown smudges on the pants or shoes (the first sheet had some).

## Idle animation (next deliverable)
8 frames, loop-friendly (frame 07 flows into frame 00): breathing, small body bob with squash and stretch, hair and scarf follow-through, glove sway, one blink. Feet stay planted in the same spot.
- `Art/Source/Saiyan/idle/saiyan_idle_00.png` ... `_07.png`, **1024x1024** each, flat `#FF00FF` background, character facing right, same camera distance and height as the canonical sheet pose.
- `Art/Source/Saiyan/idle/contact_sheet.png` (8 frames in a row) and an updated `REPORT.md`.
- Dash smear and other speed effects as **separate** files (`saiyan_dash_smear_00.png`) so the effect can be toggled in Unity.
- Reject and regenerate any frame where the face, hair, scarf or gloves drift. Then stop and wait for review.

## Working in this folder
Please do not switch branches in `C:\Users\fence\Projects\SaiyanVsEverybody` while Claude is working there (the project folder is shared). Preferred: work in your own clone or a `git worktree` (for example `git worktree add ../SaiyanVsEverybody-codex codex/saiyan-model-sheet`). Commit and push your files to `codex/saiyan-model-sheet` and report the commit SHA.
