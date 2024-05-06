# notOSU! - A Tool for Dynamic osu!mania Storyboarding
![](https://repository-images.githubusercontent.com/669634772/939caa35-269b-44bb-92e0-4b0aa3d3ba1d)

Welcome to notOSU!, an innovative tool designed to enhance your storyboarding experience in osu!mania. This README provides guidance on setting up and configuring notOSU! for your storyboard projects.

## Getting Started

### [notOSU! Documentation](https://notosu.sh). 

### Step 1: Downloading notOSU!

Begin by downloading the latest release of notOSU!:

- Visit the [official repository](https://github.com/Tunnelbliick/notosu/releases/latest).
- Choose the most recent version to access the latest features.

### Step 2: Installation

After downloading, integrate notOSU! into your storyboard environment:

- Find the `/scriptLibrary` directory in your storyboard project.
- Extract and place the downloaded notOSU! folders into the `/scriptLibrary` directory.

Congratulations! The initial setup of notOSU! is now complete.

## Configuring a Playfield for Storyboarding

Creating a dynamic storyboard in notOSU! involves setting up layers, a playfield instance, and a DrawInstance.

### Required Components:

1. **Layers**: Define layers for receptors and notes.
2. **Playfield Instance**: Manage the gameplay area and note mechanics.
3. **DrawInstance**: Handle the drawing and animation of notes.

### Example Configuration:

```csharp
// Generate function in a storybrew script
public override void Generate() {

    var receptors = GetLayer("r");
    var notes = GetLayer("n");

    // General values
    var starttime = 0; // the starttime where the playfield is initialized
    var endtime = 257044; // the endtime where the playfield is nolonger beeing rendered
    var duration = endtime - starttime; // the length the playfield is kept alive

    // Playfield Scale
    var width = 250f; // widht of the playfield / invert to flip
    var height = 600f; // height of the playfield / invert to flip -600 = downscroll | 600 = upscropll
    var receptorWallOffset = 50f; // how big the boundary box for the receptor is 50 means it will be pushed away 50 units from the wall

    // Note initilization Values
    var sliderAccuracy = 30; // The Segment length for sliderbodies since they are rendered in slices 30 is default
    var isColored = false; // This property is used if you want to color the notes by urself for effects. It does not swap if the snap coloring is used.

    // Drawinstance Values
    var updatesPerSecond = 50; // The amount of steps the rendring engine does to render out note and receptor positions
    var scrollSpeed = 900f; // The speed at which the Notes scroll
    var fadeTime = 150; // The time notes will fade in

    Playfield field = new Playfield();
    field.initilizePlayField(receptors, notes, starttime, endtime, width, height, receptorWallOffset, Beatmap.OverallDifficulty);
    field.initializeNotes(Beatmap.HitObjects.ToList(), Beatmap.GetTimingPointAt(starttime).Bpm, Beatmap.GetTimingPointAt(starttime).Offset, isColored, sliderAccuracy);

    DrawInstance draw = new DrawInstance(field, starttime, scrollSpeed, updatesPerSecond, OsbEasing.None, true, fadeTime, fadeTime);
    draw.drawViaEquation(duration, NoteFunction, true);
}

// NoteFunction is used to manipulate the pathway and a bunch of other things the note should do on their way to the receptor
// Please be warry that this is beeing run async so you need to keep thread safety in mind when working on complex Functions.
// You can use the progress to determin how far the note is in its cycle 0 = just start | 1 = ontop of receptor / finished
// Special flags for hold bodies exist
public Vector2 NoteFunction(EquationParameters p)
{
    return p.position;
}
```

Follow these instructions to effectively set up your notOSU! Playfield and Notes for storyboard creation.

## Documentation and Community Support

For detailed guidance and advanced features, refer to the [notOSU! Documentation](https://notosu.sh). Join our community on [Discord](https://discord.notosu.sh) for support, collaboration, and sharing ideas with fellow storyboarders.

Start your creative journey in osu!mania storyboarding with notOSU! today!
