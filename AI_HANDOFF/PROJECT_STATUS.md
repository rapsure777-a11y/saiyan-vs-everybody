# PROJECT STATUS: Saiyan vs. Everybody!

Last updated: 2026-10-09, end of Milestone 1 build session. Legend used everywhere in AI_HANDOFF: **TESTED** = run in Unity and observed; **WRITTEN** = code exists, not run; **PLANNED** = not started.

## Git
- Private GitHub repo `rapsure777-a11y/saiyan-vs-everybody`. Local: `C:\Users\fence\Projects\SaiyanVsEverybody`.
- Branch `claude/milestone-1-greybox`. Latest code commit `81b44ff`; the report commit follows it (`git log -1` is always the truth). Not merged to `main` (no human playtest yet).
- The child's private photo is NOT in the repo (git-ignored, never copied into the project).

## Milestone 1: player + Frosting Fields + King Cakezilla phase 1
Estimated completion: **about 80 percent** (estimate): the slice is playable end to end in automated runs; missing a human playtest, a Windows build, real art and real audio.

| Area | State |
|---|---|
| Project compiles in Unity 6000.3.9f1 (URP, Input System 1.20, uGUI) | **TESTED** (batch compile and tests) |
| Player: run, variable jump, coyote 0.1 s, buffer 0.12 s, dash with cooldown (+0.12 s i-frames), 3 hearts, 1 s post-hit invulnerability and knockback, repeatable stars (pooled, capped), Super meter and big star burst | **TESTED** by scripted PlayMode runs (jump heights, dash distance, air dash level, fire rate, caps, cleanup, boss damage, Super spends meter) |
| Boss state machine (Intro, Idle, Telegraph, Attack, Recovery, Transition, Defeated), weighted attacks without triple repeats, 70/35 percent thresholds with clamping and invulnerable transitions, hazard cap 12, three phase 1 attacks (Cupcake Toss, Hand Slam, Frost Blob), defeat and victory | **TESTED** (EditMode logic, PlayMode runs: full fight 120 HP to victory, 50 s attack cycle, stand-still bot gets hit only after the first warning) |
| Death, Retry from checkpoint, Pause, Assist Mode (5 hearts, boss x0.66), Reduced Shake setting | **TESTED** (Retry reload, pause freeze/resume, assist values); Reduced Shake is only set, not measured |
| Frosting Fields tutorial and arena, HUD, menus, scenes in build settings | **TESTED** (renders, screenshots reviewed by me); scenes built by `Saiyan > Setup Project` |
| Input Actions asset with keyboard and gamepad bindings | **TESTED with virtual keyboard, gamepad and mouse** (9 tests) and by synthetic key presses in the built exe; a PHYSICAL gamepad is untested. A first hands-on try found dead buttons (project used the old Input Manager); fixed |
| Placeholder audio cues (generated tones) | WRITTEN; **audibility and balance UNTESTED** (I cannot hear it) |
| Windows player build | **BUILT** (91 MB, `Builds/Windows/`, not committed); launched and driven with synthetic key presses, human play pending |
| Phase 2 and 3 attacks, Cakezilla Supreme transformation | PLANNED (transitions already fire and tint the boss; the attack pool is still the phase 1 set) |
| Real art, frame-by-frame animation, real music/SFX | PLANNED (see ART_ASSETS.md) |

## Tested in Unity
EditMode **22/22** passed; PlayMode **27/27** passed (13 gameplay, 9 real-input, 5 visual-report runs). Details in `TEST_REPORT.md`. Visual evidence: `AI_HANDOFF/SCREENSHOTS/` (28 stills, 2 clips, real renders).

## Human feedback so far
- 2026-10-09: first hands-on try found dead input (fixed). After the fix the user reported: "Controls work good" (which device was not stated; a physical gamepad is still unconfirmed). No feedback yet on jump/dash feel, boss difficulty, visuals or sound.

## Tuning after first play (2026-10-09)
User feedback: fight "pretty easy", dash "could be slightly longer", controls snappy. Changes: dash 0.20 s to 0.26 s (about 3.6 units), boss health 120 to 180, boss idle/recovery shorter (phase 1: 0.8 s/0.7 s), Cupcake Toss always 2-3 cupcakes, shockwave speed 8. Tests updated to read numbers from the tuning; EditMode 22/22, PlayMode 27/27. Difficulty is NOT yet re-judged by a human.

## Known bugs and open issues
- No human has played it. Feel (jump, dash, shot rate, boss pacing), fairness and difficulty are unjudged beyond the scripted checks.
- Visuals are crude placeholders compared with the concept art (see `SCREENSHOTS/VISUAL_REVIEW.md`).
- Phase 2/3 reuse phase 1 attacks.
- A boss fight takes about 120 hits; at about 9 shots/s that is roughly 14 s of continuous hits before phases and dodging, which may be short.
- Unity batch runs of PlayMode can hang if a test waits on scaled time while paused; the tests were fixed, and `unity-run.ps1` has a timeout.
- Messages that asked for these reports arrived cut off mid-sentence twice ("If Unity is", "If"); I proceeded on the complete parts. Please resend anything missing.

## Next recommended steps
1. Human playtest on the PC (keyboard and gamepad): jump and dash feel, boss difficulty, readability. Capture notes.
2. `Saiyan > Build Windows Player`, run the exe once.
3. Decide on art: real sprite sheets for Saiyan and Cakezilla (see VISUAL_REVIEW priorities).
4. Milestone 2: phase 2 attacks (Rolling Donuts, Sprinkle Rain, Layer Pop) and phase 3 (Candle Blast Line, Cupcake Swarm, Mega Frost Beam, Supreme form).
