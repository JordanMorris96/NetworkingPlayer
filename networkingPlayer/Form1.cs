﻿using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

/*
        * ==========================================
        *               Notes for me!
        * ==========================================
        * 
        * Currently the client reads the "pause" but it is being read by worker 1.
        */

namespace networkingPlayer
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        public string receiveText;
        public string TextToSend;
        public string sendCommand;
        public string receiveCommand;

        public Form1()
        {
            InitializeComponent();

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());
            foreach(IPAddress address in localIP)
            {
                if(address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerIPtextBox.Text = address.ToString();
                }
            }
        }
        //start server code
        private void btnStart_Click(object sender, EventArgs e)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(ServerPorttextBox.Text));
            listener.Start();
            client = listener.AcceptTcpClient();
            STR = new StreamReader(client.GetStream());
            STW = new StreamWriter(client.GetStream());
            STW.AutoFlush = true; //Ask prof about this one.
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.WorkerSupportsCancellation = true;
        }

        //Connect to host code below.
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(ClientIPtextBox.Text), int.Parse(clientPorttextBox.Text));
            try
            {
                client.Connect(IpEnd);
                if (client.Connected)
                {
                    ChatScreentextbox.AppendText("Connected to Server" + "\n");
                    STR = new StreamReader(client.GetStream());
                    STW = new StreamWriter(client.GetStream());
                    STW.AutoFlush = true;
                    backgroundWorker1.RunWorkerAsync();
                    backgroundWorker2.WorkerSupportsCancellation = true;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while(client.Connected)
            {
                try
                {
                    receiveText = STR.ReadLine();
                    this.ChatScreentextbox.Invoke(new MethodInvoker(delegate ()
                    {
                        ChatScreentextbox.AppendText("Partner: " + receiveText + "\n");
                    }));
                    receiveText = "";
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if(client.Connected)
            {
                STW.WriteLine (TextToSend);
                this.ChatScreentextbox.Invoke(new MethodInvoker(delegate()
                {
                    ChatScreentextbox.AppendText("Me:" + TextToSend + "\n");
                }));
            }
            else
            {
                MessageBox.Show("Sending failed");
            }
            backgroundWorker2.CancelAsync();
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                
                if (STR.ReadLine() == "pause")
                {

                    this.axWindowsMediaPlayer1.Invoke(new MethodInvoker(delegate ()
                    {
                        axWindowsMediaPlayer1.Ctlcontrols.pause();
                    }));
                    receiveCommand = " ";
                }
                else
                {
                    MessageBox.Show("Sending failed");
                }
            }
            
            backgroundWorker3.CancelAsync();
        }


        private void SendButton_Click(object sender, EventArgs e)
        {
            if(MessagetextBox.Text != "")
            {
                TextToSend = MessagetextBox.Text;
                backgroundWorker2.RunWorkerAsync();
            }
            MessagetextBox.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.startBox1.Text = openFileDialog1.FileName;
            }
        }
        
        //Below code executes button functionality.

        private void button2_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.URL = startBox1.Text;
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.pause();
            STW.WriteLine("pause");
            backgroundWorker3.RunWorkerAsync();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       



       
    }
}
