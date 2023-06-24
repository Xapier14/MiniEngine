# MiniEngine
A full-fledged cross-platform 2D Game Engine.

## Getting Started
1. Reference the Project/Assembly.
1. Create an initializer to load initial and additional game assets.
   ```csharp
   GameEngine.AddInitializer(() =>
   {
       // do stuff

       return false; // return true if error occured in initializer.
   });
   ```
1. Start the engine by calling `GameEngine.Run()`.
   ```csharp
   GameEngine.AddInitializer(() =>
   {
       // do stuff

       return false; // return true if error occured in initializer.
   });
   GameEngine.Run();
   ```

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
    - Draw
    - Draw Spliced
- Create Documentation
- Create Example Projects
- Publish to NuGet
    
