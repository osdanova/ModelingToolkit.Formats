# ModelingToolkit.Formats

A library to work with 3D models.

The Formats package extends the core functionality with utilities to import and export models as multiple formats.

Modeling Toolkit uses Gltf's coordinate system:

* X positive: Right
* Y positive: Up
* Z positive: Forward

### Currently supported formats:

| Format | Export | Import |
| :- | :-: | :-: |
| GLTF | In Progress | No |
| FBX | No | No |

### Modeling Toolkit packages

* [ModelingToolkit](https://github.com/osdanova/ModelingToolkit): A tool to process and display 3D models. Note: At the time of writing ModelingToolkit does not use Core but it's inteded that it uses it in the future.
* [ModelingToolkit.Core](https://github.com/osdanova/ModelingToolkit.Core): The Core package contains the very basic platform agnostic objects to work with 3D models.


### Dependencies:

* [SharpGLTF](https://github.com/vpenades/SharpGLTF)
* [Assimp.Net 5.0.0](https://bitbucket.org/Starnick/assimpnet/src/master/) (Future FBX implementation)