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
                Console.Beep();
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

                if(curUser.LastPrivateChatHistory != "")
                {
                    this.saveButton.IsEnabled = true;
                }
                
                this.leaveButton.IsEnabled = false;
                this.sendPrivateMessageButton.IsEnabled = false;
                this.joinPrivateMatchServer.Content = "GIVE ME A MATCH!";

                this.curUser.HasOngoingChat = false;
                this.curUser.HasOngoingChatSearch = false;



                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run("Connection lost! Ending chat..."));
                paragraph.TextAlignment = TextAlignment.Left;
                paragraph.Foreground = Brushes.Red;
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

                this.leaveButton.IsEnabled = true;
                this.sendPrivateMessageButton.IsEnabled = true;
            });            
        }

        public void OnServerDown(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                BeforeLogin.IsSelected = true;
                errPopup = new Popup();
                string text = "Lost connection to server! Come back soon!";
                ErrorWindow popup = new ErrorWindow(text);
                popup.ShowDialog();
            });
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

            this.ResizeMode = ResizeMode.NoResize;

            this.messageTextBox.MaxLength = 1024;
        }


        private void loginAttempt(Object sender, RoutedEventArgs e)
        {                
            curUser.Username = LoginNameBox.Text;            
            string pwd = LoginPasswordBox.Password;                              
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


            if( ! login.CheckAlphanumericCharacters(curUser.Username) || curUser.Username.Length < 4)
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
                this.joinPrivateMatchServer.Content = "GIVE ME A MATCH!";
                privatechat.ExitChat(); ///at this time, its still on the matchmanager
                curUser.HasOngoingChatSearch = false;
                curUser.HasOngoingChat = false;
                Thread.Sleep(500);
            }
            else
            {
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

        private void BeginSoloSearch(object sender, RoutedEventArgs e)
        {
            MainMenu.IsSelected = true;
        }

        private void displayButtons()
        {
            this.groupChatButton.Visibility = Visibility.Visible;
            this.soloSearchButton.Visibility = Visibility.Visible;
            this.optionsButton.Visibility = Visibility.Visible;
            this.logoutButton.Visibility = Visibility.Visible;
        }

        private void hideButtons()
        {
            this.groupChatButton.Visibility = Visibility.Hidden;
            this.soloSearchButton.Visibility = Visibility.Hidden;
            this.optionsButton.Visibility = Visibility.Hidden;
            this.logoutButton.Visibility = Visibility.Hidden;
        }

        private void Shutdown(object sender, EventArgs e)
        {
            privatechat.ExitChat();
        }

        private void SaveMessageHistory(object sender, RoutedEventArgs e)
        {

            misc.sendPrivateChatHistory(curUser.LastPrivateChatConversationId, curUser.LastPrivateChatHistory);

            this.saveButton.IsEnabled = false;
        }
    }
}
