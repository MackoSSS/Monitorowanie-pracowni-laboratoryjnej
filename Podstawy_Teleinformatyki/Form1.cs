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

/////////////////////
using System.Collections;
using System.Diagnostics;
using System.IO;
//using System.Management;
/////////////////////

namespace Podstawy_Teleinformatyki
{
    public partial class Form1 : Form
    {
        private readonly TcpClient client = new TcpClient();
        private NetworkStream mainStream;
        private Stream s;
        private int portNumber;


        /////////
        private readonly TcpClient klientproc = new TcpClient();
        private NetworkStream netStream;
        private int portNumberProc;
        int takty = 50;

        string nazwapliku = "";

        string[] ProcSys = { "spoolsv", "AvastSvc", "avastui", "conhost", "svchost", "winlogon", "RtHDVCpl", "LogonUI", "csrss", "WUDFHost", "taskhostw", "wininit", "NisSrv", "sihost", "dwm", "app_updater", "SearchUI", "armsvc", "lsass", "MsMpEng", "explorer", "dllhost", "mmc", "services", "ShellExperienceHost", "audiodg", "RuntimeBroker", "sqlwriter", "smss", "System" };

        /////////
        bool czySyst = false;

        ///////////////////////////
        ///////// pobieranie listy procesow
        private string GetProcessList()
        {
            Process[] processes = Process.GetProcesses();
            string ProcessToSend = "";

            for (int i = 0; i < processes.Count(); i++)
            {
                if (processes[i].ProcessName.ToString() == "notepad")
                    Monit("Wyłącz");
                for (int j = 0; j < ProcSys.Count(); j++)
                {
                    if (processes[i].ProcessName.ToString() == ProcSys[j].ToString())
                    {
                        czySyst = true;
                        break;
                    }
                    else
                    {
                        czySyst = false;
                    }
                }
                if (czySyst == false)
                {
                    ProcessToSend = ProcessToSend + processes[i].ProcessName + "☺" + processes[i].Id + "☻" + processes[i].MainWindowTitle + "♥";
                }
                czySyst = false;
            }
            return ProcessToSend;
        }


        private static Image GrabDesktop()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            Graphics graphic = Graphics.FromImage(screenshot);
            graphic.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            return screenshot;
        }

        public Stream ConvertImage()
        {
            var image = GrabDesktop();

            var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;
            return stream;
        }


        int nr = 0;
        private void SendDesktopImage()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            var image = Image.FromStream(ConvertImage());
            mainStream = client.GetStream();

            binFormatter.Serialize(mainStream, image);
        }

        ///////////////////////////
        ///////// wysylanie procesow
        private void SendProces(string DoWyslania)
        {
            BinaryFormatter binFormatter2 = new BinaryFormatter();
            netStream = klientproc.GetStream();
            binFormatter2.Serialize(netStream, DoWyslania);
            nr++;
        }

        public void Monit(string message)
        {
            if (Process.GetProcessesByName("notepad").Any())
            {
                MessageBox.Show(message);
            }
        }

        public Form1()
        {
            Monit("Wyłącz to!");
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            portNumber = 1234;
            portNumberProc = 3003;//Int32.Parse(textBox2.Text);
            try
            {
                klientproc.Connect(textBox1.Text, portNumberProc);
                client.Connect(textBox1.Text, portNumber);

                MessageBox.Show("Connected!");
            }
            catch (Exception)
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

            if (takty == 50)
            {
                SendProces(GetProcessList());
                takty = 0;
            }
            takty++;


        }
    }
}
