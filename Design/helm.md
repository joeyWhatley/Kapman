# Kāpman: the helm

10 October 2026. A PROPOSAL, NOT BUILT. Written for the user to read; the living copy is the doc
"Kāpman: the helm", https://claude.ai/artifact/YPBbeFq8jYTJFjHDhrVi3w . This file is that page as it stood when
it was written. Nothing of it is built until the user has nodded.

The helm becomes a seat. You give it to one of the men, who finds the way as well as his hand lets him, or you keep
it and do the finding yourself.

## What the user ruled (10 October, by the picker)

Their words: "What if you can take the helm, or let one of the crew membersz then the ai will try based on their
skill to find where they need to go if that makes sense or you can do it yourself".

| Question | Their answer |
| --- | --- |
| When you take the helm yourself | You navigate by pointing on the chart. Nobody corrects for you. Her bow is not turned by keys. |
| What a man's hand at the helm decides | Keeping her line, finding a port only heard of, and night and thick air. Not her speed. |
| Why you would ever take it yourself | He must sleep. A man holds the helm for a watch and then wearies. |
| How to go on | This page first. Built after their nod. |

## How it plays

The Voyage window gets a line "At the helm" with the watches in your order. A new game begins with Aldger, then
Brunger, then you.

1. A man steers for a watch of eight hours, and then must rest as long as he steered.
2. When his watch ends, the next rested man in your order takes the helm. The log says so and the clock runs on.
3. When no man is rested, the helm comes to you. The log says so once, and the clock drops back.
4. While you hold it, the game tells you when she is set off her line, and you lay her course again by pointing, as
   on the open sea today.
5. If you do not want the helm, she lies to and the men rest until one of them can steer.

You can change the order at any time, or take the helm yourself. With two men in the order the helm never comes to
you, but the weaker man steers half the hours. So the watches give you one real choice: who steers the night.

## What his hand decides

A man is a poor, a fair or a good hand at the helm. The fair hand steers exactly as the steersman does after the
change of 10 October (DECISIONS 137).

| | A poor hand | A fair hand | A good hand |
| --- | --- | --- | --- |
| Wind and tide: how much of their push he allows for | None of it | A third | Two thirds |
| Set 8 km off her line | He does not notice. You are told at 16 km. | With land in sight he lays her course again himself. Out of sight of land he tells you. | He lays her course again himself, with land in sight and on the open sea. |
| Told to make for a port he has been to | He takes her there, though it is only a ring on your chart. | The same. | The same. |
| Told to make for a port he has not been to | He looks in one place in its ring, then gives up. | He looks in two places. | He looks in three, and begins nearer to it. |
| By night he makes out a landfall at | 9 km | 12 km, as today | 18 km |
| Thick air | He loses his reckoning soonest, because he allows for nothing. | As today. | He loses it last. |

In thick air no hand lays her course again, because none can see. When the air clears she sails on by herself,
whoever steers.

## The four men

Today the game knows the crew only as a number with health, heart and weariness. This gives each man a hand at the
helm and the places he has been.

| Man | His seat | Hand at the helm | Where his sheet says he has been |
| --- | --- | --- | --- |
| Aldger | The steersman | Good | Doccinga, Dorestat, Eoforwic |
| Ravn | The trader's seat, and the first teacher | Fair | Dorestat, Sliesthorp, Hleidr, Skiringssal |
| Brunger | The mate, who speaks for the oars | Fair | Daventre, Diusburg, Colonia, Mogontia |
| Beornwine | The lookout, and the reader | Poor | Gyrwe, Lindisfarena, Eoforwic |

The hands are my first guesses. The places are each man's home and the places his written sheet names
(Design/content/spine.json: home, met). Of the twelve ports of the first release that is only Dorestat, Doccinga and
Daventre, so most ports would still be found by looking.

## How it is built

About three sittings, by the rule of 7 October: screens as UI Builder files, flows and rules as graphs.

| Part | What it is |
| --- | --- |
| Screens | The line "At the helm" and the list of watches in the Voyage window. Each man's seat and hand in the Crew window. |
| Flow, as a graph | "Voyage - the helm": a watch ends, the helm changes hands, it comes to you, nobody can steer, a man is told to make for a port, he looks, he finds it or gives up. |
| Rules, as graphs | How long a man holds the helm. Who takes it next. What he allows for wind and tide. Whether he lays her course again himself. How far he makes out a landfall. Where he looks for a port he has not been to, and how often. |
| Table | Game/Data/helm_793.json: each man's hand, where he has been, the hours of a watch, and every other number. |
| Code | The list of the men one by one, the reckoning itself, and the sailing of a search. Each rule also stands in code as the fallback. |
| Tests | A test holds each graph and its code to the same answer. A play test gives pictures from the game. The long walk counts how often you are asked. |

1. The men one by one, the seat, the watches and the screens.
2. His hand in keeping her line, by night and in thick air.
3. Finding a port only heard of.

## The user's to rule

- Where each man has been. Their sheets cover three of the twelve ports.
- The hands: Aldger good, Ravn and Brunger fair, Beornwine poor.
- The watch: eight hours at the helm, then eight of rest.
- Do you weary at the helm? Proposed: no. Holding it costs your attention and nothing else.
- Does a man at the helm leave his own seat empty (Beornwine steering, nobody keeps the lookout)? Proposed: not in the
  first cut.
- A man lost drops out of the watches. A man hired later brings his own hand and places (hiring, the road's step 27).
- Every line is a placeholder for the user to write.
