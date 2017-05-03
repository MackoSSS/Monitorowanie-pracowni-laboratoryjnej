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

namespace Podstawy_teleinformatyki_Serwer
{
    public partial class Form2 : Form
    {
        public readonly int portNumber;
        private TcpClient client;
        private TcpClient client2;
        private TcpClient client3;
        private TcpListener server;
        private NetworkStream mainStream;
        private NetworkStream mainStream2;
        private NetworkStream mainStream3;

        ////////////
        private TcpClient klientproc;
        private TcpListener serverproc;
        NetworkStream netStream;

        private TcpClient klientproc2;
        NetworkStream netStream2;

        private readonly Thread GetProcess;
        ////////////

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
            client2 = new TcpClient();
            client3 = new TcpClient();
            Listetning = new Thread(StartListening);
            GetImage = new Thread(ReceiveImage);
            InitializeComponent();
        }

        private void StartListening()
        {
            while (!client.Connected || !klientproc.Connected)
            {
                server.Start();
                serverproc.Start();
                client = server.AcceptTcpClient();
                klientproc = serverproc.AcceptTcpClient();
            }
            GetImage.Start();
            while (!client2.Connected || !klientproc2.Connected)
            {
                client2 = server.AcceptTcpClient();

                klientproc2 = serverproc.AcceptTcpClient();
            }
            ReceiveImage2();    
        }

        private void StopListening()
        {
            server.Stop();
            client = null;

            ///////
            serverproc.Stop();
            klientproc = null;
            ///////

            if (Listetning.IsAlive) Listetning.Abort();
            if (GetImage.IsAlive) GetImage.Abort();

            ///////
            if (GetProcess.IsAlive) GetProcess.Abort();
            ///////
        }

        private void ReceiveImage()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            BinaryFormatter binFormatter2 = new BinaryFormatter();

            while (client.Connected)
            {
                mainStream = client.GetStream();
                if (radioButton1.Checked)
                {
                    pictureBox1.Image = (Image)binFormatter.Deserialize(mainStream);
                    ///////////////////////////////////////////////
                    // Pobieranie procesow
                    if (refr % 50 == 0)
                    {
                        netStream = klientproc.GetStream();
                        UzupelnijListe((String)binFormatter2.Deserialize(netStream));
                        refr = 0;
                    }
                    refr++;
                }
                pictureBox2.Image = (Image)binFormatter.Deserialize(mainStream);                
                label1.Invoke(new MethodInvoker(delegate { label1.Text = Dns.GetHostEntry(((IPEndPoint)client.Client.RemoteEndPoint).Address).HostName.ToString(); }));
               
            }
        }
        private void ReceiveImage2()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            BinaryFormatter binFormatter2 = new BinaryFormatter();

            while (client2.Connected)
            {
                mainStream2 = client2.GetStream();
                if (radioButton2.Checked)
                {
                    pictureBox1.Image = (Image)binFormatter.Deserialize(mainStream2);
                    ///////////////////////////////////////////////
                    // Pobieranie procesow
                    if (refr % 50 == 0)
                    {
                        netStream2 = klientproc2.GetStream();
                        UzupelnijListe((String)binFormatter2.Deserialize(netStream2));
                        refr = 0;
                    }
                    refr++;
                }
                pictureBox3.Image = (Image)binFormatter.Deserialize(mainStream2);
                label2.Invoke(new MethodInvoker(delegate { label2.Text = Dns.GetHostEntry(((IPEndPoint)client2.Client.RemoteEndPoint).Address).HostName.ToString(); }));
            }
        }
       
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            server = new TcpListener(IPAddress.Any, 1234);

            ///////
            serverproc = new TcpListener(IPAddress.Any, 3003);
            ///////

            Listetning.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            StopListening();
        }
       

        //////////////////// 
        ///// usuwanie/zabijanie procesu
        private void button1_Click(object sender, EventArgs e)
        {
            if(listView1.Items.Count>0)
            {
                string ProcToKill = listView1.SelectedItems[0].Text;
              //  textBox1.Text = ProcToKill;

                //Process[] prs = Process.GetProcesses("192.168.2.20");
                Process[] prs = Process.GetProcessesByName(ProcToKill, "192.168.2.20");

                foreach (Process pr in prs)
                {

                    //if (pr.ProcessName == ProcToKill)
                    //{
                        //pr.Kill();
                        //pr.WaitForExit();
                        //pr.Dispose();
                        listView1.Items.Remove(listView1.SelectedItems[0]);
                    //}
                }
            }
        }

        ///////////////////////////
        ///// sortowanie po kliknieciu na kolumne
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column);
        }

        ///////////////////////////
        ///// wpisywanie procesow do listy
        private void UzupelnijListe(string path)
        {
            string przeslaneprocesy = path;

            int miejsceprzecinka = 0;
            int miejscesrednika = 0;
            string proces = "";
            string PID = "";
            bool CzySyst = true;

            listView1.Invoke(new MethodInvoker(delegate { listView1.Items.Clear(); }));
            ArrayList alist = new ArrayList();

            for (int i = 0; i < przeslaneprocesy.Length; i++)
            {

                if (przeslaneprocesy[i].ToString() == ",")
                {
                    proces = przeslaneprocesy.Substring(miejscesrednika + 1, i - miejscesrednika - 1);

                    alist.Add(proces);

                    miejsceprzecinka = i;
                }
                if (przeslaneprocesy[i].ToString() == ";")
                {
                    PID = przeslaneprocesy.Substring(miejsceprzecinka + 1, i - miejsceprzecinka - 1);
                    //item.SubItems.Add(PID);
                    miejscesrednika = i;
                }
            }
            for (int j = 0; j < alist.Count; j++)
            {
                ListViewItem item = new ListViewItem(alist[j].ToString());
                item.ForeColor = Color.Green;
                for (int g = 0; g < ProcSys.Length; g++)
                {
                    if (alist[j].ToString() == ProcSys[g].ToString())
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

                listView1.Invoke(new MethodInvoker(delegate { listView1.Items.Add(item); }));
            }
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
