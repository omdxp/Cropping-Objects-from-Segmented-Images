using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace _28_Cropping_Objects_from_Segmented_Images
{
    public partial class Form1 : Form
    {
        Image<Bgr, byte> _imgInput;
        Image<Gray, byte> _cc;
        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _imgInput = new Image<Bgr, byte>(ofd.FileName);
                pictureBox1.Image = _imgInput.Bitmap;
            }
        }

        private void processToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_imgInput == null)
                    return;

                var tmp = _imgInput.Convert<Gray, byte>()
                    .ThresholdBinary(new Gray(100), new Gray(255))
                    .Dilate(1).Erode(1);

                Mat labels = new Mat();
                int nLabels = CvInvoke.ConnectedComponents(tmp, labels);
                _cc = labels.ToImage<Gray, byte>();

                pictureBox2.Image = tmp.Bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (_cc == null)
                    return;

                int label = (int) _cc[e.Y, e.X].Intensity;
                if (label != 0) // background not included
                {
                    var tmp = _cc.InRange(new Gray(label), new Gray(label));
                    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                    Mat hier = new Mat();

                    CvInvoke.FindContours(tmp, contours,
                        hier,
                        Emgu.CV.CvEnum.RetrType.External,
                        Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                    if (contours.Size > 0)
                    {
                        Rectangle bbox = CvInvoke.BoundingRectangle(contours[0]);

                        _imgInput.ROI = bbox;
                        var img = _imgInput.Copy();

                        _imgInput.ROI = Rectangle.Empty; // There's no Region Of Interest for image now

                        pictureBox2.Image = img.Bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
