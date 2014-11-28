using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
 

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        public Socket ScreenClient = null, MouseClient = null, KeyClient = null;
        public Socket[] server = new Socket[3];
        public Thread ST, MT, KT, WT;
        public Thread ScreenThread, MouseThread, KeyThread;
        public Form1()
        {
            InitializeComponent();
        }

        void MouseThreadF()
        {
            bool error = false;
            while (!error)
            {
                byte[] buffer = new byte[1024];
                try
                {
                    MouseClient.Receive(buffer);
                }
                catch (SocketException ex)
                {
                }
                string mes = System.Text.Encoding.UTF8.GetString(buffer);
                //MessageBox.Show(mes);
                if (mes.Contains("move"))
                {
                    //move mouse
                    string[] arr = mes.Split(' ');
                    float x = Convert.ToSingle(arr[1]);
                    float y = Convert.ToSingle(arr[2]);
                    Cursor.Position = new Point((int)(x * Screen.PrimaryScreen.Bounds.Width),(int)( y * Screen.PrimaryScreen.Bounds.Height));
                    /*
                    {
                        if (this.InvokeRequired)
                            BeginInvoke(new MethodInvoker(delegate
                            {
                                richTextBox1.AppendText(x.ToString() + " " + y.ToString()+"\r\n");
                            }));
                        else
                        {
                            richTextBox1.AppendText(x.ToString() + " " + y.ToString() + "\r\n");
                        }
                    }
                    catch (Exception ex1)
                    {
                        MessageBox.Show(ex1.Message);
                    }     */             
                }
            }
        }

        void ScreenThreadF()
        {
            bool error = false;
            while (!error)
            {
                //make screenshot and send;

                Graphics gr;
                Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height,
                    PixelFormat.Format32bppRgb);
                gr = Graphics.FromImage(bmp);
                gr.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                    0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                Rectangle cr = new Rectangle(0, 0, bmp.Width / 2, bmp.Height / 2);
                Bitmap nbmp = new Bitmap(cr.Width, cr.Height, PixelFormat.Format32bppRgb);
                gr = Graphics.FromImage(nbmp);
                gr.InterpolationMode = InterpolationMode.Low;
                gr.DrawImage(bmp, cr, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel);
                ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Compression;
                System.Drawing.Imaging.Encoder myEncoder1 = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(2);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 0L);
                EncoderParameter myEncoderParameter1 = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                myEncoderParameters.Param[1] = myEncoderParameter1;

                byte[] byteArray = new byte[0];
                using (MemoryStream stream = new MemoryStream())
                {
                    nbmp.Save(stream, jgpEncoder, myEncoderParameters);
                    stream.Position = 0;

                    byteArray = stream.ToArray();
                    stream.Close();
                }
                try
                {
                    string header = "size " + byteArray.Length.ToString();
                    ScreenClient.SendBufferSize = header.Length;
                    ScreenClient.Send(Encoding.UTF8.GetBytes(header));
                    ScreenClient.SendBufferSize = 8192;
                    ScreenClient.Send(byteArray);
                }
                catch (SocketException ex)
                {
                    if ((ex.SocketErrorCode != SocketError.TimedOut)&&
                        (ex.SocketErrorCode != SocketError.WouldBlock))
                    {
                         MessageBox.Show("ScreenThread failed");
                         try
                         {
                             if (this.InvokeRequired)
                                 BeginInvoke(new MethodInvoker(delegate
                                 {
                                     button3.PerformClick();
                                 }));
                             else
                             {
                                 button3.PerformClick();
                             }
                         }
                         catch (Exception ex1)
                         {
                             MessageBox.Show(ex1.Message);
                         }
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
            ScreenThread = new Thread(ScreenThreadF);
            ScreenThread.Name = "ScreenThread";
            ScreenThread.Start();

            MouseThread = new Thread(MouseThreadF);
            MouseThread.Name = "MouseThread";
            MouseThread.Start();

        }

        void ServerSocketThread(Object port_)
        {
            int port = (int)port_;
            IPEndPoint point;
            point = new IPEndPoint(IPAddress.Any, port);
            server[port-4001] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            server[port-4001].Bind(point);
            server[port-4001].Listen(1);
            switch ((int)port)
            {
                case 4001:
                    try
                    {
                        ScreenClient = server[port - 4001].Accept();
                    }
                    catch (SocketException ex)
                    {
                    }
                    break;
                case 4002:
                    try
                    {
                        MouseClient = server[port - 4001].Accept();
                    }
                    catch (SocketException ex)
                    {
                    }
                    break;
                case 4003:
                    KeyClient = server[port - 4001].Accept();
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
            MT.Start(4002);

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
            if (server[0] != null)
                server[0].Close();
            if (ScreenClient != null)
                ScreenClient.Close();            
            if (ST != null)
                ST.Abort();
            if (server[1] != null)
                server[1].Close();
            if (MouseClient != null)
                MouseClient.Close();
            if (MT != null)
                MT.Abort();
            if (server[2] != null)
                server[2].Close();
            if (KT != null)
                KT.Abort();
            if (ScreenThread != null)
                ScreenThread.Abort();
            if (MouseThread != null)
                MouseThread.Abort();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (WT != null)
                WT.Abort();
            if (server[0] != null)
                server[0].Close();
            if (ScreenClient != null)
                ScreenClient.Close();
            if (ST != null)
                ST.Abort();
            if (server[1] != null)
                server[1].Close();
            if (MouseClient != null)
                MouseClient.Close();
            if (MT != null)
                MT.Abort();
            if (server[2] != null)
                server[2].Close();
            if (KT != null)
                KT.Abort();
            if (ScreenThread != null)
                ScreenThread.Abort();
            if (MouseThread != null)
                MouseThread.Abort();
            Application.Exit();
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }


    }
}
