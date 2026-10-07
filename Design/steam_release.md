# Releasing on Steam

The user, 7 Oct 2026: "How much longer until we can release on steam, and how do we release on steam".

Written from what I know of Steam's rules, not from a fresh reading of Steamworks (the web search allowance was used
up that day). Every figure marked (check) should be read again on partner.steamgames.com before money or dates hang on it.

## How it is done

1. **Join Steamworks** (partner.steamgames.com). You sign as a person or a company, give a bank account and tax details
   (a tax interview; outside the US a treaty form), and Valve checks your identity. Allow a week.
2. **Pay the fee: 100 US dollars for the game** (Steam Direct). It comes back once the game has earned 1,000 dollars
   (check). From the day you pay there is a **30-day wait** before a first game may be released (check).
3. **Make the store page.** Name, short description (about 300 characters), the long text, tags, system requirements,
   languages, price, at least five screenshots, and a set of pictures in fixed sizes (the capsules: header, small, main,
   tall; the library pictures; an icon). A trailer is not required but nearly every page that sells has one. The
   capsules are the most important art you will draw: they are what people see before they see the game.
4. **Answer the content survey.** It sets the age notices. Two things in it bear on this game:
   - **Mature content**: the slave trade and raiding are in the world and the cards. Say so plainly in the survey.
   - **AI disclosure**: Steam asks whether anything in the game was made with AI tools, and prints your answer on the
     store page. The code and much of the card text here were written with an AI model, so the answer is yes for those;
     the art is yours. Write that sentence honestly and in your own words.
5. **Valve reviews the page**, and later the build. Each takes up to about five working days (check). A page must be
   public as "Coming soon" for **at least two weeks** before release (check).
6. **Upload the build** with Valve's tool (SteamPipe: a small script that sends the build folder to a "depot"). A
   Windows build from Unity is enough to start. Steam's own features are optional: cloud saves can be switched on by
   naming the save folder (no code), achievements need a small plugin (Steamworks.NET).
7. **Press release.** Steam pays monthly, keeping 30 in the 100.

Optional and worth it:
- **A demo.** Its own free page tied to the game. The first hour (Dorestat to Lundenwic) is the natural demo.
- **Steam Next Fest** (a week of demos, three times a year: about February, June and October; sign-up closes months
  before (check)). The best single chance a small game has to be seen.
- **Early Access.** For a game that plays but is not finished. It fits a first release of one life on twelve ports
  with the later books to come.
- **Wishlists.** They gather from the day the page is public, and they are what the launch stands on. This is the
  reason to put the page up long before the game is done.

## What this game still needs before each step

**Before the page can go up** (the game need not be finished):
- A frame that looks like yours: the draw-first tiers 1 and 2 of the art list (about 210 pictures, most of them tiny),
  and the capsule pictures.
- Five true screenshots and a short film of real play: so the first hour must play, and the 3D view of the ship helps.
- A name people can type: "Kapman" without the long mark for the page's address; the mark can stay in the title art.
- The paperwork of step 1 and the fee of step 2, since the 30 days run from then. **This can be started now.**

**Before a demo:**
- One good hour, play-tested by people who are not us (Design/GAME_MAKER.md stage 1). The critics' verdict on
  6 October was that there was no game yet; the frame went into the game on 7 October.
- Saving and loading that never loses a game. A standalone build that starts on a clean machine.
- Text large enough to read (Design/research/next/art.md: the mock-up's small text is under the size a PC game needs),
  and the same check on a Steam Deck if you want the Deck mark.
- Sound: only public-domain recordings, each with its source written down.

**Before release:**
- The first release whole: one life, 793 to 805, twelve ports, the money tuned so the debt bites, the cards that can
  open (about 420) read and trimmed by you, the nine chains that can start but not end either finished or left out.
- The art of tiers 1 to 3 (about 320 pictures); tier 4 as far as you wish.
- Credits and licences: the map data, the fonts, the asset packs, the texts in the chest, the sounds.
  (An audit of these has not been done yet.)
- The Unity licence: the free one holds while the game and you earn under its yearly limit (check the figure).

## How long

No honest single date exists yet, because the two largest things are unknown: whether the first hour is fun once
people play it, and how fast you draw. What can be said:

| Step | Earliest | Depends on |
|---|---|---|
| Steam paperwork done | 1 to 2 weeks from when you start it | you; it can run beside everything else |
| Store page public ("Coming soon") | about 4 to 8 weeks | the frame drawn, capsules, five screenshots, a short film |
| A demo of the first hour | about 2 to 4 months | the hour being good; play-testers; saves; a clean build |
| The first release (Early Access) | about 6 to 10 months: spring to summer 2027 | everything in "before release"; your drawing and reading time |

Steam's own waits (30 days after the fee, two weeks of "Coming soon", the reviews) mean about six weeks at the very
least even for a finished game, which is why the paperwork is worth starting early.
