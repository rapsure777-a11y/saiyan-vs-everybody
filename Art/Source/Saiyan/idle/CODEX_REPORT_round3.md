# Saiyan sprite art generation report

## Round 1 — original model sheet
Generated with Codex built-in image generation using the two approved concept images. No external service, paid API, private photo, or local model install was used. No hand or script edits. The first composite design sheet had an opaque cream background and ground shadow. It is preserved as model_sheet_v1.png. A transparent copy of the same drawings was later added by the user as model_sheet_v1_transparent.png.

## Round 2 — simplified sheet and pose test
Generated with Codex built-in image generation using the approved model sheet and concept art as references. No hand edits. The revised sheet keeps three full-body views and six expressions. It is genuinely transparent; empty corners and open gaps were checked through the PNG alpha channel. The pose-test composite contains six full-body side-view poses facing right: run, jump, shoot, dash, dash smear, and hurt recoil. Character identity and clothing remain recognizable. Some facial construction, hair detail, clothing folds, and transparent-edge color fringing still vary. These images remain review candidates.

## Round 3 — Saiyan idle, 8 frames
Generated eight separate frames with Codex built-in image generation. Each prompt used the canonical model sheet and pose test, plus the previous idle frame as an identity reference. Frame 04 is the single blink. Breathing, glove sway, and scarf follow-through are intentionally small. The first frame 00 attempt was rejected because the hair/scarf did not follow the canonical shape and its output sizing/background needed correction; it was replaced.

The generated source canvases were 1254x1254 with near-magenta backgrounds. A small PowerShell/System.Drawing processing step:
- color-keyed the near-magenta background into the specified exact #FF00FF fill,
- resized each square frame to 1024x1024 using high-quality bicubic interpolation,
- packed the eight frames into contact_sheet.png on a neutral gray background.

No character anatomy or costume was drawn or retouched by script. The final frame PNGs are each 1024x1024; the canvas corner is exact #FF00FF. The contact sheet is 2048x256. The feet stay at a common visual baseline in the contact sheet, and frame 07 returns close to frame 00.

## Quality assessment
The character reads consistently across all eight frames: side-facing profile, face, hair silhouette, hoodie and crown, gloves, cargo pants, shoes, and scarf remain recognizable. One eye blink is clear. The motion is subtle; at contact-sheet scale, the non-blink changes are modest. The hair still has more internal detail than ideal for repeated production drawing, and the scarf is a single ribbon but remains broad in places. The frames are a test milestone for owner review, not polished final animation.

## Rejected outputs
- The first idle frame 00 attempt was not kept due to scarf/hair drift and canvas/background requirements.
- An opaque magenta model-sheet attempt was not retained because its empty pixels were not uniformly #FF00FF.
- No other idle frames were rejected.

## Not done
No GIF/MP4 preview, Unity import, animation clip, Animator integration, Unity screenshot, or Unity testing was completed. This branch's art-only brief assigns Unity integration to Claude and asks to stop after idle-frame review. The game has not been visually tested with these sprites.

