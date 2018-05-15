# renderX

renderX: A 3D C# rendering API

renderX is a non dependant c# Rendering API capable of 3d rendering without the use of openGL or directX.

More Features Will Be Added Soon, such as: 
  - Raytracing
  - Shadows
  - Textures
  - Performance Fixes

This project was made after I wanted to make a 3D CAD program however I didn't want to use any other API's.

# Usage:

## Initializing The Render Processor:
```c#
  renderX renderProcessor = new renderX(displayWidth, displayHeight, fov); 
  ```
## Copying objectX data To Memory:
renderX Also includes a importer which allows you to import 3D objects.
```c#
objectX data = new objectX;
objectXImport importer = new objectXImport();

//Import The Data
importer.Import(FilePath);
data = importer.Analyse();
```
## objX Files Are Simple:
```c#
object:"objectName":(0,0,0)(0,0,0)(25,25,25)//(XYZ Position)(Rotation)(RotationPoint)
(0,0,0)(50,0,0)(50,0,50)(0,0,50).(255,255,0,0);//Each one of these lines represents a face
(0,0,0)(50,0,0)(50,50,0)(0,50,0).(255,255,255,0);//Three or four Vector3 Allowed
(0,0,0)(0,0,50)(0,50,50)(0,50,0).(255,0,255,0);//ARGB Color after the dot
(50,0,0)(50,0,50)(50,50,50)(50,50,0).(255,0,0,255);
(0,0,50)(50,0,50)(50,50,50)(0,50,50).(255,255,0,255);
(0,50,0)(50,50,0)(50,50,50)(0,50,50).(255,215,215,215);
//(X,Y,Z)(X,Y,Z)(X,Y,Z).(A,R,G,B); This is the face format
://Colon closes The Object
```
