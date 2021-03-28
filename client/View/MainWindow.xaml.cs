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
            string pwd = LoginPasswordBox.Password;


            bool success = login.tryLogin(curUser, pwd);

            this.LoginNameBox.Text = curUser.Username;

            if(success)
            {
                MainMenu.IsSelected = true;
            }
        }

        void registerAttempt(Object sender, RoutedEventArgs e)
        {
            curUser.Username = LoginNameBox.Text;
            string pwd = LoginPasswordBox.Password;

            if (this.ETTT.IsChecked == true) ///18-23
            {
                curUser.AgeCategory = AGECATEGORY.TWENTY;
            }
            else if (this.oTT.IsChecked == true) ///23+
            {

            }
            else if (this.uET.IsChecked == true) ///18-
            {

            }
        }
    }
}
