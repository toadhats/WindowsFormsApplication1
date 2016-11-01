using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Image original;
        public Form1()
        {
            InitializeComponent();
            Image original = new Bitmap(pictureBox1.Width, pictureBox1.Height);
        }

        public void LoadImage()
        {

        }
        // load image button
        private void button1_Click(Object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; // Setting this here for now
                pictureBox1.Load(openFileDialog1.FileName);
                original = pictureBox1.Image;
            }
        }

        // reset image or fill image box with a colour. right click doesn't actually work lol
        private void button2_Click(Object sender, EventArgs e)
        {
            if (e is MouseEventArgs && ((MouseEventArgs) e).Button == MouseButtons.Right)
            {
                var img = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                int x, y;
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.BackColor = colorDialog1.Color;
                    for (x = 0; x < img.Width - 1; ++x)
                    {
                        for (y = 0; y < img.Height - 1; ++y)
                        {
                            img.SetPixel(x, y, colorDialog1.Color);
                        }
                    }
                }
                pictureBox1.Image = img;
            }
            else
            {
                pictureBox1.Image = original;
            }
        }
        /// <summary>
        /// THis button does some dumb shit with random numbers and division/fake modulo operators. 
        /// The randomiser on this one gets recreated for every pixel, which seems to make a striped pattern? 
        /// "On most Windows systems, Random objects created within 15 milliseconds of one another are likely 
        /// to have identical seed values." (MSDN)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(Object sender, EventArgs e)
        {
            var img = pictureBox1.Image;
            var bmp = new Bitmap(img);
            int x, y;
            Cursor.Current = Cursors.WaitCursor;
            for (x = 0; x < bmp.Width - 1; ++x)
            {
                for (y = 0; y < bmp.Height - 1; ++y)
                {
                    var random = new Random();
                    Color pixelColor = bmp.GetPixel(x, y);
                    var baseMod = (random.NextDouble() > 0.5 ? 1 : -1);
                    var rMod = random.NextDouble() + baseMod;
                    var gMod = random.NextDouble() + baseMod;
                    var bMod = random.NextDouble() + baseMod;
                    Color newColor = Color.FromArgb(
                        Convert.ToInt32(Math.Abs(pixelColor.R / rMod % 255.0)),
                        Convert.ToInt32(Math.Abs(pixelColor.G / gMod % 255.0)),
                        Convert.ToInt32(Math.Abs(pixelColor.B / bMod % 255.0)));
                    bmp.SetPixel(x, y, newColor);
                }
            }
            pictureBox1.Image = bmp;
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Same as button 3 only this one doesn't keep recreating the rng. Pretty much just overlays rainbow snow. Boring
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(Object sender, EventArgs e)
        {
            var img = pictureBox1.Image;
            var bmp = new Bitmap(img);
            int x, y;
            var random = new Random();
            Cursor.Current = Cursors.WaitCursor;
            for (x = 0; x < bmp.Width - 1; ++x)
            {
                for (y = 0; y < bmp.Height - 1; ++y)
                {

                    Color pixelColor = bmp.GetPixel(x, y);
                    var baseMod = (random.NextDouble() > 0.5 ? 1 : -1);
                    var rMod = random.NextDouble() + baseMod;
                    var gMod = random.NextDouble() + baseMod;
                    var bMod = random.NextDouble() + baseMod;
                    Color newColor = Color.FromArgb(
                        Convert.ToInt32(Math.Abs(pixelColor.R / rMod % 255.0)),// left associative
                        Convert.ToInt32(Math.Abs(pixelColor.G / gMod % 255.0)),
                        Convert.ToInt32(Math.Abs(pixelColor.B / bMod % 255.0)));
                    bmp.SetPixel(x, y, newColor);

                }
            }
            pictureBox1.Image = bmp;
            Cursor.Current = Cursors.Default;
        }

        // Averages out the colour of each pixel based on 4 neighbours - ie a shitty blur
        private void button5_Click(Object sender, EventArgs e)
        {
            var img = pictureBox1.Image;
            var bmp = new Bitmap(img);
            int x, y;
            var random = new Random();
            Cursor.Current = Cursors.WaitCursor;
            // bmp.SetPixel(0, 0, Color.Cyan);  // find out how the pixels are indexed lol
            for (x = 0; x < bmp.Width - 1; ++x)
            {
                for (y = 0; y < bmp.Height - 1; ++y)
                {
                    var neighbours = new NeighbourSet(ref bmp, x, y);

                    var newRed = Convert.ToInt32(((neighbours.up.Color.HasValue ? neighbours.up.Color.Value.R : 0) +
                        (neighbours.right.Color.HasValue ? neighbours.right.Color.Value.R : 0) +
                        (neighbours.down.exists ? neighbours.down.Color.Value.R : 0) +
                        (neighbours.left.exists ? neighbours.left.Color.Value.R : 0)) / neighbours.Count);

                    var newGreen = Convert.ToInt32(((neighbours.up.Color.HasValue ? neighbours.up.Color.Value.G : 0) +
                        (neighbours.right.Color.HasValue ? neighbours.right.Color.Value.G : 0) +
                        (neighbours.down.Color.HasValue ? neighbours.down.Color.Value.G : 0) +
                        (neighbours.left.Color.HasValue ? neighbours.left.Color.Value.G : 0)) / neighbours.Count);

                    var newBlue = Convert.ToInt32(((neighbours.up.Color.HasValue ? neighbours.up.Color.Value.B : 0) +
                        (neighbours.right.Color.HasValue ? neighbours.right.Color.Value.B : 0) +
                        (neighbours.down.Color.HasValue ? neighbours.down.Color.Value.B : 0) +
                        (neighbours.left.Color.HasValue ? neighbours.left.Color.Value.B : 0)) / neighbours.Count);

                    var newColour = Color.FromArgb(newRed, newGreen, newBlue);
                    bmp.SetPixel(x, y, newColour);

                }
            }
            pictureBox1.Image = bmp;
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Swap all instances of one random colour with another random colour
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(Object sender, EventArgs e)
        {
            var args = (MouseEventArgs) e;
            var rng = new Random();

            var img = pictureBox1.Image;
            var bmp = new Bitmap(img);
            var colors = GetColorsInImage(bmp);

            var color1 = colors.ElementAt(rng.Next(0, colors.Count - 1)).Key;
            var color2 = colors.ElementAt(rng.Next(0, colors.Count - 1)).Key;

            pictureBox1.Image = SwapTwoColors(ref bmp, color1, color2);

        }
        //button69
        private void button7_Click(Object sender, EventArgs e)
        {
            var rng = new Random();

            var img = pictureBox1.Image;
            var bmp = new Bitmap(img);
            var colors = GetColorsInImage(bmp);
            Cursor.Current = Cursors.WaitCursor;
            for (var i = 0; i < 70; ++i)
            {
                var color1 = colors.ElementAt(rng.Next(0, colors.Count - 1)).Key;
                var color2 = colors.ElementAt(rng.Next(0, colors.Count - 1)).Key;

                pictureBox1.Image = SwapTwoColors(ref bmp, color1, color2);
            }
            Cursor.Current = Cursors.Default;
        }
        //button420
        private void button8_Click(Object sender, EventArgs e)
        {
            var rng = new Random();

            var img = pictureBox1.Image;
            var bmp = new Bitmap(img);
            var colors = GetColorsInImage(bmp);
            Cursor.Current = Cursors.WaitCursor;
            for (var i = 0; i < 420; ++i)
            {
                var color1 = colors.ElementAt(rng.Next(0, colors.Count - 1)).Key;
                var color2 = colors.ElementAt(rng.Next(0, colors.Count - 1)).Key;

                pictureBox1.Image = SwapTwoColors(ref bmp, color1, color2);
            }
            Cursor.Current = Cursors.Default;
        }

        // 革命
        private void button9_Click(Object sender, EventArgs e)
        {
            var img = pictureBox1.Image;
            var bmp = new Bitmap(img);
            var colors = GetColorsInImage(bmp);
            Cursor.Current = Cursors.WaitCursor;
            var most = colors.OrderByDescending(x => x.Value).Take(Math.Min(colors.Count / 100, 100));
            var least = colors.OrderBy(x => x.Value).Take(Math.Min(colors.Count / 100, 100));
            using (var e1 = most.GetEnumerator())
            using (var e2 = least.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    SwapTwoColors(ref bmp, e1.Current.Key, e2.Current.Key);
                }
            }
                pictureBox1.Image = bmp;
            Cursor.Current = Cursors.Default;
        }

        private ref Bitmap SwapTwoColors(ref Bitmap bmp, Color color1, Color color2)
        {
            int x, y;
            for (x = 0; x < bmp.Width - 1; ++x)
            {
                for (y = 0; y < bmp.Height - 1; ++y)
                {
                    var thisPixel = bmp.GetPixel(x, y);
                    if (thisPixel.IsSimilarTo(color1))
                        bmp.SetPixel(x, y, color1);
                    else if (thisPixel.IsSimilarTo(color2))
                        bmp.SetPixel(x, y, color1);
                }
            }
            return ref bmp;
        }

        private ConcurrentDictionary<Color, int> GetColorsInImage(Bitmap img)
        {
            var colors = new ConcurrentDictionary<Color, int>();
            // Blind faith in System.Threading lmao
            for (var x = 0; x < img.Width; ++x)
            {
                for (var y = 0; y < img.Height; ++y)
                {
                    var thisPixel = img.GetPixel(x, y);
                    colors.AddOrUpdate(thisPixel, 1, (color, count) => count + 1);
                }
            }

            return colors;
        }

        private Color GetRandomColor(Random rng)
        {
            (int r, int g, int b) rgb;
            rgb.r = rng.Next(0, 255);
            rgb.g = rng.Next(0, 255);
            rgb.b = rng.Next(0, 255);

            return Color.FromArgb(rgb.r, rgb.g, rgb.b);
        }

        private void SaveButton_Click(Object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "jpeg|*.jpg|bitmap|*.bmp|gif|*.gif";
            saveFileDialog1.Title = "save image";

            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream) saveFileDialog1.OpenFile();
                // FilterIndex starts from 1
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        pictureBox1.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        pictureBox1.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        pictureBox1.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }

                fs.Close();
            }

        }

        private void button10_Click(Object sender, EventArgs e)
        {
            var img = pictureBox1.Image;
            var bmp = new Bitmap(img);
            int x, y;
            var timer = new Stopwatch();
            timer.Start();
            Cursor.Current = Cursors.WaitCursor;
            for (y = 0; y < bmp.Height - 1; ++y)
            {
                for (x = 0; x < bmp.Width - 1; ++x)
                {
                    var random = new Random();
                    Color pixelColor = bmp.GetPixel(x, y);
                    var rMod = timer.ElapsedTicks % 32; ;
                    var gMod = timer.ElapsedTicks % 48;
                    var bMod = timer.ElapsedTicks * timer.ElapsedMilliseconds % 64;
                   Color newColor = Color.FromArgb(
                        Convert.ToInt32(Math.Abs(pixelColor.R + (random.Next(1, 25) - rMod)) % 255.0 + 1),
                        Convert.ToInt32(Math.Abs(pixelColor.G + (random.Next(1, 25) - gMod)) % 255.0 + 1),
                        Convert.ToInt32(Math.Abs(pixelColor.B + (random.Next(1, 25) - bMod)) % 255.0 + 1));
                    bmp.SetPixel(x, y, newColor);
                }
            }
            timer.Stop();
            pictureBox1.Image = bmp;
            Cursor.Current = Cursors.Default;
        }
    }

    public class NeighbourSet
    {
        public int Count = 0;
        public NeighbourPoint up, right, down, left;

        /// <summary>
        /// Constructor takes a ref to a bitmap to try and avoid copying it a billion times, and the coordinates of a point on that bitmap
        /// I'm sure i'll regret this when i try and use more than one thread.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        public NeighbourSet(ref Bitmap bmp, int _x, int _y)
        {
            //If the first test passes and the neighbour exists, increment neighbourCount as well. Is there a better way to do this? I hope so lol
            up = new NeighbourPoint { x = _x, y = _y - 1, exists = _y - 1 > 0 && ++Count > 0 };
            right = new NeighbourPoint { x = _x + 1, y = _y, exists = _x + 1 < bmp.Width && ++Count > 0 };
            down = new NeighbourPoint { x = _x, y = _y + 1, exists = _y + 1 < bmp.Height && ++Count > 0 };
            left = new NeighbourPoint { x = _x - 1, y = _y, exists = _x - 1 > 0 && ++Count > 0 };

            if (up.exists)
                up.Color = bmp.GetPixel(up.x, up.y);

            if (right.exists)
                right.Color = bmp.GetPixel(right.x, right.y);

            if (down.exists)
                down.Color = bmp.GetPixel(down.x, down.y);

            if (left.exists)
                left.Color = bmp.GetPixel(left.x, left.y);
        }
    }

    public class NeighbourPoint
    {
        public int x, y;
        public bool exists;
        public Color? Color;
    }

    public static class Helpers
    {
        public static bool IsSimilarTo(this Color color, Color other)
        {
            if (Math.Abs(color.R - other.R) < 10 &&
               Math.Abs(color.G - other.G) < 10 &&
               Math.Abs(color.B - other.B) < 10
                ) return true;

            return false;
        }
    }
}


