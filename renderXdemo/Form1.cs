using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace renderXdemo
{
    public partial class Form1 : Form
    {
        int displayWidth;
        int displayHeight;
        bool FullScreen = false;

        renderX renderProcessor;
        objectX allObjects;

        public Form1()
        {
            InitializeComponent();
        }

        Vector3 camPosi = new Vector3(0, 0, 0);
        objectX data;

        private void Form1_Load(object sender, EventArgs e)
        {
            displaySize dSize = new displaySize();
            dSize.ShowDialog();

            displayWidth = displaySize.W;
            displayHeight = displaySize.H;

            if (displaySize.appTerminated)
            {
                Application.Exit();
            }
            else
            {
                DoubleBuffered = true;

                if (displaySize.fScreen)
                {
                    FullScreen = true;

                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    this.Size = new Size(displayWidth, displayHeight);
                    int WindowWidth = displayWidth - ClientSize.Width;
                    int WindowHeight = displayHeight - ClientSize.Height;
                    this.Size = new Size(displayWidth + WindowWidth, displayHeight + WindowHeight);
                }

                data = new objectX();

                objectXImport importer = new objectXImport();
                OpenFileDialog oDialog = new OpenFileDialog();
                if (oDialog.ShowDialog() == DialogResult.OK)
                {
                    importer.Import(oDialog.FileName);
                    data = importer.Analyse();

                }
                else
                {
                    Application.Exit();
                    
                }


                renderProcessor = new renderX(displayWidth, displayHeight, 1f); //1.5708Rads = 90Deg
                

                timer1.Start();
            }
                
        }

        int oldMouseX = 0;
        int oldMouseY = 0;

        Vector3 camPosition = new Vector3(0, 0, -155);
        Vector3 camRotation = new Vector3(0, 0, 0);

        Vector2 Keydelta = new Vector2(0, 0);


        private void timer1_Tick(object sender, EventArgs e)
        {   
            float deltax = oldMouseX - this.PointToClient(Cursor.Position).X;
            float deltay = oldMouseY - this.PointToClient(Cursor.Position).Y;

            oldMouseX = this.PointToClient(Cursor.Position).X;
            oldMouseY = this.PointToClient(Cursor.Position).Y;

            if (rdown | ldown){
                if (rdown){
                    if (Keydelta.x > 0){ 
                        Keydelta.x = 0;}
                    Keydelta.x--;
                }else if (ldown)
                {
                    if (Keydelta.x < 0){
                        Keydelta.x = 0;}
                    Keydelta.x++;
                }
            }else{
                Keydelta.x = 0;
            }

            if (udown | bdown)
            {
                if (udown)
                {
                    if (Keydelta.y > 0){
                        Keydelta.y = 0;}
                    Keydelta.y--;
                }
                else if (bdown)
                {
                    if (Keydelta.y < 0){
                        Keydelta.y = 0;}
                    Keydelta.y++;
                }
            }
            else
            {
                Keydelta.y = 0;
            }

            //TODO: FINISH


            if (mmbdown){
                
                camPosition = Pan3D(camPosition, camRotation, -deltax / 2, -deltay / 2);

                this.BackgroundImage = renderProcessor.ProcessData(camPosition, camRotation, data, new Vector3(0, 0, 0));
            }

            if (mousedown){
                camPosi = camPosi + new Vector3(-deltax / 2, -deltay / 2, 0);

                data.objXData[0].Rotation = camPosi;

                this.BackgroundImage = renderProcessor.ProcessData(camPosition, camRotation, data, new Vector3(0, 0, 0));
                
            }
            Stopwatch sw = new Stopwatch();

            if (righdown){
                
                
                camRotation = camRotation + new Vector3(0, -deltay / 8, -deltax / 8);
                camPosition = Pan3D(camPosition, camRotation, Keydelta.x / 8f, 0, Keydelta.y / 8f);
                sw.Start();

                this.BackgroundImage = renderProcessor.ProcessData(camPosition, camRotation, data, new Vector3(0, 0, 0));

                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds.ToString() + "ms to display image");

            }

            this.Text = "camPos: " + camPosition.ToString() + ", camRot: " + camRotation.ToString();
         //   this.Text = "camPos: X:" + Keydelta.x.ToString() + "Y: 0"; 
         //   this.Text = "rdown: " + rdown.ToString() + ", ldown: " + ldown.ToString() + ", udown: " + udown.ToString() + ", bdown: " + bdown.ToString();

        }

        Vector3 Pan3D(Vector3 Input, Vector3 Rotation, float deltaX, float deltaY, float deltaZ = 0)
        {
            Vector3 I = new Vector3(Input);
            Vector3 RADS = new Vector3(0f, Rotation.y / 57.2958f, Rotation.z / 57.2958f);

            float sinX = (float)Math.Sin(RADS.z); //0
            float sinY = (float)Math.Sin(RADS.y); //0
            

            float cosX = (float)Math.Cos(RADS.z); //0
            float cosY = (float)Math.Cos(RADS.y); //0

            
            


            float XAccel = (cosX * -deltaX + (sinY * deltaY) * sinX) + (sinX * -deltaZ) * cosY;
            float YAccel = (cosY * deltaY) + (sinY * deltaZ);
            float ZAccel = (sinX * deltaX + (sinY * deltaY) * cosX) + (cosX * -deltaZ) * cosY;

            I = I + new Vector3(XAccel, YAccel, ZAccel);

            return I;
        }


        bool mousedown = false;
        bool mmbdown = false;
        bool righdown = false;

        bool rdown = false;
        bool ldown = false;
        bool udown = false;
        bool bdown = false;

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            mousedown = false;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                mousedown = true;
            }

            mmbdown = false;
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                mmbdown = true;
            }

            righdown = false;
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                righdown = true;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                rdown = true;
            }

            if (e.KeyCode == Keys.A)
            {
                ldown = true;
            }

            if (e.KeyCode == Keys.W)
            {
                udown = true;
            }

            if (e.KeyCode == Keys.S)
            {
                bdown = true;
            }

            
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                rdown = false;
            }

            if (e.KeyCode == Keys.A)
            {
                ldown = false;
            }

            if (e.KeyCode == Keys.W)
            {
                udown = false;
            }

            if (e.KeyCode == Keys.S)
            {
                bdown = false;
            }
        }

        

    }
}
