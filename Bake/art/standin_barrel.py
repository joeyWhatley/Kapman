"""THE BARREL OF THE TOP BAR (the user, 8 Oct 2026: "replace the bread with a pic of a barrel from the assets we have").

The barrel of the Vikings pack, drawn by the editor on a plain ground (Kapman > Pictures > The barrel:
Bake/art/out/icons/barrel.png), is cut from its ground and put in the tones of the game, as the coin is:
Art/standin/hud/hud.stores.png, 36 x 36.

Run:  python standin_barrel.py
"""
import os
from standin_icons import OUT, ROOT
from standin_icons_make import make_toned

if __name__ == "__main__":
    src = os.path.join(OUT, "barrel.png")
    im = make_toned(src, {"mode": "dark", "lo": 60, "hi": 120})
    dest = os.path.join(ROOT, "Art", "standin", "hud")
    os.makedirs(dest, exist_ok=True)
    im.save(os.path.join(dest, "hud.stores.png"))
    print("wrote", os.path.join(dest, "hud.stores.png"), im.size)
