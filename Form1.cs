using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Lab3_AOCI
{
    public partial class Form1 : Form
    {
        
        Photoshop photoshop;
        PointF[] pts = new PointF[4];
        int c = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            var result = dlg.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = dlg.FileName;
                Image<Bgr, byte>  srcImg = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);

                imageBox1.Image = srcImg;
                photoshop = new Photoshop(srcImg);
            }
            pts = new PointF[4];
        }

        //Масштабирование
        private void button2_Click(object sender, EventArgs e)
        {
            if (photoshop is null) return;
            try
            {
                double kx = double.Parse(textBox1.Text);
                double ky = double.Parse(textBox2.Text);
                imageBox2.Image = photoshop.GetScaledImage(kx, ky);
            }
            catch(FormatException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void imageBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var imgCopy = photoshop.GetSourceImage().Copy();

            int x = (int)(e.Location.X / imageBox1.ZoomScale);
            int y = (int)(e.Location.Y / imageBox1.ZoomScale);

            pts[c] = new Point(x, y);
            c++;
            if (c >= 4)
                c = 0;

            //Point center = new Point(x, y);
            int radius = 2;
            int thickness = 2;
            var color = new Bgr(Color.Red).MCvScalar;

            for (int i = 0; i < 4; i++)
                CvInvoke.Circle(imgCopy, new Point((int)pts[i].X, (int)pts[i].Y), radius, color, thickness); // функция, рисующая на изображении круг с заданными параметрами

            imageBox1.Image = imgCopy;

        }

        //Сдвиг
        private void button3_Click(object sender, EventArgs e)
        {
            if (photoshop is null) return;
            try
            {
                double shiftX = double.Parse(textBox3.Text);
                double shiftY = double.Parse(textBox4.Text);
                imageBox2.Image = photoshop.GetShearedImage(shiftX, shiftY);
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Поворот
        private void button4_Click(object sender, EventArgs e)
        {
            if (photoshop is null) return;
            imageBox2.Image = photoshop.GetRotatedImage(pts[0].X, pts[0].Y, int.Parse(textBox5.Text));
        }

        //Отражение
        private void button5_Click(object sender, EventArgs e)
        {
            if (photoshop is null) return;
            imageBox2.Image = photoshop.GetReflectionImage(checkBox1.Checked, checkBox2.Checked);
        }

        //Проекция
        private void button6_Click(object sender, EventArgs e)
        {
            if (photoshop is null) return;
            imageBox2.Image = photoshop.GetGomoImage(pts);
        }
    }
}
