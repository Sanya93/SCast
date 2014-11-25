using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace WindowsFormsApplication1
{
    
    public partial class Form2 : Form
    {
        public Thread ST, MT, KT, WT;
        public Thread ScreenThread, MouseThread, KeyThread;
        public Socket ScreenClient = null, MouseClient = null, KeyClient = null;
        public Form2()
        {
            InitializeComponent();
            
        }

        void ScreenThreadF()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                try
                {
                ScreenClient.Receive(buffer);
                }
                catch (SocketException ex)
                {
                    if ((ex.SocketErrorCode != SocketError.TimedOut) &&
                        (ex.SocketErrorCode != SocketError.WouldBlock))
                    {
                        MessageBox.Show("ScreenThread failed");
                        break;
                    }

                }
                string result = System.Text.Encoding.UTF8.GetString(buffer);
                MessageBox.Show(result);
            }
            /*int i = 10;
            while (i > 0)
            {
                i--;
                MessageBox.Show("ScreenThread running");
                //make screenshot and send;
                Thread.Sleep(10000);
            }*/
        }

        void WaitingThread()
        {
            while (ST.ThreadState == ThreadState.Running)
            {
            }
            //MessageBox.Show("Screen conected");
            ScreenThread = new Thread(ScreenThreadF);
            ScreenThread.Name = "ScreenThread";
            ScreenThread.Start();
        }

        void ClientSocketThread(Object port)
        {
            IPAddress addr = IPAddress.Parse(textBox1.Text);
            IPEndPoint point = new IPEndPoint(addr, (int)port);
            switch ((int)port)
            {
                case 4001:
                    try
                    {
                        ScreenClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                        ScreenClient.Connect(point);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            MessageBox.Show("Server not found");
                            try {
                                if (this.InvokeRequired) 
                                    BeginInvoke(new MethodInvoker(delegate 
                                        { 
                                        ConnectMenuItem.Enabled = true;
                                        CloseMenuItem.Enabled = false;
                                        })); 
                                else 
                                {
                                    ConnectMenuItem.Enabled = true;
                                        CloseMenuItem.Enabled = false; 
                                }
                                } 
                            catch (Exception ex1) 
                            { 
                                MessageBox.Show(ex1.Message); 
                            }
                          }
                    }
                    break;
                case 4002:
                    try
                    {
                    MouseClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    MouseClient.Connect(point);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            MessageBox.Show("Server not found");
                        }
                    }
                    break;
                case 4003:
                    try
                    {
                    KeyClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    KeyClient.Connect(point);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            MessageBox.Show("Server not found");
                        }
                    }
                    break;
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ST != null)
                ST.Abort();
            if (MT != null)
                MT.Abort();
            if (KT != null)
                KT.Abort();
            if (WT != null)
                WT.Abort();
            if (ScreenThread != null)
                ScreenThread.Abort();
            Form1 frm = new Form1();
            frm.Show();
            frm = null;
        }

        private void ConnectMenuItem_Click(object sender, EventArgs e)
        {
            CloseMenuItem.Enabled = true;
            ConnectMenuItem.Enabled = false;

            //screen thread
            ST = new Thread(ClientSocketThread);
            ST.Name = "ST";
            ST.Start(4001);

            //mouse thread
            MT = new Thread(ClientSocketThread);
            MT.Name = "MT";
            //MT.Start(4002);

            //keyboard thread
            KT = new Thread(ClientSocketThread);
            KT.Name = "KT";
            //KT.Start(4003);

            //waiting thread
            WT = new Thread(WaitingThread);
            WT.Name = "WT";
            WT.Start();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            ScreenThread.Abort();
            ScreenClient.Close();
        }

    }
}
