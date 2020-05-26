using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XnaFan.ImageComparison;

namespace TravianMapAnalyzer
{
    public partial class Form1 : Form
    {
        Bitmap Map;
        string location = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
        int xFrom = -200, xTo = 200;
        int yFrom = -200, yTo = 200;
        static List<Cropper> Croppers = new List<Cropper>();

        readonly List<Cropper> croppers1 = new List<Cropper>();
        readonly List<Cropper> croppers2 = new List<Cropper>();
        readonly List<Cropper> croppers3 = new List<Cropper>();
        readonly List<Cropper> croppers4 = new List<Cropper>();

        private TimeSpan start;

        private string server;

        public byte[,] map2d;
        //2D map array
        //0 /
        //1 15c
        //2 50%
        //3 25%

        public static List<Bitmap> VillageMap = new List<Bitmap>();
        //VillageMap List location
        //0 - 1115
        //1 - 3339
        //2 - 3447
        //3 - 3456
        //4 - 3546
        //5 - 4347
        //6 - 4356
        //7 - 4437
        //8 - 4446
        //9 - 4536
        //10 - 5346
        //11 - 5436

        public static List<Bitmap> Oasis = new List<Bitmap>();
        //Oasis List location
        //0 - 50%
        //1 - 25%
        //2 - crop/wood
        //3 - crop/clay
        //4 - crop/iron

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string dir = "";
            string[] loc = location.Split('\\');
            for (int i = 0; i < loc.Length - 1; i++)
            {
                dir += loc[i] + "\\";
            }
            location = dir + @"data\";

            VillageMap.Add(new Bitmap(location + "1-1-1-15_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "3-3-3-9_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "3-4-4-7_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "3-4-5-6_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "3-5-4-6_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "4-3-4-7_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "4-3-5-6_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "4-4-3-7_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "4-4-4-6_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "4-5-3-6_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "5-3-4-6_58x58" + ".jpg", true));
            VillageMap.Add(new Bitmap(location + "5-4-3-6_58x58" + ".jpg", true));

            Oasis.Add(new Bitmap(location + "OO_27_27" + ".jpg", true));
            Oasis.Add(new Bitmap(location + "O_27_27" + ".jpg", true));
            Oasis.Add(new Bitmap(location + "DO_27_27" + ".jpg", true));
            Oasis.Add(new Bitmap(location + "HO_27_27" + ".jpg", true));
            Oasis.Add(new Bitmap(location + "ZO_27_27" + ".jpg", true));

            xFrom = Convert.ToInt16(numericUpDown2.Value);
            yFrom = Convert.ToInt16(numericUpDown3.Value);
            xTo = Convert.ToInt16(numericUpDown4.Value);
            yTo = Convert.ToInt16(numericUpDown5.Value);

            comboBox1.SelectedIndex = 4;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Thread ReadThread = new Thread(new ThreadStart(CheckMap));
            ReadThread.Start();
            start = DateTime.Now.TimeOfDay;
            button6.Enabled = false;
            label29.Text = "";
        }

        public void CheckMap()
        {
            for (int x = xFrom; x < xTo; x += 10)
            {
                DoInvoke(delegate
                {
                    label5.Text = "X:" + x;
                    progressBar1.Value = x - xFrom;
                });
                for (int y = yFrom; y < yTo; y += 10)
                {
                    DoInvoke(delegate { label6.Text = "Y:" + y; });
                    string url = $"https://{server}/map_block.php?tx0={x}&ty0={y}&tx1={x+9}&ty1={y+9}";
                    //string linkk = "https://"+server+"/map_block.php?tx0=" + x + "&ty0=" + y  + "&tx1=" + (x + 9) + "&ty1=" + (y + 9) + ";
                    // Console.WriteLine("link" + linkk);
                    /*
                    string localFilename = location + @"\mapa.jpg";
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(linkk, localFilename);
                    }
                    Map = new Bitmap(location + @"mapa.jpg", true);
                    */

                    System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                    System.Net.WebResponse response = request.GetResponse();
                    System.IO.Stream responseStream = response.GetResponseStream();
                    Map = new Bitmap(responseStream);

                    for (int x1 = 0; x1 < 10; x1++)
                    {
                        for (int y1 = 0; y1 < 10; y1++)
                        {
                            Rectangle cropRect;
                            Bitmap target;
                            cropRect = new Rectangle((x1 * 60 + 1), (y1 * 60 + 1), 58, 58);
                            target = new Bitmap(cropRect.Width, cropRect.Height);
                            using (Graphics g = Graphics.FromImage(target))
                            {
                                g.DrawImage(Map, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
                            }
                            float Diff = ExtensionMethods.PercentageDifference((Image)target, (Image)VillageMap.ElementAt(0), 4);
                            if (Diff < 0.01)
                            {
                                int pravix = x + x1;
                                int praviy = y + 9 - y1;
                                int xx = pravix - xFrom;
                                int yy = praviy - yFrom;
                                map2d[xx, yy] = 1;
                                // Console.WriteLine("15c NA KOORDIH x:" + pravix + " in y=" + praviy);
                            }


                            for (byte i = 0; i < Oasis.Count; i++) //checking for oasis
                            {
                                cropRect = new Rectangle((x1 * 60 + 38), (y1 * 60 + 7), 16, 16);
                                target = new Bitmap(cropRect.Width, cropRect.Height);
                                using (Graphics g = Graphics.FromImage(target))
                                {
                                    g.DrawImage(Map, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
                                }
                                float diff = ExtensionMethods.PercentageDifference((Image)target, (Image)Oasis.ElementAt(i), 20);
                                if (diff < 0.35)
                                {
                                    int correctX = x + x1;
                                    int correctY = y + 9 - y1;
                                    int mapX = correctX - xFrom;
                                    int mapY = correctY - yFrom;
                                    //Console.WriteLine("xx " + xx + " ,yy " + yy + "  mapa" + mapa2d.Length);

                                    if (i >= 1) map2d[mapX, mapY] = 3;
                                    else map2d[mapX, mapY] = 2;

                                    break;
                                }
                            }
                        }
                    }
                    Map.Dispose();
                }
            }
            DoInvoke(delegate { CheckForCropers(); progressBar1.Value = progressBar1.Maximum; });
        }

        private void DoInvoke(MethodInvoker del)
        {
            if (InvokeRequired) { Invoke(del); }
            else { del(); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("xfrom" + xFrom);
            if (xFrom > -400 && yFrom > -400 && xTo < 400 && yTo < 400)
            {
                xFrom -= 10;
                numericUpDown2.Value = xFrom;
                yFrom -= 10;
                numericUpDown3.Value = yFrom;
                xTo += 10;
                numericUpDown4.Value = xTo;
                yTo += 10;
                numericUpDown5.Value = yTo;
            }
            UpdateLabel();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (xFrom < -10 && yFrom < -10 && xTo > 10 && yTo > 10)
            {
                xFrom += 10;
                numericUpDown2.Value = xFrom;
                yFrom += 10;
                numericUpDown3.Value = yFrom;
                xTo -= 10;
                numericUpDown4.Value = xTo;
                yTo -= 10;
                numericUpDown5.Value = yTo;
            }
            UpdateLabel();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            xFrom = Convert.ToInt16(numericUpDown2.Value);
            UpdateLabel();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            yFrom = Convert.ToInt16(numericUpDown3.Value);
            UpdateLabel();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            xTo = Convert.ToInt16(numericUpDown4.Value);
            UpdateLabel();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            yTo = Convert.ToInt16(numericUpDown5.Value);
            UpdateLabel();
        }

        public void CheckForCropers()
        {
            for (int i = 0; i < map2d.GetLength(0); i++)
            {
                for (int j = 0; j < map2d.GetLength(1); j++)
                {
                    if (map2d[i, j] == 1)
                    {
                        if (i >= 3 && j >= 3 && i <= map2d.GetLength(0) - 4 && j <= map2d.GetLength(1) - 4)
                        {
                            byte num50 = 0;
                            byte num25 = 0;
                            for (int k = i - 3; k <= i + 3; k++)
                            {
                                for (int l = j - 3; l <= j + 3; l++)
                                {
                                    //Console.WriteLine(k + "/" + l);
                                    if (map2d[k, l] == 2) { num50++; }
                                    if (map2d[k, l] == 3) { num25++; }
                                }
                            }
                            int bonus = 0;
                            int left = 3;
                            for (int k = 0; k < 6; k++)
                            {
                                if (k < 3 && num50 > 0)
                                {
                                    bonus += 50;
                                    num50--;
                                    left--;
                                }
                                if (k >= 3 && num25 > 0 && left > 0)
                                {
                                    bonus += 25;
                                    num25--;
                                    left--;
                                }
                            }
                            int xx = i + xFrom;
                            int yy = j + yFrom;

                            Insert(xx, yy, bonus);
                            Croppers.Add(new Cropper
                            {
                                X = xx,
                                Y = yy,
                                Bonus = bonus
                            });
                            //Console.WriteLine("Croper {0}/{1} ima {2}%", i + xfrom, j + yfrom, bonus);
                        }
                        else richTextBox5.AppendText((i + xFrom) + "/" + (j + yFrom) + "\n");
                    }
                }
            }
            //Console.WriteLine("Time elapsed:" + (DateTime.Now-start));
            int totalsec = Convert.ToInt16(Math.Round((DateTime.Now.TimeOfDay - start).TotalSeconds));
            int sec = totalsec % 60;
            int min = totalsec / 60;
            label27.Text = "Time elapsed: " + min + " min and " + sec + " sec";
            InsertToBox(75);
        }

        public void Insert(int xx, int yy, int bonus)
        {
            if (xx >= 0)
            {
                if (yy >= 0)//++
                {
                    croppers2.Add(new Cropper
                    {
                        X = xx,
                        Y = yy,
                        Bonus = bonus
                    });
                }
                else//+-
                {
                    croppers4.Add(new Cropper
                    {
                        X = xx,
                        Y = yy,
                        Bonus = bonus
                    });
                    //richTextBox4.AppendText(xx + "/" + yy + "  " + bonus + "%\n");
                }
            }
            else
            {
                if (yy >= 0)//-+
                {
                    croppers1.Add(new Cropper
                    {
                        X = xx,
                        Y = yy,
                        Bonus = bonus
                    });
                    //richTextBox1.AppendText(xx + "/" + yy + "  " + bonus + "%\n");
                }
                else//--
                {
                    croppers3.Add(new Cropper
                    {
                        X = xx,
                        Y = yy,
                        Bonus = bonus
                    });
                    //richTextBox3.AppendText(xx + "/" + yy + "  " + bonus + "%\n");
                }
            }
        }

        public void InsertToBox(int bonus)
        {
            List<Cropper> SortedCrop1 = croppers1.OrderBy(o => o.Bonus).ToList();
            List<Cropper> SortedCrop2 = croppers2.OrderBy(o => o.Bonus).ToList();
            List<Cropper> SortedCrop3 = croppers3.OrderBy(o => o.Bonus).ToList();
            List<Cropper> SortedCrop4 = croppers4.OrderBy(o => o.Bonus).ToList();
            int num = 0;

            foreach (Cropper i in SortedCrop1)
            {
                if (i.Bonus >= bonus)
                {
                    richTextBox1.AppendText(i.X + "/" + i.Y + " - " + i.Bonus + "%\n");
                    num++;
                }
            }
            label20.Text = "Count: " + num;
            num = 0;
            foreach (Cropper i in SortedCrop2)
            {
                if (i.Bonus >= bonus)
                {
                    richTextBox2.AppendText(i.X + "/" + i.Y + " - " + i.Bonus + "%\n");
                    num++;
                }
            }
            label21.Text = "Count: " + num;
            num = 0;
            foreach (Cropper i in SortedCrop3)
            {
                if (i.Bonus >= bonus)
                {
                    richTextBox3.AppendText(i.X + "/" + i.Y + " - " + i.Bonus + "%\n");
                    num++;
                }
            }
            label22.Text = "Count: " + num;
            num = 0;
            foreach (Cropper i in SortedCrop4)
            {
                if (i.Bonus >= bonus)
                {
                    richTextBox4.AppendText(i.X + "/" + i.Y + " - " + i.Bonus + "%\n");
                    num++;
                }
            }
            label23.Text = "Count: " + num;
            num = 0;
        }
        public void button4_Click_1(object sender, EventArgs e)
        {
            label29.Text = "";
            server = textBox1.Text;

            string url = $"https://{server}/map_block.php?tx0=10&ty0=10&tx1=19&ty1=19";
            System.Net.WebRequest request = System.Net.WebRequest.Create(url);
            try
            {
                System.Net.WebResponse response1 = request.GetResponse();
                System.IO.Stream responseStream = response1.GetResponseStream();
                Map = new Bitmap(responseStream);
                button6.Enabled = true;
                map2d = new byte[(xTo - xFrom), (yTo - yFrom)];
            }
            catch (Exception)
            {
                label7.Text = "This link is invalid!";
            }

            progressBar1.Maximum = xTo - xFrom + 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<CropperFilt> Filtered = new List<CropperFilt>();
            richTextBox6.Clear();
            int x = Convert.ToInt16(numericUpDown6.Value);
            int y = Convert.ToInt16(numericUpDown1.Value);
            int bonus = comboBox1.SelectedIndex * 25;
            foreach (Cropper i in Croppers)
            {
                if (i.Bonus >= bonus)
                {
                    double dist = Math.Sqrt(Math.Pow(Math.Abs(i.X - x), 2) + Math.Pow(Math.Abs(i.Y - y), 2));
                    Filtered.Add(new CropperFilt
                    {
                        X = i.X,
                        Y = i.Y,
                        Bonus = i.Bonus,
                        Distance = dist
                    });
                }
            }
            List<CropperFilt> SortedCrop = Filtered.OrderBy(o => o.Distance).ToList();
            foreach (CropperFilt i in SortedCrop)
            {
                richTextBox6.AppendText(Math.Round(i.Distance, 1) + "\t" + i.X + "|" + i.Y + "\t" + i.Bonus + "%\n");
            }
        }

        public void UpdateLabel()
        {
            label7.Text = "Will check " + (xTo - xFrom) + "x" + (yTo - yFrom) + "(" + (yTo - yFrom) * (xTo - xFrom) + ") squares";
            label26.Text = "Estimated time required: " + Math.Round(Convert.ToDouble((yTo - yFrom) * (xTo - xFrom) / 10000), 1) + " min";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
            richTextBox3.Clear();
            richTextBox4.Clear();
            int bonus = comboBox2.SelectedIndex * 25;
            InsertToBox(bonus);
        }

        public class Cropper
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Bonus { get; set; }
        }

        public class CropperFilt : Cropper
        {
            public double Distance { get; set; }
        }

    }
}
