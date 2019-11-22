using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;



namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        #region Definitions
        //Constants for API Calls...
        private const int WM_DRAWCLIPBOARD = 0x308;
        private const int WM_CHANGECBCHAIN = 0x30D;

        //Handle for next clipboard viewer...
        private IntPtr mNextClipBoardViewerHWnd;

        //API declarations...
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool ChangeClipboardChain(IntPtr HWnd, IntPtr HWndNext);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]

 
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        #endregion

        IntPtr nextClipboardViewer;
        public Form1()
        {
            InitializeComponent();
            nextClipboardViewer = (IntPtr)SetClipboardViewer( (IntPtr)this.Handle );
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public enum ShowWindowCommands : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,    //用最近的大小和位置显示，激活
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }
        [DllImport("shell32.dll")]
        public static extern IntPtr ShellExecute(
        IntPtr hwnd,
        string lpszOp,
        string lpszFile,
        string lpszParams,
        string lpszDir,
        ShowWindowCommands FsShowCmd
        );

        private void button1_Click(object sender, EventArgs e)
        {
            AddUrl(textBox2.Text.ToString());
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //ShellExecute(IntPtr.Zero, "open", "cmd",@"/c ping www.126.com -t", null, ShowWindowCommands.SW_SHOWNORMAL);
            string ss = listBox1.Text;
            if (ss.Length == 0) return;
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string proxystr = " --proxy 127.0.0.1:1080";
            string url = textBox2.Text.ToString();
            if (url.Length == 0) return;
            if (checkBox1.Checked)
            {
                if (url.Contains(proxystr)) return;
                textBox2.Text += proxystr;
            }
            else
            {
                textBox2.Text = textBox2.Text.Replace(proxystr, "");
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox2.Text = listBox1.Text;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string ss = listBox1.Text;
            if (ss.Length == 0) return;
            ShellExecute(IntPtr.Zero, "open", "cmd", "/c " + ss, null, ShowWindowCommands.SW_SHOWNORMAL);
        }
        static string urlfilename = Application.StartupPath + "\\urldata.txt";
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FileStream fs = new FileStream(urlfilename, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);  //保存文本的位置在Debug文件夹下
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                sw.Write(listBox1.Items[i]);
                sw.WriteLine();
            }

            sw.Close();

        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            string str;
            if (!File.Exists(urlfilename)) return;
        

            StreamReader sr = new StreamReader(urlfilename, false);
            while((str = sr.ReadLine()) != null)
            {
                listBox1.Items.Add(str);
            }
            sr.Close();
        }
        #region Message Process
        //Override WndProc to get messages...
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    {
                        //The clipboard has changed...
                        //##########################################################################
                        // Process Clipboard Here :)........................
                        //##########################################################################
                        SendMessage(mNextClipBoardViewerHWnd, m.Msg, m.WParam.ToInt32(), m.LParam.ToInt32());

                        //显示剪贴板中的文本信息
                        if (Clipboard.ContainsText())
                        {
                            string ss = Clipboard.GetText();
                            if (!ss.Contains("http")) return;
                            textBox1.Text = ss;
                          //  MessageBox.Show("ddd");
                        }
                        //显示剪贴板中的图片信息
                        if (Clipboard.ContainsImage())
                        {
         
                        }
                        break;
                    }
                case WM_CHANGECBCHAIN:
                    {
                        //Another clipboard viewer has removed itself...
                        if (m.WParam == (IntPtr)mNextClipBoardViewerHWnd)
                        {
                            mNextClipBoardViewerHWnd = m.LParam;
                        }
                        else
                        {
                            SendMessage(mNextClipBoardViewerHWnd, m.Msg, m.WParam.ToInt32(), m.LParam.ToInt32());
                        }
                        break;
                    }
            }
            base.WndProc(ref m);
        }
        #endregion

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string url = textBox1.Text.ToString();
            if (url.Length == 0) return;
            if (!url.Contains("http")) return;
            string str = "youtube-dl -f best " + url;

            if (checkBox1.Checked) str += " --proxy 127.0.0.1:1080";
            textBox2.Text = str;

            AddUrl(str);
        }
        private bool IsUrlValue(string url)
        {
            if (url.Length == 0) return false;
            if (!url.Contains("http")) return false;
            if (url.Contains("youtube")) return true;
            if (url.Contains("chaturbate")) return true;
            if (url.Contains("pornhub")) return true;
            return false;
        }
        private void AddUrl(string url)
        {

            if (!IsUrlValue(url)) return;

            if (!listBox1.Items.Contains(url))
            {
                listBox1.Items.Add(url);
                ShellExecute(IntPtr.Zero, "open", "cmd", "/c " + url, null, ShowWindowCommands.SW_SHOWNORMAL);
            }

            listBox1.SetSelected(listBox1.FindString(url), true);
        }
    }
}
