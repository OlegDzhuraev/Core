# Core
My tools and extensions for Unity Engine, which I'm use in all my projects to speedup development. It allows to reduce amount of code - by implementing frequently used functionality. Mainly there stored tools, which is not enough big or good to move them in their own repos.

This repo was exposed to public only just because I want to import it easily with Package Manager, but if you're interested, feel free to use it.

List of the main included features you can read below. 

## Tools
Tools can be found in the top menu, the button named **Tools**.

### Setup Project Tool
Tool allows to atuo-generate project folders structure and quckly tune most frequently needed for me Editor and Project settings.

## Level Design Tools
These tools also can be found in the top menu in **Tools** menu item.

### Transform Randomize
Allows to randomize rotation, scale and position of the scene-selected transforms.

## Extensions
Contains some extensions for Transform, Color, Vectors, Random and other components. Some examples below.

### Random extensions
Get random element from a list or array:
```cs
List<T> someList = new List<T>();

void Start() 
{
  var randomedT = someList.Random();
}
```

Randomize vector values:
```cs
Vector3 vec = RandomExtensions.GetRandomizedVector3(-5f, 5f);
```

### Physics extensions
You can quickly find objects of specific type T in sphere:
```cs
PhysicsExtensions.GetObjectsOfTypeInSphere<T>(pos, radius);

// for 2d
PhysicsExtensions.GetObjectsOfTypeIn2DCircle<T>(pos, radius);
```

## Audio
Allows to play audio directly from code without setting up AudioSources in prefabs.

Full initialization and usage example:
```cs
using InsaneOne.Core;
using UnityEngine;

public class TestAudio : MonoBehaviour
{
  [SerializeField] AudioClip clip;

  void Start()
  {
    // Initializes Core Audio system
    Audio.Init();

    // Configurations for 3D and 2D sounds.
    var data3DSound = new AudioGroupData()
    {
      Is3D = true,
      MinDistance3D = 2f,
      MaxDistance3D = 60f,
      DopplerLevel = 0f
    };
      
    var data2DSound = new AudioGroupData() { Is3D = false };

    // Setting up some different audio layers, both for 3d and 2d sounds. Audio layers is useful to limiting specific type sounds amount,
    // also audio layer stores audio settings, for example Min/Max distance or Audio Mixer Group (see code for more info).
    Audio.UpdateLayer(AudioLayer.Interaction, data3DSound);
    Audio.AddSourcesInLayer(AudioLayer.Interaction, 8);
      
    Audio.UpdateLayer(AudioLayer.Ambience, data3DSound);
    Audio.AddSourcesInLayer(AudioLayer.Ambience, 3);
      
    Audio.UpdateLayer(AudioLayer.UI, data2DSound);
    Audio.AddSourcesInLayer(AudioLayer.UI, 2);
  }

  void Update()
  {
    // Playing 3D audio (Interaction audio layer was setup as 3d earlier) in specified layer with 50% volume and 10% pitch randomization at transform position.
    if (Input.GetMouseButtonDown(0))
      Audio.Play(AudioLayer.Interaction, clip, transform.position, 0.5f, 0.1f);
  }
}

// Used just to make more readable code
public static class AudioLayer
{
  public const int Ambience = 10;
  public const int Interaction = 20;
  public const int UI = 100;
}
```

## Templates
In the Project Manager window, in context menu now exist a new partition **InsaneOne/Templates**, which includes some ready code file templates, which are frequently used by me in gamedev. Possible, will be removed in future or reworked to smth better, actually not very useful.

## UI
I've added some new elements and templates for UI, which is missing in Unity default package. Now it still very simple, but I want to improve it in future.

**Floating panel** - allows to create floating in a 3d world (following some object) UI-panel with some info.

**TabControl** - classic tab control element.

**PopupWindow** - allows to create a popup window with any title, text and Apply/Cancel buttons with apply callback.

**Fader** - commonly used in a game projects. Fades screen alpha into some color. Requires DOTween.

## Localization
Repo contains localization extension, which allows to read CSV-based localization and translate ingame texts for selected language.

```cs
// firstly, you need to run this in some game initialization code:
Localization.Initialize();

// use SetLanguage to change game lang:
Localization.SetLanguage("English"); // id of the lang

// Get any localized text:
var text = Localization.GetText("localeString");
```

Also, there exist useful component for localization without code - **LocalizedTMPText**. Add it to your text object and write localeId in its text field.

Localization uses **StreamingAssets** to contain a localization file - to allow modify it without game rebuild or allow modding of localization for players.

## Architect
Some code architect ready-made things. Probably not the best ones :)

### ServiceLocator
Alternative to the Singleton.

```cs
// setup in the game initialization code:
ServiceLocator.Register(new SomeClass());

// ...

// usage in any other class:
var someClass = ServiceLocator.Get<SomeClass>();
```

## Utility

### Pause Utility
Allows to pause game and use multiple pause affectors object. 
So, for example, two differect objects wants to pause game. Next call of unpause will actually **not** unpause game until **both** affector objects call it. 

```cs
using InsaneOne.Core.Utility;

class SomePauserObject : MonoBehaviour, IPauseAffector
{
  void SomeAction()
  {
    PauseUtility.Pause(this);
  }

  void SomeOtherAction()
  {
    PauseUtility.Unpause(this);
  }
}
```

### Timer
Deltatime-based timer to speedup any timer-based features creation.
```cs
Timer timer;

void Start() 
{
  timer = new Timer(5f);
}

void Update() 
{
  timer.DoTick()
  
  if (timer.IsReady())
  {
    // do something
  }
}
```

### DelayedDestruction
Allows to destroy a GameObject with attached component with the time delay.
```cs
gameObject.DelayedDestroy(3f);
```

### MainCamera
Most of projects use only one camera, which can be received by calling Camera.main. But in the old Unity versions it is not cached and can cause performance issues. This utility helps to solve this problem by caching the Main Camera. 

```cs
var cam = MainCamera.Cached;
```

## Shaders
This repo contains some PBR shaders, mainly to allow load textures from one mask (Metal-Roughness-AO, etc).

## License
MIT License
