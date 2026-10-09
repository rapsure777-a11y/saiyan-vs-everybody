# REVIEW BRIEF FOR CHATGPT (read this instead of the repo)

Report time: 2026-10-09, mid Milestone 1. Branch `claude/milestone-1-greybox`, code commit `3b07aab` (reports committed right after; `git log -1` is truth). Status words: TESTED = run in Unity; WRITTEN = code only; PLANNED.

## 1. What changed since the last report
First report. Project created: Unity 6000.3.9f1 (URP package, Input System, uGUI), private git repo, player module and boss data/hazard code written.

## 2. What is currently working
Nothing is verified. Nothing has been compiled or run in Unity yet.

## 3. What is not working / missing
Boss controller and attacks, level, HUD, menu, scenes, tests, `.inputactions` asset: not written yet. No art beyond procedural placeholders. Phases 2 and 3 not started.

## 4. Decisions that need review
- Procedural placeholder art (no sprite sheets exist; concept art is posters).
- One runtime assembly with folders/namespaces instead of many asmdefs.
- Dash has 0.12 s of invulnerability (of 0.2 s) and Super gives 1 s invulnerability. Too generous or right?
- Boss clamps a big hit to the 70/35 percent line so transitions can never be skipped.
- Boss health 120, shot damage 1 at about 9 shots/s, Super 12; Assist Mode = 5 hearts and boss x0.66.
- Every attack: marker and sound at least 0.6 s before damage; max 12 hazards alive.
- Phase 2/3 reuse the phase 1 attack pool at first (transitions still happen).

## 5. Relevant files
`Assets/Scripts/Player/` (PlayerController, PlayerHealth, PlayerShooter, StarShot, SuperMeter, PlayerVisual, JumpTimers, PlayerIntent), `Assets/Scripts/Boss/` (BossTuning, BossHealth, AttackSelector, Hazards), `Assets/Scripts/Core/` (PlaceholderArt, AudioHooks, Fx, Pool, GameSettings), `Docs/BUILD_BRIEF.md`, `AI_HANDOFF/*`.

## 6. Commit and branch
`claude/milestone-1-greybox` @ `3b07aab` (WIP).

## 7. Recommended next milestone
Finish M1: boss state machine + Cupcake Toss, Hand Slam, Frost Blob; Frosting Fields tutorial and arena; HUD/pause/retry/win; then compile, EditMode (selector never 3x repeat, thresholds, jump timers, super meter) and PlayMode (scene loads, jump height, shots cleaned up, boss phase thresholds, retry) tests, then a human playtest. Only then M2 (phases 2 and 3).

## 8. Questions for a second opinion
1. For a kid-friendly boss, is 120 HP at about 9 shots/s (about 13 s of continuous hits) a sensible fight length, or should phase 1 alone be about 30-40 s?
2. Is a shockwave the player must jump a good phase 1 difficulty, or should the first attack set avoid jump-timing checks?
3. Procedural placeholders now vs waiting for real sprites: which keeps momentum with the least rework?
4. Any fairness traps you see in: hand-slam column (floor to 5 u tall, so a platform does not protect you) and the puddle (3.5 s)?
