# ModelingToolkit.Formats

A library to work with 3D models.

## Packages

| Package | Description |
| - | :- |
| [ModelingToolkit](https://github.com/osdanova/ModelingToolkit) | A tool to process and display 3D models |
| [ModelingToolkit.Formats](https://github.com/osdanova/ModelingToolkit.Formats) | An extension of core that supports importing and exporting using other formats |
| [ModelingToolkit.Core](https://github.com/osdanova/ModelingToolkit.Core) | The very basic platform agnostic objects to work with 3D models |

## Dependencies

* ModelingToolkit.Core
* [SharpGLTF.Toolkit - 1.0.1](https://github.com/vpenades/SharpGLTF)
* [AssimpNet - 5.0.0-beta1](https://bitbucket.org/Starnick/assimpnet/src/master/) (Future FBX implementation)

### Currently supported formats:

| Format | Export | Import |
| :- | :-: | :-: |
| GLTF | Model | - |
| FBX | Model | Model |

## Usage
Use the MtPorter class to import scenes to MtScenes and export them.