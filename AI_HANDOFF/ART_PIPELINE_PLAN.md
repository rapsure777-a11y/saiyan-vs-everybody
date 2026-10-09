# Art upgrade plan: sprites and animation (written 2026-10-09)

Status: **no production sprites exist yet. Nothing in this file has been generated.** The current visuals are procedural placeholder shapes and are not a substitute for finished cartoon sprites.

## 1. Tool investigation (what this environment can and cannot do)
| Capability | Found |
|---|---|
| Image generation inside my toolset | **None.** No image-generation tool is available to me. The only attached design service (Canva) needs your login and is not a consistent-sprite generator. |
| Image-generation API keys on the PC | None found (checked variable names only). No paid service was used or will be without approval. |
| Local image generation (Stable Diffusion, ComfyUI, etc.) | **Not installed.** No models on disk. Hardware: AMD Radeon RX 7900 XT (20 GB VRAM), 31 GB RAM, 127 GB free disk. That is enough for SDXL-class models, but it needs a large install (Python 3.10-3.12 alongside the current 3.14, ComfyUI with DirectML or ROCm, SDXL about 7 GB, plus reference/consistency models about 3-5 GB), which I will not do without your approval. |
| Image processing | Python 3.14 is installed but without Pillow or NumPy (installable with pip, free). ffmpeg is installed. |
| Unity | Sprite import, slicing, Animator, animation clips and events are all available and I can automate them. |

## 2. Options to get real art (my recommendation first)
**Option B (recommended): you generate the frames, I build everything else.** Your concept art came from ChatGPT image generation, so that tool already understands the designs. I supply a *sprite request pack* (exact reference, prompts, frame list, canvas, naming, background color) in small batches; you drop the PNGs into a folder; my pipeline cleans, aligns, packs, imports and animates them, and produces contact sheets for review. Cheapest in time, strongest quality and consistency, and nothing leaves your control. Cost to you: running the prompts and saving the outputs.

**Option A: local generation on your 7900 XT (free, private, automated).** I install and drive ComfyUI + SDXL with reference-image conditioning. Pros: no manual prompting, everything stays on your PC. Cons: a large install and downloads, AMD-on-Windows is the slow and fiddly path, and frame-to-frame consistency (same face, hands, clothes) is the hard part of diffusion; expect heavy rejection and cleanup, and it may not reach concept-art quality. I would test 8 idle frames first and stop if consistency is poor.

**Option C: cut-out (puppet) animation.** Generate or draw *separate parts* once (Saiyan: head, hair, torso, arms, legs, scarf; Cakezilla: tiers, face parts, mouth shapes, gloves, crown, candles) and animate them in Unity with transforms and a few hand-drawn swap frames. Far fewer images, smooth motion, ideal for the huge boss; weaker for Saiyan's expressive full-body poses. Still needs the parts as clean separate art (they cannot be sliced from the poster because hidden areas are missing).

**Suggested hybrid:** Saiyan = frame-by-frame (B or A); Cakezilla = layered parts with swappable faces/mouths/hands (C), using B or A to produce the parts and expression frames.

## 3. What I can build now, whichever option you pick
Sprite import and slicing automation, frame cleaning (background key-out, trim, alignment to a shared pivot), sprite-sheet packing with metadata, contact-sheet generator, animation clip and Animator builders with frame timing and events, an animation preview scene (select Saiyan/Cakezilla, state, loop, speed, scale), runtime sprite swapping, and the swap of the procedural visuals out only after the replacements work. Hitboxes stay independent of the visuals. These are art-agnostic; I will test them with clearly labeled synthetic test frames, never presented as the real art.

## 4. Sprite request pack, step 0 (model sheet), before any animation
Why first: one approved design sheet is the reference for every later frame, which is the best defense against drifting faces and outfits.
- Inputs: `References/ConceptArt/02_Saiyan_vs_King_Cakezilla.png` (and `01_Game_Key_Art.png`).
- Ask for: a single character model sheet of Saiyan on a plain flat background: side view facing right (the gameplay view), front view, 3/4 view, plus 6 face expressions (smirk, shout, hurt, happy, determined, surprised). Same costume as the concept art: short tousled dark brown hair, blue eyes, blue hoodie with gold crown emblem, white cartoon gloves, blue-and-white oversized shoes, dark pants, flowing blue scarf. Clean ink outline, flat cel colors with simple shading, classic-meets-modern cartoon style, no background scenery.
- I review the sheet and we approve it (or revise) before generating frames.

## 5. Technical spec for every sprite frame (so the pipeline accepts it)
- One PNG per frame, character facing **right**, full body in frame, nothing cropped.
- Canvas 1024x1024 per frame at generation time (I will trim and downscale to the game size); same camera distance and same character height in every frame, feet near the bottom center.
- Background: transparent PNG if the tool supports it; otherwise a flat pure magenta (#FF00FF) background that I key out. No shadows or scenery.
- Naming: `saiyan_<animation>_<NN>.png` (for example `saiyan_idle_00.png`), frames in order, same folder per animation; extras (smears, impacts) as `saiyan_dash_smear_00.png`.
- First animation deliverable: `idle`, 8 frames, loop-friendly (frame 7 leads back into frame 0): gentle breathing, hair and scarf follow-through, a blink on one frame.
- Drop location: `Art/Source/Saiyan/<animation>/` (raw 1024 sources ARE committed on the generator branch so they can travel through git; processed sheets Claude builds go under `Assets/Art`).
- A self-contained brief for Codex (or any other generator) is in `Docs/CODEX_SPRITE_BRIEF.md`.

## 6. Production order (unchanged from your brief)
A: approved Saiyan idle working in Unity with screenshot and preview. B: run/jump/shoot/dash replace the procedural player. C: Cakezilla idle, expressions and the three phase 1 attacks. D: polish and effects. E: phase 2 and 3 animations after review.

## 7. Decision needed
Which option do you want: B (you generate, recommended), A (I set up local generation; needs your OK for the install and downloads), or C/hybrid? If B, tell me when you have the model sheet and I will review it, or ask me for the exact prompt text for ChatGPT.
