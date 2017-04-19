using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

//using System.Windows;
//using System.Management;

namespace Podstawy_teleinformatyki_Serwer
{
    public partial class Form2 : Form
    {
        public readonly int portNumber;
        private TcpClient client;
        private TcpListener server;
        private NetworkStream mainStream;

        private readonly Thread Listetning;
        private readonly Thread GetImage;

        ////////////
        string procesy="";
        int refr = 0;
        string[] ProcSys = { "svchost", "winlogon", "RtHDVCpl", "LogonUI", "csrss", "WUDFHost", "taskhostw", "wininit", "NisSrv", "sihost", "dwm", "app_updater", "SearchUI", "armsvc", "lsass", "MsMpEng", "explorer", "dllhost", "mmc", "services", "ShellExperienceHost", "audiodg", "RuntimeBroker", "sqlwriter", "smss", "System" };
        /// /////////
     

        public Form2(int port)
        {
            portNumber = port;
            client = new TcpClient();
            Listetning = new Thread(StartListening);
            GetImage = new Thread(ReceiveImage);
            InitializeComponent();
        }

        private void StartListening()
        {
            while (!client.Connected)
            {
                server.Start();
                client = server.AcceptTcpClient();
            }
            GetImage.Start();
        }

        private void StopListening()
        {
            server.Stop();
            client = null;

            if (Listetning.IsAlive) Listetning.Abort();
            if (GetImage.IsAlive) GetImage.Abort();
        }

        private void ReceiveImage()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();

            while (client.Connected)
            {
                mainStream = client.GetStream();
                pictureBox1.Image = (Image)binFormatter.Deserialize(mainStream);
                pictureBox2.Image = (Image)binFormatter.Deserialize(mainStream);
                pictureBox3.Image = (Image)binFormatter.Deserialize(mainStream);

               // label1.Text = Dns.GetHostEntry("192.168.43.179").HostName.ToString();
                label1.Invoke(new MethodInvoker(delegate { label1.Text = Dns.GetHostEntry(((IPEndPoint)client.Client.RemoteEndPoint).Address).HostName.ToString(); }));
                
                ///////////////////////////////////////////////
                // Pobieranie procesow
               
                if (refr % 50 == 0)
                {
                    listView1.Invoke(new MethodInvoker(delegate { listView1.Items.Clear(); }));
                    ArrayList alist = new ArrayList();
                    
                    //Process[] processes = Process.GetProcesses("DESKTOP-UCD1E8F");
                    //Process[] processes = Process.GetProcesses("USER2-KOMPUTER");
                    //Process[] processes = Process.GetProcesses("192.168.1.15");
                    //Process[] processes = Process.GetProcesses("192.168.56.2");
                    Process[] processes = Process.GetProcesses(Dns.GetHostEntry(((IPEndPoint)client.Client.RemoteEndPoint).Address).HostName.ToString());
                    bool CzySyst = true;
                    int i = 0;
                    foreach (Process process in processes)
                    {

                        alist.Add(process.ProcessName);
                        
                        ListViewItem item = new ListViewItem(alist[i].ToString());
                        item.ForeColor = Color.Green;
                        ///////////////////////////////////////

                        for (int g = 0; g < ProcSys.Length;g++)
                        {
                            if (alist[i].ToString() == ProcSys[g].ToString())
                            {
                                item.ForeColor = Color.Green;
                                item.SubItems.Add("Systemowy");
                                CzySyst = true;
                                break;
                            }
                            else
                            {
                                CzySyst = false;
                            }
                        }
                        if (CzySyst == false)
                        {
                            item.ForeColor = Color.Red;
                            item.SubItems.Add("Inny");
                        }
                        
                        ////////////////////////////////////////
                        listView1.Invoke(new MethodInvoker(delegate { listView1.Items.Add(item); }));
                        i++;
                    }

                    alist.Clear();
                    procesy = "";
                    refr = 0;
                }
                refr++;
               
                /////////////////////////////////////////////////
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            server = new TcpListener(IPAddress.Any, 1234);
            Listetning.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            StopListening();
        }

        

        ///////////////////////////
        ///// sortowanie po kliknieciu na kolumne
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column);
        }
    }
    
    ///////////////////////////
    ///// Klasa potrzebna do sortowania kolumn
    class ListViewItemComparer : IComparer
    {
        private int col;
        public ListViewItemComparer()
        {
            col = 0;
        }
        public ListViewItemComparer(int column)
        {
            col = column;
        }
        public int Compare(object x, object y)
        {
            return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
        }
    }

    ///////////////////////////
}
