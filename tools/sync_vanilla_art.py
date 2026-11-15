"""Copy mod power icons and potion images to the paths the game's own atlas loader falls back to, so they show up
even when BaseLib's icon hooks get skipped (tiny one-line getters can be inlined past Harmony).
  power  -> images/powers/kappastweaks-<name>_power.png     (also the vanilla big-icon path)
  potion -> images/potions/large/kappastweaks-<name>.png
Run from the repo root after adding or changing art: python3 -I tools/sync_vanilla_art.py"""
import glob, os, re, shutil

ROOT = os.getcwd()
MOD = os.path.join(ROOT, "KappasTweaks", "images")
snake = lambda n: re.sub(r"(?<!^)(?=[A-Z])", "_", n).lower()

def families():
    art = os.path.join(ROOT, "KappasTweaksCode", "Domino", "BonesmanArt.cs")
    if not os.path.exists(art):
        return {}
    src = open(art).read()
    block = src[src.index("_powerFamilies"):]
    block = block[:block.index("});")]
    return {cls: fam for fam, names in re.findall(r'\["(\w+)"\] = "([^"]+)"', block) for cls in names.split()}

def first(*paths):
    return next((p for p in paths if os.path.exists(p)), None)

def sync(src, dst):
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    if not os.path.exists(dst) or open(src, "rb").read() != open(dst, "rb").read():
        shutil.copyfile(src, dst)
        return True
    return False

fam = families()
changed, missing = [], []
POWER_BASES = re.compile(r"class (\w+)\b[^{]*:[^{]*\b(KappasTweaksPower|BonesmanPower|ICustomPower)\b")
for f in glob.glob(os.path.join(ROOT, "KappasTweaksCode", "**", "*.cs"), recursive=True):
    m = POWER_BASES.search(open(f).read())
    if not m or "abstract class" in open(f).read():
        continue
    cls = m.group(1)
    name = snake(cls)
    src = first(os.path.join(MOD, "powers", "big", f"{name}.png"), os.path.join(MOD, "powers", f"{name}.png"),
                os.path.join(MOD, "powers", "big", "families", f"{fam.get(cls, '_')}.png"))
    if src is None:
        missing.append(name); continue
    if sync(src, os.path.join(ROOT, "images", "powers", f"kappastweaks-{name}.png")):
        changed.append(name)

for f in glob.glob(os.path.join(MOD, "potions", "*.png")):
    name = os.path.basename(f)[:-4]
    if name.endswith("_outline"):
        continue
    if sync(f, os.path.join(ROOT, "images", "potions", "large", f"kappastweaks-{name}.png")):
        changed.append(name)

print("synced:", ", ".join(changed) or "nothing new")
print("no art yet (keeps placeholder):", ", ".join(missing) or "none")
