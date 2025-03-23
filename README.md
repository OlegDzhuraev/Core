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
    Audio.AddLayer(AudioLayer.Interaction, data3DSound, 8);
    Audio.AddLayer(AudioLayer.Ambience, data3DSound, 3);
    Audio.AddLayer(AudioLayer.UI, data2DSound, 2);
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
### AudioData
Extension for AudioClip. Allows to setup more sound settings in the inspector:
- Sound variations
- Volume
- Pitch random
- Loop toggle

Can be used with Audio system, described above.
Main idea is to move sound setup from the prefab AudioSource settings to ScriptableObject or your own scripts. 

```cs
[SerializeField] AudioData data;

// <...>
Audio.Play(AudioLayer.Interaction, data, transform.position);
```

### SoundMixer
Allows to mix several AudioSources, driven by some mix paramter. For example, changing sound by Engine RPM change.

```cs
using InsaneOne.Core;
using UnityEngine;

public class TestSoundMix : MonoBehaviour
{
  SoundMixer soundMixer;
  
  void Start()
  {
      // setup audio system before below code runs (see prev example), if you want to use this system

      // get audio from the Audio system of previous example. You can use AudioSources directly, if you dont need this system
      Audio.TryGetFreeSource(AudioLayer.Interaction, out var sourceA);
      Audio.TryGetFreeSource(AudioLayer.Interaction, out var sourceB);

      // initialization of the sound mixer (you can pass any audio sources amount)
      soundMixer = new SoundMixer(sourceA, sourceB);
  }

  void Update()
  {
    // set any mix value from 0 to 1, and volume of specified sounds will be changed accordingly
    if (Input.GetKeyDown(KeyCode.Alpha1)) 
        soundMixer.UpdateMix(0.5f);

    if (Input.GetKeyDown(KeyCode.Alpha2)) 
        soundMixer.UpdateMix(Random.Range(0f, 1f));

    // you also can tween your value and pass it to the UpdateMix method
  }
}
```

## Templates
In the Project Manager window, in context menu now exist a new partition **InsaneOne/Templates**, which includes some ready code file templates, which are frequently used by me in gamedev. Possible, will be removed in future or reworked to smth better, actually not very useful.

## UI
I've added some new elements and templates for UI, which is missing in Unity default package. Now it still very simple, but I want to improve it in the future.

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
Localization.SetLanguage("English"); // id of the language

// Get any localized text:
var text = Localization.GetText("localeString");
```

Also, there exist useful component for localization without code - **LocalizedTMPText**. Add it to your text object and write localeId in its text field.

Localization uses **StreamingAssets** to contain a localization file - to allow modify it without game rebuild or allow modding of localization for players.

## Architect
Some code architect ready-made things. Probably not the best ones :)

### Context
Context class allows you to semi-automatically provide some specific context data-class to any of your components.

Initialization:
```cs
class GameBootstrap : MonoBehaviour
{
  void Awake()
  {
    var context = new YourContext(); // YourContext - it can be any your class with data, which should be shared
    // setup here your context class with required data
    Context<YourContext>.Initialize(context); // will initialize all objects on scene, which have components, deriven from the ContextBehaviour<YourContext> by injecting your context
  }
}
```

In order to provide context to a new spawned objects, use Context.Spawn() instead of GameObject.Instantiate():
```cs
Context<YourContext>.Spawn(prefab, new Vector3(15, 0, 25)); // you can pass position, rotation and parent like in the original GameObject.Instantiate
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
```

**Note:** You need to initialize Context in **Awake** before any other components. Use **ScriptExecutionOrder** for this.

Additional info: The **Context** class is implemented in this way to reduce the number of required actions on the developer's part. An alternative would be some kind of initialization of ContextBehaviour via Awake method of this abstract class, but I found it uneffective to override this method in your own classes every time.

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
This library contains some built-in components. You can check it in the Sources/Components folder.

In this partition can be found info about some of these components.

### Teams
A lot of games have teams for game players and NPCs. There is implementation for this functionality.

Currently, team represented by **int** value.

**How to use:**

First of all, there is extension for Unity's GameObjects to make work with teams easier. But to enable them, you need to add scripting define symbol `INSANE_TEAMS_EXTENSION` into the **Player Settings**.
After it's done, you can use this example code:

```cs
using InsaneOne.Core;
// <...>
    
[SerializeField] GameObject enemy;

void Start()
{
  gameObject.SetTeam(0); // set this object team
  enemy.SetTeam(1); // set different team to the enemy object
}

void Update()
{
  var myTeam = gameObject.GetTeam(); // get team of this object
  var enemyTeam = enemy.GetTeam(); // get Enemy object team

  if (myTeam != enemyTeam)
    DoAttack(enemy); // proceed some action if teams are different
}
```

Actually, this code works with custom **TeamBehaviour** component - adds it to any teamed objects, and stores actual object team in this component.

You can also create TeamsSettings asset, and setup, which teams will be enemies to others. To create it, click **RMB** in **Project Window**, and in the context menu select **InsaneOne** -> **TeamsSettings**.

After creation, drag'n'drop it to the field **Teams Settings** of the **CoreData** asset (which is created automatically).

To use your teams settings:

```cs
var isEnemies = gameObject.IsTeamEnemyTo(otherGameObject); // API can change
```

## Dependency Injection
There are some tools to implement basic dependency injection. All dependencies will be injected in fields of the same type as dependency (or interface) and with `[Inject]` attribute in the dependency-receiver classes.

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
Allows to pause game and use multiple pause affectors object. 
So, for example, two different objects wants to pause game. Next call of unpause will actually **not** unpause game until **both** affector objects call it. 

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
Deltatime-based timer to speed up any timer-based features creation.
```cs
Timer timer;

void Start() 
{
  timer = new Timer(5f); // creating 5 secodns timer.
}

void Update() 
{
  timer.DoTick(); // iterating timer in the update (for example, you can do it only with condition, to imitate some pause)
  
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
Most of the projects use only one camera, which can be received by calling Camera.main. But in the old Unity versions it is not cached and can cause performance issues. This utility helps to solve this problem by caching the Main Camera. 

```cs
var cam = MainCamera.Cached;
```

## Shaders
This repo contains some PBR shaders, mainly to allow load textures from one mask (Metal-Roughness-AO, etc).

## License
MIT License
