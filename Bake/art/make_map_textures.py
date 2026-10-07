"""The paper of the chart (Assets/Kapman/Game/UI/Map): made here, by rule, so it can be made again.

The user, 8 Oct 2026: "Can you give the map more texture on the colours so they are not so hard". The chart is flat
fills of a few colours. This is one clear tile laid over all of them (and moved with the land as the chart is moved):

  paper.png   384 x 384, shown two screen pixels to one of its own (768 x 768, as the frame's grain is). Three things
              in it, all as specks of the one dark and the one pale tone, never a smooth shade (the chart is pixels):
                - broad soft patches, some lighter and some darker, drawn in an ordered dither: they break up a wide
                  flat field of one colour, as washed paint lies unevenly;
                - a fine grain everywhere;
                - short fibres, as of rag paper.
              It tiles without a seam.

Run:  python make_map_textures.py           (then Unity imports it; MapView.uss lays it as .map-paper)
"""
import os, uuid
import numpy as np
from PIL import Image
from make_ui_textures import META, BAYER4

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, "..", "..", "Assets", "Kapman", "Game", "UI", "Map"))
N = 384
DARK = (38, 16, 44)          # the frame's dark Byzantium, not black
PALE = (255, 244, 214)       # warm paper

# how strong each thing is (alpha of 255); the patches' cover is how much of a patch's middle is specks
PATCH_ALPHA, PATCH_COVER = 22, 0.78
GRAIN_DARK, GRAIN_PALE = (0.12, 12, 30), (0.10, 12, 32)       # share of texels, least and most alpha
FIBRES, FIBRE_ALPHA = 170, (14, 26)
CAP = 54


def smooth(rng, sigma):
    """Noise with no seam: white noise low-passed in the frequency domain (periodic by its making)."""
    f = np.fft.fft2(rng.standard_normal((N, N)))
    ky = np.fft.fftfreq(N)[:, None]
    kx = np.fft.fftfreq(N)[None, :]
    out = np.real(np.fft.ifft2(f * np.exp(-2.0 * (np.pi * sigma) ** 2 * (kx ** 2 + ky ** 2))))
    return out / out.std()


def make():
    rng = np.random.default_rng(793)
    signed = np.zeros((N, N), dtype=np.float32)        # under 0 dark, over 0 pale; its size is the alpha
    # the broad patches, in an ordered dither
    broad = np.clip((0.7 * smooth(rng, 20.0) + 0.3 * smooth(rng, 8.0)) / 1.9, -1.0, 1.0)
    ys, xs = np.mgrid[0:N, 0:N]
    bayer = (np.array(BAYER4, dtype=np.float32).reshape(4, 4)[ys % 4, xs % 4] + 0.5) / 16.0
    on = np.abs(broad) * PATCH_COVER > bayer
    signed += np.where(on, np.sign(broad) * PATCH_ALPHA, 0.0)
    # the fine grain
    r = rng.random((N, N))
    a = rng.random((N, N))
    share, lo, hi = GRAIN_DARK
    signed -= np.where(r < share, lo + a * (hi - lo), 0.0)
    share2, lo2, hi2 = GRAIN_PALE
    signed += np.where((r >= share) & (r < share + share2), lo2 + a * (hi2 - lo2), 0.0)
    # the fibres: short strokes, lying mostly along the sheet
    for i in range(FIBRES):
        x, y = rng.integers(0, N), rng.integers(0, N)
        length = int(rng.integers(3, 11))
        slope = rng.choice([0, 0, 0, 1, -1])
        tone = (1.0 if i % 2 else -1.0) * (FIBRE_ALPHA[0] + rng.random() * (FIBRE_ALPHA[1] - FIBRE_ALPHA[0]))
        for k in range(length):
            yy = (y + (k // 3) * slope) % N
            signed[yy, (x + k) % N] += tone
    alpha = np.clip(np.abs(signed), 0, CAP).astype(np.uint8)
    rgba = np.zeros((N, N, 4), dtype=np.uint8)
    for c in range(3):
        rgba[..., c] = np.where(signed < 0, DARK[c], PALE[c])
    rgba[..., 3] = alpha
    return Image.fromarray(rgba, "RGBA")


def main():
    os.makedirs(OUT, exist_ok=True)
    path = os.path.join(OUT, "paper.png")
    make().save(path)
    meta = path + ".meta"
    if not os.path.exists(meta):
        with open(meta, "w", encoding="utf-8", newline="\n") as f:
            f.write(META % uuid.uuid4().hex)
    print("wrote", path)


if __name__ == "__main__":
    main()
