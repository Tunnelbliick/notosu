# Storybrew osu!Mania Modcharts
## How do i use this?

### Initial Setup

1. Download all files
2. Drag and drop the Folders into your `/scriptLibrary` Directory
3. Drag and drop `AForge.dll` and `AForge.Imaging.dll` into the Main Storybrew Directory
4. Add `AForge.dll` and `AForge.Imaging.dll` to Assembly References *they are an Image editing library that is used for some Full Transformation effekts*
5. Initial setup complete!

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
var endtime = 10000;
var duration = 10000;

// Playfield Scale
var width = 250f;
var height = 500f;

// Note initilization Values
var bpm = 69f;
var offset = 69f;
var sliderAccuracy = 40;

// Drawinstance Values
var updatesPerSecond = 30;
var scrollSpeed = 900f;
var rotateNotesToFaceReceptor = false;
var fadeTime = 50;

var recepotrBitmap = GetMapsetBitmap("sb/sprites/receiver.png"); // The receptor sprite
var receportWidth = recepotrBitmap.Width;

Playfield field = new Playfield();
field.initilizePlayField(receptors, notes, startime, endtime, receportWidth, 60, 0);
field.ScalePlayField(starttime + 1, 1, OsbEasing.None, width, height); // Its important that this gets executed AFTER the Playfield is initialized otherwise this will run into "overlapped commands" and break
field.initializeNotes(Beatmap.HitObjects.ToList(), notes, bpm, offset, sliderAccuracy);

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
