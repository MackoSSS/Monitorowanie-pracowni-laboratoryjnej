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
using System.IO;

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
        int refrzakaz = 0;
        string[] ProcSys = { "svchost", "winlogon", "RtHDVCpl", "LogonUI", "csrss", "WUDFHost", "taskhostw", "wininit", "NisSrv", "sihost", "dwm", "app_updater", "SearchUI", "armsvc", "lsass", "MsMpEng", "explorer", "dllhost", "mmc", "services", "ShellExperienceHost", "audiodg", "RuntimeBroker", "sqlwriter", "smss", "System" };
        string[] ProcZakazane = { "proc1", "proc2", "proc3" };
        /////////
     

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

            int refr2 = 0;

            while (client.Connected)
            {
                mainStream = client.GetStream();

                netStream = klientproc.GetStream();

                
                if (radioButton1.Checked)
                {
                    pictureBox1.Image = (Image)binFormatter.Deserialize(mainStream);
                    ///////////////////////////////////////////////
                    // Pobieranie procesow
                    if (refr % 50 == 0)
                    {
                        //netStream = klientproc.GetStream();
                        UzupelnijListe((String)binFormatter2.Deserialize(netStream), 1);
                        refr = 0;
                    }
                    refr++;
                }
                if(checkBox1.Checked)
                {
                    if (refrzakaz % 50 == 0)
                    {
                        CzyUruchomionyZakazany((String)binFormatter2.Deserialize(netStream), 1);
                        refrzakaz = 0;
                    }
                    refrzakaz++;
                }

                //CzyUruchomionyZakazany((String)binFormatter2.Deserialize(netStream), 1);
                //netStream.Flush();

                pictureBox2.Image = (Image)binFormatter.Deserialize(mainStream);                
                label1.Invoke(new MethodInvoker(delegate { label1.Text = Dns.GetHostEntry(((IPEndPoint)client.Client.RemoteEndPoint).Address).HostName.ToString(); }));

                
                //netStream.Flush();
            }
        }
        private void ReceiveImage2()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            BinaryFormatter binFormatter2 = new BinaryFormatter();
            
            int refr2 = 0;

            while (client2.Connected)
            {
                mainStream2 = client2.GetStream();

                netStream2 = klientproc2.GetStream();
                if (radioButton2.Checked)
                {
                    pictureBox1.Image = (Image)binFormatter.Deserialize(mainStream2);
                    ///////////////////////////////////////////////
                    // Pobieranie procesow
                    if (refr2 % 50 == 0)
                    {
                        //netStream2 = klientproc2.GetStream();
                        UzupelnijListe((String)binFormatter2.Deserialize(netStream2),2);
                        refr2 = 0;
                    }
                    refr2++;
                }
                if (checkBox1.Checked)
                {
                    if (refrzakaz % 50 == 0)
                    {
                        CzyUruchomionyZakazany((String)binFormatter2.Deserialize(netStream), 2);
                        
                    }
                    
                }
                netStream2.Flush();

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
       

        ///////////////////////////
        ///// sortowanie po kliknieciu na kolumne
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listaZakazanych();
        }

        private void listaZakazanych()
        {
            if(checkBoxNotatnik.Checked)
            {
                ProcZakazane[0] = "notepad";
                
            }
            else
            {
                ProcZakazane[0] = "proc1";
            }
            if(checkBoxMozilla.Checked)
            {
                ProcZakazane[1] = "firefox";
            }
            else
            {
                ProcZakazane[1] = "proc2";
            }
            if(checkBoxKalkulator.Checked)
            {
                ProcZakazane[2] = "calc";
            }
            else
            {
                ProcZakazane[2] = "proc3";
            }
        }

        ///////////////////////////
        ///// sprawdzanie czy jest zakazany proces w tle
        private void CzyUruchomionyZakazany(string path, int idKomputera)
        {
            string przeslaneprocesy = path;

            int miejsceprzecinka = 0;
            int miejscesrednika = 0;
            string proces = "";
            string PID = "";
            bool CzySyst = true;
            bool CzyZakazany = false;
            int lZakazanych = 0;

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
                    miejscesrednika = i;
                }
            }
            for (int j = 0; j < alist.Count; j++)
            {
                for (int g = 0; g < ProcZakazane.Length; g++)
                {
                    if (alist[j].ToString() == ProcZakazane[g].ToString())
                    {
                        CzyZakazany = true;
                        if (idKomputera == 1)
                        {
                            pictureBox4.Image = Image.FromFile("images\\alert.png");
                        }
                        if (idKomputera == 2)
                        {
                            pictureBox5.Image = Image.FromFile("images\\alert.png");
                        }
                        lZakazanych++;
                        break;
                    }
                    else
                    {
                        CzyZakazany = false;
                    }
                }
            }
            if (lZakazanych == 0)
            {
                if (idKomputera == 1)
                {
                    pictureBox4.Image = null;
                }
                if (idKomputera == 2)
                {
                    pictureBox5.Image = null;
                }
            }
        }

        ///////////////////////////
        ///// wpisywanie procesow do listy
        private void UzupelnijListe(string path,int idKomputera)
        {
            string przeslaneprocesy = path;

            int miejsceprzecinka = 0;
            int miejscesrednika = 0;
            int miejscenawiasu = 0;
            string proces = "";
            string PID = "";
            bool CzySyst = true;
            bool CzyZakazany = false;
            int lZakazanych = 0;
            string nazwa_karty = "";

            listView1.Invoke(new MethodInvoker(delegate { listView1.Items.Clear(); }));
            ArrayList alist = new ArrayList();
            ArrayList karty = new ArrayList();

            for (int i = 0; i < przeslaneprocesy.Length; i++)
            {

                if (przeslaneprocesy[i].ToString() == "☺")
                {
                    proces = przeslaneprocesy.Substring(miejscesrednika + 1, i - miejscesrednika - 1);

                    alist.Add(proces);

                    miejsceprzecinka = i;
                }
                if (przeslaneprocesy[i].ToString() == "☻")
                {
                    PID = przeslaneprocesy.Substring(miejsceprzecinka + 1, i - miejsceprzecinka - 1);
                    //item.SubItems.Add(PID);
                    miejscenawiasu = i;
                }
                if (przeslaneprocesy[i].ToString() == "♥")
                {
                    nazwa_karty = przeslaneprocesy.Substring(miejscenawiasu + 1, i - miejscenawiasu - 1);
                    karty.Add(nazwa_karty);
                    
                    miejscesrednika = i;
                }
            }

            for (int j = 0; j < alist.Count; j++)
            {

                ListViewItem item = new ListViewItem(alist[j].ToString());
                ListViewItem item2 = new ListViewItem(alist[j].ToString());
                ListViewItem czas = new ListViewItem();
                String teraz = DateTime.Now.ToString();
                var listViewItem = new ListViewItem(teraz);
                //czas.SubItems.Add(teraz);
                item2.SubItems.Add(teraz);

                string str = karty[j].ToString();
                int i = str.IndexOf('☻');
                if (i >= 0) str = str.Substring(i + 1);

                if (str != "") item2.SubItems.Add(str);

                if ((alist[j].ToString() == "firefox" && str != "") || (alist[j].ToString() == "chrome" && str != "") || (alist[j].ToString() == "opera" && str != "") || (alist[j].ToString() == "iexplorer" && str != ""))
                {
                    listView2.Invoke(new MethodInvoker(delegate { listView2.Items.Add(item2); }));
                    listView2.Invoke(new MethodInvoker(delegate { listView2.Items.Add((ListViewItem)item2.Clone()); }));
               
                }

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
                for (int g = 0; g < ProcZakazane.Length; g++)
                {
                    if (alist[j].ToString() == ProcZakazane[g].ToString())
                    {
                        item.ForeColor = Color.Red;
                        item.SubItems.Add("Zakazany");
                        CzyZakazany = true;
                        if (idKomputera == 1)
                        {
                            pictureBox4.Image = Image.FromFile("images\\alert.png");
                        }
                        if (idKomputera == 2)
                        {
                            pictureBox5.Image = Image.FromFile("images\\alert.png");
                        }
                        lZakazanych++;
                        break;
                    }
                    else
                    {
                        CzyZakazany = false;
                    }
                }
                if (CzySyst == false && CzyZakazany == false)
                {
                    item.ForeColor = Color.DarkOrange;
                    item.SubItems.Add("Inny");
                }

                listView1.Invoke(new MethodInvoker(delegate { listView1.Items.Add(item); }));
            }
            if(lZakazanych==0)
            {
                if (idKomputera == 1)
                {
                    pictureBox4.Image = null;
                }
                if (idKomputera == 2)
                {
                    pictureBox5.Image = null;
                }
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
