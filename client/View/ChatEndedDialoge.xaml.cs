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
using System.Windows.Shapes;
using client.Model;

namespace client.View
{
    /// <summary>
    /// Interaction logic for ChatEndedDialoge.xaml
    /// </summary>
    public partial class ChatEndedDialoge : Window
    {

        public void OnWindowClose(object sender, EventArgs e) { }

        public ChatEndedDialoge(CHATTYPE type)
        {
            InitializeComponent();
            if(type != CHATTYPE.PRIVATECHAT)
            {
                addFriend.Visibility = Visibility.Hidden;
            }

        }
    }
}
