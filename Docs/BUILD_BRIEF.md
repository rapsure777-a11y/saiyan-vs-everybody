# AGENT BRIEF: Saiyan vs. Everybody! — Unity playable vertical slice

You are the lead Unity gameplay engineer and technical artist. Build an ORIGINAL, family-friendly, Windows PC 2D boss-fight action platformer vertical slice called **Saiyan vs. Everybody!** using the visual references packaged alongside this brief. The lead character is a stylized cartoon child based on a private reference photo; do not upload or commit that photograph to any remote/public repository.

## Reference assets (inspect these before implementation)
- `References/ConceptArt/01_Game_Key_Art.png` — title, mood, palette, multiple bosses, overall game fantasy.
- `References/ConceptArt/02_Saiyan_vs_King_Cakezilla.png` — primary visual target for Saiyan, first boss, candy kingdom environment.
- `References/PrivatePhoto/Saiyan_Original_Reference.jpg` — PRIVATE visual likeness reference ONLY; never put this in a public repository, build, analytics, or online service. Ask permission before using any external image-processing services. Add `References/PrivatePhoto/` to `.gitignore`.

Concept art is illustrative, NOT production-ready character sprites, sprite sheets, rigging, animation data, layered art, or game-ready 3D models. Do not simply place the full poster as a gameplay background and call it complete. Build original separated gameplay assets/placeholder visuals first and use the reference images for silhouette, color, character consistency and polish. No copyrighted Cuphead/Crash assets or traced characters, sound, or stages.

## Target and tooling
- Unity 6 LTS if already installed; otherwise detect the available compatible Unity version and report it. 2D URP preferred if supported. C# scripts, Input System with gamepad plus keyboard, 16:9 camera, 1080p Windows first.
- Start by inspecting the repo and Unity installation. Do not overwrite existing work; branch for new changes when using an existing project. State precisely what was tested in the Unity Editor versus what was only statically checked.
- If running in a cloud environment without Unity, create a clean, importable project and C# scripts, but DO NOT falsely claim it compiles or runs. Provide exact local Unity import/test instructions and do not merge untested gameplay into a working branch without approval.

## Desired experience
A 30–60-second playful tutorial run-up followed by one exciting, easy-to-read single-screen boss fight against **King Cakezilla** in the Frosting Fields candy kingdom. Main focus: tight, responsive movement and funny Cuphead-inspired *energy* with an original clean modern/classic cartoon presentation. Saiyan is small, brave, expressive: tousled dark brown hair, blue eyes, blue hero hoodie, small gold crown badge, white cartoon gloves, blue shoes, dark pants. Cakezilla: enormous layered birthday cake, pink frosting, red cherry-like nose, candle/crown topper, giant white gloves and theatrical expressions.

## Player mechanics
- Horizontal run; variable-height jump; coyote time ~0.1 sec; jump buffer ~0.12 sec.
- Air/ground dash with cooldown and readable cartoon puff; define and document invulnerability behavior, if any.
- Fast repeatable horizontal star shots; facing-direction aiming initially.
- 3 hearts and temporary ~1 second post-hit invulnerability, visual blink and knockback.
- Super meter charged by hits or pickups, activated to produce a large but fair star burst.
- Gamepad: left stick/d-pad move, A jump, X shoot, B or RB dash, Y super. Keyboard: A/D or arrows, Space jump, J shoot, K dash, L super. Configurable through Input Actions.

## Boss — exact design intent
One-screen arena with a flat floor and 1–2 low platforms. Cakezilla's attacks must all provide clear warning colors, positions and sound cues before damage. Phase thresholds at 70% and 35%; boss never takes cheap hits during transition cinematics. Attack state machine: Intro -> Idle -> Telegraph -> Attack -> Recovery -> Transition -> Defeated; weighted phase-aware selection; avoid identical attacks three times in a row.

Phase 1 (100–70%): Cupcake Toss (1–3 parabolic projectiles), Hand Slam (marked floor zone + traveling shockwave), Frost Blob (marked landing zone and short-lived puddle).

Phase 2 (70–35%): retain a rotating selection of phase 1 attacks, add Rolling Donuts (jumpable enemies), Sprinkle Rain (marked columns with a safe lane), Layer Pop (two levels of candy shots, clear safe response).

Phase 3 (35–0%): visually transform into Cakezilla Supreme; add Candle Blast Line (sequential telegraphed floor bursts), Cupcake Swarm (destroyable flyers), Mega Frost Beam (clear, dodgable sweep). Keep projectile counts bounded, avoid unavoidable attack combinations, and expose attack tuning fields in ScriptableObjects or inspector.

Recommended initial tunings, not absolutes: 3 player hearts; movement 6 world units/s; jump velocity 12 (tune with gravity); dash 14 units/s for 0.2 s; shot damage 1; super damage 12; post-hit invuln 1 s. Prioritize fun feel and fairness over the exact numbers.

## Scenes and UX
- `MainMenu`: title, Play, Controls, Quit.
- `Level01_FrostingFields`: brief jump/dash/shoot tutorial with signs and small obstacles; boss entrance, under-8-second skippable intro, fixed camera, phase transitions, and a victory sequence.
- UI: three clear hearts, super meter, boss name and health bar, one or two controller hints, Retry/Pause, win screen reading `SWEET VICTORY!` and `Golden Candle Star` reward.
- Checkpoint immediately before fight; quick retry; accessibility option for easier boss damage/extra hearts, readable attack telegraphs, reduced screen shake.
- Audio architecture with placeholder sound hooks; do not claim finished music/voice assets unless present.

## Production sequence and validation gates
1. Inventory existing project / editor version, define dependencies, create the project structure and a short DEVELOPMENT_LOG.md.
2. Create functional greybox player/controller/input/camera and simple tutorial.
3. Implement health, stars, hit feedback, boss state machine and fully playable PHASE 1.
4. Run editor tests/play mode locally if possible; verify all controls, player death/retry, projectile cleanup, no unavoidable hits. Fix before advancing.
5. Add phase 2 and 3 with transitions, tune readability and difficulty.
6. Build original stylized art/animations from production-ready assets or documented placeholders, guided by the packaged images; preserve recognizable character features without embedding the private photo in runtime files.
7. Polish UI, boss intro, music/SFX integration points and victory/retry loop.
8. Produce build instructions, known issues, controls, and a test report distinguishing verified from unverified.

## Deliverables
- Unity project (not just pseudocode), importable and structured under Assets/Art, Animations, Audio, Prefabs, Scenes, Scripts, UI, Settings.
- A playable Windows build if Unity is available; otherwise an editor-ready project + exact build steps.
- `README.md`, `DEVELOPMENT_LOG.md`, `TEST_REPORT.md`, `.gitignore` excluding private photo and generated folders, plus documented inspector tuning values.
- Commit logical steps if repo exists. Never publish the child's photo publicly.
- First response: briefly report repository/editor discovery and your proposed first implementation milestone; then start building rather than re-asking settled design decisions.
