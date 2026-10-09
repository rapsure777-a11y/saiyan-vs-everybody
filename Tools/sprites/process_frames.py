"""
Sprite frame processor: raw generated frames (flat #FF00FF background) -> game-ready transparent sprites.

What it does (and does NOT do): background key-out with proper edge un-mixing (no magenta fringe), optional colour matching of every frame to the first
frame (fixes slow colour drift between generated frames; it never redraws anything), colour bleed fix under transparent pixels (so Unity filtering cannot
show halos), feet-baseline measurement for a shared pivot, downscale, metadata, contact sheet and preview GIF.

Usage:
  python Tools/sprites/process_frames.py --src Art/Source/Saiyan/idle --character Saiyan --anim idle --fps 8 --loop \
      --out Assets/Resources/Art --report AI_HANDOFF/SCREENSHOTS --tag M-A_2026-10-09
"""
import argparse, glob, json, os, sys
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

MAGENTA = np.array([255.0, 0.0, 255.0], dtype=np.float32)


def key_frame(img):
    """Returns float RGB (0..255, un-mixed) and alpha (0..1)."""
    a = np.asarray(img.convert("RGB")).astype(np.float32)
    R, G, B = a[..., 0], a[..., 1], a[..., 2]
    mag = np.clip((np.minimum(R, B) - G) / 255.0, 0, 1)              # 1 on pure magenta, 0 on neutral/blue/red
    alpha = 1.0 - np.clip((mag - 0.30) / 0.40, 0, 1)
    # un-mix: observed = alpha * colour + (1 - alpha) * magenta
    safe = np.maximum(alpha, 0.05)[..., None]
    rgb = np.clip((a - (1.0 - alpha)[..., None] * MAGENTA) / safe, 0, 255)
    alpha = np.where(alpha > 0.97, 1.0, np.where(alpha < 0.03, 0.0, alpha))
    return rgb, alpha


def match_colours(rgb, alpha, ref_rgb, ref_alpha):
    """Per-channel histogram matching over foreground pixels so a frame has the same colour distribution as the reference."""
    fg, rfg = alpha > 0.9, ref_alpha > 0.9
    out = rgb.copy()
    for c in range(3):
        s = rgb[..., c][fg]; r = ref_rgb[..., c][rfg]
        if s.size == 0 or r.size == 0:
            continue
        order = np.argsort(s); sc = np.empty_like(s, dtype=np.float32); sc[order] = np.linspace(0, 1, s.size, dtype=np.float32)
        rs = np.sort(r); rq = np.linspace(0, 1, rs.size, dtype=np.float32)
        mapped = np.interp(sc, rq, rs)
        ch = out[..., c]; ch[fg] = mapped; out[..., c] = ch
    return out


def box_blur(arr, r):
    """Separable box blur on a 2D float array (edge-padded), applied twice for a smooth falloff."""
    out = arr.astype(np.float32)
    for _ in range(2):
        for axis in (0, 1):
            pad = np.pad(out, [(r, r) if k == axis else (0, 0) for k in range(2)], mode="edge")
            c = np.cumsum(pad, axis=axis, dtype=np.float64)
            c = np.concatenate([np.zeros_like(np.take(c, [0], axis=axis)), c], axis=axis)
            n = out.shape[axis]
            hi = np.take(c, np.arange(2 * r + 1, 2 * r + 1 + n), axis=axis); lo = np.take(c, np.arange(0, n), axis=axis)
            out = ((hi - lo) / (2 * r + 1)).astype(np.float32)
    return out


def bleed_fix(rgb, alpha, radius=24):
    """Fill the colour of fully transparent pixels with the nearby foreground colour, so bilinear filtering never mixes in a stray colour."""
    pre = (rgb * alpha[..., None]).astype(np.float32)
    acc = np.stack([box_blur(pre[..., c], radius) for c in range(3)], axis=-1)
    wa = box_blur(alpha.astype(np.float32), radius)
    fill = acc / np.maximum(wa, 1e-4)[..., None]
    trans = alpha < 0.02
    out = rgb.copy(); out[trans] = np.clip(fill[trans], 0, 255)
    return out


def to_rgba_image(rgb, alpha):
    arr = np.dstack([np.clip(rgb, 0, 255), alpha * 255.0]).round().astype(np.uint8)
    return Image.fromarray(arr, "RGBA")


def downscale(img, size):
    return img.convert("RGBa").resize((size, size), Image.LANCZOS).convert("RGBA")      # premultiplied resize: no dark/bright halos


def feet_metrics(alpha_img):
    a = np.asarray(alpha_img)[..., 3] > 128
    ys, xs = np.where(a)
    bot = ys.max()
    band = a[max(0, bot - int(0.02 * a.shape[0])): bot + 1]
    fx = np.where(band.any(0))[0]
    return bot, (fx.min() + fx.max()) / 2.0, ys.min(), xs.min(), xs.max()


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--src", required=True); ap.add_argument("--character", required=True); ap.add_argument("--anim", required=True)
    ap.add_argument("--prefix", default=None); ap.add_argument("--fps", type=float, default=8); ap.add_argument("--loop", action="store_true")
    ap.add_argument("--size", type=int, default=512); ap.add_argument("--ppu", type=float, default=200)
    ap.add_argument("--out", required=True); ap.add_argument("--report", default=None); ap.add_argument("--tag", default="M")
    ap.add_argument("--no-colour-match", action="store_true")
    a = ap.parse_args()
    prefix = a.prefix or f"{a.character.lower()}_{a.anim}"
    files = sorted(glob.glob(os.path.join(a.src, f"{prefix}_[0-9][0-9].png")))
    if not files:
        sys.exit("no frames found: " + os.path.join(a.src, prefix + "_NN.png"))
    outdir = os.path.join(a.out, a.character, a.anim); os.makedirs(outdir, exist_ok=True)
    keyed = [key_frame(Image.open(f)) for f in files]
    ref_rgb, ref_alpha = keyed[0]
    frames, before, after = [], [], []
    for (rgb, alpha), f in zip(keyed, files):
        before.append(rgb[alpha > 0.9].mean(0))
        if not a.no_colour_match:
            rgb = match_colours(rgb, alpha, ref_rgb, ref_alpha)
        after.append(rgb[alpha > 0.9].mean(0))
        rgb = bleed_fix(rgb, alpha)
        frames.append(downscale(to_rgba_image(rgb, alpha), a.size))
    # shared pivot from the feet of all frames (median), baseline-aligned only if a frame is off by more than 1.5 px
    m = [feet_metrics(fr) for fr in frames]
    base = float(np.median([x[0] for x in m])); fx = float(np.median([x[1] for x in m]))
    shifted = 0
    for i, fr in enumerate(frames):
        d = int(round(base - m[i][0]))
        if abs(m[i][0] - base) > 1.5:
            canvas = Image.new("RGBA", fr.size, (0, 0, 0, 0)); canvas.paste(fr, (0, d)); frames[i] = canvas; shifted += 1
    for i, fr in enumerate(frames):
        fr.save(os.path.join(outdir, f"{prefix}_{i:02d}.png"))
    pivot = [round(fx / a.size, 4), round(1.0 - base / a.size, 4)]
    top = min(x[2] for x in m)
    meta = {"character": a.character, "name": a.anim, "fps": a.fps, "loop": bool(a.loop), "ppu": a.ppu, "frameSize": a.size, "frames": len(frames),
            "pivot": pivot, "heightPx": int(base - top), "events": []}
    with open(os.path.join(outdir, "anim.json"), "w") as fh:
        json.dump(meta, fh, indent=2)
    lib_path = os.path.join(a.out, a.character, "library.json")
    names = []
    if os.path.exists(lib_path):
        names = json.load(open(lib_path)).get("animations", [])
    if a.anim not in names:
        names.append(a.anim)
    with open(lib_path, "w") as fh:
        json.dump({"character": a.character, "animations": names}, fh, indent=2)
    print(f"{len(frames)} frames -> {outdir}; pivot {pivot}; character height {meta['heightPx']} px = {meta['heightPx'] / a.ppu:.2f} units at {a.ppu} ppu; baseline-shifted frames: {shifted}")
    if not a.no_colour_match:
        b = np.array(before); af = np.array(after)
        print("mean fg colour spread across frames (max-min RGB) before:", (b.max(0) - b.min(0)).round(1), "after:", (af.max(0) - af.min(0)).round(1))
    if a.report:
        os.makedirs(a.report, exist_ok=True)
        sky = (120, 175, 245, 255); cell = 256
        sheet = Image.new("RGBA", (cell * len(frames), cell + 22), (60, 60, 70, 255)); d = ImageDraw.Draw(sheet)
        for i, fr in enumerate(frames):
            bg = Image.new("RGBA", (a.size, a.size), sky); bg.alpha_composite(fr)
            sheet.paste(bg.resize((cell, cell), Image.LANCZOS), (i * cell, 22)); d.text((i * cell + 6, 5), f"{a.anim} {i:02d}", fill=(255, 255, 255, 255))
        sheet.convert("RGB").save(os.path.join(a.report, f"{a.tag}_{a.character.lower()}-{a.anim}_contact-sheet.jpg"), quality=90)
        gif = []
        for fr in frames:
            bg = Image.new("RGBA", (a.size, a.size), sky); bg.alpha_composite(fr); gif.append(bg.resize((320, 320), Image.LANCZOS).convert("P", palette=Image.ADAPTIVE, colors=128))
        gif[0].save(os.path.join(a.report, f"{a.tag}_{a.character.lower()}-{a.anim}_preview.gif"), save_all=True, append_images=gif[1:], duration=int(1000 / a.fps), loop=0, disposal=2)
        print("wrote contact sheet and preview gif to", a.report)


if __name__ == "__main__":
    main()
