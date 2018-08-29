using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Threading.Tasks;

namespace renderXdemo
{
    public class renderX
    {
        int BitmapWidth;
        int BitmapHeight;
        float FOV;

        float AspectRatio;

       
        Bitmap bmp;

        public renderX(int Width, int Height, float FieldOfViewInRadians){
            BitmapWidth = Width;
            BitmapHeight = Height;
            FOV = FieldOfViewInRadians;
            AspectRatio = (float)BitmapWidth / (float)BitmapHeight;

            bmp = new Bitmap(BitmapWidth, BitmapHeight);
        }

        public void ChangeResolution(int newWidth, int newHeight){
            BitmapWidth = newWidth;
            BitmapHeight = newHeight;
            AspectRatio = BitmapWidth / BitmapHeight;
        }

        public void ChangeFOV(int newFieldOfView){
            FOV = newFieldOfView;
        }

        public Bitmap ProcessData(Vector3 camPos, Vector3 camRot, objectX objectXData, Vector3 lightPosition, bool reBake = false){
            GC.Collect();
            Bitmap bmp = new Bitmap(BitmapWidth, BitmapHeight);
            objX[] TransformedObjects = new objX[objectXData.objXData.Length];
            List<Face3D> TransformedFaces = new List<Face3D>();
            Stopwatch sww = new Stopwatch();
            sww.Start();

            foreach (objX xData in objectXData.objXData){

                int i = 0;
                foreach (Face3D fData in xData.objectFaces)
                {
                    Vector3[] FinalCoords = new Vector3[fData.FourCoords.Length];
                    int ii = 0;
                    foreach (Vector3 vData in fData.FourCoords){

                        Vector3 newXYZ = vData + xData.Position; //Moves The 3 Dimensional Point to the new Point

                        Vector3 newRot = FindNewPosition(newXYZ, xData.RotationOffset + xData.Position, xData.Rotation); //Calculates The new Vector3 Position After A Rotation

                        Vector3 newOUT = FindNewPosition(newRot, camPos, camRot); //Calculates The Final Vector3 Point For The Camera Rotation

                        FinalCoords[ii] = newOUT;

                        ii++;
                    }

                    TransformedFaces.Add(new Face3D(FinalCoords, fData.col, fData.drawOrder));
                    
                    i++;
                }

            }

            

            TransformedFaces = SortList(TransformedFaces, camPos); //Faces Sorted From Futherst To Closest

            sww.Stop();
            Console.WriteLine(sww.ElapsedMilliseconds.ToString() + "ms to process Faces");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (Face3D fData in TransformedFaces)
            {
                Vector2[] outputPositions = new Vector2[fData.FourCoords.Length];
                int i = 0;
                foreach (Vector3 vData in fData.FourCoords)
                {
                    i++;
                    if (!XYZtoXY(vData, camPos, FOV, out outputPositions[i - 1], AspectRatio)){
                        continue; //TODO: Add A Better Camera Position Checking System
                    }
                }

                DrawFace(outputPositions, fData.col, bmp, false);

            }
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds.ToString() + "ms to render Faces");

           // RayCast(new Vector3(0, 0, 0), new Vector3(25, 25, 25), objectXData);

            return bmp;
        }

        public void BakeLightMaps(){ 
            


        }

        void DrawFace(Vector2[] FaceVectors, Color col, Bitmap TargetBitmap, bool EnableAntiAlias = false)
        {
            SolidBrush blueBrush = new SolidBrush(col);

            PointF[] curvePoints = new PointF[FaceVectors.Length];

            int i = 0;
            foreach(Vector2 V3 in FaceVectors)
            {
                curvePoints[i] = new PointF(V3.x, V3.y);
                
                i++;
            }

            using (var graphics = Graphics.FromImage(TargetBitmap))
            {
                if (EnableAntiAlias){
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                }

                graphics.FillPolygon(blueBrush, curvePoints);
            }
        }

        void DrawLine(Bitmap tBmp, int posX, int posY, int posX2, int posY2)
        {
            Pen blackPen = new Pen(Color.Red, 1);

            using (var graphics = Graphics.FromImage(tBmp))
            {
                graphics.DrawLine(blackPen, posX, posY, posX2, posY2);
            }

        }


        List<Face3D> SortList(List<Face3D> InputList, Vector3 camPos) //Sort The Face List To Detect Which Face Will Be Rendered First
        {
            foreach (Face3D fData in InputList)
            {
                float FurthZ = 0;
                foreach (Vector3 vt in fData.FourCoords){

                    if (Vector3.Distance(vt, camPos) > FurthZ){
                        FurthZ = Vector3.Distance(vt, camPos); //Z-Buffer Calculation Incorrect, No idea how to fix
                    }

                   // if (vt.z - camPos.z > FurthZ)
                     //   FurthZ = vt.z - camPos.z;
                }
                fData.drawOrder = FurthZ;
            }

            InputList.Sort(delegate(Face3D x, Face3D y) { return y.drawOrder.CompareTo(x.drawOrder); });

            // No Idea how I wrote this code but it works.
            return InputList; // Rename variables and add documentary
        }

        public Vector3 RayCast(Vector3 From, Vector3 TargetPoint, objectX allObjects)
        {
            float slopeXY = (From.x - TargetPoint.x) / (From.y - TargetPoint.y);
            float slopeYX = (From.y - TargetPoint.y) / (From.x - TargetPoint.x);

            float slopeXZ = (From.x - TargetPoint.x) / (From.z - TargetPoint.z);
            float slopeZX = (From.z - TargetPoint.z) / (From.x - TargetPoint.x);

            float slopeYZ = (From.y - TargetPoint.y) / (From.z - TargetPoint.z);
            float slopeZY = (From.z - TargetPoint.z) / (From.y - TargetPoint.y);

            foreach (objX xData in allObjects.objXData)
            {
                foreach (Face3D fData in xData.objectFaces)
                {
                    foreach (Vector3 vData in fData.FourCoords)
                    {
                        float posX = slopeXY * vData.y;
                        float posY = slopeYX * vData.x;

                        Debug.WriteLine("Raycast posX: " + posX.ToString() + "posY: " + posY.ToString());

                        Vector2 outputA = new Vector2(0, 0);
                        Vector2 outputB = new Vector2(0, 0);

                        XYZtoXY(From, new Vector3(25, 25, -115), 1, out outputA, AspectRatio);
                        XYZtoXY(TargetPoint, new Vector3(25, 25, -115), 1, out outputA, AspectRatio);



                        DrawLine(bmp, (int)outputA.x, (int)outputA.y, (int)outputB.x, (int)outputB.y);

                    
                    }
                    

                }
            
            
            }

            return new Vector3(0, 0, 0);
        }

        bool XYZtoXY(Vector3 XYZ, Vector3 camPos, float CameraFOV, out Vector2 XY, float resolutionOffset)
        {
            float PreCalculatedZ = XYZ.z - camPos.z; // Calculate The Z Offset Once Instead Of Multiple Times To Increase Performance. 
            if (PreCalculatedZ <= 0){ //Checks For Whether The 3 Dimensional Point Is Infront Of The Camera This Saves About 60 CPU clock cycles
                XY = new Vector2(0, 0); 
                return false; //Returns that the object is not inside the viewport
            }
            else{ //Calculate The XYZ position to XY Position
                float posX = ((BitmapWidth / 2) + (getPitchAngle(camPos, XYZ, PreCalculatedZ) * ((float)BitmapWidth / (float)CameraFOV)));
                float posY = BitmapHeight - ((BitmapHeight / 2) + (getYawAngle(camPos, XYZ, PreCalculatedZ) * ((float)BitmapHeight / (float)CameraFOV * resolutionOffset)));



                XY = new Vector2(posX, posY); //Returns the new XY Position
                return true;
            }
        }

        float getYawAngle(Vector3 cam, Vector3 coord, float CalculatedZ)
        {
            return (float)(coord.y - cam.y) / CalculatedZ;
        }

        float getPitchAngle(Vector3 cam, Vector3 coord, float CalculatedZ)
        {
            return (float)(coord.x - cam.x) / CalculatedZ; 
        }


        Vector3 ZXRotate(Vector3 Input, Vector3 offset, float ZInDEG)
        {
            Vector3 tPos = new Vector3(Input - offset);
            double lsX = (tPos.x) * Math.Cos(ZInDEG) - (tPos.z) * Math.Sin(ZInDEG);
            double lsZ = (tPos.z) * Math.Cos(ZInDEG) + (tPos.x) * Math.Sin(ZInDEG);

            return (new Vector3((float)lsX, tPos.y, (float)lsX) + offset);
        }

        Vector3 YZRotate(Vector3 Input, Vector3 offset, float YInDEG)
        {
            Vector3 tPos = new Vector3(Input - offset);
            double ndZ = (tPos.z) * Math.Cos(YInDEG) - (tPos.y) * Math.Sin(YInDEG);
            double ndY = (tPos.y) * Math.Cos(YInDEG) + (tPos.z) * Math.Sin(YInDEG);

            return (new Vector3(tPos.x, (float)ndY, (float)ndZ) + offset);
        }

        Vector3 XYRotate(Vector3 Input, Vector3 offset, float XInDEG)
        {
            Vector3 tPos = new Vector3(Input - offset);
            double neX = (tPos.x) * Math.Cos(XInDEG) - (tPos.y) * Math.Sin(XInDEG);
            double neY = (tPos.y) * Math.Cos(XInDEG) + (tPos.x) * Math.Sin(XInDEG);
            return (new Vector3((float)neX, (float)neY, tPos.z) + offset);
        }


        Vector3 FindNewPosition(Vector3 oldPos, Vector3 offset, Vector3 angleinDEG)  //2018-05-12: Vector3 Offset Gets modified for no reason, TODO: make xz first, then yz, then yx
        {
            //Converts Everything into Degrees
            Vector3 rotValue = new Vector3(angleinDEG.x / 57.2958f, angleinDEG.y / 57.2958f, angleinDEG.z / 57.2958f);

            //Rotate on ZX Plane
            double fiX = (oldPos.x - offset.x) * Math.Cos(rotValue.z) - (oldPos.z - offset.z) * Math.Sin(rotValue.z);
            double fiZ = (oldPos.z - offset.z) * Math.Cos(rotValue.z) + (oldPos.x - offset.x) * Math.Sin(rotValue.z);
            
            //Returns the image back to the original position
            fiX = fiX + offset.x;
            fiZ = fiZ + offset.z;

            //Rotate on ZY Plane
            double ndZ = (fiZ - offset.z) * Math.Cos(rotValue.y) - (oldPos.y - offset.y) * Math.Sin(rotValue.y);
            double ndY = (oldPos.y - offset.y) * Math.Cos(rotValue.y) + (fiZ - offset.z) * Math.Sin(rotValue.y);

            //Returns the image back to the original position
            ndZ = ndZ + offset.z;
            ndY = ndY + offset.y;

            //Rotate on XY Plane
            double neX = (fiX - offset.x) * Math.Cos(rotValue.x) - (ndY - offset.y) * Math.Sin(rotValue.x);
            double neY = (ndY - offset.y) * Math.Cos(rotValue.x) + (fiX - offset.x) * Math.Sin(rotValue.x);

            //Returns the image back to the original position
            neX = neX + offset.x;
            neY = neY + offset.y;

            //Returns the newly rotated Vector
            return new Vector3((float)neX, (float)neY, (float)ndZ);
        }
    }

    public class Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float posX, float posY, float posZ){
            x = posX;
            y = posY;
            z = posZ;
        }

        public Vector3(Vector3 oldVector3){
            x = oldVector3.x;
            y = oldVector3.y;
            z = oldVector3.z;
        }

        public static float Distance(Vector3 From, Vector3 To){
            return (float)Math.Sqrt(Math.Pow(From.x - To.x, 2) + Math.Pow(From.y - To.y, 2) + Math.Pow(From.z - To.z, 2));
        }

        public static Vector3 operator +(Vector3 A, Vector3 B)
        {
            return new Vector3(A.x + B.x, A.y + B.y, A.z + B.z);
        }

        public static Vector3 operator -(Vector3 A, Vector3 B)
        {
            return new Vector3(A.x - B.x, A.y - B.y, A.z - B.z);
        }

        public override string ToString()
        {
            return "Vector3 X: " + x.ToString() + ", Y: " + y.ToString() + ", Z:" + z.ToString(); 
        }
    }

    public class Vector2
    {
        public float x;
        public float y;

        public Vector2(float posX, float posY){
            x = posX;
            y = posY;
        }

        public Vector2(Vector2 oldVector2){
            x = oldVector2.x;
            y = oldVector2.y;
        }

        public static float Distance(Vector2 From, Vector2 To){
            return (float)Math.Sqrt(Math.Pow(From.x - To.x,2) + Math.Pow(From.y - To.y,2));
        }
    
    
    
    }

    public class objectX
    {
        public objX[] objXData;


    }

    public class objX
    {
        public string Name;
        
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 RotationOffset;

        public Face3D[] objectFaces;
    }

    public class LightData
    {
        Light[] Lights;
    
    }

    public class Light
    {
        public Vector3 Position;
        public float Range;




    }

    public class Face3D
    {
        public float drawOrder;
        public Vector3[] FourCoords;
        public Color col;

        public Face3D(Vector3[] FCoords, Color FaceColor, float ID){
            drawOrder = ID;
            col = FaceColor;
            FourCoords = FCoords;
        }

        public Face3D(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color FaceColor, float ID){
            drawOrder = ID;
            col = FaceColor;
            FourCoords = new Vector3[3];
            FourCoords[0] = A;
            FourCoords[1] = B;
            FourCoords[2] = C;
            FourCoords[3] = D;
        }

        public Face3D(Face3D CopyFrom)
        {
            drawOrder = CopyFrom.drawOrder;
            col = CopyFrom.col;
            FourCoords = new Vector3[4];
            FourCoords[0] = CopyFrom.FourCoords[0];
            FourCoords[1] = CopyFrom.FourCoords[1];
            FourCoords[2] = CopyFrom.FourCoords[2];
            FourCoords[3] = CopyFrom.FourCoords[3];
        
        }
    
    }

    public class objectXImport
    {
        string[] FileInput;
        //objectX[] objects = new objectX[64];


        string[] existingNames = new string[64];
        int nameID = 0;

        public bool FileContainsErrors = false;

        public void Import(string FileName)
        {
            FileInput = File.ReadAllLines(FileName);
        }

        public objectX Analyse()
        {
            bool lFace = false;
            string TargetFace = "";
            int FacesAdded = 0;

            List<objX> objects = new List<objX>();

            objX workObject = new objX();

            List<Face3D> readyToAdd = new List<Face3D>();



            int i = 1;

            int xID = 0;
            foreach (string s in FileInput)
            {
                string input = s;
                if (s.Contains("//"))
                {
                    input = s.Substring(0, s.IndexOf("//"));
                }

                if (input.StartsWith("object:"))
                {
                    if (lFace)
                    {
                        DrawInRed("Error on Line " + i.ToString() + ": Expected objectX Face. Got new objectX");

                    }

                    string name = input.Substring(input.IndexOf('"')).Substring(0, input.IndexOf('"')).Replace(@"""", "");

                    string LastVectors = input.Substring(input.IndexOf(":("));

                    LastVectors = LastVectors.Replace(":", "");

                    // MessageBox.Show(LastVectors);

                    Vector3[] rV = RetriveVectors(LastVectors);

                    if (!NameExists(name))
                    {
                        AddName(name);
                        workObject = new objX();
                        workObject.Name = name;
                        workObject.Position = rV[0];
                        workObject.Rotation = rV[1];
                        workObject.RotationOffset = rV[2];

                    }
                    else
                    {
                        DrawInRed("Error on Line " + i.ToString() + @": objectX """ + name + @""" exists more than once");
                    }
                    FacesAdded = 0;
                    lFace = true;
                }
                else if (input.StartsWith(":"))
                {
                    if (!lFace)
                    {
                        DrawInRed("Error on Line " + i.ToString() + @": Expected anything but "":"". Got "":""");
                    }
                    else
                    {

                    }

                    if (FacesAdded == 0)
                    {
                        DrawInYellow("Warning on Line " + i.ToString() + @": ObjectX has no faces");
                    }

                    
                    FacesAdded = 0;
                    lFace = false;
                    //  objects.Add(workObject);

                    workObject.objectFaces = readyToAdd.ToArray();
                    objects.Add(workObject);
                    readyToAdd.Clear();
                }
                else if (input.StartsWith("("))
                {
                    if (!lFace)
                    {
                        DrawInRed("Error on Line " + i.ToString() + @": Expected new objectX. Got objectX Face");
                    }
                    else
                    {
                        Color facecolor = new Color();

                        Vector3[] fourCoords = new Vector3[0];

                        int amount = (input.Length - input.Replace(")(", "").Length) / 2;
                        FacesAdded++;

                        if (amount > 3)
                        {
                            DrawInRed("Error on Line " + i.ToString() + @": Maximum of 4 Vector3 or 2 Colors per objectX Face");
                        }

                        if (!input.Contains(".(") | !input.Contains("."))
                        {
                            DrawInRed("Error on Line " + i.ToString() + @": No Color Found For objectX Face");
                        }
                        else
                        {
                            string colSplit = input.Substring(input.IndexOf('.'));
                            if (!colSplit.Contains(";"))
                            {
                                DrawInRed("Error on Line " + i.ToString() + @": Expected "";""");
                            }
                            else
                            {
                                colSplit = colSplit.Replace(";", "");
                            }

                            if (colSplit.Contains('.'))
                            {
                                colSplit = colSplit.Replace(".", "");
                            }

                            if (colSplit.Contains(')'))
                            {
                                colSplit = colSplit.Replace(")", "");
                            }

                            if (colSplit.Contains('('))
                            {
                                colSplit = colSplit.Replace("(", "");
                            }

                            facecolor = ParseColor(colSplit, i);


                        }

                        if (input.Contains("."))
                        {
                            string dataSplit = input.Substring(0, input.IndexOf('.'));
                            int Amount = (dataSplit.Length - dataSplit.Replace(")(", "").Length) / 2;

                            if (Amount < 2 || Amount > 3)
                            {
                                DrawInRed("Error on Line " + i.ToString() + @": Only Three or Four Vector3 Positions are accepted");
                            }
                            else
                            {
                                fourCoords = RetriveVectors(dataSplit);


                            }



                        }
                        else
                        {
                            DrawInRed("Error on Line " + i.ToString() + @": Failed To Extract Face Data");
                        }




                        readyToAdd.Add(new Face3D(fourCoords, facecolor, 0));

                    }
                }




                i++;
            }

            objectX output = new objectX();
            output.objXData = objects.ToArray();

            return output;
        }

        Vector3[] RetriveVectors(string input)
        {
            int Data = (input.Length - input.Replace(")(", "").Length) / 2;


            //  string first = input.Substring(0,input.IndexOf(")(")).Replace(")","").Replace("(","");
            Vector3[] data;

            string a;
            string b;
            string c;
            string d;

            if (Data == 2)
            {
                data = new Vector3[3];
            }
            else
            {
                data = new Vector3[4];
                d = input.Split(new string[] { ")(" }, StringSplitOptions.None)[3].Replace(")", "");
                data[3] = Vectorize(d);
            }

            a = input.Split(new string[] { ")(" }, StringSplitOptions.None)[0].Replace("(", "");
            b = input.Split(new string[] { ")(" }, StringSplitOptions.None)[1];
            c = input.Split(new string[] { ")(" }, StringSplitOptions.None)[2].Replace(")", "");

            //     MessageBox.Show(input);



            //    MessageBox.Show(a);
            //    MessageBox.Show(b);
            //   MessageBox.Show(c);

            data[0] = Vectorize(a);
            data[1] = Vectorize(b);
            data[2] = Vectorize(c);


            return data;
        }

        Vector3 Vectorize(string first)
        {
            string x = first.Split(',')[0];
            string y = first.Split(',')[1];
            string z = first.Split(',')[2];

            float VectorX = float.Parse(x);
            float VectorY = float.Parse(y);
            float VectorZ = float.Parse(z);

            return new Vector3(VectorX, VectorY, VectorZ);
        }

        Color ParseColor(string ColorSplit, int i)
        {
            int dota = (ColorSplit.Length - ColorSplit.Replace(",", "").Length);

            Color col = Color.FromArgb(255, 255, 255, 255);
            if (dota > 3)
            {
                DrawInRed("Error on Line " + i.ToString() + @": Color take no more than 4 numbers");
            }
            else
            {
                string a = ColorSplit.Split(',')[0];
                string r = ColorSplit.Split(',')[1];
                string g = ColorSplit.Split(',')[2];
                string b = ColorSplit.Split(',')[3];

                int A;
                int R;
                int G;
                int B;

                int Alpha;
                int Red;
                int Green;
                int Blue;

                if (!int.TryParse(a, out A))
                {
                    DrawInRed("Error on Line " + i.ToString() + @": Failed to Parse Alpha Color");
                }

                if (!int.TryParse(r, out R))
                {
                    DrawInRed("Error on Line " + i.ToString() + @": Failed to Parse Red Color");
                }

                if (!int.TryParse(g, out G))
                {
                    DrawInRed("Error on Line " + i.ToString() + @": Failed to Parse Green Color");
                }

                if (!int.TryParse(b, out B))
                {
                    DrawInRed("Error on Line " + i.ToString() + @": Failed to Parse Blue Color");
                }

                if (!Clamp(A, 255, 0, out Alpha))
                {
                    DrawInRed("Error on Line " + i.ToString() + @": Alpha Color is out of Range (0-255)");
                }
                if (!Clamp(R, 255, 0, out Red))
                {
                    DrawInRed("Error on Line " + i.ToString() + @": Red Color is out of Range (0-255)");
                }
                if (!Clamp(G, 255, 0, out Green))
                {
                    DrawInRed("Error on Line " + i.ToString() + @": Green Color is out of Range (0-255)");
                }
                if (!Clamp(B, 255, 0, out Blue))
                {
                    DrawInRed("Error on Line " + i.ToString() + @": Blue Color is out of Range (0-255)");
                }

                col = Color.FromArgb(Alpha, Red, Green, Blue);

            }
            return col;
        }

        bool Clamp(int value, int max, int min, out int output)
        {
            if (value > max)
            {
                output = max;
                return false;
            }
            else if (value < min)
            {
                output = min;
                return false;
            }
            else
            {
                output = value;
                return true;
            }

        }

        void AddFaceToObjectX(Vector3[] face)
        {


        }


        Vector3 StringToVector3(string input)
        {
            return null;
        }

        bool NameExists(string Name)
        {
            if (existingNames.Length == 0)
            {
                return false;
            }
            bool NameFound = false;
            foreach (string str in existingNames)
            {
                if (str != null && str.Contains(Name))
                {
                    NameFound = true;
                    break;
                }
            }
            return NameFound;
        }

        void AddName(string Name)
        {
            existingNames[nameID] = Name;
            nameID++;
        }

        void DrawInRed(string Text)
        {
            ConsoleColor col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Text);
            Console.ForegroundColor = col;
            FileContainsErrors = true;
        }

        void DrawInYellow(string Text)
        {
            ConsoleColor col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Text);
            Console.ForegroundColor = col;
        }

    }



}
