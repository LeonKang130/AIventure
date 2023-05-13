# AIventure
An AI-driven roguelike RPG game by Anonimo group.

## Project Structure

-   scenes - Godot4 scenes(`*.tscn`).
-   scripts - Godot4 scripts(`*.cs/gd`).
-   arts - Images and sound effects.
-   fonts - `*.ttf` files.
-   network(W.I.P) - external **library project** that contains functionality for interaction with the APIs.

## Recommended Manners of Conduct

-   Please try to use `C#` for your Godot4 scripts instead of using `GDScript`.
-   Please obey **CamelCase** when your are using any `dotnet` languages(`C#/F#/VB`), among which I personally strongly suggest `C#/F#`.
-   You can refer to the documentation of Godot4 at its [official website](https://docs.godotengine.org/en/stable/contributing/ways_to_contribute.html#contributing-to-the-documentation). You are recommended to follow the 2D game example on the website before actually coding this project.
-   By default, we are using Godot4 .NET version. Incompatible version might cause you problems when you are trying to build and run the project.

-   Please organize additional components as external libraries in the project and add reference of the library using `dotnet add [<PROJECT>] reference [-f|--framework <FRAMEWORK>] [--interactive] <PROJECT_REFERENCES>`, like `dotnet add app/app.csproj reference lib/lib.csproj`. By adding the reference of your project to our main one, you can call your own functions without any effort other than `using/open` as long as you wrote your project in any of the `dotnet` languages.
-   Please try to explain what your code does by adding brief documentation.

## Current Status

Please check out *demo.mp4* for what it looks like right now.
