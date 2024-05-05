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
var receptors = GetLayer("r");
var notes = GetLayer("n");

// General values
var starttime = 0;
var endtime = 134573;
var duration = endtime - starttime;

// Playfield Scale
var width = 250f;
var height = -500;

// Note initilization Values
var bpm = Beatmap.GetControlPointAt(9999).Bpm;
var offset = Beatmap.GetControlPointAt(9999).Offset;

// Drawinstance Values
var updatesPerSecond = 50;
var scrollSpeed = 1000f;
var rotateNotesToFaceReceptor = false;
var fadeTime = 60;
var sliderAccuracy = 40;

Playfield field = new Playfield();
field.initilizePlayField(receptors, notes, starttime, endtime, 250f, height, 50);
field.initializeNotes(Beatmap.HitObjects.ToList(), bpm, offset, false, sliderAccuracy);

DrawInstance draw = new DrawInstance(field, starttime, scrollSpeed, updatesPerSecond, OsbEasing.None, rotateNotesToFaceReceptor, fadeTime, fadeTime);

draw.DrawNotesByOriginToReceptor(duration);
```

Follow these instructions to effectively set up your notOSU! Playfield and Notes for storyboard creation.

## Documentation and Community Support

For detailed guidance and advanced features, refer to the [notOSU! Documentation](https://notosu.sh). Join our community on [Discord](https://discord.notosu.sh) for support, collaboration, and sharing ideas with fellow storyboarders.

Start your creative journey in osu!mania storyboarding with notOSU! today!
