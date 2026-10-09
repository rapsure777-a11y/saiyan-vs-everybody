# Review: Saiyan model sheet v1 (Codex, 2026-10-09)

Source: branch `codex/saiyan-model-sheet`, commit `bae0269`, file `Art/Source/Saiyan/model_sheet.png` (1536x1024). **Genuinely generated art** (Codex built-in image generation from the two concept images; no edits by hand or script; no private photo, no paid service, per its REPORT.md). **Not yet integrated in Unity.** Not an animation asset: it is a single composite design reference.

## Verdict: strong design reference; revise before animating
What works: the character is on-model against the concept art (tousled dark brown hair, big blue eyes, friendly determined face, blue hoodie with gold crown emblem, oversized white gloves, big blue-and-white sneakers with crown logos, charcoal cargo pants, flowing blue scarf). The side view faces right (the gameplay view) with the scarf streaming behind. All six expressions are distinct and readable (smirk, shout, hurt with a cheek scratch, happy, angry, surprised). Costume and palette are consistent across views.

Problems for production:
1. **Too detailed to repeat across hundreds of frames:** many thin hair strands, fabric folds, gradient shading, scuffs on pants and shoes. Frame-to-frame drift will be likely and expensive to fix. Needs bolder, simpler outline and flat 2-tone cel shading (the classic cartoon look the target calls for).
2. **Opaque cream background with ground shadows:** cannot be used directly; needs transparent or flat magenta, no shadow.
3. **Small face variations between views:** eye size, brow and mouth differ slightly from front to three-quarter view (Codex's own report says the same).
4. **Only static standing poses:** nothing proves the design survives motion. A pose test (run, jump, shoot, dash, hurt) should pass review before any animation frames are produced.
5. Minor: hair highlights are more orange than the concept's darker brown; the profile view hides the chest emblem (acceptable).

Next: Round 2 requests (simplified sheet, clean background, 5-pose test) are appended to `Docs/CODEX_SPRITE_BRIEF.md`. No animation frames until the pose test is reviewed.
