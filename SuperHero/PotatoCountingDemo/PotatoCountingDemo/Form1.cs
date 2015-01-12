using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace PotatoCountingDemo
{
    public partial class Form1 : Form
    {
        private PlantCounter plantCounter;

        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;",
                InitialDirectory = @"C:\Users\Gary\Pictures\Potatoes"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Stream stream = ofd.OpenFile())
                    {
                        this.pictureBox1.Image = Image.FromStream(stream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error loading Image: " + ex.Message,
                        "Error!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void clusterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.classifyToolStripMenuItem_Click(null, null);

            var pc = new PlantCounter(
                    (Bitmap)this.pictureBox1.Image,
                    162,
                    3);

            this.pictureBox2.Image = (Bitmap)
                pc.ShowClusters();

            MessageBox.Show(
                String.Format("This image contains {0} plants.",
                    pc.GetClusterCount().ToString()),
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void classifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.pictureBox2.Image = (Bitmap)
                new PlantCounter(
                    (Bitmap)this.pictureBox1.Image,
                    1).Classify();
        }

        private void countPlantsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.clusterToolStripMenuItem_Click(null, null);
        }

        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;",
                InitialDirectory = @"C:\Users\Gary\Pictures\Blackgrass"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Stream stream = ofd.OpenFile())
                    {
                        this.pictureBox1.Image = Image.FromStream(stream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error loading Image: " + ex.Message,
                        "Error!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void loadCropSampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;",
                InitialDirectory = @"C:\Users\Gary\Pictures\Blackgrass"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Stream stream = ofd.OpenFile())
                    {
                        this.pictureBox3.Image = Image.FromStream(stream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error loading Image: " + ex.Message,
                        "Error!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void loadBlackGrassSampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;",
                InitialDirectory = @"C:\Users\Gary\Pictures\Blackgrass"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Stream stream = ofd.OpenFile())
                    {
                        this.pictureBox4.Image = Image.FromStream(stream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error loading Image: " + ex.Message,
                        "Error!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void detectBlackGrassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.pictureBox2.Image = new BlackGrassDetector(
                this.pictureBox1.Image as Bitmap, 
                this.pictureBox3.Image as Bitmap, 
                this.pictureBox4.Image as Bitmap).DetectBlackGrass();
        }
    }
}
