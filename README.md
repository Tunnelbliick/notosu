# Storybrew osu!Mania Modcharts
## How do i use this?

### Initial Setup

1. Download all files
2. Drag and drop the Folders into your `/scriptLibrary` Directory
3. Initial setup complete!

### Setting up a Playfield

To get a Playfield and Notes we need 3 Things. 
1. Layers for our Receptor and Notes
2. A Playfield Instance
3. A DrawInstance

Here is a short example to get it up and running
```csharp
var receptors = GetLayer("r");
var notes = GetLayer("n");

// General values
var starttime = 0;
var endtime = 24000;
var duration = endtime - starttime;

// Playfield Scale
var width = 200f;
var height = 500;

// Note initilization Values
var bpm = 190f;
var offset = 0f;

// Drawinstance Values
var updatesPerSecond = 50;
var scrollSpeed = 800f;
var rotateNotesToFaceReceptor = false;
var fadeTime = 60;

var recepotrBitmap = GetMapsetBitmap("sb/sprites/receiver.png"); // The receptor sprite
var receportWidth = recepotrBitmap.Width;

Playfield field = new Playfield();
field.initilizePlayField(receptors, notes, starttime, endtime, width, height, 50);
field.initializeNotes(Beatmap.HitObjects.ToList(), bpm, offset, false, sliderAccuracy);

DrawInstance draw = new DrawInstance(field, starttime, scrollSpeed, updatesPerSecond, OsbEasing.None, rotateNotesToFaceReceptor, fadeTime, fadeTime);

// All effekts have to be executed before calling the draw Function.
// Anything that is done after the draw Function call will not be rendered out.
draw.drawNotesByOriginToReceptor(duration);
```

# What is currently supported?
- Moving of note origin and receptors
- Rotation of note and receptors
- Rotation of the Playfield
- Moving the Playfield
- Swapping individual Playfield columns
- Drawing Notes over a Path via Anchor points
- Fulltransformation of Playfield (skew, tilt, etc) *this implementation is pretty bad atm and needs to be redone*

The drawinstace will handle all the Notes the only thing you have to manipulate is the origin and the receptors. The notes will follow automatically.

This might not seem like alot but by allowing everything to be moved individually, you can come up with alot of effekts already, granted that not everything is possible with this current setup.

# Contribution
Anyone is welcome to contribute and expand this library, i just wanted to make a **BASE** for people to work of from.

If you have Bugs or other stuff feel free to either create and Issue or make a PR if you have changes.
