# Complete animation list (written 2026-10-09)

Status legend: DONE = generated, integrated and tested. TODO = not started. Frame counts are targets; smooth, expressive motion matters more than the exact number. Everything is a side view facing right unless noted. File naming: `Art/Source/<Character>/<animation>/<character>_<animation>_<NN>.png` (two-digit, zero-based, play order), 1024x1024, flat `#FF00FF` background, same scale as `saiyan_idle_00.png` (standing height about 930 px), Saiyan's feet contact line on the same row as in the idle frames. For airborne poses the feet may rise above that line (the line is the ground contact point, not a rule that feet must touch it).

## A. Saiyan: playable character (the brief's required set plus what the game actually uses)

### Batch B1: locomotion (requested first)
| # | Animation | Frames | Loop | Notes |
|---|---|---:|---|---|
| 1 | **idle** | 8 | yes | **DONE** |
| 2 | **walk** | 8-10 | yes | slow arm swing, small bob; used for gentle gamepad stick pushes (below about 55 percent speed) |
| 3 | **run** | 10-12 | yes | full cartoon run with exaggerated lean, big arm pumps, scarf streaming straight back; contact / down / passing / up poses; used at full speed (keyboard always runs) |
| 4 | run start (accelerate) | 3-4 | no | leaning in from standing, leads into run frame 0 (polish; optional) |
| 5 | run stop / turn skid | 4-5 | no | skid with dust, then settle to idle (polish; optional) |

### Batch B2: air
| # | Animation | Frames | Loop | Notes |
|---|---|---:|---|---|
| 6 | **jump takeoff** (anticipation + launch) | 4-6 | no | crouch and squash, then spring up with stretch; the physics jump starts at the last squash frame |
| 7 | **jump rise** | 4-6 | hold/loop | arms up, scarf trailing down, legs tucked |
| 8 | jump apex / transition | 2-3 | no | short hang, then leads into fall (optional but makes the arc read) |
| 9 | **fall** | 4-6 | yes | arms out, scarf flaring up, legs reaching down |
| 10 | **landing** | 4-6 | no | squash, small dust, recover to idle |

### Batch B3: dash and shooting
| # | Animation | Frames | Loop | Notes |
|---|---|---:|---|---|
| 11 | **dash (ground)** | 6-8 | no | forward lean, stretched body, speed smear drawn as a SEPARATE image set (`saiyan_dash_smear_NN.png`) so it can be toggled |
| 12 | **dash (air)** | 5-6 | no | horizontal flying pose |
| 13 | **shoot (standing)** | 6-8 | no | anticipation, glove thrust, recoil, return; **mark the frame where the star leaves the glove** (animation event) |
| 14 | **run + shoot** | 8-12 | yes | the run cycle with the front arm firing; same foot timing as run |
| 15 | shoot in air (rise) | 4 | hold | shoot pose with the jump-rise legs |
| 16 | shoot in air (fall) | 4 | hold | shoot pose with the fall legs |

### Batch B4: reactions
| # | Animation | Frames | Loop | Notes |
|---|---|---:|---|---|
| 17 | **hurt** | 5-7 | no | recoil with squash, eyes shut, stars/impact frame, then recover |
| 18 | **defeat** | 10-14 | no | knocked back, spins or slumps, ends on a held pose (dizzy stars optional) |

### Batch B5: Super and celebration
| # | Animation | Frames | Loop | Notes |
|---|---|---:|---|---|
| 19 | **super** (charge and fire) | 12-16 | no | power-up pose, glow, big thrust; **mark the frame where the giant star launches** |
| 20 | **victory** | 12-16 | loop last 4 | jump for joy, fist pump, smile; used on the SWEET VICTORY screen |
| 21 | impatient idle (optional) | 12-16 | no | plays after about 6 s still: look around, tap a foot, blow hair; adds personality |
| 22 | intro/arrival pose (optional) | 6-8 | no | little pose when the boss fight begins |

Saiyan totals: core needed to switch sprite art on by default (walk, run, jump takeoff/rise/fall/land, dash, shoot, run-shoot, hurt) about 70-90 frames; the full brief set about 140-190 frames.

## B. Saiyan effects (drawn as separate transparent pieces)
| Effect | Frames | Notes |
|---|---:|---|
| Star shot (spinning) | 4-6 | the projectile, yellow star with outline |
| Star trail / sparkle | 4 | |
| Muzzle flash at the glove | 3-4 | |
| Dash dust cloud | 5-6 | |
| Dash speed smear | 3-4 | the separate smear set from #11 |
| Jump dust | 4 | |
| Landing impact dust | 5-6 | |
| Hit spark on the boss | 4-5 | |
| Giant Super star and burst rings | 6-8 | |
| Super-ready glow / aura | 6 loop | |
| Star pickup idle and collect sparkle | 4 + 4 | |
| Target dummy pop | 5 | |
| Victory confetti | 8 loop | |

## C. King Cakezilla (boss), animated from separate parts plus a few drawn swaps
Recommended method: draw the PARTS once (cake tiers, face base, eyes and pupils, eyebrows, nose, crown, candles and flames, frosting drips, left glove, right glove, forearms), then add swap sets: **mouth shapes** (smug closed, laugh wide x3, shout, gritted teeth, spit/puff, hurt wide, defeated), **eyebrow/eye sets** (angry, smug, surprised, hurt squint, dizzy), **glove poses** (fist, open palm, pointing, slam fist, cupping, taunt wave, pinch). Unity moves the parts; swaps and a few key frames supply the expression. This gives dramatic motion without thousands of full-frame images.

Boss animations requested in the brief:
| # | Animation | Phase | Notes |
|---|---:|---|---|
| 1 | Idle breathing and wobble (tiers jiggle, flames flicker, drips sway) | 1 | loop |
| 2 | Laughing / taunting | 1 | mouth swaps plus body shake |
| 3 | Intro entrance | 1 | slides in, slam, roar |
| 4 | Cupcake Toss windup and release | 1 | glove glows, throw |
| 5 | Hand Slam windup, impact, recovery | 1 | glove hovers and shakes, slams, shockwave, returns |
| 6 | Frost Blob (spit) | 1 | cheeks puff, mouth shape, spit |
| 7 | Damage reaction | 1 | squash flash, brows and mouth swap, face flinch |
| 8 | Phase 2 transformation (70 percent) | 2 | cake cracks, frosting surge, roar |
| 9 | Rolling Donut attack | 2 | throws donuts |
| 10 | Sprinkle Rain attack | 2 | arms up, sprinkles fall |
| 11 | Layer Pop attack | 2 | tier pops open and fires candy |
| 12 | Phase 3 Supreme transformation (35 percent) | 3 | golden form, taller crown, flaming candles |
| 13 | Candle Blast | 3 | candles flare, floor bursts |
| 14 | Cupcake Swarm | 3 | summons the flyers |
| 15 | Mega Frost Beam | 3 | charge, beam, recover |
| 16 | Final defeat | 3 | collapses, crumbles, confetti |
Priority: Milestone C = idle, taunt, intro, toss, slam, spit, damage. Phases 2 and 3 only after the first set passes review.

Boss and level effects: cupcake projectile (spin 6) and burst (5); frosting blob and splash (6) and puddle wobble/fade (4+4); shockwave (4-6 loop); slam impact (5-6); rolling donut (roll 8 loop) and hit (4); sprinkle falling (4) and landing pop (4); Layer Pop candy shots (6); Candle Blast warning sparks (4) and flame column (6-8); Cupcake Swarm flyer (flap 6-8, pop 5); Mega Frost Beam charge (8), beam loop (6), impact (6), end (4); boss explosion/crumble (8-10); candle flame flicker (4 loop).

## D. How to request art from Codex (the loop)
1. One animation at a time, in the batch order above, starting with **walk and run**.
2. For each: the individual frames, a contact sheet (all frames in one row), and for events (shoot, super) say which frame index is the key frame.
3. Use `saiyan_idle_00.png` and the pose test as the identity and scale reference, with the locked scarf (one narrow ribbon), hair silhouette, glove design and shoes; do not change them.
4. I process and import every batch the same day, show it in the preview scene and in the game, and report anything that drifts. Then the next batch.

## E. Needed to turn sprite art ON by default
walk, run, jump takeoff, jump rise, fall, landing, dash, shoot (and run-shoot), hurt. Until all of these exist the sprite appears only with F9, because a static drawing sliding around the level looks worse than the placeholder.
