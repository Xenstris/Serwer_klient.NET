using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Globalization;


namespace Serwer
{
    public partial class Form1 : Form
    {
        public Thread listenThread;
        public List<Socket> lista = new List<Socket>();
        public List<User> lista_user = new List<User>();
        public Socket localListeningSocket;
      




        public Form1()
        {
            InitializeComponent();

            listenThread = new Thread(StartServer);
            listenThread.Start(this);

            using (StreamReader sr = new StreamReader("Baza danych.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!(line == ""))
                    {
                        string[] red = line.Split('|');
                        string result_login = red[0];

                        listBox2.Items.Add(result_login);
                    }
                }
            }



        }

        public void StartServer(Object form)
        {
            RichTextBox board = ((Form1)form).richTextBox1;

            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress iPAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, 50000);

            ((Form1)form).localListeningSocket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ((Form1)form).localListeningSocket.Bind(localEndPoint);
            ((Form1)form).localListeningSocket.Listen(10);

            
            while (true)
            {

                

                User nowy_uzytkownik = new User();
                nowy_uzytkownik.add_socket(((Form1)form).localListeningSocket.Accept());
                lista_user.Add(nowy_uzytkownik);


                Thread Goorge = new Thread(() => new_connection(form, nowy_uzytkownik, board));
                nowy_uzytkownik.add_thread(Goorge);
                Goorge.Start();

                


            }
        }

        

        public void send_to_all(string data)
        {



            data = data + Environment.NewLine;


            byte[] wiadomosc = Encoding.ASCII.GetBytes(data);

            foreach (User element in lista_user)
            {
                if(element.my_logged() == true)
                {
                    element.my_socket().Send(wiadomosc);
                }
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void new_connection(Object form, User uzytkownik, RichTextBox board)
        {
            string login = "";
            string password;
            string data;


            


            while (true)
            {
                


                byte[] bytes = null;

                
                

                bytes = new byte[1024];
                int bytesRec = uzytkownik.my_socket().Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);



                



                string[] cut = data.Split(' ');

                if (data.IndexOf("<DC>") > -1)
                {
                    data = uzytkownik.my_login() + " has disconnected";
                    break;
                }
                if (data.IndexOf("<BOX>") > -1)
                {
                    using (StreamReader sr = new StreamReader("Skrzynka/" + uzytkownik.my_login() + ".txt"))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            line = line + Environment.NewLine;
                            byte[] mess = Encoding.ASCII.GetBytes(line);
                            uzytkownik.my_socket().Send(mess);
                        }

                        
                    }

                    File.WriteAllText("Skrzynka/" + uzytkownik.my_login() + ".txt", String.Empty);

                }

                string firstWord = cut[0];
                

                //send_to_all(data);
                //send_to_all(firstWord);
                

                switch (firstWord)
                {
                    case "/all":
                        string str = data;
                        string result = string.Join(" ", str.Split().Skip(1));
                        data = login + ": " + result;
                        data = check_bad_word(data);

                        send_to_all(data);

                        board.Text += data + Environment.NewLine; //  nie musi byc / tylko do pokazania  działania 
                        break;

                    case "/register":
                        bool free = true;
                        
                        login = cut[1];

                        bool bad_word = login_bad_word(login);

                        password = cut[2];

                        if (bad_word)
                        {
                            byte[] me = Encoding.ASCII.GetBytes("<BAD_WORD>");
                            uzytkownik.my_socket().Send(me);
                        }
                        else
                        {
                            if (login != "" && password != "")
                            {

                                using (StreamReader sr = new StreamReader("Baza danych.txt"))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        if (!(line == ""))
                                        {
                                            string[] red = line.Split('|');
                                            string result_login = red[0];
                                            if (login == result_login)
                                            {
                                                free = false;
                                                byte[] mess = Encoding.ASCII.GetBytes("<BUSY>");
                                                uzytkownik.my_socket().Send(mess);
                                            }
                                        }
                                    }
                                }
                                if (free)
                                {
                                    using (StreamWriter save = new StreamWriter("Baza danych.txt", true))
                                    {
                                        save.WriteLine(login + "|" + password);
                                    }
                                    byte[] mes = Encoding.ASCII.GetBytes("<LOG>");
                                    uzytkownik.my_socket().Send(mes);

                                    string nazwaa = "Skrzynka/" + login + ".txt";

                                    using (StreamWriter save = new StreamWriter(nazwaa, true))
                                    {
                                        
                                    }
                                    
                                }
                            }
                            else
                            {
                                byte[] mess = Encoding.ASCII.GetBytes("<PLANE>");
                                uzytkownik.my_socket().Send(mess);
                            }
                        }

                        
                        break;

                    case "/login":
                        bool jest = false;
                        bool correct = false;

                        using (StreamReader sr = new StreamReader("Baza danych.txt"))
                        {
                            login = cut[1];
                            string check_password = cut[2];

                            foreach (User element in lista_user)
                            {
                                if(element.my_login() == login && element.my_logged() == true)
                                {
                                    jest = true;
                                }
                            }

                            if(jest == false)
                            {

                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    if (!(line == ""))
                                    {

                                        string[] red = line.Split('|');
                                        string result_login = red[0];
                                        string result_password = red[1];

                                        if (login == result_login && check_password == result_password)
                                        {
                                            correct = true;


                                        }
                                    }

                                }
                            }
                            else
                            {
                                byte[] messs = Encoding.ASCII.GetBytes("<ALREADY_LOG>");
                                uzytkownik.my_socket().Send(messs);
                                correct = true;
                               
                            }
                        }
                        if(correct)
                        {
                            data = login + " conntected to server :)";
                            send_to_all(data);

                            uzytkownik.add_login(login);
                            uzytkownik.logged(true);
                            listBox2.Items.Remove(uzytkownik.my_login());
                            listBox1.Items.Add(uzytkownik.my_login());

                            byte[] mess = Encoding.ASCII.GetBytes("<START>");
                            uzytkownik.my_socket().Send(mess);




                            string nazwaa = "Skrzynka/" + login + ".txt";



                            bool boxowa = false;



                            using(StreamReader srr = new StreamReader(nazwaa))
                            {
                                while(srr.ReadToEnd() != String.Empty)
                                {
                                    boxowa = true;
                                }
                            }


                            if (boxowa)
                            {
                                mess = Encoding.ASCII.GetBytes("<BOX>");
                                uzytkownik.my_socket().Send(mess);
                            }

                            
                        }

                        if (correct == false)
                        {
                            byte[] messs = Encoding.ASCII.GetBytes("<BAD_PASS>");
                            uzytkownik.my_socket().Send(messs);

                        }

                        break;
                        

                    case "/pm":
                        string name = cut[1];
                        string strv = data;

                        bool online = false;

                        string resultt = string.Join(" ", strv.Split().Skip(2));

                        data = "Prywatna wiadomosc od " + login + ": " + resultt;

                        data = check_bad_word(data) + Environment.NewLine;



                        foreach(User element in lista_user)
                        {
                            if(name == element.my_login() && element.my_logged() == true)
                            {
                                byte[] priv = Encoding.ASCII.GetBytes(data);
                                element.my_socket().Send(priv);
                                online = true;
                            }
                            
                        }

                        if(online == false)
                        {
                            string data2 = "Prywatna wiadomosc od " + login + ": " + resultt;

                            string nazwa = "Skrzynka/" + name + ".txt";

                            data2 = check_bad_word(data2);

                            using (StreamWriter save = new StreamWriter(nazwa, true))
                            {
                                save.WriteLine(data2);
                            }


                        }

                        

                        break;






                }

                


            }

            send_to_all(data);

            data = "<DC>";

            uzytkownik.logged(false);

            byte[] wiadomosc = Encoding.ASCII.GetBytes(data);
            uzytkownik.my_socket().Send(wiadomosc);   //wysyłamy wiadomość do klienta, żeby go rozłączyło

            
            listBox1.Items.Remove(uzytkownik.my_login());

            listBox2.Items.Add(uzytkownik.my_login());

            lista_user.Remove(uzytkownik);

            uzytkownik.my_socket().Shutdown(SocketShutdown.Both);

            uzytkownik.my_thread().Abort();

            uzytkownik.my_socket().Close();

            


        }



        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] end = Encoding.ASCII.GetBytes("<SERV_OUT>");
            foreach(User element in lista_user)
            {
                element.my_socket().Send(end);
            }
            
        }
        public bool login_bad_word(string login)
        {
            bool jest = false;

            using (StreamReader sr = new StreamReader("Bad_word.txt"))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    if(!(line==""))
                    {
                        if(line == login)
                        {
                            jest = true;
                        }
                    }
                }
            }

            return jest;
        }

        public string check_bad_word(string a)
        {
            string[] cut = a.Split(' ');

            int size = cut.Length;

            using (StreamReader sr = new StreamReader("Bad_word.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if(!(line==""))
                    {
                        for(int i=0; i<size; i++)
                        {
                            if(line == cut[i])
                            {
                                cut[i] = "";
                                int length = line.Length;
                                for(int j=0; j<length; j++)
                                {
                                    cut[i] += "*";
                                }
                            }
                        }
                    }
                }
            }

            a = "";

            for(int i=0; i<size; i++)
            {
                a += cut[i] + " ";
            }

            return a;
        }

        
    }
}
