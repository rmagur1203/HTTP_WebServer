using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCP_Listener;

namespace TCP_Listener
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public TcpListener tcp;

        public int port;
        public string[] richline;
        public string richtext;
        public int clientlength = 0;
        public string path;

        delegate void CrossThreadSafetySetText(Control ctl, String text);

        private void CSafeSetText(Control ctl, String text)
        {

            /*
            * InvokeRequired 속성 (Control.InvokeRequired, MSDN)
            *   짧게 말해서, 이 컨트롤이 만들어진 스레드와 현재의 스레드가 달라서
            *   컨트롤에서 스레드를 만들어야 하는지를 나타내는 속성입니다.  
            * 
            * InvokeRequired 속성의 값이 참이면, 컨트롤에서 스레드를 만들어 텍스트를 변경하고,
            * 그렇지 않은 경우에는 그냥 변경해도 아무 오류가 없기 때문에 텍스트를 변경합니다.
            */
            if (ctl.InvokeRequired)
                ctl.Invoke(new CrossThreadSafetySetText(CSafeSetText), ctl, text);
            else
                ctl.Text = text;
        }

        /*private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.html = new System.Collections.Specialized.StringCollection();
            foreach (string line in richTextBox1.Lines)
            {
                Properties.Settings.Default.html.Add(line);
            }
            Properties.Settings.Default.Save();

            try
            {
                tcp.Stop();
            }
            catch (Exception ex) { }

            Thread tcpserver = new Thread(tcp_server);
            tcpserver.Start();

            richline = richTextBox1.Lines;
            richtext = richTextBox1.Text;
            path = textBox1.Text;

            status.Text = "Status : Server Open";
        }*/

        public void tcp_server()
        {
            tcp = new TcpListener(port);
            tcp.Start();

            try
            {
                while (true)
                {
                    TcpClient tc = tcp.AcceptTcpClient();

                    NetworkStream cs = tc.GetStream();
                    StreamReader cr = new StreamReader(cs);
                    StreamWriter cw = new StreamWriter(cs);

                    //MessageBox.Show(cr.ReadLine());

                    string cpath = cr.ReadLine().Split(new[] { "GET " }, StringSplitOptions.None)[1].Split(new[] { " HTTP" }, StringSplitOptions.None)[0];

                    if (cpath == path)
                    {
                        clientlength += 1;

                        CSafeSetText(status, "Status : Client Connected");
                        CSafeSetText(clientlen, "접속자 수 : " + clientlength);

                        cw.WriteLine("HTTP/1.1 200 OK");
                        //cw.WriteLine("Content-Length: " + Encoding.UTF8.GetBytes(richtext).Length);
                        //cw.WriteLine("Content-Length: " + Encoding.Default.GetByteCount(richtext));
                        cw.WriteLine("Connection: close");
                        cw.WriteLine("Content-Type: text/html; charset=utf-8");
                        cw.WriteLine("");

                        foreach (string line in richline)
                        {
                            cw.WriteLine(line);
                        }
                        cw.Flush();
                    }
                    else
                    {
                        /*cw.WriteLine("HTTP/1.1 404 Not Found");
                        cw.WriteLine("");
                        cw.Flush();*/

                        cw.WriteLine("HTTP/1.1 200 OK");
                        //cw.WriteLine("Content-Length: " + Encoding.Default.GetByteCount(richtext));
                        cw.WriteLine("Connection: close");
                        cw.WriteLine("Content-Type: text/html; charset=utf-8");
                        cw.WriteLine("");
                        cw.WriteLine("<html><body><h1>404 Not Found</h1><h2>HTTP ERROR 404</h2></body></html>");
                        cw.Flush();
                    }
                }
            }
            catch(Exception ex) { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Lines = Properties.Settings.Default.html.Cast<string>().ToArray();

            textBox1.Text = Properties.Settings.Default.path;

            textBox2.Text = WebGetText("https://ip.pe.kr/").Split(new[] { "<h1 class=\"cover-heading\">" }, StringSplitOptions.None)[1].Split(new[] { "</h1>" }, StringSplitOptions.None)[0];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.html = new System.Collections.Specialized.StringCollection();
            foreach (string line in richTextBox1.Lines)
            {
                Properties.Settings.Default.html.Add(line);
            }
            Properties.Settings.Default.Save();

            try
            {
                if (tcp != null)
                    tcp.Stop();
            }
            catch (Exception) { }

            clientlength = 0;
            CSafeSetText(clientlen, "접속자 수 : " + clientlength);

            port = int.Parse(textBox3.Text);

            Thread tcpserver = new Thread(tcp_server);
            tcpserver.Start();

            richline = richTextBox1.Lines;
            richtext = richTextBox1.Text;
            path = textBox1.Text;

            status.Text = "Status : Server Open";
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            try
            {
                tcp.Stop();
            }
            catch (Exception) { }

            Properties.Settings.Default.html = new System.Collections.Specialized.StringCollection();
            foreach (string line in richTextBox1.Lines)
            {
                Properties.Settings.Default.html.Add(line);
            }

            Properties.Settings.Default.path = textBox1.Text;

            Properties.Settings.Default.Save();
        }

        private string WebGetText(string url)
        {
            try
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                System.Net.WebResponse response = request.GetResponse();

                System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

                return sr.ReadToEnd();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
