# The chart as the game: what is built, how to play it, what is next

4 October 2026, night. You chose "the chart is the game" in the picker. This is the first playable slice of it.

## How to play it

1. Open `Assets/Kapman/Scenes/L_Chart.unity` and press Play. (To rebuild the scene: Kapman > Levels > Build the chart game (2D).)
2. "New game" on the title. The chart is the screen. She lies at Dorestat; the port's menu is up.
3. Click a port on the chart. Its card says how far by water and how many days. Press "Set course".
4. In the port's menu choose "Cast off" (3). She sails the charted line by herself.
5. Speed: `+` and `-` for 1x, 2x, 4x. `Space` pauses.
6. Drag the chart to look round; the wheel zooms. `F`, or "Follow her", brings the view back to her.
7. When she comes in, the port's graph moors her and its menu opens. "Explore the town and markets" docks the market beside the chart.

## What it is

There is no 3D level in this scene. The whole world is in the game's world frame at 1:60, which is the clock's own rate:
60 game seconds a second. So her real speed in metres a second is her speed on the chart, and Dorestad to Utrecht (30 km
by the river) took her six and a half game hours in the test, which is six and a half minutes at 1x.

| Part | File | What it does |
|---|---|---|
| The scene | `Editor/Game/ChartBuilder.cs` | Builds `L_Chart`: the world frame, the clock, a camera that draws nothing, the ship, and the game as it is added to every level. Switches off what only makes sense in 3D. |
| Her mark | `Code/Game/Chart/ChartBody.cs` | Moves her toward the next point of the course at the speed the world's own ships make in the same wind: her polar, beating, the oars in a calm, lying to in a gale. |
| The ports | `Code/Game/Chart/ChartWorld.cs` | Makes a port with a berth for each of the 50 trading places at the start, and puts her at Dorestad. |
| The screen | `Code/Game/Map/MapView.cs` (chart mode) | The map is open from the start, follows her, takes no camera, and lets cards and menus come up over it. |
| The picture | `Code/Game/Map/MapArt.cs`, `MapArt/world/` | Your map: 36 PNG tiles the game reads at Play. See `MapArt/README.md`. |
| The wind | `WorldSim.WeatherAt` | The same weather model the AI fleet sails by. |

Everything else is the game as it stood: the markets, the price notes, the 138 ships of the AI fleet, the chronicle
(Lindisfarne is 13 days old news at the start), the port menus, the chest, the fog of war, save and load.

## Tested

`ChartGameTests` (PlayMode, headless): the scene opens, the clock runs, 50 ports and 138 ships are made, the chart is
open with its picture, she lies moored at Dorestad. A course is set to Traiectum, she is cast off through the port's
menu, she sails it and is moored there by the port's graph. Three pictures of the screen are saved to `Captures/chart`.
`MapArtTests` (EditMode): the picture's frame covers the sailing world and every tile is on disk.

## Known gaps in this slice

- The port's menu and cards open over the middle of the chart, on top of her. They should sit beside it.
- The toll-man's call does not come in this scene: the encounter runner looks for berths in the level chain's file.
- She starts with no food aboard ("food 0 days"): buy stores before a long leg.
- Ports that were not yet founded in 793 have a port object too.
- The fog over unknown waters washes the picture out more than it should.
- No cards at sea yet beyond the meetings, raiders and king's men the simulation already makes.
- No sound in this scene.
- The 3D window (watching her on the pixel sea) is not attached: the demo for it is in the KapmanLook project.

## Next, in the order of the concept (`research_indie_and_concept.md`)

1. The word on the item.
2. The day stick, and the clock held while a panel is open.
3. Cards at sea in chart mode.
4. The voyage log and the winter screen.
5. Four named crew.
6. The hold as a grid.
7. The 3D window.
