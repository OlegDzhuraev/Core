# InsaneOne Core
My tools and extensions for Unity Engine, which I'm use in all my projects to speedup development. It allows to reduce amount of code - by implementing frequently used functionality. Mainly there stored some garbage tools, which is not enough big or good to move them in their own repo.

This repo was exposed to public only just because I want to import it easily with Package Manager, but if you're interested, feel free to use it.

List of the main included features you can read below. 

## Tools
Tools can be found in the top menu, button named **Tools**.

### Setup Project Tool
Tool allows to atuo-generate project folders structure and quckly tune most frequently needed for me Editor and Project settings.

### Level Design - Transform Randomize
Allows to randomize rotation, scale and position of the scene-selected transforms.

## Extensions
Contains some extensions for Transform, Color, Vectors, Random and other components. Some examples below.

### Random extensions
Get random element from list or array:
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

## Templates
In Project Manager in context menu now exist a new partition InsaneOne/Templates, which includes some ready code file templates, which are frequently used by me in gamedev. 

## UI
I've added some new elements and templates for UI, which is missing in Unity default package. Now it still very simple, but I want to improve it in future.

## Utility

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
Allows to destroy GameObject with attached component with time delay.
```cs
gameObject.DelayedDestroy(3f);
```

### MainCamera
Most of projects use only one camera, which can be received by calling Camera.main. But in old Unity versions it is not cached and can cause performance issues. This utility helps to solve this problem by caching a Main Camera. 

```cs
var cam = MainCamera.Cached;
```

## License
MIT License
