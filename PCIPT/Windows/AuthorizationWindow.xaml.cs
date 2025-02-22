using PCIPT.Core.DataHandler;
using PCIPT.Database.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using PCIPT.Database.Dtos;
using System.Data;
using System.IO;

namespace PCIPT.Windows
{
    /// <summary>
    /// Logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        static DbContext DbContext { get; set; } = new();

        Thread? asyncConnectThread;

        public AuthorizationWindow()
        {
            InitializeComponent();

            asyncConnectThread = new(delegate ()
            {
                try
                {
                    DbContext.Connect("Server=WIN-00R1JQV3UDA\\SQLEXPRESS; Database=PCIPT; Trusted_connection=True; Encrypt=False");
                }
                catch (Exception ex)
                {
                    return;
                }

                var data = new SelectAllAccountEntriesCommand(DbContext.SqlConnection).Execute();
                if (data == null)
                {
                    return;
                }
                int count = 0;
                foreach (DataRow row in data.Tables[0].Rows)
                {
                    if ((string)row["role"] == "admin")
                    {
                        count++;
                    }
                }

                DoCmd(delegate () {
                    LoginStack.Visibility = Visibility.Visible;
                    TryLoginWithToken();
                });

                /*
                string salt = new SaltCreator(new Random()).GetSalt(10);
                string password = "diploma" + salt;
                
                DoCmd(delegate () {
                    string message = new AddAccountEntryCommand(DbContext.SqlConnection).Execute(new PasswordDto("Admin", Hasher.GetHashString(password), salt, "admin"));
                    ConnectStatusText.Text = message;
                });
                */
            });
            asyncConnectThread.IsBackground = true;
            asyncConnectThread.Start();
        }

        public static void DoCmd(ThreadStart th)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, th);
        }

        private void TryLoginWithToken()
        {
            try
            {
                StreamReader sr = new("token");
                var dataRow = new SelectLoginByTokenCommand(DbContext.SqlConnection).Execute(Hasher.GetHashString(sr.ReadLine() ?? ""));

                if (dataRow == null)
                {
                    return;
                }

                var account = new SelectAccountByLoginCommand(DbContext.SqlConnection).Execute((string)dataRow["login"]);
                if (account == null)
                {
                    //MiniErrorBox.Text = "Wrong login";
                    //MiniErrorBox.Background = Brushes.Red;
                    return;
                }

                //MiniErrorBox.Text = "Logged in via token as " + (string)account["login"];
                //MiniErrorBox.Background = Brushes.Green;
            }
            catch (Exception)
            {
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var account = new SelectAccountByLoginCommand(DbContext.SqlConnection).Execute(LoginBox.Text);
            if (account == null)
            {
                //MiniErrorBox.Text = "Wrong login";
                //MiniErrorBox.Background = Brushes.Red;
                return;
            }
            if (Hasher.GetHashString(PasswordBox.Password + (string)account["salt"]) != (string)account["passhash"])
            {
                //MiniErrorBox.Text = "Wrong password";
                //MiniErrorBox.Background = Brushes.Red;
                return;
            }
            if (RememberMeCheckBox.IsChecked ?? false)
            {
                if (new DeleteTokenByLoginCommand(DbContext.SqlConnection).Execute(LoginBox.Text) != "")
                {
                    //MiniErrorBox.Text = "Connection error";
                    //MiniErrorBox.Background = Brushes.Red;
                    return;
                }

                string token = new SaltCreator(new Random()).GetSalt(40);

                if (new AddTokenEntryCommand(DbContext.SqlConnection).Execute(new TokenDto(Hasher.GetHashString(token), LoginBox.Text)) != "")
                {
                    //MiniErrorBox.Text = "Token saving error";
                    //MiniErrorBox.Background = Brushes.Red;
                    return;
                }

                StreamWriter sw = new("token");
                sw.Write(token);
                sw.Close();
            }

            //MiniErrorBox.Text = "Amazing!";
            //MiniErrorBox.Background = Brushes.Green;

            return;
        }
    }
}
