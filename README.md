![GitHub License](https://img.shields.io/github/license/OlegDzhuraev/Core) ![Static Badge](https://img.shields.io/badge/unity%20version-2022%2B-blue) ![GitHub last commit](https://img.shields.io/github/last-commit/OlegDzhuraev/Core) ![GitHub package.json version](https://img.shields.io/github/package-json/v/OlegDzhuraev/Core)

<p align="center">
    <img src="https://dzhuraev.com/GithubData/CoreLogoWide3.png" alt="Core">
</p>

# Core
My tools and extensions for Unity Engine, which I use in all my projects to speed up development. It allows reducing the amount of code by implementing frequently used functionality. Mainly, it stores tools which are not big or good enough to be moved into their own repos.

This repo was made public only because I wanted to import it easily with Package Manager, but if you're interested, feel free to use it.

You can find the list of main included features below.

## Tools
Tools can be found in the top menu, under the button named **Tools**.

### Setup Project Tool
This tool allows you to auto-generate the project folder structure and quickly tune the Editor and Project settings I need most frequently.

## Level Design Tools
These tools can also be found in the top menu, under the **Tools** menu item.

### Transform Randomize
Allows you to randomize the rotation, scale and position of the transforms selected in the scene.

### Object Placer
Places prefabs from an assigned palette onto scene colliders by clicking in the Scene view, with support for physics layer filtering, aligning to the surface normal, and randomizing position, rotation and scale. It can be toggled on/off from the tool window or from its own Scene view overlay toggle.

## Extensions
Contains some extensions for Transform, Color, Vectors, Random and other components. Some examples below.

### Random extensions
Get a random element from a list or array:
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
You can quickly find objects of a specific type T within a sphere:
```cs
PhysicsExtensions.GetObjectsOfTypeInSphere<T>(pos, radius);

// for 2d
PhysicsExtensions.GetObjectsOfTypeIn2DCircle<T>(pos, radius);
```

## Audio
Allows you to play audio directly from code without setting up AudioSources in prefabs.

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

    // Setting up some different audio layers, both for 3d and 2d sounds. Audio layers are useful for limiting the amount of a specific sound type,
    // audio layers also store audio settings, for example Min/Max distance or Audio Mixer Group (see code for more info).
    Audio.AddLayer(AudioLayer.Interaction, data3DSound, 8);
    Audio.AddLayer(AudioLayer.Ambience, data3DSound, 3);
    Audio.AddLayer(AudioLayer.UI, data2DSound, 2);
  }

  void Update()
  {
    // Playing 3D audio (Interaction audio layer was set up as 3d earlier) in the specified layer with 50% volume and 10% pitch randomization at transform position.
    if (Input.GetMouseButtonDown(0))
      Audio.Play(AudioLayer.Interaction, clip, transform.position, 0.5f, 0.1f);
  }
}

// Just used to make the code more readable
public static class AudioLayer
{
  public const int Ambience = 10;
  public const int Interaction = 20;
  public const int UI = 100;
}
```
### AudioData
Extension for AudioClip. Allows you to set up more sound settings in the inspector:
- Sound variations
- Volume
- Pitch random
- Loop toggle

Can be used with the Audio system described above.
The main idea is to move sound setup from the prefab's AudioSource settings to a ScriptableObject or your own scripts.

```cs
[SerializeField] AudioData data;

// <...>
Audio.Play(AudioLayer.Interaction, data, transform.position);
```

### SoundMixer
Allows you to mix several AudioSources, driven by some mix parameter. For example, changing the sound based on the engine's RPM.

```cs
using InsaneOne.Core;
using UnityEngine;

public class TestSoundMix : MonoBehaviour
{
  SoundMixer soundMixer;
  
  void Start()
  {
      // set up the audio system before the code below runs (see the previous example), if you want to use this system

      // get audio from the Audio system of the previous example. You can use AudioSources directly, if you don't need this system
      Audio.TryGetFreeSource(AudioLayer.Interaction, out var sourceA);
      Audio.TryGetFreeSource(AudioLayer.Interaction, out var sourceB);

      // initialization of the sound mixer (you can pass any number of audio sources)
      soundMixer = new SoundMixer(sourceA, sourceB);
  }

  void Update()
  {
    // set any mix value from 0 to 1, and volume of specified sounds will be changed accordingly
    if (Input.GetKeyDown(KeyCode.Alpha1)) 
        soundMixer.UpdateMix(0.5f);

    if (Input.GetKeyDown(KeyCode.Alpha2)) 
        soundMixer.UpdateMix(Random.Range(0f, 1f));

    // you can also tween your value and pass it to the UpdateMix method
  }
}
```

## Templates
In the Project window, the context menu now has a new **InsaneOne/Templates** section, which includes some ready-made code file templates that I frequently use in game dev. This might be removed in the future or reworked into something better — honestly, not very useful as is.

## UI
I've added some new elements and templates for UI that are missing from Unity's default package. It's still very simple for now, but I want to improve it in the future.

- **Element** — base abstract component with show/hide functionality, WasShown/WasHidden events, with support for ViewModel. You can use it in code to implement your own UI elements.
- **FloatingPanel** — allows you to create a UI panel floating in the 3d world (following some object) with some info.
- **TabControl** — classic tab control element.
- **PopupWindow** — allows you to create a popup window with any title, text and Apply/Cancel buttons with an apply callback.
- **Fader** — commonly used in game projects. Fades the screen alpha into some color. Requires DOTween.
- **Hint** — shows a non-interactable pop-up with info text near the cursor or some other element.
- **ProgressBar** — component to automate the usage of a filled Image component.
- **CircularLayout** — allows you to automatically place elements on a circle with a specified radius. Alternative to **Horizontal/VerticalLayout**.

## Localization
The repo contains a localization extension, which allows you to read CSV-based localization and translate in-game texts for the selected language.

```cs
// firstly, you need to run this in some game initialization code:
Localization.Initialize();

// use SetLanguage to change the game's language:
Localization.SetLanguage("English"); // id of the language

// Get any localized text:
var text = Localization.GetText("localeString");
```

Also, there is a useful component for localization without code — **LocalizedTMPText**. Add it to your text object and write localeId in its text field.

Localization uses **StreamingAssets** to store the localization file — this allows you to modify it without rebuilding the game, or to allow players to mod the localization.

## Architect
Some ready-made architecture-related code. Probably not the best ones :)

### Context
The Context class allows you to semi-automatically provide a specific context data class to any of your components.

Initialization:
```cs
class GameBootstrap : MonoBehaviour
{
  void Awake()
  {
    var context = new YourContext(); // YourContext can be any class of yours holding data that should be shared
    // setup here your context class with required data
    Context<YourContext>.Initialize(context); // will initialize all objects in the scene that have components derived from ContextBehaviour<YourContext>, by injecting your context
  }
}
```

In order to provide context to newly spawned objects, use ContextSpawner.Spawn() instead of GameObject.Instantiate():
```cs
ContextSpawner.Spawn<YourContext>(prefab, new Vector3(15, 0, 25)); // you can pass position, rotation and parent like in the original GameObject.Instantiate
// ContextSpawner.Spawn<T1, T2>(...), <T1, T2, T3>(...) etc. are also available, if the prefab needs several context types at once
```

Context access in your component:
```cs
class YourClass : ContextBehaviour<YourContext>
{
  void Start()
  {
    Debug.Log(context.SomeVariable); // you can access any context variable now
  }
}

// or you can do this, if you have several contexts:
class YourClass : MonoBehaviour, IContext<YourContextA>, IContext<YourContextB> //, etc...
{
  public void OnContextReload(YourContextA contextA)
  {
   Debug.Log(contextA.SomeVariable); // you can access any context variable now and you can cache context in your class
  }

  public void OnContextReload(YourContextB contextB)
  {
   // do something
  }

  void Start()
  {
    Debug.Log(cachedContextA.SomeVariable); // if you cached the context in OnContextReload, you can access it in the Start method too
  }
}

```

You can also read the current context value directly, without implementing `IContext<T>` (e.g. from a plain, non-MonoBehaviour class):
```cs
var context = Context<YourContext>.Get(); // returns null if not initialized yet
```

**Note:** You need to initialize Context in **Awake** before any other components. Use **ScriptExecutionOrder** for this.

Additional info: The **Context** class is implemented in this way to reduce the number of required actions on the developer's part. An alternative would be some kind of initialization of ContextBehaviour via the Awake method of this abstract class, but I found it ineffective to override this method in your own classes every time.

### ServiceLocator
Alternative to the Singleton.

```cs
// setup in the game initialization code:
ServiceLocator.Register(new SomeClass());

// ...

// usage in any other class:
var someClass = ServiceLocator.Get<SomeClass>();
```

## Components
This library contains some built-in components. You can check them in the Sources/Components folder.

Info about some of these components can be found in this section.

### Teams
A lot of games have teams for players and NPCs. There is an implementation of this functionality here.

Currently, a team is represented by an **int** value.

**How to use:**

First of all, there is an extension for Unity's GameObjects to make working with teams easier. But to enable it, you need to add the scripting define symbol `INSANE_TEAMS_EXTENSION` to the **Player Settings**.
After it's done, you can use this example code:

```cs
using InsaneOne.Core;
// <...>
    
[SerializeField] GameObject enemy;

void Start()
{
  gameObject.SetTeam(0); // set this object's team
  enemy.SetTeam(1); // set a different team to the enemy object
}

void Update()
{
  var myTeam = gameObject.GetTeam(); // get the team of this object
  var enemyTeam = enemy.GetTeam(); // get the enemy object's team

  if (myTeam != enemyTeam)
    DoAttack(enemy); // take some action if teams are different
}
```

Actually, this code relies on the custom **TeamBehaviour** component — add it to any teamed object, and it will store that object's actual team.

You can also create a TeamsSettings asset and set up which teams will be enemies to others. To create it, click **RMB** in the **Project Window**, and in the context menu select **InsaneOne** -> **TeamsSettings**.

After creation, drag and drop it into the **Teams Settings** field of the **CoreData** asset (which is created automatically).

To use your teams settings:

```cs
var isEnemies = gameObject.IsTeamEnemyTo(otherGameObject); // API can change
```

## Dependency Injection
There are some tools to implement basic dependency injection. All dependencies will be injected into fields of the same type as the dependency (or interface) that carry the `[Inject]` attribute in the dependency-receiver classes.

```cs
var service = new SomeService(); // this is an example dependency
var diTarget = new SomeTarget(); // this is an example class, which will receive dependencies

var diContainer = new Container(InjectionType.All);
diContainer.AddAsDependency(service); // collecting dependencies
// you can add any number of dependencies

injectContainer.AddAsTarget(diTarget); // adding dependencies receivers (targets)
// you can add any number of targets

injectContainer.Resolve(); // pushing all dependencies to all receivers (targets)
```

Dependency receiver (target) class example:
```cs
class SomeTarget()
{
  [Inject] SomeService service;

  // your logic here
}
```

## Utility

### Pause Utility
Allows you to pause the game and use multiple pause source objects.
So, for example, two different objects want to pause the game — the next call to unpause will actually **not** unpause the game until **both** affector objects call it.

```cs
using InsaneOne.Core.Utility;

class SomePauserObject : MonoBehaviour, IPauseSource
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
Delta-time-based timer to speed up the creation of any timer-based features.
```cs
Timer timer;

void Start() 
{
  timer = new Timer(5f); // creating a 5 second timer.
}

void Update() 
{
  timer.DoTick(Time.deltaTime); // updating the timer every frame (for example, you can do this only under some condition, to imitate a pause)
  
  if (timer.IsReady())
  {
    // do something
  }
}
```

### DelayedDestruction
Allows you to destroy a GameObject with the attached component after a time delay.
```cs
gameObject.DelayedDestroy(3f);
```

### MainCamera
Most projects use only one camera, which can be retrieved by calling `Camera.main`. But in older Unity versions it is not cached and can cause performance issues. This utility helps solve this problem by caching the Main Camera.

```cs
var cam = MainCamera.Cached;
```

## Shaders
This repo contains some PBR shaders, mainly to allow loading textures from a single mask (Metal-Roughness-AO, etc).
