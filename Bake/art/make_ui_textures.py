"""The frame's own small pictures (Assets/Kapman/Game/UI/Xp): made here, by rule, so they can be made again.

The user, 7 Oct 2026, of the first cut of the frame (smooth shaded bars and buttons): "can you texture the UI colour so
it's not so sleek and glossy"; "it just needs more flair, more depth, more style".

  grain.png   a clear tile of dark and light specks and a few scratches, laid over any ground of the frame (the ground's
              colour is the style sheet's): worn paint, not gloss. 64 x 64, shown two screen pixels to one of its own.
  sheen.png   a strip for the top of a bar: light in dots, thick at the top and thinning out (an old console's shading,
              not a smooth one). 8 x 32, shown 16 x 64 from the bar's top.
  corner.png  the piece at each corner of the world's frame: a gold bracket with a stud. 14 x 14, shown three to one.

Run:  python make_ui_textures.py
"""
import os, random, uuid
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, "..", "..", "Assets", "Kapman", "Game", "UI", "Xp"))
BAYER4 = [0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5]

META = """fileFormatVersion: 2
guid: %s
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 0
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 0
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID:
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def save(im, name):
    path = os.path.join(OUT, name)
    im.save(path)
    meta = path + ".meta"
    if not os.path.exists(meta):
        with open(meta, "w", encoding="utf-8", newline="\n") as f:
            f.write(META % uuid.uuid5(uuid.NAMESPACE_URL, "kapman/xp/" + name).hex)
    print(name, im.size)


def grain():
    r = random.Random(793)
    n = 64
    im = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    px = im.load()
    for y in range(n):
        for x in range(n):
            k = r.random()
            if k < 0.055:
                px[x, y] = (30, 12, 34, r.randint(34, 62))            # a dark speck
            elif k < 0.11:
                px[x, y] = (255, 250, 236, r.randint(40, 78))         # a light one
            elif k < 0.42:
                px[x, y] = (30, 12, 34, r.randint(5, 16))             # the ground is never quite even
            elif k < 0.70:
                px[x, y] = (255, 250, 236, r.randint(5, 18))
    # a few scratches, where the paint is thin: short slanting lines, wrapped at the tile's edge
    for _ in range(7):
        x, y = r.randrange(n), r.randrange(n)
        dx = r.choice((-1, 1))
        light = r.random() < 0.6
        for i in range(r.randint(5, 13)):
            px[(x + dx * i) % n, (y + i // 2) % n] = (255, 250, 236, 70) if light else (30, 12, 34, 56)
    # and two or three worn patches: a looser scatter of light specks
    for _ in range(3):
        cx, cy = r.randrange(n), r.randrange(n)
        for _ in range(26):
            px[(cx + int(r.gauss(0, 4))) % n, (cy + int(r.gauss(0, 3))) % n] = (255, 250, 236, r.randint(30, 60))
    save(im, "grain.png")


def sheen():
    w, h = 8, 32
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = im.load()
    for y in range(h):
        if y == 0:
            share = 1.0                                               # a lit line along the very top
        else:
            share = max(0.0, 0.62 * (1.0 - y / 20.0))                 # then dots, thinning out to nothing
        dark = max(0.0, (y - 23) / 9.0) * 0.55                        # and a shade gathering at the foot
        for x in range(w):
            t = (BAYER4[(y & 3) * 4 + (x & 3)] + 0.5) / 16.0
            if t < share:
                px[x, y] = (255, 240, 250, 150 if y == 0 else 96)
            elif t < dark:
                px[x, y] = (30, 10, 34, 110)
    save(im, "sheen.png")


def corner():
    art = [
        "DDDDDDDDDDDDDD",
        "DHGGGGGGGGGGgD",
        "DGgggggggggggD",
        "DGgDDDDDDDDDDD",
        "DGgD..........",
        "DGgD.DDDD.....",
        "DGgD.DHGD.....",
        "DGgD.DGgD.....",
        "DGgD.DDDD.....",
        "DGgD..........",
        "DGgD..........",
        "DGgD..........",
        "DggD..........",
        "DDDD..........",
    ]
    tone = {"D": (46, 18, 51, 255), "G": (217, 164, 65, 255), "g": (150, 104, 40, 255), "H": (247, 227, 160, 255), ".": (0, 0, 0, 0)}
    im = Image.new("RGBA", (14, 14), (0, 0, 0, 0))
    for y, row in enumerate(art):
        for x, c in enumerate(row):
            im.putpixel((x, y), tone[c])
    save(im, "corner.png")


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    grain()
    sheen()
    corner()
