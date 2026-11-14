"""Procedural card art for the Bonesman's domino cards, in the game's flat cel-shaded style.
Usage: python3 -I domino_art.py <out_dir>   -> <out_dir>/<name>.png at 1000x760."""
import math, random, sys, os
from PIL import Image, ImageDraw, ImageFilter

S = 2                      # supersampling
W, H = 1000 * S, 760 * S
INK = (34, 28, 26, 255)
BONE, BONE_SH, BONE_HI = (238, 228, 200, 255), (196, 182, 146, 255), (255, 250, 236, 255)
SPOTS = [(.27, .27), (.73, .27), (.27, .5), (.5, .5), (.73, .5), (.27, .73), (.73, .73)]
FACES = [[], [3], [0, 6], [0, 3, 6], [0, 1, 5, 6], [0, 1, 3, 5, 6], [0, 1, 2, 4, 5, 6]]

def rrect(d, box, r, **kw):
    d.rounded_rectangle(box, radius=r, **kw)

def tile(w, top, bottom, pip_color=INK, glow=None):
    """Upright domino (w x 2w) with cel shading and a thick outline, as an RGBA image."""
    h, ow = 2 * w, max(6, w // 14)
    pad = ow * 2
    im = Image.new("RGBA", (w + 2 * pad, h + 2 * pad), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    box = (pad, pad, pad + w, pad + h)
    r = w // 6
    rrect(d, box, r, fill=BONE)
    # hard-edged shadow band on the right and bottom, highlight band top-left
    sh = Image.new("L", im.size, 0); sd = ImageDraw.Draw(sh)
    sd.rectangle((pad + w * .80, pad, pad + w, pad + h), fill=255)
    sd.rectangle((pad, pad + h * .93, pad + w, pad + h), fill=255)
    m = Image.new("L", im.size, 0); rrect(ImageDraw.Draw(m), box, r, fill=255)
    from PIL import ImageChops
    im.paste(Image.new("RGBA", im.size, BONE_SH), (0, 0), ImageChops.multiply(sh, m))
    hl = Image.new("L", im.size, 0); ImageDraw.Draw(hl).rectangle((pad + w * .08, pad + h * .03, pad + w * .2, pad + h * .45), fill=255)
    im.paste(Image.new("RGBA", im.size, BONE_HI), (0, 0), ImageChops.multiply(hl, m))
    # divider
    d.rectangle((pad + w * .1, pad + w - ow * .45, pad + w * .9, pad + w + ow * .45), fill=INK)
    for half, n in enumerate((top, bottom)):
        for i in FACES[n]:
            cx, cy = pad + SPOTS[i][0] * w, pad + half * w + SPOTS[i][1] * w
            pr = w * .085
            if glow:
                for k in range(3, 0, -1):
                    gc = glow[:3] + (int(70 / k),)
                    d.ellipse((cx - pr * (1 + k * .5), cy - pr * (1 + k * .5), cx + pr * (1 + k * .5), cy + pr * (1 + k * .5)), fill=gc)
            d.ellipse((cx - pr, cy - pr, cx + pr, cy + pr), fill=pip_color)
            d.ellipse((cx - pr * .55, cy - pr * .6, cx - pr * .1, cy - pr * .15), fill=(255, 255, 255, 70))
    rrect(d, box, r, outline=INK, width=ow)
    return im

def place(canvas, im, cx, cy, angle=0, shadow=True):
    rot = im.rotate(angle, resample=Image.BICUBIC, expand=True)
    x, y = int(cx - rot.width / 2), int(cy - rot.height / 2)
    if shadow:
        a = rot.split()[3].point(lambda v: int(v * .55))
        sh = Image.new("RGBA", rot.size, (0, 0, 0, 255)); sh.putalpha(a)
        canvas.alpha_composite(sh, (x + 18 * S, y + 22 * S))
    canvas.alpha_composite(rot, (x, y))

def background(c1, c2, rays=None, seed=0):
    rnd = random.Random(seed)
    im = Image.new("RGBA", (W, H))
    d = ImageDraw.Draw(im)
    for yy in range(H):
        t = yy / H
        d.line((0, yy, W, yy), fill=tuple(int(c1[i] * (1 - t) + c2[i] * t) for i in range(3)) + (255,))
    if rays:
        cx, cy, n, col = rays
        ov = Image.new("RGBA", (W, H), (0, 0, 0, 0)); od = ImageDraw.Draw(ov)
        for k in range(n):
            a0 = 2 * math.pi * k / n + rnd.uniform(-.05, .05)
            a1 = a0 + math.pi / n * rnd.uniform(.5, .9)
            R = W * 1.5
            od.polygon([(cx, cy), (cx + R * math.cos(a0), cy + R * math.sin(a0)), (cx + R * math.cos(a1), cy + R * math.sin(a1))], fill=col)
        im.alpha_composite(ov)
    # angular dark shards in the corners, the vanilla vignette
    ov = Image.new("RGBA", (W, H), (0, 0, 0, 0)); od = ImageDraw.Draw(ov)
    for corner in [(0, 0), (W, 0), (0, H), (W, H)]:
        for _ in range(3):
            pts = [corner] + [(corner[0] + rnd.uniform(-1, 1) * W * .45, corner[1] + rnd.uniform(-1, 1) * H * .45) for _ in range(2)]
            od.polygon(pts, fill=(10, 6, 14, rnd.randint(60, 110)))
    im.alpha_composite(ov)
    return im

def burst(canvas, cx, cy, r_in, r_out, n, fill, angle=0, seed=1):
    rnd = random.Random(seed)
    pts = []
    for k in range(2 * n):
        a = angle + math.pi * k / n
        rr = (r_out * rnd.uniform(.8, 1.15)) if k % 2 == 0 else r_in
        pts.append((cx + rr * math.cos(a), cy + rr * math.sin(a)))
    d = ImageDraw.Draw(canvas)
    d.polygon(pts, fill=fill, outline=INK, width=6 * S)

def speed_lines(canvas, x0, y0, x1, y1, n, width, col=(255, 255, 255, 200), seed=2, spread=(60, 140)):
    rnd = random.Random(seed)
    d = ImageDraw.Draw(canvas)
    for _ in range(n):
        ox, oy = rnd.uniform(-1, 1) * spread[0] * S, rnd.uniform(-1, 1) * spread[1] * S
        ax, ay, bx, by = x0 + ox, y0 + oy, x1 + ox * .3, y1 + oy * .3
        ang = math.atan2(by - ay, bx - ax) + math.pi / 2
        wv = width * rnd.uniform(.5, 1.2)
        d.polygon([(ax, ay), (bx + wv * math.cos(ang), by + wv * math.sin(ang)), (bx - wv * math.cos(ang), by - wv * math.sin(ang))], fill=col)

def save(canvas, out, name):
    canvas.convert("RGB").resize((1000, 760), Image.LANCZOS).save(os.path.join(out, name + ".png"))

def main(out):
    os.makedirs(out, exist_ok=True)
    # Double Six: one big double-six tile bursting with light
    c = background((150, 40, 30), (60, 10, 20), rays=(W * .5, H * .5, 18, (255, 170, 60, 50)), seed=1)
    burst(c, W * .5, H * .5, 260 * S, 420 * S, 14, (255, 200, 80, 255), seed=3)
    place(c, tile(230 * S, 6, 6), W * .5, H * .5, angle=-14)
    save(c, out, "double_six")
    # Snake Eyes: 1|1 laid sideways, pips glowing red like eyes
    c = background((30, 70, 45), (8, 25, 18), rays=(W * .5, H * .55, 14, (120, 255, 140, 30)), seed=2)
    place(c, tile(200 * S, 1, 1, pip_color=(200, 20, 20, 255), glow=(255, 40, 30)), W * .5, H * .52, angle=90)
    save(c, out, "snake_eyes")
    # Blank Stare: an empty half staring out of the dark
    c = background((70, 50, 110), (20, 12, 40), rays=(W * .5, H * .5, 10, (200, 180, 255, 25)), seed=3)
    place(c, tile(230 * S, 0, 4), W * .5, H * .5, angle=6)
    save(c, out, "blank_stare")
    # Set Piece: 5|5 slammed down with an impact burst
    c = background((40, 70, 140), (12, 18, 50), seed=4)
    burst(c, W * .5, H * .78, 120 * S, 330 * S, 11, (255, 240, 200, 255), angle=.2, seed=5)
    speed_lines(c, W * .5, -H * .2, W * .5, H * .45, 16, 18 * S, seed=6, spread=(420, 30))
    place(c, tile(200 * S, 5, 5), W * .5, H * .45, angle=-4)
    save(c, out, "set_piece")
    # Tile Toss: a tile spinning through the air with motion arcs
    c = background((170, 90, 30), (70, 25, 15), seed=5)
    speed_lines(c, -W * .1, H * .9, W * .5, H * .5, 12, 26 * S, seed=7)
    for k, (a, alpha) in enumerate([(70, 60), (50, 110)]):
        ghost = tile(170 * S, 3, 5); ghost.putalpha(ghost.split()[3].point(lambda v: int(v * alpha / 255)))
        place(c, ghost, W * (.32 + k * .08), H * (.66 - k * .07), angle=a, shadow=False)
    place(c, tile(170 * S, 3, 5), W * .58, H * .45, angle=28)
    save(c, out, "tile_toss")
    # Pip Shot: pips fired off a tile like buckshot
    c = background((120, 20, 40), (40, 6, 18), seed=6)
    place(c, tile(170 * S, 6, 2), W * .26, H * .52, angle=-70)
    d = ImageDraw.Draw(c); rnd = random.Random(9)
    for _ in range(9):
        x, y, r = W * rnd.uniform(.5, .92), H * rnd.uniform(.22, .8), 26 * S * rnd.uniform(.7, 1.2)
        speed_lines(c, x - 260 * S, y, x, y, 1, r * .8, col=(255, 210, 160, 150), seed=rnd.randint(0, 99))
        d.ellipse((x - r, y - r, x + r, y + r), fill=INK, outline=(255, 220, 170, 255), width=4 * S)
    save(c, out, "pip_shot")
    # Domino Effect: a row toppling in sequence
    c = background((50, 110, 120), (12, 30, 40), seed=7)
    ImageDraw.Draw(c).rectangle((0, H * .8, W, H), fill=(18, 14, 20, 255))
    faces = [(1, 2), (2, 3), (3, 4), (4, 5), (5, 6)]
    for k, (a, b) in enumerate(faces):
        lean = [-68, -52, -34, -16, 0][k]
        t = tile(120 * S, a, b)
        place(c, t, W * (.17 + k * .165) - 30 * S * (lean / -68), H * .8 - 150 * S + abs(lean) * 1.1 * S, angle=lean)
    save(c, out, "domino_effect")
    # Boneyard Shuffle: tiles swirling, the open ends reset
    c = background((90, 60, 40), (25, 15, 12), rays=(W * .5, H * .5, 16, (255, 220, 160, 25)), seed=8)
    rnd = random.Random(11)
    for k in range(7):
        a = 2 * math.pi * k / 7
        place(c, tile(110 * S, rnd.randint(0, 6), rnd.randint(0, 6)), W * .5 + 300 * S * math.cos(a), H * .5 + 230 * S * math.sin(a), angle=-math.degrees(a) + 90)
    place(c, tile(150 * S, 0, 0), W * .5, H * .5, angle=-8)
    save(c, out, "boneyard_shuffle")
    # Count the Pips: two tiles meeting end to end, tally marks scratched behind
    c = background((40, 40, 60), (12, 12, 22), seed=9)
    d = ImageDraw.Draw(c)
    for g in range(3):
        bx = W * (.12 + g * .3)
        for k in range(5):
            if k < 4: d.line((bx + k * 36 * S, H * .14, bx + k * 36 * S - 10 * S, H * .3), fill=(230, 220, 190, 160), width=12 * S)
        d.line((bx - 20 * S, H * .27, bx + 150 * S, H * .16), fill=(230, 220, 190, 160), width=12 * S)
    place(c, tile(150 * S, 2, 4), W * .37, H * .6, angle=90)
    place(c, tile(150 * S, 4, 6), W * .63, H * .6, angle=90)
    save(c, out, "count_the_pips")

def power_icon(out):
    """Chain power: a small domino on transparent, 256 and 64."""
    big = Image.new("RGBA", (256 * S, 256 * S), (0, 0, 0, 0))
    place(big, tile(95 * S, 3, 5), 128 * S, 128 * S, angle=-28, shadow=False)
    big = big.resize((256, 256), Image.LANCZOS)
    big.save(os.path.join(out, "chain_power_big.png"))
    big.resize((64, 64), Image.LANCZOS).save(os.path.join(out, "chain_power.png"))

if __name__ == "__main__":
    main(sys.argv[1]); power_icon(sys.argv[1])
