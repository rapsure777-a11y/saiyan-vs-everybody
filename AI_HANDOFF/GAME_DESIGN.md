# GAME DESIGN (current)

Source of truth for the original plan: `Docs/BUILD_BRIEF.md`. This file records what is currently designed and any changes. Status of each item is in PROJECT_STATUS.md.

## Pitch
Original, family-friendly 2D boss-fight platformer for Windows PC. A small brave cartoon kid, Saiyan (tousled dark brown hair, blue eyes, blue hoodie with gold crown badge, white gloves, blue shoes, dark pants, blue scarf from the key art), fights King Cakezilla in the Frosting Fields candy kingdom. Hybrid classic-and-modern cartoon energy; no Cuphead/Crash assets.

## Saiyan: abilities and controls
| Action | Keyboard | Gamepad |
|---|---|---|
| Move | A/D or arrows | left stick / d-pad |
| Jump (variable height) | Space | A |
| Shoot star | J | X |
| Dash | K | B or RB |
| Super | L | Y |
| Pause | Esc | Start |
Values (inspector, `PlayerTuning`): run 6 u/s, jump velocity 12, gravity 28 (about 2.6 u jump height), fall gravity x1.5, release-early cut x0.45, coyote 0.1 s, jump buffer 0.12 s, dash 14 u/s for 0.2 s, cooldown 0.55 s, dash i-frames 0.12 s (works on ground and in air). Shots: 1 damage, one every 0.11 s, max 24 alive, fly in the facing direction. 3 hearts, 1 s post-hit invulnerability with blink and knockback. Super: meter fills from star hits (1.5 per hit) and star pickups, full = 100; spends all of it for one giant piercing star (12 damage, once) plus two helper stars, 1 s invulnerable while firing.

## King Cakezilla (one-screen arena, flat floor, two low one-way platforms)
State machine: Intro, Idle, Telegraph, Attack, Recovery, Transition, Defeated. Weighted choice, never the same attack three times in a row. Phase lines at 70 percent and 35 percent; he is invulnerable during transitions and a hit that would cross a line is clamped to it. Health 120 (Assist Mode x0.66). Every attack warns with a flashing yellow/red marker and a sound for at least 0.6 s (tuned 0.9 to 1.15 s) before anything can hurt. At most 12 damaging objects alive at once.
- Phase 1 (100-70 percent): **Cupcake Toss** (1-3 cupcakes on parabolas to marked floor spots, small blast on landing), **Hand Slam** (red column marked on the floor, hand slams, low shockwave travels along the floor: jump or dash it), **Frost Blob** (marked landing circle, leaves a harmful puddle for 3.5 s that goes safe while fading).
- Phase 2 (70-35): keeps phase 1 attacks, adds Rolling Donuts, Sprinkle Rain (safe lane), Layer Pop. PLANNED.
- Phase 3 (35-0): becomes Cakezilla Supreme; adds Candle Blast Line, Cupcake Swarm, Mega Frost Beam. PLANNED.
Until phase 2/3 attacks exist, the transitions will still happen (invulnerable cinematic, tint change, faster rhythm) but reuse the phase 1 attack pool. This is a known gap, not a finished feature.

## Level: Frosting Fields
Tutorial run-up of roughly 30-60 s: signs teach run, jump (hold for higher), dash, shoot; small obstacles; gumdrop target dummies; star pickups that charge Super; checkpoint right before the arena; camera follows through the tutorial, then locks to the single-screen arena. Boss intro under 8 s and skippable. Victory: `SWEET VICTORY!` with `Golden Candle Star` reward. Retry returns to the checkpoint quickly (intro skipped on retry).

## UI and accessibility
Hearts, Super meter, boss name and health bar, controller hints, Pause with Retry, Assist Mode (5 hearts, easier boss), Reduced Screen Shake, strong telegraph colors plus sound cues (the player is hard of hearing: every audio cue also has a visual).

## Art direction
Match the approved concept art: saturated sky blue, candy pink frosting, cream, gold stars, bold dark outlines. Animation needs: run, idle, jump, fall, land, dash, shoot, hurt, victory for Saiyan; boss idle, telegraph (per attack), slam, spit, hurt flash, phase transform, defeat. See ART_ASSETS.md.

## Changes from the original plan
- Phase 2 and 3 are deliberately postponed to milestone 2 (the user asked to start with the player, the environment, and phase 1).
- Dash invulnerability is a design choice the brief left open.

## Update 2026-10-09 (as built in Milestone 1)
- Arena: camera 11.2 x 19.9 units; boss root at arena x+17; his front edge (invisible wall for the player, pass-through for stars) at x+14; two one-way platforms 2.0 units up; the left wall closes when the fight starts. Retry restarts at x+1.8 with the intro skipped.
- Hazards: Hand Slam column 2.6 wide, floor to 5 high, armed for the slam only, then two low shockwaves (0.8 high); Cupcake Toss 1-3 arcs with blast radius 0.95; Frost Blob puddle 2.8 wide, 3.5 s, safe while fading. Warnings: 0.9 s, 1.15 s and 1.0 s (the minimum allowed is 0.6 s, enforced by a test).
- Transitions (70/35 percent) last 2.6 s, clear all hazards, make the boss invulnerable and tint him (phase 3 adds a gold aura). The attack pool is unchanged until M2.
- Accessibility as built: Assist Mode (5 hearts, boss health x0.66), Reduced screen shake (shake x0.15), and every sound cue has a visual (flashing markers, glove glow, banners).
