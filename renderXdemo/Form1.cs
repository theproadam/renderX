using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
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

            
                renderProcessor = new renderX(displayWidth, displayHeight, 1); 
                timer1.Start();
            }
                
        }

        int oldMouseX = 0;
        int oldMouseY = 0;

        Vector3 camPosition = new Vector3(0, 0, -155);
        Vector3 camRotation = new Vector3(0, 0, 0);

        private void timer1_Tick(object sender, EventArgs e)
        {
            float deltax = oldMouseX - this.PointToClient(Cursor.Position).X;
            float deltay = oldMouseY - this.PointToClient(Cursor.Position).Y;

         //   this.Text = "deltaX: " + deltax + ", deltaY: " + deltay;

            oldMouseX = this.PointToClient(Cursor.Position).X;
            oldMouseY = this.PointToClient(Cursor.Position).Y;




            if (mmbdown){
               // camPosition = camPosition + new Vector3(deltax / 2, -deltay / 2, 0);

                camPosition = Pan3D(camPosition, camRotation, -deltax / 2, -deltay / 2);

                

                this.BackgroundImage = renderProcessor.ProcessData(camPosition, camRotation, data, new Vector3(0, 0, 0));
            }

            if (mousedown){
                camPosi = camPosi + new Vector3(-deltax / 2, -deltay / 2, 0);

                data.objXData[0].Rotation = camPosi;
                this.BackgroundImage = renderProcessor.ProcessData(camPosition, camRotation, data, new Vector3(0, 0, 0));
            }

            if (righdown){
                camRotation = camRotation + new Vector3(0, -deltay / 8, -deltax / 8);
                this.BackgroundImage = renderProcessor.ProcessData(camPosition, camRotation, data, new Vector3(0, 0, 0));

            }

            this.Text = "camPos: " + camPosition.ToString() + ", camRot: " + camRotation.ToString();

         //   Debug.WriteLine("CamPos: " + camPosition.ToString());
          //  Debug.WriteLine("CamROT: " + camRotation.ToString());
            
        }

        Vector3 Pan3D(Vector3 Input, Vector3 Rotation, float deltaX, float deltaY)
        {
            Vector3 I = new Vector3(Input);
            Vector3 RADS = new Vector3(0f, Rotation.y / 57.2958f, Rotation.z / 57.2958f);

            double sinX = Math.Sin(RADS.z); //0
            double sinY = Math.Sin(RADS.y); //0

            double cosX = Math.Cos(RADS.z); //0
            double cosY = Math.Cos(RADS.y); //0


            float XAccel = (float)cosX * -deltaX;
            float YAccel = (float)cosY * deltaY;
            float ZAccel = (float)sinX * deltaX + ((float)sinY * deltaY);

            I = I + new Vector3(XAccel, YAccel, ZAccel);

            return I;
        }


        bool mousedown = false;
        bool mmbdown = false;
        bool righdown = false;

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


    }
}
