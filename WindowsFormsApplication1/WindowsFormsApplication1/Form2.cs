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
using System.IO;

namespace WindowsFormsApplication1
{
    
    public partial class Form2 : Form
    {
        public Thread ST, MT, KT, WT;
        public Thread ScreenThread, MouseThread, KeyThread;
        public Socket ScreenClient = null, MouseClient = null, KeyClient = null;
        public bool conected = false,Controled = false;
        public string[] errors = {"Server not found","Connection was reset"};
        public int ErrorCode = 0;
        public Form2()
        {
            InitializeComponent();
        }
     void ScreenThreadF()
        {
            bool error = false;
            int size = 0;
            int rbc = 0;
            byte[] buffer = new byte[8192];
            using (MemoryStream stream = new MemoryStream())
            {
                while (!error)
                {
                    try
                    {
                        Array.Clear(buffer, 0, buffer.Length);
                        ScreenClient.Receive(buffer);
                    }
                    catch (SocketException ex)
                    {
                        if ((ex.SocketErrorCode != SocketError.TimedOut) &&
                            (ex.SocketErrorCode != SocketError.WouldBlock))
                        {
                            ErrorCode = 2;
                            error = true;
                            break;
                        }
                    }
                    if (!error)
                    {

                        string result = System.Text.Encoding.UTF8.GetString(buffer);

                        if (result.Contains("size"))
                        {
                            string str = result.Substring(5);
                            size = Convert.ToInt32(str);
                            stream.Position = 0;
                            rbc = 0;
                        }
                        else
                        {
                            if (rbc < size)
                            {
                                stream.Write(buffer, 0, buffer.Length);
                                rbc += buffer.Length;
                            }
                            if (rbc >= size)
                            {
                                Bitmap bmp = new Bitmap(stream);
                                try
                                {
                                    if (this.InvokeRequired)
                                        BeginInvoke(new MethodInvoker(delegate
                                        {
                                            pictureBox1.Image = bmp;
                                        }));
                                    else
                                    {
                                        pictureBox1.Image = bmp;
                                    }
                                }
                                catch (Exception ex1)
                                {
                                    MessageBox.Show(ex1.Message);
                                }  
                            }
                        }
                    }
                }
            }
        }

        void WaitingThread()
        {
            while ((ST.ThreadState == ThreadState.Running)||(MT.ThreadState == ThreadState.Running)||(KT.ThreadState == ThreadState.Running))
            {
            }
            if (ErrorCode == 1)
            {
                MessageBox.Show(errors[ErrorCode-1]);
                return;
            }
            conected = true;
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        ControlMenuItem.Enabled = true;
                    }));
                else
                {
                    ControlMenuItem.Enabled = true;
                }
            }
            catch (Exception ex1)
            {
                MessageBox.Show(ex1.Message);
            }  
            ScreenThread = new Thread(ScreenThreadF);
            ScreenThread.Name = "ScreenThread";
            ScreenThread.Start();

            
        }

        void ResetClient()
        {

            try
            {
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
                            ErrorCode = 1;
                            ResetClient();
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
                            ErrorCode = 1;
                            ResetClient();
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
                            ErrorCode = 1;
                            ResetClient();
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
            MT.Start(4002);

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
            if (WT != null)
                WT.Abort();
            if (ScreenClient != null)
                ScreenClient.Close();
            if (ST != null)
                ST.Abort();
            if (MouseClient != null)
                MouseClient.Close();
            if (MT != null)
                MT.Abort();
            if (KT != null)
                KT.Abort();
            if (ScreenThread != null)
                ScreenThread.Abort();
            if (MouseThread != null)
                MouseThread.Abort();
            Controled = false;
            conected = false;
            ConnectMenuItem.Enabled = true;
            CloseMenuItem.Enabled = false;
            ControlMenuItem.Enabled = false;

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (conected && Controled)
            {
                float x = (float)e.X / pictureBox1.Width;
                float y = (float)e.Y / pictureBox1.Height;
                string mes = "move " + x.ToString() + ' ' + y.ToString() + ' ';
                //MessageBox.Show(mes);
                try
                {
                    //MouseClient.SendBufferSize = mes.Length;
                    MouseClient.Send(Encoding.UTF8.GetBytes(mes));
                }
                catch (SocketException ex)
                {
                    if ((ex.SocketErrorCode != SocketError.TimedOut) &&
                                (ex.SocketErrorCode != SocketError.WouldBlock))
                        return;
                    //MessageBox.Show("MouseClient failed");
                }
            }
            //label2.Text = "X = " + x.ToString() + " Y = " + y.ToString();
        }

        private void начатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rectangle rect = pictureBox1.Bounds;
            rect.X += this.Left;
            rect.Y += this.Top;
            Cursor.Clip = rect;
            Controled = true;
        }

        private void завершитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Clip = Screen.PrimaryScreen.Bounds;
            Controled = false;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //
            label2.Text = e.Button.ToString();
            label2.Text +=" ";
            label2.Text +=e.Clicks.ToString();
            label2.Text += " ";
            label2.Text += e.Delta.ToString();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //
            label3.Text = e.Button.ToString();
            label3.Text += " ";
            label3.Text += e.Clicks.ToString();
            label3.Text += " ";
            label3.Text += e.Delta.ToString();
        }

        void this_MouseWheel(object sender, MouseEventArgs e)
        {
            label1.Text = e.Delta.ToString();
        }
    }
}
