# renderX
##### RenderX Is A Non Dependant C# Rendering API Capable of 3D Rendering Graphics Without The Use of OpenGL or DirectX.

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
  It is also recommended to enable double buffering to prevent screen flicker!
## Copying objectX data To Memory:
renderX Also includes a importer which allows you to import 3D objects.
```c#
objectX data = new objectX();
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
## Editing Object Rotation Before Or During Runtime:
Each object is stored in an array, Which is then sent into the render program
These objects can be retrived from the objectX dataStore by using:
```c#
data.objXData[0].Name = "";                
data.objXData[0].objectFaces = new Face3D[0];                
data.objXData[0].Position = new Vector3(0, 0, 0);               
data.objXData[0].Rotation = new Vector3(0, 0, 0);                
data.objXData[0].RotationOffset = new Vector3(0, 0, 0);
```
## Rendering The Objects To Screen:
RenderX outputs to a bitmap.
```c#
this.BackgroundImage = renderProcessor.ProcessData(camPosition, camRotation, data, lightPosition);
```

# Known Bugs
- Runs out of memory fast on high resolutions
- Z Buffer Incorrect
- Camera 3D Pan isin't finished yet
