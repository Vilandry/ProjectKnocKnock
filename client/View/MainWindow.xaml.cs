using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Threading;

using client.View;
using client.Model;
using client.Controller;

namespace client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoginController login;
        PrivateChatController privatechat;
        MiscController misc;

        volatile User curUser;
        Popup errPopup;

        #region Events

        public void OnRandomChatMessageArrived(object sender, MessageArrivedEventArgs msg)
        {            
            string username = msg.MessageSender;

            if(username != curUser.Username)
            {
                //Console.Beep();
            }

            string text = msg.Message;

            string displayText = Utility.MessageFormatter(username,text);
            Trace.WriteLine(displayText);

            this.Dispatcher.Invoke(() =>
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(displayText));

                if (username == curUser.Username)
                {
                    paragraph.TextAlignment = TextAlignment.Right;
                    paragraph.Foreground = Brushes.Orange;
                }
                else
                {
                    paragraph.TextAlignment = TextAlignment.Left;
                    paragraph.Foreground = Brushes.Purple;
                }

                this.privateChatHistory.Document.Blocks.Add(paragraph);

                this.messageTextBox.Focus();
                this.privateChatHistory.ScrollToEnd();
            });
        }

        public void OnChatEnded(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.saveButton.Visibility = Visibility.Visible;
                this.addFriendButton.Visibility = Visibility.Visible;
                this.BlockButton.Visibility = Visibility.Visible;

                this.saveButton.IsEnabled = true;
                this.addFriendButton.IsEnabled = true;
                this.BlockButton.IsEnabled = true;

                this.messageTextBox.IsEnabled = false;

                if (curUser.LastPrivateChatHistory != "")
                {
                    this.saveButton.IsEnabled = true;
                }
                
                this.leaveButton.IsEnabled = false;
                this.sendPrivateMessageButton.IsEnabled = false;
                this.joinPrivateMatchServer.Content = "GIVE ME A MATCH!";

                

                this.curUser.HasOngoingChat = false;
                this.curUser.HasOngoingChatSearch = false;



                /*Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run("Connection lost! Ending chat..."));
                paragraph.TextAlignment = TextAlignment.Left;
                paragraph.Foreground = Brushes.Red;*/
            });

        }

        public void OnChatBegins(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                privateChatHistory.Document.Blocks.Clear();
                Console.Beep();
                this.PrivateChat.IsSelected = true;
                this.saveButton.Visibility = Visibility.Hidden;
                this.addFriendButton.Visibility = Visibility.Hidden;
                this.leaveButton.Visibility = Visibility.Visible;
                this.sendPrivateMessageButton.Visibility = Visibility.Visible;
                this.BlockButton.Visibility = Visibility.Hidden;
                this.messageTextBox.IsEnabled = true;

                this.leaveButton.IsEnabled = true;
                this.sendPrivateMessageButton.IsEnabled = true;
            });            
        }

        public void OnAlreadyJoined(object sender, EventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                ErrorWindow popup = new ErrorWindow("This user is already connected to the privatechatqueue!");
                popup.ShowDialog();

                curUser.HasOngoingChatSearch = false;

                this.lookingForAny.IsEnabled = true;
                this.lookingForFemale.IsEnabled = true;
                this.lookingForMale.IsEnabled = true;
                this.lookingForOther.IsEnabled = true;

            });

            this.joinPrivateMatchServer.Content = "GIVE ME A MATCH!";
            privatechat.LeaveQueue(); ///at this time, its still in the matchmanager
            curUser.HasOngoingChatSearch = false;
            curUser.HasOngoingChat = false;
        }

        public void OnServerDown(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                hideButtons();
                BeforeLogin.IsSelected = true;
                errPopup = new Popup();
                string text = "Lost connection to server! Come back soon!";
                ErrorWindow popup = new ErrorWindow(text);
                popup.ShowDialog();
               
            });

            curUser.HasOngoingChat = false;
            curUser.HasOngoingChatSearch = false;
            privatechat.ExitChat();
            privatechat.LeaveQueue();
        }
        
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            curUser = new User();
            login = new LoginController();
            privatechat = new PrivateChatController(curUser);
            misc = new MiscController();

            this.privateChatHistory.IsReadOnly = true;
            errPopup = new Popup();
            hideButtons();

            privateChatHistory.Document.Blocks.Clear();
            
            curUser.HasOngoingChat = false;
            curUser.HasOngoingChatSearch = false;

            privatechat.chatBegins += OnChatBegins;
            privatechat.MessageArrived += OnRandomChatMessageArrived;
            privatechat.chatEnded += OnChatEnded;
            privatechat.lostConnection += OnServerDown;
            misc.lostConnection += OnServerDown;
            privatechat.inqueue += OnAlreadyJoined;

            this.ResizeMode = ResizeMode.NoResize;

            this.messageTextBox.MaxLength = 1024;
        }

        private void SelectSaveHistory(object sender, MouseButtonEventArgs e)
        {
            var castedSender = sender as ListViewItem;
            var item = castedSender.Content as MessageHistoryEntry;          


            loadedHistory.Document.Blocks.Clear();
            //var item = (MessageHistoryEntry)(sender as ListView).SelectedItem;

            if (item != null)
            {
                string res = misc.GetChatHistoryMessage(item.Entrystring.ToString());

                string[] lines = res.Split("\n");

                foreach (string line in lines)
                {
                    if (line == "") { continue; }
                    Trace.WriteLine(line + " <- was the line");
                    string displayText;
                    if (!line.Contains(":"))
                    {
                        displayText = line;


                        this.Dispatcher.Invoke(() =>
                        {
                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add(new Run(displayText));

                            paragraph.TextAlignment = TextAlignment.Left;
                            paragraph.Foreground = Brushes.Purple;

                            this.loadedHistory.Document.Blocks.Add(paragraph);

                            this.loadedHistory.ScrollToEnd();
                        });
                    }
                    else
                    {
                        string username = line.Split(":", 2)[0];
                        string Text = line.Split(":", 2)[1].Trim();
                        displayText = Utility.MessageFormatter(username, Text);


                        this.Dispatcher.Invoke(() =>
                        {
                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add(new Run(displayText));

                            if (username == curUser.Username)
                            {
                                paragraph.TextAlignment = TextAlignment.Right;
                                paragraph.Foreground = Brushes.Orange;
                            }
                            else
                            {
                                paragraph.TextAlignment = TextAlignment.Left;
                                paragraph.Foreground = Brushes.Purple;
                            }

                            this.loadedHistory.Document.Blocks.Add(paragraph);

                            this.loadedHistory.ScrollToEnd();
                        });
                    }

                    
                    
                    
                }
            }
            else { Trace.WriteLine("item was empty!"); }

             
        }

        private void BlockLastChatMate(object sender, RoutedEventArgs e)
        {
            this.addFriendButton.IsEnabled = false;
            this.BlockButton.IsEnabled = false;
            misc.SendBlockRequest(curUser.Username, curUser.LastPrivateChatUsername);
        }

        private void AddLastChatMate(object sender, RoutedEventArgs e)
        {
            this.BlockButton.IsEnabled = false;
            this.addFriendButton.IsEnabled = false;
            misc.SendFriendRequest(curUser.Username, curUser.LastPrivateChatUsername);
        }

        

        private void loginAttempt(Object sender, RoutedEventArgs e)
        {                
            curUser.Username = LoginNameBox.Text;            
            string pwd = LoginPasswordBox.Password;
            LoginPasswordBox.Password = "";


            if(!login.CheckAlphanumericCharacters(curUser.Username))
            {
                ErrorWindow popup = new ErrorWindow("Wrong username or password! Make sure You registered with this username and did not misstype the password!");
                popup.ShowDialog();
                return;
            }
            int success = login.tryLogin(curUser, pwd);                

                
            if (success == 1)                
            {                    
                MainMenu.IsSelected = true;                    
                displayButtons();                
            }               
            else if (success == 0)              
            {
                ErrorWindow popup = new ErrorWindow("Wrong username or password! Make sure You registered with this username and did not misstype the password!");
                popup.ShowDialog();
            }
            else if(success == -1)
            {
                ErrorWindow popup = new ErrorWindow("Could not connect to the servers! Maybe it's our fault, maybe You have no connection... Who knows?");
                popup.ShowDialog();
            }

        }

        private void registerAttempt(Object sender, RoutedEventArgs e)
        {
            curUser.Username = SignupNameBox.Text;
            string pwd = SignupPasswordBox.Password;
            SignupPasswordBox.Password = "";

            Trace.WriteLine("\n\n\n" + Utility.CreateMD5(pwd) + "\n\n\n");

            if (this.ETTT.IsChecked == true) ///18-23
            {
                curUser.AgeCategory = AGECATEGORY.SEMI;
            }
            else if (this.oTT.IsChecked == true) ///23+
            {
                curUser.AgeCategory = AGECATEGORY.ADULT;
            }
            else if (this.uET.IsChecked == true) ///18-
            {
                curUser.AgeCategory = AGECATEGORY.YOUNG;
            }


            if (this.maleButton.IsChecked == true) ///18-23
            {
                curUser.Gender = GENDER.MALE;
            }
            else if (this.femaleButton.IsChecked == true) ///23+
            {
                curUser.Gender = GENDER.FEMALE;
            }
            else if (this.otherButton.IsChecked == true) ///18-
            {
                curUser.Gender = GENDER.OTHER;
            }


            if( ! login.CheckAlphanumericCharacters(curUser.Username) || curUser.Username.Length < 4 || curUser.Username == "SERVER")
            {
                ErrorWindow popup = new ErrorWindow("Your name contains illegal characters! Make sure to use only letters and numbers! Also Your username must be at least 4 character long");
                popup.ShowDialog();
                return;
            }

            if (!login.CheckAlphanumericCharacters(pwd) || pwd.Length < 5)
            {
                ErrorWindow popup = new ErrorWindow("Illegal password! Make sure to use only letters and numbers. Also Your password must be at least 5 character long!");
                popup.ShowDialog();
                return;
            }



            int success = login.tryRegister(curUser, pwd);
            if (success == 1)
            {
                MainMenu.IsSelected = true;
                displayButtons();
            }
            else if (success == 0)
            {
                ErrorWindow popup = new ErrorWindow("Couldnt register the current username, since it's taken!");
                popup.ShowDialog();
            }
            else if (success == -1)
            {
                ErrorWindow popup = new ErrorWindow("Could not connect to the servers! Maybe it's our fault, maybe You have no connection... Who knows?");
                popup.ShowDialog();
            }

        }

        private void joinPrivateMatchQueue(Object sender, RoutedEventArgs e)
        {
            GENDER LookingForSex = GENDER.FEMALE;
            if (this.lookingForFemale.IsChecked == true)
            {
                LookingForSex = GENDER.FEMALE;
            }
            else if (this.lookingForMale.IsChecked == true)
            {
                LookingForSex = GENDER.MALE;
            }
            else if (this.lookingForOther.IsChecked == true)
            {
                LookingForSex = GENDER.OTHER;
            }
            else if (this.lookingForAny.IsChecked == true)
            {
                LookingForSex = GENDER.ANY;
            }
            else
            {
                Trace.WriteLine("joinPrivateMatchQueue error: nothing was checked wtf");
                return;
            }

            Thread t = new Thread(
                (_) => {
                if (!privatechat.HandlePrivateChatting(LookingForSex))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.joinPrivateMatchServer.Content = "GIVE ME A MATCH!";
                    });
                    
                }
            });

            if (curUser.HasOngoingChatSearch || curUser.HasOngoingChat)
            {
                this.lookingForAny.IsEnabled = true;
                this.lookingForFemale.IsEnabled = true;
                this.lookingForMale.IsEnabled = true;
                this.lookingForOther.IsEnabled = true;

                this.joinPrivateMatchServer.Content = "GIVE ME A MATCH!";
                privatechat.LeaveQueue(); ///at this time, its still in the matchmanager
                curUser.HasOngoingChatSearch = false;
                curUser.HasOngoingChat = false;
                Thread.Sleep(500);
            }
            else
            {
                this.lookingForAny.IsEnabled = false;
                this.lookingForFemale.IsEnabled = false;
                this.lookingForMale.IsEnabled = false;
                this.lookingForOther.IsEnabled = false;


                curUser.HasOngoingChatSearch = true;
                t.Start();
                this.joinPrivateMatchServer.Content = "Stop searching";
            }

        }

        private void sendPrivateMessage(object sender, RoutedEventArgs e)
        {

            string msg = this.messageTextBox.Text;
            privatechat.handeMessaging(curUser.Username, msg);
            messageTextBox.Text = "";
            messageTextBox.Focus();
        }


        #region tabFunctions
        private void BeginSoloSearch(object sender, RoutedEventArgs e)
        {
            MainMenu.IsSelected = true;
            privatechat.ExitChat();
        }

        private void BeginHistory(object sender, RoutedEventArgs e)
        {           
            MessageHistoryPage.IsSelected = true;
            privatechat.ExitChat();
            string[] histories = misc.GetUserHistoryIDs(curUser.Username);

            foreach(string history in histories)
            {
                   MessageHistoryEntry entry = new MessageHistoryEntry();
                   ChatHistoryListView.Items.Clear();
                   entry.Entrystring = history;

                string[] elements = history.Split("|");

                if(elements[1] == curUser.Username)
                {
                    entry.Partner = elements[2];
                }
                else if(elements[2] == curUser.Username)
                {
                    entry.Partner = elements[1];
                }

                ChatHistoryListView.Items.Add(entry);
            }

            loadedHistory.Document.Blocks.Clear();

        }

        private void BeginFriendList(object sender, RoutedEventArgs e)
        {
            this.FriendListPage.IsSelected = true;
            string list = misc.GetFriendLists(curUser.Username);
            privatechat.ExitChat();

            string[] mutualList = list.Split("!")[0].Split("|");
            string[] onlyLovedBySenderList = list.Split("!")[1].Split("|");
            string[] onlySenderLovedByList = list.Split("!")[2].Split("|");
            this.MutualFriendList.Items.Clear();
            this.LovedBySenderFriendList.Items.Clear();
            this.SenderLovedByFriendList.Items.Clear();


            foreach (string name in mutualList)
            {
                if (name == "") { continue; }
                ListViewItem entry = new ListViewItem();
                entry.Content = name;
                this.MutualFriendList.Items.Add(entry);
            }

            foreach (string name in onlyLovedBySenderList)
            {
                if (name == "") { continue; }
                ListViewItem entry = new ListViewItem();
                entry.Content = name;
                this.LovedBySenderFriendList.Items.Add(entry);
            }

            foreach (string name in onlySenderLovedByList)
            {
                if (name == "") { continue; }
                ListViewItem entry = new ListViewItem();
                entry.Content = name;
                this.SenderLovedByFriendList.Items.Add(entry);
            }            
        }

        #endregion

        private void displayButtons()
        {
            this.groupChatButton.Visibility = Visibility.Visible;
            this.soloSearchButton.Visibility = Visibility.Visible;
            this.logoutButton.Visibility = Visibility.Visible;
            this.messageHistoryButton.Visibility = Visibility.Visible;
        }

        private void hideButtons()
        {
            this.groupChatButton.Visibility = Visibility.Hidden;
            this.soloSearchButton.Visibility = Visibility.Hidden;
            this.logoutButton.Visibility = Visibility.Hidden;
            this.messageHistoryButton.Visibility = Visibility.Hidden;
        }

        private void Shutdown(object sender, EventArgs e)
        {
            privatechat.ExitChat();
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            privatechat.LeaveQueue();
            privatechat.ExitChat();
            try
            {
                privatechat.Client.Close();
            }
            catch(Exception er)
            {
                Trace.WriteLine("error during closing pc client er msg: " + er.Message);
            }
            this.Close();
        }

        private void SaveMessageHistory(object sender, RoutedEventArgs e)
        {

            misc.sendPrivateChatHistory(curUser.LastPrivateChatConversationId, curUser.LastPrivateChatHistory, curUser.Username);

            this.saveButton.IsEnabled = false;
        }

        public class MessageHistoryEntry
        {
            public int EpochTime { get; private set; }
            public string TimeString { get; private set; }
            public string[] Usernames { get; private set; }
            public string Partner { get; set; }

            string entrystring;
            public string Entrystring
            {
                get { return entrystring; }
                set
                {
                    entrystring = value;

                    var parts = entrystring.Split("|");
                    EpochTime = int.Parse(parts[0]);
                    TimeString = DateTimeOffset.FromUnixTimeSeconds(EpochTime).ToString("0:MM/dd/yy H:mm:ss");
                    Usernames = parts.Skip(1).ToArray();
                }
            }
        }
    }
}
