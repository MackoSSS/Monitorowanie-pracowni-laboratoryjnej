using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Podstawy_Teleinformatyki
{
    public partial class Form1 : Form
    {
        private readonly TcpClient client = new TcpClient();
        private NetworkStream mainStream;
        private int portNumber;

        private static Image GrabDesktop()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            Graphics graphic = Graphics.FromImage(screenshot);
            graphic.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);

            return screenshot;
        }

        private void SendDesktopImage()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            mainStream = client.GetStream();
            binFormatter.Serialize(mainStream, GrabDesktop());

        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            portNumber = 1234;
            try
            {
                client.Connect(textBox1.Text, portNumber);
                MessageBox.Show("Connected!");
            }
            catch(Exception)
            {
                MessageBox.Show("Failed to connect...");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text.StartsWith("Share"))
            {
                timer1.Start();
                button2.Text = "Stop sharing";
            }
            else 
            {
                timer1.Stop();
                button2.Text = "Share my desktop";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SendDesktopImage();
        }
    }
}
