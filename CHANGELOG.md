# CHANGE LOG

All notable changes to this project will be documented in this file.

## RELEASES

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