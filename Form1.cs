using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
//using System.Net.WebSockets;

using Newtonsoft;

namespace Ladota_Lobbies
{
    public partial class Form1 : Form
    {
        const string LOBBIES_API_URL = "https://eurobattle.net/api/la/live/lobbies";
        const string STATS_API_URL = "https://eurobattle.net/new/api/la/stats?search={0}";
        const int QUERY_TIMEOUT = 2000;
        public class Lobby
        {
            public string dotaVariant { get; set; }
            public string usedSlots { get; set; }
            public string maxSlots { get; set; }
            public string gn { get; set; }
            public List<string> players { get; set; }
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Stats
        {
            public string name { get; set; }
            public int rating { get; set; }
            public int highest_rating { get; set; }
            public int games { get; set; }
            public int wins { get; set; }
            public int loses { get; set; }
            public int kills { get; set; }
            public int deaths { get; set; }
            public int assists { get; set; }
            public int creepkills { get; set; }
            public int creepdenies { get; set; }
            public int neutralkills { get; set; }
            public int towerkills { get; set; }
            public int raxkills { get; set; }
            public int courierkills { get; set; }
            public string winp { get; set; }
            public int rank { get; set; }
        }


        List<Lobby> ls = new List<Lobby> { };
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DisplayLobbies(ls = GetLobbies());
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != System.Windows.Forms.CloseReason.WindowsShutDown && e.CloseReason != System.Windows.Forms.CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            DisplayLobbies(ls = GetLobbies());
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var lv = (ListView)sender;
                    //MessageBox.Show("checking " + lv.SelectedItems[0]);
                    string playerName = lv.SelectedItems[0].SubItems[1].Text;
                    var playerStats = GetStats(playerName);
                    if (playerStats.Count > 0)
                    {
                        lv.SelectedItems[0].SubItems[2].Text = playerStats[0].rating.ToString();
                        lv.SelectedItems[0].SubItems[3].Text = playerStats[0].games.ToString();
                        lv.SelectedItems[0].SubItems[4].Text = playerStats[0].winp;
                        lv.SelectedItems[0].SubItems[5].Text = playerStats[0].wins.ToString() + "/" + playerStats[0].loses.ToString();
                        lv.SelectedItems[0].SubItems[6].Text = string.Format("{0}-{1}-{2}", ((float)playerStats[0].kills / (playerStats[0].wins+playerStats[0].loses)).ToString("0.00"), (playerStats[0].deaths / (playerStats[0].wins + playerStats[0].loses)).ToString("0.00"), (playerStats[0].assists / (playerStats[0].wins + playerStats[0].loses)).ToString("0.00"));
                        lv.SelectedItems[0].SubItems[7].Text = string.Format("{0}-{1}-{2}", playerStats[0].creepkills.ToString(), playerStats[0].creepdenies.ToString(), playerStats[0].neutralkills.ToString());
                        lv.SelectedItems[0].SubItems[8].Text = playerStats[0].rank.ToString();
                        // MessageBox.Show(playerStats[0].name + " " + playerStats[0].rating);
                    }
                    else
                    {
                        for (int i = 1; i <= 8; i++)
                        {
                            lv.SelectedItems[0].SubItems[i].Text = "";
                        }
                        MessageBox.Show("Stats unavailable");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + "   " + ex.StackTrace.ToString());
            }

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.BringToFront();
            this.Select();

        }



        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //-------------------------------------------------------------------------------
        void DisplayLobbies(List<Lobby> lobbies)
        {
            listView1.Items.Clear();
            listView1.Groups.Clear();
            if (lobbies.Count > 0)
            {
                labelGamesCount.Text = String.Format("There are {0} lobbies", lobbies.Count);
                for (int i = 0; i < lobbies.Count; i++)
                {
                    ListViewGroup lvGroup = new ListViewGroup() { Header = String.Format("{0}- {1} [{2}/{3}] {4}", i + 1, lobbies[i].gn, lobbies[i].usedSlots, lobbies[i].maxSlots, lobbies[i].dotaVariant) };
                    listView1.Groups.Add(lvGroup);
                    for (int i2 = 0; i2 < lobbies[i].players.Count; i2++)
                    {
                        ListViewItem lvItem = new ListViewItem() { Text = (i2 + 1).ToString(), Group = lvGroup };
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text= lobbies[i].players[i2]});
                        for(int i3 = 1; i3<= 7; i3++)
                        {
                            lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = "" });
                        }
                        listView1.Items.Add(lvItem);
                    }
                }
            }
            else
                labelGamesCount.Text = String.Format("There are no open lobbies", lobbies.Count);
        }



        //-------------------------------------------------------------------------------
        public static string GetRemoteContent(string url, int timeout)
        {
            string res = "";
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = timeout;
                WebResponse response = request.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());
                res = sr.ReadToEnd();
                sr.Close();
            }
            catch (WebException ex) { }

            return res;
        }

       
        public static List<Lobby> GetLobbies()
        {
            try
            {
                var recv = GetRemoteContent(LOBBIES_API_URL, QUERY_TIMEOUT);
                if (recv == null) recv = "";
                List<Lobby> result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Lobby>>(recv);
                if (result == null) result = new List<Lobby> { };
                return result;
            }
            catch (System.Net.WebException ex)
            {
                return new List<Lobby> { };
            }
        }
        public static List<Stats> GetStats(string playerName)
        {
            try
            {
                var recv = GetRemoteContent(string.Format(STATS_API_URL, playerName), QUERY_TIMEOUT * 2);
                if (recv == null) recv = "";
                List<Stats> result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stats>>(recv);
                if (result == null) result = new List<Stats> { };
                return result;
            }
            catch (System.Net.WebException ex)
            {
                return new List<Stats> { };
            }
        }


    }
}
