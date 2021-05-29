# InsaneOne Core
My tools and extensions for Unity engine, which I'm use in all my projects to speedup development. It allows to reduce count of code by implementing frequently needed functionality.

Repo was exposed to public only just because I want to import it easily with Package Manager, but if you're interested, feel free to use it.

List of the main included features you can read below. 

## Setup Project Tool
Can be found in the top menu. Tool allows to atuo-generate project folders structure and quckly tune most frequently needed for me Editor and Project settings.

## Extensions
Contains some extensions for Transform, Color, Vectors, Random and other components. 

## Templates
In Project Manager in context menu now exist a new partition InsaneOne/Templates, which includes some ready code file templates, which are frequently used by me in gamedev. 

## UI
Unity UI is very raw for me, so I've added some new elements and templates. Now it still very simple, but I want to improve it in future.

## Utility

### Timer
Deltatime-based timer to speedup any timer-based features creation.

### DelayedDestruction
Allows to destroy GameObject with attached component with time delay.

### MainCamera
Most of projects use only one camera, which can be received by calling Camera.main. But in old Unity versions it is not cached and can cause performance issues. This utility helps to solve this problem by caching a Main Camera. 

## Interfaces
Includes inferfaces for some gamedev-specific things. Still do not sure that it is best realisation for this.

## License
MIT License
