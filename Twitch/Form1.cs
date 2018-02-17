using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;
using System.Threading;

namespace Twitch
{
    public partial class Form1 : Form
    {
        TcpClient tcpclient;
        StreamReader reader;
        StreamWriter writer;
        string channel = "nizlmmk";
        int gameTimer = 60000;
        Question question = new Question();
        static string[] questionARR = File.ReadAllLines("questionsDB.txt");
        static List<string> questions = questionARR.ToList();
        static Random r = new Random();
        string[] split;
        bool wyrRunning = false;
        List<string> voted = new List<string>();
        string user;
        int index;

        public Form1()
        {
            InitializeComponent();
            Reconnect();
            timer2.Interval = gameTimer;

        }

        void timer1_Tick(object sender, EventArgs e)
        {
            label1.SelectionStart = label1.Text.Length;
            label1.ScrollToCaret();
            if (!tcpclient.Connected)
            {
                Reconnect();       
            }
           
            if(tcpclient.Available > 0 || reader.Peek() >= 0)
            {
                var message = reader.ReadLine();

                if (message.Contains("PING :tmi.twitch.tv"))
                {
                    writer.WriteLine("PONG :tmi.twitch.tv");
                    writer.Flush();
                }
                else if (message.Contains("PRIVMSG"))
                {
                    cleanMessage(message);
                }
                else
                {
                    label1.Text += message;
                }
                label1.Text += Environment.NewLine;
            }
        }
        public void cleanMessage(string message)
        {
            int removeIndex = message.IndexOf('#') - message.IndexOf('!');
            message = message.Remove(message.IndexOf('!'), removeIndex);
            message = message.Remove(message.IndexOf('#'), 8);
            user = message.Substring(0, (message.IndexOf(' ')) + 2);
            message = message.Remove(0, (message.IndexOf(' ')) + 2);
            user = user.Remove(user.IndexOf(':'), 1);
            user = user.Remove(user.IndexOf(':'), 1);
            user = user.Remove(user.IndexOf(' '), 1);
            label1.AppendText(user + ">");
            label1.AppendText(message);
            if (message.StartsWith("!"))
            {
                findCommand(message.ToLower());
            }
            if (true)
            {
                if (true)
                {
                    if (true)
                    {
                        if (true)
                        {
                            if (true)
                            {
                                if (true)
                                {
                                    if (true)
                                    {

                                    }

                                }

                            }
                        }
                    }
                }
            }
        }

        public void sendQuestion(int requested)
        {
            if( requested > 0 && requested <= (questions.Count()-1))
            {
                splitQ(requested);
            }
            else
            {
                splitQ(r.Next(questions.Count));
            }
            void splitQ(int index)
            {
                question.setIndex(index);             // set index of question external obj 
                sendChat($"Question number: {index}, !1 or !2 to vote!");
                if (Regex.IsMatch(questions[index], @"\sor\s"))
                {
                    split = Regex.Split(questions[index], @"\sor\s");
                    sendChat($"1) {split[0]}");
                    sendChat("OR");
                    sendChat($"2) {split[1]}");
                }
                else
                {
                    label1.AppendText($"question ) {index} didnt pass regex \\sor\\s");
                }
            }
        }

        public void displayResults()
        {
            double userP1, userP2;
            int total = question.getSelected1() + question.getSelected2();
            if(total > 0)
            {
                userP1 = (question.getSelected1() / total) * 100;
                userP2 = (question.getSelected2() / total) * 100;
                sendChat(Convert.ToString(question.getSelected1()) + " Users (" + userP1 + "%) selected" + " \"" + split[0].Substring(17) + "\"");
                sendChat(Convert.ToString(question.getSelected2()) + " Users (" + userP2 + "%) selected" + " \"" + split[1].Remove(split[1].Length - 1) + "\"");
            }
            else
            {
                sendChat(Convert.ToString(question.getSelected1()) + " Users selected" + " \"" + split[0].Substring(17) + "\"");
                sendChat(Convert.ToString(question.getSelected2()) + " Users selected" + " \"" + split[1].Remove(split[1].Length - 1) + "\"");
            }
            question.resetSelected();
                split[0] = null;
                split[1] = null;
        }

        private void findCommand(string message)
        {
            int exists = -1;
            exists = voted.IndexOf(user);
            int timer;
            if (message.StartsWith("!remove") && wyrRunning)
            {
                if (user.ToLower() == channel)
                {
                    remove(question.getIndex());
                    sendChat($"question {question.getIndex()} removed by {user}");
                }else
                {
                    sendWhisper(user, "Only the broadcaster can remove a question at this time");
                }
            }
           
            if (message.StartsWith("!1") && wyrRunning == true)
            {
                voted.Add(user);

                if (exists == -1)
                {                  
                    question.incSelected1();
                    label1.AppendText("inc 1");                 
                }
                else
                {
                    sendUser(user," You can only vote once per question!");
                }                
            }

            if (message.StartsWith("!2") && wyrRunning == true)
            {
                voted.Add(user);
                if (exists == -1)
                {
                    question.incSelected2(); 
                    label1.AppendText("inc 2");
                }
                else
                {
                    sendUser(user, " You can only vote once per question!");
                }
            }
        
            if (message.StartsWith("!wyr") || message.StartsWith("!wouldyouratherb0t") || message.StartsWith("!wouldyourather"))
            {
                if (message.Contains("help") || message.StartsWith("!wouldyouratherb0t") || message.StartsWith("!wouldyourather"))
                {
                    sendUser(user, "Thank you for using me! Visit http://www.smhax.com/wouldyourather/ for a full list of commands and to send feedback.");
                    sendUser(user, "!wyr [question number optional] - starts a new question, after " + gameTimer/1000 + " seconds the results will be displayed! If the number is not entered a random is selected");
                    sendUser(user, "!1 or !2 - votes for the corresponding selection");      
                    label1.AppendText("help executed for: " + user);
                }
                else if(message.StartsWith("!wyr timer") && int.TryParse(message.Remove(0,10), out timer))
                {
                    if (timer > 3 && timer < 300 && wyrRunning != true)
                    {
                        timer2.Interval = timer*1000;
                        sendUser(user, $"timer switched to {timer} seconds");                        
                    }
                    else
                    {
                        sendUser(user, "timer not changed: the timer should be between 3 and 300 seconds and the game cannot be running!");                      
                    }
                }
                else if(message.Contains("add") && user == channel)
                {
                    // !wyr add
                    string question = message.Substring(8, message.Length-8);
                    if(Regex.IsMatch(questions[index], @"\sor\s"))
                    {
                        File.AppendText(question);
                        Console.WriteLine($"question added number: {questions.Count}");
                    }

//split = Regex.Split(questions[index], @"\sor\s");


                }
                else if(wyrRunning != true && message.StartsWith("!wyr"))
                {
                    wyrRunning = true;
                    timer2.Enabled = true;
                    voted.Clear();
                    uint requested;
                    if (uint.TryParse(message.Remove(0,4), out requested))
                    {         
                        sendQuestion((int)requested);
                        label1.AppendText($"user {user} requested question number {requested} ");
                    }
                    else
                    {
                        sendQuestion(-1);
                    }
                }
            }
            else
            {
                label1.AppendText(message);
            }
        }

        void timer2_Tick(object sender, EventArgs e)
        {
            if (wyrRunning)
            {
                timer2.Stop();
                timer2.Start();
                
                displayResults();
                timer2.Enabled = false;
                wyrRunning = false;
            }
        }

        private void sendChat(string toChat)
        {
            writer.WriteLine(":nizlmmk!nizlmmk@nizlmmk.tmi.twitch.tv PRIVMSG #" + channel + " : " + toChat);
            writer.Flush();
        }

        private void sendUser(string user, string toChat)
        {   
            writer.WriteLine(":" + user + "!" + user + "@" + user + ".tmi.twitch.tv PRIVMSG #" + channel + " : " + "@" + user + " " + toChat);
            label1.AppendText(":" + user + "!" + user + "@" + user + ".tmi.twitch.tv PRIVMSG #" + channel + " : " + "@" + user + " " + toChat);
            writer.Flush();
        }
        private void sendWhisper(string user, string toChat)
        {

            writer.WriteLine(":" + user + "!" + user + "@" + user + ".tmi.twitch.tv PRIVMSG #" + channel + " :/w " + user + " " + toChat);
            label1.AppendText(":" + user + "!" + user + "@" + user + ".tmi.twitch.tv PRIVMSG #" + channel + " : " + "@" + user + " " + toChat);
            writer.Flush();
        }
        private void remove(int index)
        {
            label1.AppendText("removing question index" + index + "from questionsDB.txt");
            questions.RemoveAt(index);
            File.WriteAllLines("questionsDB.txt", questions);
        }

        private void Reconnect()
        {
            tcpclient = new TcpClient("irc.chat.twitch.tv", 6667);
            reader = new StreamReader(tcpclient.GetStream());
            writer = new StreamWriter(tcpclient.GetStream());;

            var userName = "wouldyouratherb0t";
            var nickName = "nizlBOT";
            var password = File.ReadAllText("password.txt");

            writer.WriteLine("PASS " + password + Environment.NewLine
                + "NICK " + nickName + Environment.NewLine
                + "USER " + userName + " 8 * :" + userName);
            writer.WriteLine("CAP REQ :twitch.tv/membership");
            writer.WriteLine("JOIN #" + channel);
            writer.Flush();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
