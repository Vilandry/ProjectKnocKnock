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
        User curUser;
        Popup errPopup;

        public void OnRandomChatMessageArrived(object sender, MessageArrivedEventArgs msg)
        {
            Console.Beep();
            string username = msg.MessageSender;
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

                this.privateChatHistory.Focus();
                this.privateChatHistory.ScrollToEnd();
            });


        }

        public MainWindow()
        {
            InitializeComponent();
            curUser = new User();
            login = new LoginController();
            privatechat = new PrivateChatController();
            this.privateChatHistory.IsReadOnly = true;
            errPopup = new Popup();
            hideButtons();
            privatechat.MessageArrived += OnRandomChatMessageArrived;
            curUser.HasOngoingChat = false;
            curUser.HasOngoingChatSearch = false;
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
                if (!privatechat.HandlePrivateChatting(curUser, LookingForSex))
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
                privatechat.ExitChat(curUser);
                curUser.HasOngoingChatSearch = false;
                curUser.HasOngoingChat = false;
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
        }

        private void BeginSoloSearch(object sender, RoutedEventArgs e)
        {
            BeforeLogin.IsSelected = true;
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
            privatechat.ExitChat(curUser);
        }

    }
}
