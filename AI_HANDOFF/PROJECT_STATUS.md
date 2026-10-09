# PROJECT STATUS: Saiyan vs. Everybody!

Last updated: 2026-10-09 (report written mid-Milestone 1). Legend used everywhere in AI_HANDOFF: **TESTED** = actually run in Unity and observed; **WRITTEN** = code exists but has not been compiled or run; **PLANNED** = not started.

## Git
- Repo: private GitHub repo `saiyan-vs-everybody` (account rapsure777-a11y). Local: `C:\Users\fence\Projects\SaiyanVsEverybody`.
- Branch: `claude/milestone-1-greybox`. Latest code commit: `3b07aab` (WIP scaffold). Report commits follow it; `git log -1` is always the truth.
- The private photo `References/PrivatePhoto/Saiyan_Original_Reference.jpg` is NOT in the repo (git-ignored, and I never copied it into the project folder).

## Milestone 1: "first playable" (greybox player + Frosting Fields + King Cakezilla phase 1)
Estimated completion: **about 35 percent written, 0 percent tested** (estimate, not a measurement).

| Area | State |
|---|---|
| Unity project scaffold, asmdefs, packages (Input System 1.20, URP 17.3, uGUI, test framework), .gitignore | WRITTEN, never opened in Unity |
| Player: input (Input Actions with keyboard+gamepad), run, variable jump, coyote 0.1, buffer 0.12, dash with cooldown and short i-frames, hearts, post-hit invulnerability and knockback, star shots (pooled, capped), Super meter and big star burst, placeholder shape-built Saiyan | WRITTEN, not compiled |
| Boss data: BossTuning (ScriptableObject), BossHealth with 70/35 thresholds and clamp-on-threshold, AttackSelector (weighted, never the same attack 3 times in a row), hazards (slam column, shockwave, puddle, arcing cupcake), telegraph markers | WRITTEN, not compiled |
| Boss controller state machine, 3 phase-1 attacks, boss visuals | NOT YET WRITTEN (in progress) |
| Frosting Fields level + tutorial, camera, HUD/pause/retry/win screens, main menu scene, scene builder | NOT YET WRITTEN |
| Input Actions asset file (.inputactions) | NOT YET WRITTEN (code falls back to built-in default bindings, which also covers the brief's controls) |
| EditMode and PlayMode tests | NOT YET WRITTEN |
| Phase 2 and 3 attacks, Cakezilla Supreme transformation | PLANNED (milestone 2; phase thresholds and transition exist in the design only) |

## Tested in Unity so far
Nothing. Unity 6000.3.9f1 is installed on this PC and will be used for compile, EditMode and PlayMode runs, but no run has happened yet for this project.

## Known bugs and blockers
None observed yet (nothing has been run). Expect compile errors on the first Unity import; they will be fixed before any "works" claim.
- Concept art provided is poster art, not sprites. All visuals are procedural placeholders (see ART_ASSETS.md).
- The message that requested this reporting system was cut off mid-sentence ("If Unity is"); assumed meaning: if Unity is unavailable, do not claim compile/run. Please resend the rest if it said something else.

## Next recommended steps
1. Finish boss controller + 3 attacks + visuals, level, HUD, scene builder.
2. First Unity compile, fix errors, EditMode tests, then PlayMode tests with screenshots.
3. User plays it and reports feel (jump, dash, shooting, boss fairness).
4. Milestone 2: phases 2 and 3.
