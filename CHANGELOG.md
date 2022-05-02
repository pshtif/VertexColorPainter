# CHANGE LOG

All notable changes to this project will be documented in this file.

## RELEASES

### Release 0.6.3 - 2.5.2022

### Added

- Added option to export FBX from VCP asset directly

### Release 0.6.2 - 27.4.2022

### Changed

- Changed painted model naming to the name of mesh inside FBX instead of FBX name
- Changed submesh highlighting now works on CTRL even when autoFill is not enabled

### Release 0.6.1 - 25.4.2022

### Added

- Added option to enable/disable overlay rendering

### Release 0.6.0 - 21.4.2022

### Added

- Added support for other channels painting (UV0-UV3)
- Added custom masked shaders to visualise painted channels

### Changed

- Changed painted model is now rendered on top of the sceneview using custom shaders
- Multiple refactors in the code base to implement multi channel painting

### Release 0.5.3 - 8.4.2022

### Changed

- Changed reimporting now doesn't construct new mesh but copies data to keep existing scene references

### Fixed

- Fixed reimporting issue not saving assets

### Release 0.5.2 - 5.4.2022

### Added

- Added now showing the info which object I am paiting.

### Fixed

- Fixed issues when object gets destroyed when going out of scene/prefab without disabling painting.

### Release 0.5.1 - 31.3.2022

### Changed

- Changed the new editor raycasting to support gameobject filtering

### Release 0.5.0 - 30.3.2022

### Added

- Support for skinned meshes

### Changed

- Refactoring of huge parts to add skinned mesh support
- Changed editor raycasting implementation to avoid internal Unity bug that resulted in rendering glitches on MacOS

### Release 0.4.0 - 14.3.2022

### Added

- Color pick for Fill tool
- Color pick now supports transformed objects with worldToLocal handling
- More UI help texts

### Changed

- PaintedMeshFilter component is obsolete and will be removed
- Reimporting and mesh handling is now moved to asset object
- Assets are now saved inside a VCPAsset scriptable object

### Release 0.3.0 - 10.3.2022

### Added

- Added support to enable closes point paint (automatically paints closest vertex to hit point even outside of brushsize bounds)
- Color pick for Paint tool
- Color pick now supports transformed objects with worldToLocal handling
- Added on screen help text for tooling

### Changed

- Removed mesh isolation due to issues on some platforms and gizmo handling in this mode
- Optimizations in fill tool
- More refactoring through codebase

### Release 0.2.1 - 8.3.2022

#### Added

- Added vertex checking option if they are unique (not same for different submeshes)

### Release 0.2.0 - 8.3.2022

#### Added

- New color tool where you change color directly instead of painting
- New color editor tool where you see palette of a whole object and can recolorize

#### Changed

- Complete refactoring of the codebase to cleanup

#### Fixed

- Reimport now correctly reindexes old color data

### Release 0.1.9 - 1.3.2022

#### Changed

- Naming of painted assets is now fbx_name + mesh_name + _painted so it is possible to auto save multiple modified assets from single FBX 

### Release 0.1.8 - 28.2.2022

#### Changed

- Convert all path checks to lowercase :)

### Release 0.1.7 - 23.2.2022

#### Changed

- Changed painted geometry now saves as an Unity asset to the FBX folder due to prefab referencing

### Release 0.1.6 - 21.2.2022

#### Fixed

- Fixed GUI uses now scaled pixelRect to avoid scaling problems

### Release 0.1.5 - 15.2.2022

#### Added

- Added submesh right click pick
- Added direct color change in scene view on right click
- Added initial reimport mapping

### Release 0.1.4 - 11.2.2022

#### Added

- Added autofill functionality
- Added submesh list with colors

### Release 0.1.3 - 9.2.2022

#### Added

- Added mesh data moved to instance upon painting
- Added PaintedMeshFilter to hide MeshFilter and let user know that it is a painted instance

#### Changed

- Changed brushes gizmos sizes now caluclated based on viewport

### Release 0.1.2 - 17.1.2022

#### Added

- Added ability to frame painted object in the view
- Added ability to isolate (render only) the painted object (EXPERIMENTAL)
- Added settings menu for auto framing/isolation

#### Changed

- Changed brushes gizmos now have dynamic width calculation
- Changed brushes gizmos now have color of brush

#### Fixed

- Fixel some meta files in the npm repo

### Release 0.1.1 - 13.1.2022

#### Added
- Added undo handling
- Added color fill now as a brush tool so you click on part of object to fill color

### Release 0.1.0 - 12.1.2022

#### Added
- Added initial version :)