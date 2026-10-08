"""Step 2 of standin_icons.py: the chosen pictures made into the game's pictures.

    python standin_icons_make.py

Reads Bake/art/standin_icon_choices.json (sign -> the picture's number on its sheet, where to cut it, how it is turned
into a sign), fetches each picture 1200 wide, and writes:
  Art/standin/hud/hud.fn.<sign>.png   A SIGN OF A BUTTON: white, with the thing's darkness in the alpha (three strengths
                                      of ink, in dots between them), 32 x 32. The style sheet gives it the colour of the
                                      words beside it, so it greys and lights with its button.
  Art/standin/hud/hud.coin.png        THE COIN of the top bar: cut from its ground, in the game's tones, 36 x 36.
  Bake/art/out/icons/made.json        each picture's file, author, licence and page (for Design/art_standins.md)
  Bake/art/out/icons/made.png         one sheet to look at: each large, and at its own size on a button's ground
"""
import io, json, os, sys
from standin_search import get, fetch, shippable, plain
from standin_icons import OUT, CHOICES, ROOT

SIGN = 32
TOP = 36                      # the coin and the barrel of the top bar: shown one to one (.frame-top-picture)
BAYER4 = [0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5]


def source(choice, found, sign):
    """The chosen picture's facts (from the candidates, or looked up by its title), and the picture fetched 1200 wide."""
    if "title" in choice:
        title = choice["title"]
    else:
        rows = found.get(choice.get("from", sign), [])
        row = next((r for r in rows if r["n"] == choice["n"]), None)
        if row is None:
            raise SystemExit("%s: no candidate number %s" % (sign, choice.get("n")))
        title = row["title"]
    d = get({"action": "query", "titles": title, "prop": "imageinfo", "iiprop": "url|size|extmetadata", "iiurlwidth": 1200, "format": "json"})
    page = next(iter((d.get("query", {}).get("pages", {}) or {}).values()), {})
    ii = (page.get("imageinfo") or [{}])[0]
    em = ii.get("extmetadata", {})
    lic = em.get("LicenseShortName", {}).get("value", "")
    if not shippable(lic):
        raise SystemExit("%s: %s has a licence that may not be shipped (%s)" % (sign, title, lic))
    facts = {"title": title, "licence": lic, "author": plain(em.get("Artist", {}).get("value", ""))[:160], "page": ii.get("descriptionurl"),
             "what": plain(em.get("ImageDescription", {}).get("value", ""))[:200]}
    os.makedirs(os.path.join(OUT, "large"), exist_ok=True)
    path = os.path.join(OUT, "large", "%s.jpg" % sign)
    mark = path + ".title"
    if os.path.exists(path) and (not os.path.exists(mark) or io.open(mark, encoding="utf-8").read() != title):
        os.remove(path)
    fetch(ii.get("thumburl") or ii.get("url"), path)
    io.open(mark, "w", encoding="utf-8").write(title)
    return facts, path


def thing(im, choice):
    """How much of each pixel is the thing and not its ground, 0..1 (an array), the picture's greys, and the cut picture.
    mode "dark":    the thing is told from a plain ground by its colour (the ground's colour is the corners');
    mode "darker":  the thing is darker than its ground (a black anchor on earth): told by its darkness;
    mode "lighter": the thing is lighter than its ground (pale figures on a painted stone);
    mode "lines":   a drawing in dark lines on a pale sheet: the lines are the thing."""
    import numpy as np
    from PIL import Image, ImageFilter
    x0, y0, x1, y1 = choice.get("cut", [0, 0, 1, 1])
    w, h = im.size
    im = im.convert("RGB").crop((int(x0 * w), int(y0 * h), int(x1 * w), int(y1 * h)))
    if choice.get("turn"):
        im = im.rotate(choice["turn"], expand=True, fillcolor=im.getpixel((2, 2)))
    a = np.asarray(im).astype(np.float32)
    grey = a.mean(axis=2)
    mode = choice.get("mode", "dark")
    if mode == "lines":
        paper = np.percentile(grey, 92)
        mask = np.clip((paper - grey) / max(1.0, paper * float(choice.get("hard", 0.55))), 0.0, 1.0)
        if choice.get("hatch"):
            # (a drawing shaded with many fine strokes: where the strokes lie thick it is the thing, where they lie thin
            # it is sky and water)
            r = float(choice.get("hatch"))
            dense = np.asarray(Image.fromarray((mask * 255).astype("uint8"), "L").filter(ImageFilter.GaussianBlur(r))).astype(np.float32) / 255.0
            lo, hi = float(choice.get("lo", 0.22)), float(choice.get("hi", 0.42))
            return np.clip((dense - lo) / max(0.01, hi - lo), 0.0, 1.0), grey, im
        # thin lines are made thick enough to be left when the drawing is made small
        thick = int(choice.get("thick", max(1, round(min(grey.shape) / 110.0))))
        if thick > 0:
            sheet = Image.fromarray((mask * 255).astype("uint8"), "L").filter(ImageFilter.MaxFilter(thick * 2 + 1))
            mask = np.asarray(sheet).astype(np.float32) / 255.0
        return mask, grey, im
    if mode in ("darker", "lighter"):
        lo, hi = float(choice.get("lo", 30.0)), float(choice.get("hi", 80.0))
        if mode == "darker":
            ground = np.percentile(grey, float(choice.get("ground", 60.0)))
            mask = np.clip((ground - grey - lo) / max(1.0, hi - lo), 0.0, 1.0)
        else:
            ground = np.percentile(grey, float(choice.get("ground", 30.0)))
            mask = np.clip((grey - ground - lo) / max(1.0, hi - lo), 0.0, 1.0)
    else:
        k = max(2, min(a.shape[0], a.shape[1]) // 24)
        corners = np.concatenate([a[:k, :k].reshape(-1, 3), a[:k, -k:].reshape(-1, 3), a[-k:, :k].reshape(-1, 3), a[-k:, -k:].reshape(-1, 3)])
        ground = np.median(corners, axis=0)
        far = np.sqrt(((a - ground) ** 2).sum(axis=2))
        lo, hi = float(choice.get("lo", 40.0)), float(choice.get("hi", 85.0))
        mask = np.clip((far - lo) / max(1.0, hi - lo), 0.0, 1.0)
    # a thing of thin parts (the beam and cords of a balance) is grown, so that it is left when made small
    if choice.get("grow"):
        sheet = Image.fromarray((mask * 255).astype("uint8"), "L").filter(ImageFilter.MaxFilter(int(choice["grow"]) * 2 + 1))
        mask = np.asarray(sheet).astype(np.float32) / 255.0
    # specks of the ground are swept away
    sweep = int(choice.get("sweep", max(1, round(min(grey.shape) / 160.0))))
    if sweep > 0:
        sheet = Image.fromarray((mask * 255).astype("uint8"), "L").filter(ImageFilter.MedianFilter(sweep * 2 + 1))
        mask = np.asarray(sheet).astype(np.float32) / 255.0
    return mask, grey, im


def fitted(mask, size, margin=1):
    """The box of the thing in the mask, squared and a little wider: (x0, y0, x1, y1) in the mask's pixels."""
    import numpy as np
    ys, xs = np.where(mask > 0.35)
    if len(xs) == 0:
        return 0, 0, mask.shape[1], mask.shape[0]
    x0, x1, y0, y1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
    side = max(x1 - x0, y1 - y0)
    side = int(side * (1.0 + 2.0 * margin / float(size))) + 2
    cx, cy = (x0 + x1) / 2.0, (y0 + y1) / 2.0
    return int(cx - side / 2.0), int(cy - side / 2.0), int(cx + side / 2.0), int(cy + side / 2.0)


def squared(sheet, box, fill):
    """The box cut out of a picture, with what falls outside the picture filled."""
    from PIL import Image
    side = box[2] - box[0]
    out = Image.new(sheet.mode, (side, side), fill)
    out.paste(sheet.crop((max(0, box[0]), max(0, box[1]), min(sheet.width, box[2]), min(sheet.height, box[3]))), (max(0, -box[0]), max(0, -box[1])))
    return out


def make_sign(path, choice):
    """A BUTTON'S SIGN: white, with the thing's darkness in the alpha, in dots where it is half dark."""
    import numpy as np
    from PIL import Image
    mask, grey, im = thing(Image.open(path), choice)
    if choice.get("mode") == "lines":
        ink = mask
    else:
        # the whole thing is ink; inside it, its own light and shade lighten or darken the ink a little
        inside = mask > 0.5
        look = grey
        if choice.get("relief"):
            # (a thing whose marks are cut or raised in one colour: each place against the places about it)
            from PIL import ImageFilter
            soft = np.asarray(Image.fromarray(grey.astype("uint8"), "L").filter(ImageFilter.GaussianBlur(float(choice["relief"])))).astype(np.float32)
            look = grey - soft
        lo, hi = (np.percentile(look[inside], 5), np.percentile(look[inside], 95)) if inside.any() else (0.0, 255.0)
        shade = np.clip((look - lo) / max(1.0, hi - lo), 0.0, 1.0)             # 0 its darkest, 1 its lightest
        if choice.get("flip"):
            shade = 1.0 - shade
        ink = mask * (1.0 - float(choice.get("shade", 0.5)) * shade)
    box = fitted(mask, SIGN)
    square = squared(Image.fromarray((ink * 255).astype("uint8"), "L"), box, 0)
    small = np.asarray(square.resize((SIGN, SIGN), Image.LANCZOS)).astype(np.float32) / 255.0
    small = np.clip(small * float(choice.get("gain", 1.25)), 0.0, 1.0)
    out = np.zeros((SIGN, SIGN, 4), dtype="uint8")
    out[..., :3] = 255
    for y in range(SIGN):
        for x in range(SIGN):
            t = (BAYER4[(y & 3) * 4 + (x & 3)] + 0.5) / 16.0
            step = 2.0 * small[y, x]                                           # three strengths of ink: none, half, whole
            level = int(step) + (1 if step - int(step) > t else 0)
            out[y, x, 3] = (0, 150, 255)[max(0, min(2, level))]
    return Image.fromarray(out, "RGBA")


def make_toned(path, choice, size=TOP):
    """THE COIN (and anything else that keeps its light and shade): the thing cut from its ground, in the game's tones."""
    import numpy as np
    from PIL import Image, ImageOps
    import standin_filter
    mask, grey, im = thing(Image.open(path), choice)
    box = fitted(mask, size, margin=0)
    rgb = squared(im, box, (0, 0, 0))
    alpha = squared(Image.fromarray((mask * 255).astype("uint8"), "L"), box, 0)
    small = ImageOps.autocontrast(rgb.resize((size, size), Image.LANCZOS), cutoff=1)
    lift = float(choice.get("lift", 1.0))                     # under 1: a dark thing is made lighter (silver on the purple bar)
    if lift != 1.0:
        small = small.point(lambda v: int(255.0 * (v / 255.0) ** lift + 0.5))
    toned = standin_filter.tone(small).convert("RGBA")
    a = np.asarray(alpha.resize((size, size), Image.LANCZOS))
    px = np.asarray(toned).copy()
    px[..., 3] = np.where(a > 110, 255, 0)
    return Image.fromarray(px, "RGBA")


def inked(im, colour):
    from PIL import Image
    if im.width != SIGN:
        return im
    out = Image.new("RGBA", im.size, colour + (255,))
    out.putalpha(im.split()[3])
    return out


def make():
    from PIL import Image
    found = json.load(io.open(os.path.join(OUT, "candidates.json"), encoding="utf-8"))
    choices = json.load(io.open(CHOICES, encoding="utf-8"))
    dest = os.path.join(ROOT, "Art", "standin", "hud")
    os.makedirs(dest, exist_ok=True)
    rows, pictures = [], []
    only = set(sys.argv[1:])
    for sign, choice in choices.items():
        if sign.startswith("_"):
            continue
        facts, path = source(choice, found, sign)
        im = make_toned(path, choice) if choice.get("kind") == "toned" else make_sign(path, choice)
        name = choice.get("slot", "hud.fn.%s" % sign)
        im.save(os.path.join(dest, name + ".png"))
        pictures.append((sign, im))
        rows.append({"slot": name, **facts, "says": choice.get("says", "")})
        print(sign, "->", name, "|", facts["title"][:60], "|", facts["licence"])
    json.dump(rows, io.open(os.path.join(OUT, "made.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=1)
    # one sheet to look at: each picture four times its size, then at its own size on the bar's purple and on a button's ground
    cell = 150
    per = 9
    lines = (len(pictures) + per - 1) // per
    sheet = Image.new("RGB", (cell * per, (cell + 60) * lines), (203, 213, 189))
    for i, (sign, im) in enumerate(pictures):
        cx, cy = (i % per) * cell, (i // per) * (cell + 60)
        big = inked(im.resize((im.width * 4, im.height * 4), Image.NEAREST) if im.width != SIGN else im, (112, 41, 99))
        if im.width == SIGN:
            big = inked(im, (112, 41, 99)).resize((SIGN * 4, SIGN * 4), Image.NEAREST)
        sheet.paste(big, (cx + (cell - big.width) // 2, cy + 6), big)
        sheet.paste((112, 41, 99), (cx, cy + cell + 4, cx + cell // 2, cy + cell + 56))
        lit = inked(im, (240, 210, 122))
        sheet.paste(lit, (cx + 14, cy + cell + 10), lit)
        one = inked(im, (112, 41, 99))
        sheet.paste(one, (cx + cell // 2 + 14, cy + cell + 10), one)
    sheet.save(os.path.join(OUT, "made.png"))


if __name__ == "__main__":
    make()
