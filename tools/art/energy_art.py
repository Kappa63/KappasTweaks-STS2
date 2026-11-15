"""Bonesman energy art: 5 legacy energy-counter layers (128px) and the card/text energy icons.
Usage: python3 -I energy_art.py <images/character dir> <preview dir>"""
import math, sys, os
from PIL import Image, ImageDraw, ImageFilter

SS = 8
INK, BONE, BONE_SH, BONE_HI = (48, 34, 30, 255), (236, 224, 192, 255), (196, 180, 140, 255), (255, 250, 234, 255)
DARK, DARK_HI, GLOW = (44, 32, 30, 255), (78, 60, 52, 255), (255, 214, 140)

def canvas(n): return Image.new("RGBA", (n * SS, n * SS), (0, 0, 0, 0))
def done(im, n): return im.resize((n, n), Image.LANCZOS)

def layer_base(n=128):  # static: dark disc with a bone rim
    im = canvas(n); d = ImageDraw.Draw(im); c, r = n * SS / 2, n * SS * 0.44
    d.ellipse((c - r, c - r, c + r, c + r), fill=BONE, outline=INK, width=4 * SS)
    ri = r * 0.8
    d.ellipse((c - ri, c - ri, c + ri, c + ri), fill=DARK, outline=INK, width=3 * SS)
    d.arc((c - r + 3 * SS, c - r + 3 * SS, c + r - 3 * SS, c + r - 3 * SS), 20, 110, fill=BONE_SH, width=5 * SS)
    return done(im, n)

def layer_pips(n=128):  # rotates: ring of domino pips inside the rim
    im = canvas(n); d = ImageDraw.Draw(im); c = n * SS / 2; R = n * SS * 0.29
    for k in range(10):
        a = 2 * math.pi * k / 10; x, y = c + R * math.cos(a), c + R * math.sin(a); pr = (5 if k % 2 == 0 else 3.5) * SS
        d.ellipse((x - pr, y - pr, x + pr, y + pr), fill=BONE, outline=INK, width=int(1.5 * SS))
    return done(im, n)

def layer_segments(n=128):  # rotates: notches cut into the bone rim, like dividing lines on a tile
    im = canvas(n); d = ImageDraw.Draw(im); c = n * SS / 2; r0, r1 = n * SS * 0.36, n * SS * 0.44
    for k in range(6):
        a = 2 * math.pi * k / 6 + 0.26
        d.line((c + r0 * math.cos(a), c + r0 * math.sin(a), c + r1 * math.cos(a), c + r1 * math.sin(a)), fill=INK, width=3 * SS)
    return done(im, n)

def layer_glow(n=128):  # static: warm glow behind the number
    im = Image.new("RGBA", (n, n), (0, 0, 0, 0)); d = ImageDraw.Draw(im); c = n / 2
    for k in range(18, 0, -1):
        r = n * 0.26 * k / 18; d.ellipse((c - r, c - r, c + r, c + r), fill=GLOW + (int(235 * (1 - k / 18) ** 1.1),))
    return im.filter(ImageFilter.GaussianBlur(2))

def layer_shine(n=128):  # static front: glossy crescent on the rim
    im = canvas(n); d = ImageDraw.Draw(im); c, r = n * SS / 2, n * SS * 0.40
    d.arc((c - r, c - r, c + r, c + r), 205, 250, fill=BONE_HI, width=5 * SS)
    return done(im, n)

def gem(n):  # card-cost gem / text icon: a bone hexagon with a dark domino face
    im = canvas(n); d = ImageDraw.Draw(im); c = n * SS / 2; R = n * SS * 0.46
    hexa = [(c + R * math.cos(math.pi / 6 + k * math.pi / 3), c + R * math.sin(math.pi / 6 + k * math.pi / 3)) for k in range(6)]
    d.polygon(hexa, fill=BONE)
    d.polygon([hexa[0], hexa[1], hexa[2], (c, c)], fill=BONE_SH)
    ri = R * 0.62
    inner = [(c + ri * math.cos(math.pi / 6 + k * math.pi / 3), c + ri * math.sin(math.pi / 6 + k * math.pi / 3)) for k in range(6)]
    d.polygon(inner, fill=DARK, outline=INK)
    d.polygon([inner[3], inner[4], (c, c)], fill=DARK_HI)
    w = max(3, int(n * SS * (0.05 if n > 40 else 0.08)))
    d.line(hexa + [hexa[0]], fill=INK, width=w, joint="curve")
    d.line(inner + [inner[0]], fill=INK, width=max(2, w // 2), joint="curve")
    return done(im, n)

if __name__ == "__main__":
    out, prev = sys.argv[1], sys.argv[2]
    layers = [layer_base(), layer_pips(), layer_segments(), layer_glow(), layer_shine()]
    for i, l in enumerate(layers, 1):
        l.save(os.path.join(out, "energy", f"layer{i}.png"))
    gem(74).save(os.path.join(out, "energy_icon_big.png"))
    gem(24).save(os.path.join(out, "energy_icon_text.png"))
    comp = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    for l in layers: comp.alpha_composite(l)
    comp.resize((384, 384), Image.LANCZOS).save(os.path.join(prev, "orb_preview.png"))
