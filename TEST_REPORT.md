# TEST REPORT (Milestone 1, 2026-10-09, Unity 6000.3.9f1, Windows)

Run with `Tools/unity-run.ps1`. Code commit `81b44ff`.

## Automated: EditMode 22/22 passed
Jump timers (buffer, coyote windows, one jump per press); attack selector (never three in a row over 30 seeds x 2000 picks, weights respected, zero weight never picked, all attacks chosen within 60 picks for 200 seeds); boss health (thresholds fire once in order, big hits clamp to the 70/35 lines, an invulnerable boss ignores damage, defeat fires once); player health (hit, invulnerability, death once, granted invulnerability); Super meter; design checks (telegraph minimums, phase lines, arc maths, arena geometry, platforms reachable by the jump).

## Automated: PlayMode 27/27 passed (18 scripted-input runs + 9 real-input runs)
Level builds; player lands; full jump versus short hop height; dash distance and level air dash; fire rate, shot cap and despawn; stars damage the boss and charge Super; Super deals about 12 and spends the meter; phase thresholds protect the boss and the fight can be won with no hazards left; 50 s of attacks vary, never triple and stay under the hazard cap; a stand-still bot is hit but only after the first warning (and not within 0.6 s of it); death then Retry restarts at the checkpoint in the Fight state; pause freezes and resumes; Assist Mode values; plus 5 visual-report runs that produced the screenshots and clips.

## Real input (added after the first hands-on try found dead buttons)
Root cause: the project used the OLD Input Manager (`activeInputHandler: 0`), so the new Input System, which all controls and UI use, was inactive in the built game. Fixed in ProjectSettings and in `ProjectSetup`. Arrow keys were also missing from the bindings; added. 9 new PlayMode tests (`RealInputTests`) drive virtual keyboard, gamepad and mouse devices through the shipped Input Actions asset and the UI input module: keyboard move/jump/shoot/dash/Super/pause, arrow keys, gamepad stick, d-pad, A/X/B/RB/Y/Start, menu navigation and Play by gamepad and by mouse click, Resume by gamepad. The rebuilt exe was also driven with real Windows key presses (Enter started the game, held D moved Saiyan). The keyboard and mouse tests set the editor to route input without Game-view focus; that is a test setting only.

## NOT verified (do not assume these work)
- Any human play: feel, fairness, difficulty, readability at speed.
- A PHYSICAL gamepad and a person pressing the keys (virtual devices and synthetic key presses pass; a real controller and real hands have not been tried yet).
- Audio (generated placeholder tones; not listened to).
- The Windows build beyond a short synthetic-key smoke run (menu to tutorial, running); no human play session yet.
- Performance and frame rate (not measured).
- The Reduced Shake option's effect (the setting exists, the effect was not measured).
- Phase 2 and 3 attacks and the Supreme form (do not exist yet).
- Visual quality against the concept art: reviewed by me from the screenshots only (see `AI_HANDOFF/SCREENSHOTS/VISUAL_REVIEW.md`).
