# MiniEngine
A full-fledged cross-platform 2D Game Engine.

## Setting up a MiniEngine project
1. Install the `metool` command-line tool.
1. Reference the Project/Assembly.
1. Get an instance of the GameEngine.
   ```csharp
   var gameEngine = GameContext.GetGameEngine();
   ```
1. Create an initializer to load initial and additional game assets.
   ```csharp
   var gameEngine = GameContext.GetGameEngine();
   gameEngine.AddInitializer(() =>
   {
       // do stuff

       return false; // return true if error occured in initializer.
   });
   ```
1. Initialize the engine by calling `gameEngine.Initialize()` then start it by calling `gameEngine.Run()`.
   ```csharp
   var gameEngine = GameContext.GetGameEngine();
   gameEngine.AddInitializer(() =>
   {
       // do stuff

       return false; // return true if error occured in initializer.
   });
   gameEngine.Initialize()
   gameEngine.Run();
   ```
1. Create a `./Assets/.indexfile` in the root directory.\
   This file will be used for defining asset folders that will be referenced by the game.
   ```
   Audio
   Sprites
   Scenes
   ```
   Will mark the folders `./Assets/Audio`, `./Assets/Sprites`, and `./Assets/Scenes` as game assets.
1. Run the `metool pack` command on the `./Assets` directory.
   ```shell
   metool pack ./Assets
   ```
   This will produce a `Assets.mea` file that contains the assets specified by your `.indexfile`.
1. Run `dotnet run` to run the project.

## Basic Architecture
MiniEngine uses a simple ECS architecture.\
Game objects are usually defined as an `Entity` that may contain one or many `Component`.
Both of these objects/classes are ideally data-only. Behavior and logic are done by a `System` that act on its corresponding `Component`.

These Entities are then contained in a Scene.\
Scenes are functionally equivalent to a game world or room.\
You can switch scenes as needed for switching between levels or ui screens.
## Using game assets
MiniEngine supports the use of a packed asset file that acts as a read-only virtual file system.
1. Build and install the `MiniEngine.Tools.CLI` project and install as a dotnet tool.
1. Create a `.indexfile` file in your project directory. Files and folders specified in this file are set to be packed by the CLI tool.
1. Run `metool pack` on the project directory. The tool will produce a `assets.mea` file that contains your game assets.
1. Place and include this file on your project's output directory.
1. Reference a file by calling `Resources.GetResource(string path)`. You may also opt for implicit conversion via strings when calling functions that uses `MemoryResource`.

## Progress
### Current State
> Runnable with basic functionality included.

### Roadmap
- Create Basic Unit Structures
  - ~Size~
  - ~Vector2~
  - ~Color~
- Create BaseEngine Structure
  - ~Create engine init and cleanup~
  - ~Create engine loop~
- Create Entity Component System
  - ~Create System Service (Register & Execute)~
    - ~Support user-defined systems~
  - ~Add wiring via reflection~
  - Implement Basic Components and Systems
    - ~Add Transform and TransformSystem~
    - ~Add Motion and MotionSystem~
    - ~Add Sprite and DrawSystem~
  - Implement Complex Components and Systems
    - Add PhysicsBehavior and PhysicsSystem
    - ~Add Script and ScriptSystem~
    - Design a scripting API
- Create Windowing System
  - ~Implement window creation and respect engine configuration~
  - Add additional window configuration methods to be ran at runtime
- Create Resource Manager
  - ~Create packed file format~
    - ~Create index format~
  - ~Create loader to load resources into memory~
  - ~Create texture cache~
  - Create font loader
- Create Graphics Abstraction Layer
  - Add Primitive Draw Functions
    - Rectangle
    - Ellipse
    - Cicle
    - ~Pixel~
    - Line
    - Curved Line
  - Add Sprite Draw Functions
    - ~Draw~
    - Draw Spliced
- Create Documentation
- Create Example Projects
- Publish to NuGet
    
