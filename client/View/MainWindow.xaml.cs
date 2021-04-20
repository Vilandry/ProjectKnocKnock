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
            string username = msg.MessageSender;
            string text = msg.Message;

            string displayText = Utility.MessageFormatter(username,text);
            Trace.WriteLine(displayText);

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(displayText));

            this.privateChatHistory.Document.Blocks.Add(paragraph);

            this.privateChatHistory.Focus();
            this.privateChatHistory.ScrollToEnd();

            this.privateChatHistory.Document.Blocks.Add
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
        }

        private void loginAttempt(Object sender, RoutedEventArgs e)
        {                
            curUser.Username = LoginNameBox.Text;            
            string pwd = LoginPasswordBox.Password;              
            Trace.WriteLine("\n\n\n" + Utility.CreateMD5(pwd) + "\n\n\n");
                
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
                curUser.AgeCategory = AGECATEGORY.TWENTY;
            }
            else if (this.oTT.IsChecked == true) ///23+
            {
                curUser.AgeCategory = AGECATEGORY.TWENTYFIVEPLUS;
            }
            else if (this.uET.IsChecked == true) ///18-
            {
                curUser.AgeCategory = AGECATEGORY.SIXTEEN;
            }


            if (this.maleButton.IsChecked == true) ///18-23
            {
                curUser.Gender = SEX.MALE;
            }
            else if (this.femaleButton.IsChecked == true) ///23+
            {
                curUser.Gender = SEX.FEMALE;
            }
            else if (this.otherButton.IsChecked == true) ///18-
            {
                curUser.Gender = SEX.OTHER;
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

            string msg = curUser.Username + "|" + (int)curUser.AgeCategory + "|" + (int)curUser.Gender + "|" + (int)curUser.LookingForSex;
        }

        private void BeginSoloSearch(object sender, RoutedEventArgs e)
        {
            BeforeLogin.IsSelected = true;
            SEX temp = SEX.FEMALE;
            privatechat.HandlePrivateChatting(curUser, temp);
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



    }
}
