using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Checklist
{
    public partial class ImageWindow : Form
    {
        //private VMS.TPS.Common.Model.API.Image image;
        //private int currentSlice;

        public ImageWindow()//string task, VMS.TPS.Common.Model.API.Image image, bool itemChecked
        {
            //this.image = image;
            
            InitializeComponent();

            /*this.checkBox.Text = task;
            this.checkBox.Checked = itemChecked;

            if (image != null)
            {
                VVector userOrigin = image.UserOrigin;
                VVector imageOrigin = image.Origin;

                currentSlice = (int)((userOrigin.z - imageOrigin.z) / image.ZRes);

                pictureBoxImage.MouseWheel += new MouseEventHandler(pictureBoxImage_MouseWheel);
                pictureBoxImage.MouseHover += new EventHandler(pictureBoxImage_MouseHover);

                UpdateImage();
            }*/
        }

        private void pictureBoxImage_MouseHover(object sender, EventArgs e)
        {
            pictureBoxImage.Focus();
        }
        
        private void UpdateImage()
        {
            /*VVector userOrigin = image.UserOrigin;
            VVector imageOrigin = image.Origin;

            double userOriginIndexX = (userOrigin.x - imageOrigin.x) / image.XRes;
            double userOriginIndexY = (userOrigin.y - imageOrigin.y) / image.YRes;
            double userOriginIndexZ = (userOrigin.z - imageOrigin.z) / image.ZRes;

            pictureBoxImage.Image = ConvertToBitmap(image, currentSlice);

            Pen userOriginPen;
            if (currentSlice == (int)userOriginIndexZ)
                userOriginPen = new Pen(Color.Green);
            else
                userOriginPen = new Pen(Color.Yellow);
            userOriginPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            Graphics g = Graphics.FromImage(pictureBoxImage.Image);
            g.DrawLine(userOriginPen, 0, (int)Math.Round(userOriginIndexY), pictureBoxImage.Image.Width, (int)Math.Round(userOriginIndexY));
            g.DrawLine(userOriginPen, (int)Math.Round(userOriginIndexX), 0, (int)Math.Round(userOriginIndexX), pictureBoxImage.Image.Height);    */        
        }

        private void pictureBoxImage_MouseWheel(object sender, MouseEventArgs e)
        {
            /*if (e.Delta < 0)
                currentSlice--;
            else
                currentSlice++;

            if (currentSlice < 0)
                currentSlice = 0;
            else if (currentSlice > image.ZSize - 1)
                currentSlice = image.ZSize - 1;

            UpdateImage();*/
        }

        /*private Bitmap ConvertToBitmap(VMS.TPS.Common.Model.API.Image image, int planeIndex)
        {
            int[,] voxelPlane = new int[image.XSize, image.YSize];
            byte[] byteArray = new byte[image.XSize * image.YSize * 3];

            Bitmap bmp = new Bitmap(image.XSize, image.YSize, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            int minValue = image.Level - image.Window / 2;
            int maxValue = image.Level + image.Window / 2;
            double scale = 255.0 / ((double)(maxValue - minValue));
            byte value;

            image.GetVoxels(planeIndex, voxelPlane);
            int arrayPos = 0;
            for (int y = 0; y < image.YSize; y++)
            {
                for (int x = 0; x < image.XSize; x++)
                {
                    if (voxelPlane[x, y] < minValue)
                        value = 0;
                    else if (voxelPlane[x, y] > maxValue)
                        value = 255;
                    else
                        value = (byte)(((double)voxelPlane[x, y] - (double)minValue) * scale);
                    byteArray[arrayPos++] = value;
                    byteArray[arrayPos++] = value;
                    byteArray[arrayPos++] = value;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, bmpData.Scan0, byteArray.Length);
            bmp.UnlockBits(bmpData);
                        
            return bmp;
        }*/

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.IsHandleCreated)
            {
                this.DialogResult = (checkBox.Checked ? DialogResult.Yes : DialogResult.No);
                this.Close();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
