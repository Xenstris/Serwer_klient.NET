using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.IO;

namespace Serwer
{
    public class User
    {

        List<string> skrzynka = new List<string>();
        bool skrzynka_empty = false;
        Socket socket;
        string login;
        bool logged_in = false;
        Thread Fred;

        public void add_skrzynka(string txt)
        {
            skrzynka.Add(txt);
        }

        public void check_skrzynka(bool t)
        {
            skrzynka_empty = t;
        }

        public void add_socket(Socket s)
        {
            socket = s;
        }

        public void add_login(string l)
        {
            login = l;
        }

        public void logged(bool f)
        {
            logged_in = f;
        }

        public void add_thread(Thread Th)
        {
            Fred = Th;
        }

        public string my_login()
        {
            return login;
        }

        public Socket my_socket()
        {
            return socket;
        }

        public bool my_logged()
        {
            return logged_in;
        }

        public Thread my_thread()
        {
            return Fred;
        }

        public List<string> my_skrzynka()
        {
            return skrzynka;
        }

        public bool checked_skrzynka()
        {
            return skrzynka_empty;
        }

        

    }
}
