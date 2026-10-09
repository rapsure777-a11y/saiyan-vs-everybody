# Visual review M1 (2026-10-09): in-game vs approved concept art

Compared: the in-game stills in this folder against `References/ConceptArt/01_Game_Key_Art.png` and `02_Saiyan_vs_King_Cakezilla.png` (concept art, not gameplay). Target: polished hybrid classic-and-modern cartoon boss game with expressive, frame-by-frame animation quality in the spirit of Cuphead, with original designs.

## What already matches
- Palette and mood: saturated blue sky, candy pink frosting, cream cake, gold stars and crown (compare `12_fight-overview-idle-hud`).
- King Cakezilla's silhouette and parts: tiered pink-frosted cake, angry brows, red nose, big teeth, candle crown, two giant white gloves (`11_king-cakezilla-closeup`).
- Saiyan's costume read: blue hoodie, gold crown badge, white gloves, blue shoes, dark pants, brown hair, blue eyes, blue scarf (`01_saiyan-closeup-idle`).
- Readability of attacks: every attack has a flashing yellow/red warning shape on the floor and a glove that pulses before it acts (`20`, `22`, `24`). Saiyan is drawn in front of boss gloves and hazards (`23`).

## Shortcomings, ranked by impact on reaching the target look (with specific fixes)
1. **Rendering style: flat vector shapes vs the concept's inked, shaded, textured look.** No gradients, rim light, cel shading, paper/grain texture or varied line weight. Fix: replace placeholders with hand-drawn sprites that have a thick-to-thin ink line, 2-3 tone cel shading and a subtle paper texture overlay; add a light post-process grain/vignette.
2. **Saiyan's design and animation are crude.** Hair tufts read as rabbit ears, the mouth is a black bar, the scarf looks like a stick arm, limbs are stiff rectangles, no hood or cargo pockets, no face expressions (concept has big expressive eyes, rosy cheeks, a determined smirk, and a dramatic cape-like scarf). Fix: draw a real character sheet (front 3/4 view); animate frame by frame: idle 6-8 frames, run 8-10, jump/fall/land with squash and stretch, dash with smear frames, shoot with anticipation, hurt, victory; add 3-4 face swaps (smirk, shout, hurt, happy).
3. **Saiyan is small on screen** (about 1.7 of 11.2 units tall). Fix: scale the sprite to roughly 2.2-2.5 units (keep the hitbox small), and pull the camera in a little for the boss fight.
4. **Cakezilla is static.** Only breathing and pupil tracking; face never changes; gloves float with a stump instead of attached arms; the concept has tilted, asymmetrical tiers, thick dripping frosting with highlights, and a hugely deformable mouth. Fix: frame-by-frame expressions (smug, shout, hurt flash with squash, laugh, rage), arms that stretch from the body to the glove, tiers that wobble on slams, drips that animate.
5. **Phase 3 is not a transformation yet.** `43_phase-3-cakezilla-supreme` is a gold tint plus aura. Fix: a real second form (taller crown, flaming candles, crack-and-reveal transition, new color script and face), keyed to the planned phase 3 attacks.
6. **Environment is sparse:** block castles and ellipse hills, repeating ground, no foreground layer, no waterfalls/wafer details/candy props that the concept shows. Fix: 3 parallax layers with real candy-kingdom art, foreground frosting blobs and sprinkles, wafer-tile ground, animated clouds and floating confetti.
7. **Combat effects are thin:** no hit sparks with impact words, no dust on landing/dash (only white circles), tiny shockwave, no hit-stop or screen flash on boss damage. Fix: stylized impact frames (starburst + "POW"-style burst), dust clouds, 2-3 frame hit-stop, a bolder shockwave with a crest, cupcakes with faces and trails.
8. **UI and type:** default system font with blurry outlines, flat panels. Fix: a bold hand-lettered title/banner font (the key art logo style), chunky bordered panels with candy motifs, crisp outlines, an animated Super-ready glow.
9. **Camera feel:** fixed and a bit empty at the left of the arena. Fix: slight zoom-in on slams/transitions, subtle parallax drift.

## Not judged yet
Motion quality (everything is code-driven placeholder animation), audio, and fairness by a human player: none of these can be judged from stills; the two clips only show timing, and the scripted bot is not a human playtest.
