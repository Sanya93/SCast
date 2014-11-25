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

    public partial class Form1 : Form
    {
        public Socket ScreenClient = null, MouseClient = null, KeyClient = null, s = null;
        public Thread ST, MT, KT, WT;
        public Thread ScreenThread, MouseThread, KeyThread;
        public Form1()
        {
            InitializeComponent();
        }

        void ScreenThreadF()
        {
            int i = 10;
            while (i > 0)
            {
                i--;
                //MessageBox.Show("ScreenThread running");
                
                //make screenshot and send;
               
                //дождаться сообщения от драйвера и отправить
                byte[] buffer = new byte[1024];
                buffer = Encoding.UTF8.GetBytes("Like, this is screen");
                //MessageBox.Show(ScreenClient.ReceiveBufferSize.ToString());
                try
                {
                    ScreenClient.Send(buffer);
                }
                catch (SocketException ex)
                {
                    if ((ex.SocketErrorCode != SocketError.TimedOut)&&
                        (ex.SocketErrorCode != SocketError.WouldBlock))
                    {
                         MessageBox.Show("ScreenThread failed");
                         break;
                    }
                    
                }
                Thread.Sleep(5000);
             }
        }

        void WaitingThread()
        {
            while ((ST.ThreadState == ThreadState.Running)||(MT.ThreadState == ThreadState.Running)||(KT.ThreadState == ThreadState.Running))
            {
            }
            //MessageBox.Show("Screen conected");
            ScreenThread = new Thread(ScreenThreadF);
            ScreenThread.Name = "ScreenThread";
            ScreenThread.Start();
        }

        void ServerSocketThread(Object port)
        {
            IPEndPoint point;
            point = new IPEndPoint(IPAddress.Any, (int)port);
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            s.Bind(point);
            s.Listen(1);
            switch ((int)port)
            {
                case 4001:
                    ScreenClient = s.Accept();
                    break;
                case 4002:
                    MouseClient = s.Accept();
                    break;
                case 4003:
                    KeyClient = s.Accept();
                    break;
            }
       
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = true;

            //screen thread
            ST = new Thread(ServerSocketThread);
            ST.Name = "ST";
            ST.Start(4001);

            //mouse thread
            MT = new Thread(ServerSocketThread);
            MT.Name = "MT";
            //MT.Start(4002);

            //keyboard thread
            KT = new Thread(ServerSocketThread);
            KT.Name = "KT";
            //KT.Start(4003);

            //waiting thread
            WT = new Thread(WaitingThread);
            WT.Name = "WT";
            WT.Start();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 fr = new Form2();
            fr.Show();
            fr = null;
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = false;
            if (WT != null)
                WT.Abort();
            if (ST != null)
                ST.Abort();
            if (MT != null)
                MT.Abort();
            if (KT != null)
                KT.Abort();
            if (ScreenThread != null)
                ScreenThread.Abort();
            if (s != null)
                s.Close();
            MessageBox.Show("receive complete");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (WT != null)
                WT.Abort();
            if (ST != null)
                ST.Abort();
            if (MT != null)
                MT.Abort();
            if (KT != null)
                KT.Abort();
            if (ScreenThread != null)
                ScreenThread.Abort();
            if (s != null)
                s.Close();
            Application.Exit();
        }

    }
}
