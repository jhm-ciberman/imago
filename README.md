# Imago

**A fast, modern C# game framework. No editor in the way. Just C# you'll actually enjoy writing.**

Imago is a 3D game framework for .NET. It ships with a forward renderer, a 2D sprite system, a retained-mode UI with over 50 controls, glTF/OBJ asset loading, input handling, and a developer console. Everything is written in plain C# that you can read, edit, and debug like any other .NET code. No editor, no content pipeline, no external tools between you and your game.

I built Imago for my game Medieval Life over several years. I'm extracting it so others can read it, fork it, and lift whatever's useful.

## Features

- **Forward 3D renderer** with cascaded shadow maps, mouse picking, particles, gizmos, a sky dome, and an ImGui overlay for debug UI.
- **Automatic draw-call batching** with no per-frame allocations in the render path. Compatible renderables collapse into single instanced draws.
- **2D sprite and text rendering** with an immediate-mode drawing API, alpha blending, and inline emoji support.
- **Write GLSL, attach it to a C# class with one attribute, done.** No visual shader graph, no content pipeline.
- **Your shaders run on Vulkan, OpenGL, OpenGL ES, and Direct3D 11** without changes.
- **Automatic shader variants** for transparent, alpha-tested, wireframe, shadow-receiving, foggy, and other rendering modes.
- **Retained-mode UI** in the spirit of WPF, with measure/arrange layout, styles, data binding, and over 50 controls out of the box: panels, grids, docks, buttons, text, lists, scroll viewers, images, tooltips, popups, and more.
- **UI templates** written in XML with C# code-behind, compiled to type-safe partial classes at build time by a Roslyn source generator. Full IntelliSense, zero runtime reflection.
- **MVVM-friendly** out of the box. UI controls inherit from `ObservableObject` and commands bind naturally. Nothing forces MVVM on you.
- **Assets**: glTF 2.0 and Wavefront OBJ with skeletal animation, PNG and JPG textures with runtime atlasing, TTF and OTF fonts.
- **Input**: keyboard, mouse, and gamepad with event and polling APIs. Events route front-to-back through the UI so modal dialogs block input from the scene behind them.
- **In-game developer console** with commands, autocomplete, and history. Attribute-based command registration. One line to add it to your game.

## Platforms

|             | Vulkan | Direct3D 11 | OpenGL | OpenGL ES |
| :---------- | :----: | :---------: | :----: | :-------: |
| Windows     |   ✅   |     ✅      |   ✅   |    ✅     |
| Linux       |   ✅   |     --      |   ✅   |    ✅     |
| macOS       | ✅ (1) |     --      |   ❌   |    --     |

(1) Via [MoltenVK](https://github.com/KhronosGroup/MoltenVK).

## Architecture

Three projects:

- **Imago**: the framework itself. Application, rendering, scene graph, input, UI controls, assets, dev console.
- **Imago.Support**: foundational types shared with consumers. Math primitives (`Vector2Int`, `Rect`, etc.), collections, colors, tweening, extensions.
- **Imago.Generators**: compile-time Roslyn source generators used internally.

## Requirements

- .NET 10 SDK
- C# 14

## Building

```sh
dotnet build
```

No code generation step, no content pipeline, no external tools. If it doesn't build, that's a bug.

## Acknowledgments

Imago is named in memory of my grandfather Armando, whose educational video production was called "Imago" (Latin for "image"). This project carries his name.

It also stands on other people's work:

- [Veldrid](https://github.com/mellinoe/veldrid) by Eric Mellino, the cross-platform graphics library that shaped how I think about rendering in .NET.
- [FontStashSharp](https://github.com/FontStashSharp/FontStashSharp) for font rendering and text layout.
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) for image loading.
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) for the MVVM patterns Imago leans on.
- [glTF2Loader](https://github.com/KhronosGroup/glTF-CSharp-Loader) for glTF parsing.

## License

Imago is released under the [MIT license](LICENSE).
