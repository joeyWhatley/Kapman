"""THE USER'S OWN PICTURES OF FACES AS STAND-INS FOR EVERY PORTRAIT OF SOMEBODY MET.

The user, 7 Oct 2026, with pictures of faces: "do the same for all these [the Byzantium / PSX filter] and then
exchange them for all NPC portraits we have made by AI in the game"; of the engraving of a king: "use that second one
for Alfred"; and, with six more: "filter these and use where fits".

Each picture is cut to head and shoulders, put through the game's filter (standin_filter.tone) and kept in
Art/standin/portrait_pool. Then every portrait slot of the art list that is somebody met (person.*, role.*, weight.*:
not the player's own, self.*) is given one of them as Art/standin/portrait/<slot>.png, where it fits: each face is
marked with what it can stand for (a churchman, a king, a lord, a fighting man, a trader, a seaman, a poor man, a
woman...), each slot is read for what it is (its id, its name and the notes of the data), and the slot takes a face
of its kind: the same one every time for the same slot. A few are given by hand (Ravn, the lender, Alfred).

A slot that has a picture of the very person (standin_people.py: a coin, a manuscript, a statue from Wikimedia
Commons) keeps that: this file does not write over a slot named in Bake/art/standin_people_choices.json.

THESE ARE NOT THE GAME'S. They are photographs of real people from other people's work, supplied by the user as
stand-ins to look at while the game is made. Art/standin is not in the repository; none of them may ship. They are
listed as such in Design/art_standins.md.

Run:  python standin_portraits.py
"""
import csv, hashlib, io, json, os, re, shutil, sys
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
ART = os.path.join(ROOT, "Art")
SRC = os.path.join(ART, "standin", "source", "people")
sys.path.insert(0, HERE)
import standin_filter

# the file; what it can stand for; and the cut: the middle of the face across and down, and how much of the height is kept
FACES = [
    ("p01.png", {"seaman", "common"}, (0.53, 0.42, 0.80)),           # lean, curly, a moustache, aboard a ship
    ("p02.png", {"fighter", "chief", "old"}, (0.50, 0.42, 0.74)),     # long hair and a long plaited beard, scarred
    ("p03.png", {"trader", "common"}, (0.52, 0.50, 0.82)),            # a braided goatee, a worked tunic
    ("p04.png", {"poor", "seaman", "common"}, (0.50, 0.50, 0.84)),    # a ragged hood, a full beard, wide eyes
    ("p05.png", {"common", "poor", "fighter"}, (0.46, 0.50, 0.84)),   # heavy, looking over his shoulder
    ("p06.png", {"woman"}, (0.49, 0.37, 0.70)),                       # dark plaits, a fur collar
    ("p07.png", {"fighter", "raider"}, (0.49, 0.50, 0.82)),           # shaven head, marked skin, laughing
    ("p08.png", {"woman", "fighter"}, (0.48, 0.50, 1.00)),            # fair plaits, a shoulder plate
    ("p09.png", {"fighter"}, (0.57, 0.43, 0.64)),                     # heavy, a mail coif on his shoulders
    ("p10.png", {"fighter", "chief", "seaman"}, (0.46, 0.46, 0.86)),  # beads in his beard, looking up
    ("p11.png", {"chief", "trader", "old", "common"}, (0.49, 0.46, 0.86)),   # grey-bearded, a fur collar
    ("p12.jpg", {"alfred"}, (0.49, 0.44, 0.86)),                      # an old engraving of a crowned king: Alfred, and nobody else
    ("p13.png", {"lord", "young", "church"}, (0.49, 0.38, 0.72)),     # a young man, fair, a fine hooded cloak
    ("p14.png", {"king"}, (0.45, 0.42, 0.80)),                        # a circlet on long hair, a fur mantle
    ("p15.png", {"fighter", "guard"}, (0.49, 0.42, 0.84)),            # an iron helmet and a mail coif
    ("p16.jpg", {"lord", "fighter", "young"}, (0.48, 0.17, 0.36)),    # a young man standing with a sword, a cloak
    ("p17.png", {"trader", "lord", "common"}, (0.48, 0.46, 0.90)),    # a fringe, a moustache, a ring brooch on a grey cloak
    ("p18.png", {"church"}, (0.48, 0.40, 0.82)),                      # cropped hair, a beard, crosses on his collar
]
FACES_OF = {os.path.splitext(name)[0]: what for name, what, _ in FACES}
BY_HAND = {"person.cast.crew.ravn": "p04", "person.lender": "p03"}
SIZE = (128, 160)                                                   # twice the slot's 64 x 80: a face needs the pixels

WOMAN = ("woman", "wife", "widow", "abbess", "nun", "girl", "daughter", "queen", "mother", "sister", "alewife", "lady", "maid", "midwife")
# the people of weight have no title in the art list: who they were, by their slot (kings; churchmen; the rest are lords)
WEIGHT = {"king": ("offa", "cenwulf", "ecgberht", "beorhtric", "ecgfrith", "aethelberht", "eadwald", "cuthred", "praen", "charles", "sigfred"),
          "church": ("wulfred", "hildebold", "leo", "heathoberht", "angilbert"), "trader": ("isaac",)}
# what a slot is, by its title ("Portrait of Wido, the count, ..."; "Stand-in portrait for any abbot ...") (the first kind that fits is taken)
KINDS = [
    ("king", ("king", "emperor", "queen")),
    ("church", ("bishop", "abbot", "priest", "monk", "deacon", "clerk", "cleric", "cellarer", "prior", "hermit", "chaplain", "missionary", "pope", "archbishop",
                "tonsure", "pilgrim", "sacristan", "novice")),
    ("lord", ("the count", " count ", "the reeve", "steward", " earl", " jarl", "ealdorman", " duke", " lord", "thegn", "the heir", "aetheling", "prince",
              "keeper of the port", "the chief,", "king's son")),
    ("raider", ("raider", "pirate", "viking")),
    ("guard", ("guard", "toll man", "king's man", "the king's ship", "commander", "watchman", "warden", "doorward")),
    ("fighter", ("warrior", "captain", "huscarl", "soldier", "lord's man", "swords", "champion", "the chief of")),
    ("trader", ("merchant", "trader", "moneyer", "dealer", "broker", "factor", "chapman", "lender", "shipmaster", "skipper", "goldsmith", "jeweller")),
    ("seaman", ("steersman", "mate", "lookout", "the hand", "pilot", "boatman", "ferryman", "fisher", "sailor", "seaman", "oarsman", "shipwright")),
    ("poor", ("beggar", "captive", "slave", "thrall", "carter", "porter", "herd", "swineherd", "leper", "outlaw")),
]


def cut(im, box):
    cx, cy, share = box
    h = int(round(im.height * share))
    w = int(round(h * SIZE[0] / float(SIZE[1])))
    if w > im.width:
        w = im.width
        h = int(round(w * SIZE[1] / float(SIZE[0])))
    x = min(max(0, int(round(cx * im.width - w / 2.0))), im.width - w)
    y = min(max(0, int(round(cy * im.height - h / 2.0))), im.height - h)
    return im.crop((x, y, x + w, y + h)).resize(SIZE, Image.LANCZOS)


def read(row):
    """What the art list says of a portrait slot: the person's title (lower case), and whether it is a woman."""
    d = row.get("description", "")
    i = d.find("The notes from the data:")
    text = d[i + 24:] if i >= 0 else ""
    m = re.search(r"Portrait of [^,]+, the ([^,:]+)[,:]", text) or re.search(r"Stand-in portrait for any (.+?) who has", text)
    title = (m.group(1) if m else "").lower()
    said = re.search(r"\bA (man|woman), about", text)
    woman = said.group(1) == "woman" if said else any(w in title or w in row["id"].lower() for w in WOMAN)
    return " " + title + " ", woman


def main():
    pool = os.path.join(ART, "standin", "portrait_pool")
    out = os.path.join(ART, "standin", "portrait")
    os.makedirs(pool, exist_ok=True)
    os.makedirs(out, exist_ok=True)
    made, fits = {}, {}
    for name, what, box in FACES:
        src = os.path.join(SRC, name)
        if not os.path.exists(src):
            print("missing:", src)
            continue
        key = os.path.splitext(name)[0]
        # (a face that is one person and nobody else is kept out of the pool: the player picks a face from the pool)
        own = os.path.join(ART, "standin", "portrait_named")
        os.makedirs(own, exist_ok=True)
        path = os.path.join(own if "alfred" in what else pool, key + ".png")
        standin_filter.tone(cut(Image.open(src).convert("RGB"), box)).save(path)
        made[key] = path
        for w in what:
            fits.setdefault(w, []).append(key)
    men = [k for k in sorted(made) if not ({"woman", "alfred", "king"} & FACES_OF[k])]
    own = set()
    choices = os.path.join(HERE, "standin_people_choices.json")
    if os.path.exists(choices):
        own = {k for k, v in json.load(io.open(choices, encoding="utf-8")).items() if v and v.get("n")}
    rows = list(csv.DictReader(io.open(os.path.join(ART, "art_list.csv"), encoding="utf-8-sig")))
    n, count = 0, {}
    for r in rows:
        if r.get("kind") != "portrait" or r["id"].startswith("self.") or r["id"] in own:
            continue
        text, woman = read(r)
        woman = woman and bool(fits.get("woman"))
        kind = next((k for k, words in KINDS if any(w in text for w in words)), "common")
        if r["id"].startswith("weight."):
            who = r["id"].split(".", 1)[1]
            kind = next((k for k, names in WEIGHT.items() if who in names), "lord")
        if r["id"] in BY_HAND and BY_HAND[r["id"]] in made:
            pick = BY_HAND[r["id"]]
        elif fits.get("alfred") and r.get("name", "").strip().lower() in ("alfred", "aelfred", "ælfred", "king alfred"):
            pick = fits["alfred"][0]                                   # (the user: "use that second one for Alfred")
        else:
            if woman:
                group = [k for k in fits["woman"] if kind in FACES_OF[k]] or fits["woman"]
            else:
                group = ([k for k in fits.get(kind, []) if "woman" not in FACES_OF[k]]
                         or [k for k in fits.get("common", []) if "woman" not in FACES_OF[k]] or men)
            pick = group[int(hashlib.md5(r["id"].encode("utf-8")).hexdigest(), 16) % len(group)]
        shutil.copyfile(made[pick], os.path.join(out, os.path.basename(r["file"])))
        label = kind + (" (woman)" if woman else "")
        count[label] = count.get(label, 0) + 1
        n += 1
    print("faces:", len(made), "; portraits given a stand-in:", n, "; kept for a picture of the very person:", len(own))
    print("  by kind:", ", ".join("%s %d" % kv for kv in sorted(count.items())))
    sheet = Image.new("RGB", (SIZE[0] * len(made) + 10, SIZE[1] + 10), (30, 20, 34))
    for i, key in enumerate(sorted(made)):
        sheet.paste(Image.open(made[key]), (5 + i * SIZE[0], 5))
    os.makedirs(os.path.join(HERE, "out"), exist_ok=True)
    sheet.save(os.path.join(HERE, "out", "standin_faces.png"))


if __name__ == "__main__":
    main()
