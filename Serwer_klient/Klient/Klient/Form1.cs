using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Klient
{
    public partial class Form1 : Form
    {
        Socket localClientSocket;
        Thread myTh;

        public Form1()
        {

            InitializeComponent();
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0]; //localhost
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 50000);

            // Creat a TCP/IP socket

            localClientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            localClientSocket.Connect(remoteEP);

            richTextBox1.Text = "Socket connected to {0}" + localClientSocket.RemoteEndPoint.ToString();

            myTh = new Thread(listen);
            myTh.Start(this);

            



        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        // Release the socket.    
        //senderSocket.Shutdown(SocketShutdown.Both);
        //   senderSocket.Close();


        bool check = false;

        public void listen(Object form)
        {
            RichTextBox board = ((Form1)form).richTextBox1;
            string data = null;
            byte[] bytes = null;

            



            while (true)
            {
               

                
                
                bytes = new byte[1024];
                int bytesRec = ((Form1)form).localClientSocket.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                

                
                

                if (data.IndexOf("<LOG>") > -1)
                {
                    MessageBox.Show("Dziękuje za rejestrację, możesz przystąpić do logowania");
                    data = "";
                }

                if (data.IndexOf("<START>") > -1)
                {
                    MessageBox.Show("Logowanie pomyślne");
                    

                    button2.Enabled = false;

                    data = "You have connected to the server" + Environment.NewLine;
                   
                    check = true;
                }

                if (data.IndexOf("<BUSY>") > -1)
                {
                    MessageBox.Show("Nazwa użytkownika zajęta!");
                    data = "";
                }

                if (data.IndexOf("<BAD_PASS>") > -1)
                {
                    MessageBox.Show("Hasło lub nazwa użytkownika są niepoprawne");
                    data = "";
                }

                if (data.IndexOf("<PLANE>") > -1)
                {
                    MessageBox.Show("Nie podano loginu lub hasła!");
                    data = "";
                }

                if (data.IndexOf("<ALREADY_LOG>") > -1)
                {
                    MessageBox.Show("To konto jest już zalogowane!");
                    data = "";
                }

                if (data.IndexOf("<SERV_OUT>") > -1)
                {
                    board.Text = Environment.NewLine + "Serwer zakończył swoją pracę, klient zostanie zamknięty za 2 sekundy";
                    Thread.Sleep(2000);
                    break;
                }

                if (data.IndexOf("<DC>") > -1)
                {
                    board.Text = Environment.NewLine + "Bywaj przyjacielu";
                    MessageBox.Show("Zostaniesz rozłączony za 2 sekundy");
                    Thread.Sleep(2000);

                    break;
                }
                if (data.IndexOf("<BOX>") > -1)
                {
                    
                    button4.Enabled = true;
                    data = "W skrzynce czeka na Ciebie wiadomość" + Environment.NewLine;
                }
                if (data.IndexOf("<BAD_WORD>") > -1)
                {
                    MessageBox.Show("Login nie może zawierać przekleństw!");
                    data = "";
                }


                board.Text = data;

            }

           



            Close();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (check)
            {
                try
                {
                    byte[] bytes = new byte[1024];
                    // Encode the data string into a byte array.    
                    byte[] msg = Encoding.ASCII.GetBytes(textBox1.Text);
                    int bytesSent = localClientSocket.Send(msg);

                    textBox1.Text = "";
                }
                catch
                {
                    MessageBox.Show("Smth went wrong");
                }
            }
            else
            {
                MessageBox.Show("ZALOGUJ SIĘ ABY WYSŁAĆ WIADOMOŚĆ!");
            }
            


        }


        string przycisk;

        
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] bytes = new byte[1024];
                // Encode the data string into a byte array.
                string password = textBox3.Text;
                password = haszowanie(password);
                string info = przycisk + " " + textBox2.Text + " " + password;
                byte[] msg = Encoding.ASCII.GetBytes(info);
                int bytesSent = localClientSocket.Send(msg);

            }
            catch
            {
                MessageBox.Show("Smth went wrong");
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            button2.Text = "Register";
            przycisk = "/register";

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            button2.Text = "Log in";
            przycisk = "/login";

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] msg = Encoding.ASCII.GetBytes("<DC>");
            int bytesSent = localClientSocket.Send(msg);
        }

        public string haszowanie(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                var result = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    result.Append(data[i].ToString("x2"));
                }

                return result.ToString();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] msg = Encoding.ASCII.GetBytes("<BOX>");
            int bytesSent = localClientSocket.Send(msg);

            button4.Enabled = false;
        }
    }
}
