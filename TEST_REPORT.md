# TEST REPORT (Milestone 1, 2026-10-09, Unity 6000.3.9f1, Windows)

Run with `Tools/unity-run.ps1`. Code commit `81b44ff`.

## Automated: EditMode 22/22 passed
Jump timers (buffer, coyote windows, one jump per press); attack selector (never three in a row over 30 seeds x 2000 picks, weights respected, zero weight never picked, all attacks chosen within 60 picks for 200 seeds); boss health (thresholds fire once in order, big hits clamp to the 70/35 lines, an invulnerable boss ignores damage, defeat fires once); player health (hit, invulnerability, death once, granted invulnerability); Super meter; design checks (telegraph minimums, phase lines, arc maths, arena geometry, platforms reachable by the jump).

## Automated: PlayMode 18/18 passed (scripted input, real game objects)
Level builds; player lands; full jump versus short hop height; dash distance and level air dash; fire rate, shot cap and despawn; stars damage the boss and charge Super; Super deals about 12 and spends the meter; phase thresholds protect the boss and the fight can be won with no hazards left; 50 s of attacks vary, never triple and stay under the hazard cap; a stand-still bot is hit but only after the first warning (and not within 0.6 s of it); death then Retry restarts at the checkpoint in the Fight state; pause freezes and resumes; Assist Mode values; plus 5 visual-report runs that produced the screenshots and clips.

## NOT verified (do not assume these work)
- Any human play: feel, fairness, difficulty, readability at speed.
- Real keyboard and gamepad input (all tests inject input through `ScriptedIntent`; the Input Actions asset loads, but no person pressed its bindings).
- Audio (generated placeholder tones; not listened to).
- Windows player build (never built).
- Performance and frame rate (not measured).
- The Reduced Shake option's effect (the setting exists, the effect was not measured).
- Phase 2 and 3 attacks and the Supreme form (do not exist yet).
- Visual quality against the concept art: reviewed by me from the screenshots only (see `AI_HANDOFF/SCREENSHOTS/VISUAL_REVIEW.md`).
