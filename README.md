# renderX

renderX: A 3D C# rendering API

renderX is a non dependant c# Rendering API capable of 3d rendering without the use of openGL or directX.

More Features Will Be Added Soon, such as: 
  - Raytracing
  - Shadows
  - Textures
  - Performance Fixes

This project was made after I wanted to make a 3D CAD program however I didn't want to use any other API's.

##Usage:

#Initializing The Render Processor:
```c#
  renderX renderProcessor = new renderX(displayWidth, displayHeight, fov); 
  ```
#Copying objectX data To Memory:
renderX Also includes a importer which allows you to import 3D objects.
```
objectX data = new objectX;
objectXImport importer = new objectXImport();

importer.Import(FilePath);
data = importer.Analyse();
```
#objX Files Are Simple:
