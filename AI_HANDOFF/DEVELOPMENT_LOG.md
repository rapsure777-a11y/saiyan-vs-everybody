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
