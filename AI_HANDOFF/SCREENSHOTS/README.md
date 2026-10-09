# Visual progress report: milestone M1 (2026-10-09)

**Every image and video in this folder is a real render of the running Unity game** (Unity 6000.3.9f1, URP, 1600x900 JPEG from a 1920x1080 render; clips are 1280x720, 30 fps). They are produced by automated PlayMode runs of the real game with scripted input, not by hand-play and not by any image generator. **No concept art or mock-up is stored here.** The approved concept art is in `References/ConceptArt/` and is labeled as such wherever it is mentioned.

Capture notes (so nothing is misread): the HUD is rendered by temporarily switching its canvas to camera space for the capture, so UI scale can differ slightly from the real overlay. Boss fights are driven by a script (the player is invulnerable in most shots so the full attack set can be photographed), so the images show the visuals, not how hard the fight feels. The clips record on a fixed 1/30 s clock.

To regenerate (also the procedure for every future milestone: change `Tag` in `Assets/Tests/PlayMode/VisualReportTests.cs`, run, encode, commit):
```
powershell -NoProfile -ExecutionPolicy Bypass -File Tools\unity-run.ps1 -Tests PlayMode -Filter "Report_"
powershell -NoProfile -ExecutionPolicy Bypass -File Tools\encode-clips.ps1 -Tag M1_2026-10-09
```
(Then convert PNG stills to JPEG to keep the folder small; the PNGs are not committed.)

## Index (28 stills, 2 clips; representative frames only)
| File | Shows |
|---|---|
| 01_saiyan-closeup-idle | Saiyan placeholder, close-up |
| 04_saiyan-dashing-closeup, 07_saiyan-jumping-closeup | dash and jump poses |
| 02, 03, 05, 06 (tutorial) | Frosting Fields run-up: signs, blocks, shooting targets, checkpoint |
| 10_boss-intro-banner | boss intro (skippable) |
| 11_king-cakezilla-closeup | boss close-up |
| 12_fight-overview-idle-hud, 13_hud-super-ready | fight screen, hearts, Super meter, boss bar with 70/35 percent ticks |
| 20 to 25 | each phase 1 attack: warning frame, then the attack (Cupcake Toss, Hand Slam, Frost Blob) |
| 30_player-shooting-at-boss, 31_super-burst | stars and the Super |
| 40 to 43 | phase 2 and phase 3 (Cakezilla Supreme, currently a gold re-tint with an aura) |
| 50_boss-defeat-sequence, 51_ui-sweet-victory | defeat and victory screen |
| 60_ui-pause-menu, 61_ui-game-over-retry, 62_ui-main-menu | UI |
| video_movement-and-combat.mp4 (8 s) | run, jump, dash, shoot, pop target dummies |
| video_boss-fight-and-phases.mp4 (26 s) | fight with attacks, Super, 70 percent and 35 percent transitions (phase 2 and 3 reuse the phase 1 attacks for now) |

See `VISUAL_REVIEW.md` in this folder for the comparison with the concept art and the list of improvements.
