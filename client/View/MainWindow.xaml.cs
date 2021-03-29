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
using knock.Model;
using knock.Controller;

namespace knock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoginController login;
        User curUser;
        
        public MainWindow()
        {
            InitializeComponent();
            curUser = new User();
            login = new LoginController();
            
        }

        private void BeginSoloSearch(object sender, RoutedEventArgs e)
        {
            BeforeLogin.IsSelected = true;
        }


        void loginAttempt(Object sender, RoutedEventArgs e)
        {
            curUser.Username = LoginNameBox.Text;
            string pwd =  LoginPasswordBox.Password;
            Trace.WriteLine("\n\n\n" + Utility.CreateMD5(pwd) + "\n\n\n");

            bool success = login.tryLogin(curUser, Utility.CreateMD5(pwd));

            this.LoginNameBox.Text = curUser.Username;

            if(success)
            {
                MainMenu.IsSelected = true;
            }
        }

        void registerAttempt(Object sender, RoutedEventArgs e)
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




            bool success = login.tryRegister(curUser, pwd);
            if (success)
            {
                MainMenu.IsSelected = true;
            }
        }
    }
}
