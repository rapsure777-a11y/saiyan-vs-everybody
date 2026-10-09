# DEVELOPMENT LOG (newest at the bottom)

Legend: TESTED / WRITTEN / PLANNED (see PROJECT_STATUS.md).

## 2026-10-09: kickoff
- Received the build brief (`Docs/BUILD_BRIEF.md`) and starter zip. Inspected both concept images: key art (cake boss, pirate-octopus, arcade-cabinet boss, grass platformer, spiky ball, coins) and the Cakezilla poster (layered pink cake, red nose, giant white gloves, candle crown, candy castles). The private photo was NOT opened by me or copied; the concept art already carries the likeness cues the brief lists.
- Discovery: Unity 6000.3.9f1 and 6000.6.4f1 installed; chose 6000.3.9f1 (same as the other project, known to work here). Pipeline: URP package referenced; sprites render with the default unlit sprite material (no 2D lights needed). No pre-existing project to protect; new folder `C:\Users\fence\Projects\SaiyanVsEverybody`, new git repo, branch `claude/milestone-1-greybox`.
- Decision: art is procedural placeholder shapes in the concept-art palette (`Core/PlaceholderArt.cs`), because no sprite sheets exist. Scenes will be built by an editor script plus runtime builders, so the project does not depend on hand-wired scene references.
- Decision: single runtime assembly `Saiyan.Runtime` with folders/namespaces (Core, Player, Boss, Level, UI) to keep compile times and asmdef wiring simple; Editor and test assemblies are separate.
- Decision: dash gives about 0.12 s of invulnerability at the start of its 0.2 s (documented, tunable `dashInvulnerability`, 0 turns it off). Super gives 1 s of invulnerability while it fires.
- Decision: Assist Mode = 5 hearts and boss health x0.66; separate Reduced Shake option.
- WRITTEN (commit `3b07aab`, not compiled): scaffold, Core utilities (settings, pool, audio hooks with generated placeholder tones, FX, camera shake), player module, boss tuning/health/selector/hazards.
- Tooling note: bash heredocs with quotes break in this environment, so files are written with the file tool instead.

## 2026-10-09 (later): Milestone 1 built and tested
- First Unity compile found one error (Input System AddAction overload); fixed. Project setup runs headless: `Saiyan > Setup Project` (URP asset, Input Actions asset, tuning asset, both scenes, build settings).
- WRITTEN then TESTED: boss controller and three phase 1 attacks, tutorial and arena, HUD/menus/pause/retry/victory, camera rig, test suites. Commits `6be6579`, `81b44ff`.
- Bugs found by the tests and fixed: (1) player stars never moved: kinematic MovePosition does not accumulate between physics steps, now velocity-driven; (2) the Super meter listened to a static event, so every meter reacted to every star, now per-shooter; (3) test worlds leaked across tests (Retry loads the level scene as the only scene), found by a "one world only" assertion and fixed with a shared cleanup; (4) a PlayMode test hung forever waiting on scaled time while paused (my own comment had also commented out the unpause call); (5) the first screenshot test hung in batch mode (WaitForEndOfFrame), replaced by camera render-to-texture.
- Visual fixes after reviewing real frames: hearts redrawn (diamond plus two circles), sign boards sized to their text, boss gloves moved off the face, Saiyan visual scaled 1.3x, Saiyan and stars drawn in front of boss gloves and hazards.
- Visual reporting added (`AI_HANDOFF/SCREENSHOTS/`): `VisualReportTests` renders the real game to stills and frame sequences, `Tools/encode-clips.ps1` encodes MP4 with ffmpeg. 28 stills (JPEG, about 11 MB total with the clips) and 2 clips kept; PNGs and frame folders are not committed.
- Test results: EditMode 22/22, PlayMode 18/18 (see TEST_REPORT.md).
- Decision: the AttackCycle test no longer asserts that all three attacks appear within 50 s (a random order can miss one); that guarantee is covered deterministically by AttackSelectorTests.EveryAttackGetsChosenWithinAReasonableNumberOfPicks (200 seeds x 60 picks).

## 2026-10-09 (evening): first hands-on try, dead input
- User ran the first Windows build: buttons did not work (gamepad untried). Root cause found: ProjectSettings had activeInputHandler 0 (old Input Manager), so the Input System was inactive in the player. All earlier tests injected input through ScriptedIntent, which bypassed the problem, and the report had flagged real input as unverified.
- Fixes: activeInputHandler set to 1 (and enforced by ProjectSetup), arrow-key bindings added. Added RealInputTests (9) using virtual keyboard, gamepad and mouse. Rebuilt the exe and drove it with real Windows key presses. EditMode 22/22, PlayMode 27/27.

## 2026-10-09 (night): feedback tuning and art-pipeline planning
- User feedback after playing: controls snappy and good, fight too easy, dash slightly longer. Applied: dash 0.26 s, boss 180 HP, quicker rhythm, 2-3 cupcakes, shockwave 8. The serialized tuning asset had to be regenerated (it held the old values and overrides code defaults).
- Art upgrade request received. Surveyed the environment: no image-generation tool, no API keys, no local diffusion install; RX 7900 XT 20 GB available. Wrote `AI_HANDOFF/ART_PIPELINE_PLAN.md` (options, recommendation, spec) and a self-contained `Docs/CODEX_SPRITE_BRIEF.md` for Codex. No sprites generated; placeholders remain.

## 2026-10-09 (late): Milestone A, Saiyan idle sprite art integrated
- Codex (image generation) delivered, after two review rounds from me: model sheet, 5-pose test, 8 idle frames (genuine generated art; idle frames processed from 1254 px magenta-keyed sources to 1024 px by Codex).
- Measured the frames before accepting them: feet baseline within 1 px, height within 4 px, feet position within 2 px, area within 3 percent; but a slow colour drift (hoodie G/B +25, hair R -25 from frame 00 to 04) that would flicker at the loop point. Corrected in processing by histogram matching to frame 00 (hoodie spread 25 to 1.6, hair 25 to 5); no redrawing.
- Built: `Tools/sprites/process_frames.py` (key-out with un-mixing, colour match, bleed fix, shared feet pivot, 512 px export at 200 ppu, anim.json and library.json, contact sheet, GIF), `SpriteImport` AssetPostprocessor (automatic sprite settings and pivot), `SpriteClips.cs` (ClipPlayer with event-safe timing, SpriteLibrary, SpriteFlipbook with frame events), `SaiyanSpriteVisual` (state-based clip choice with idle fallback, squash/lean/blink, F9 toggle, placeholder kept as fallback), `AnimationPreview` scene (F10 from the menu).
- Tests: EditMode 27/27 (new ClipPlayer tests), PlayMode 36/36 (new SpriteArt tests: library and import settings, visual shows and animates and hides the placeholder, toggle, feet on floor and hitbox unchanged, facing mirror, preview scene).
- Bugs found and fixed: library.json missing (I had added manifest writing after running the tool); a test cleanup left the preview scene alive so later screenshots showed a ghost sprite and a green hitbox box behind the placeholder (found by looking at the before shot; fixed by unloading the preview scene in cleanup and regenerating all screenshots); float-boundary assertions in a unit test.
- Decision: sprite art defaults OFF in normal play until run/jump/dash/shoot/hurt exist (otherwise the player would see a mostly static drawing sliding around); F9 toggles it.
- Process note: Codex worked in the same project folder and switched branches under me twice; a worktree or separate clone is safer.
