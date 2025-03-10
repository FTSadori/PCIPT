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
using PCIPT.Windows.ObjectCreators;

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

            Connect();
        }

        public void Connect()
        {
            asyncConnectThread = new(delegate ()
            {
                try
                {
                    DoCmd(delegate () {
                        AuthorizationPanel.Visibility = Visibility.Hidden;
                    });
                    DbContext.Connect("Server=WIN-00R1JQV3UDA\\SQLEXPRESS; Database=PCIPT; Trusted_connection=True; Encrypt=False");
                    DoCmd(delegate () {
                        AuthorizationPanel.Visibility = Visibility.Visible;
                        });
                }
                catch (Exception ex)
                {
                    DoCmd(delegate () {
                        ShowError("Connection error", true);
                    });

                    return;
                }

                /*
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
                */

                DoCmd(delegate () {
                    LoginStack.Visibility = Visibility.Visible;
                    if (CurrentLogin == "")
                    {
                        TryLoginWithToken();
                    }
                    else
                    {
                        SwitchToAccountMenuWindow(CurrentLogin);
                    }
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

        private void SwitchToAuthorizationWindow()
        {
            AuthorizationPanel.Visibility = Visibility.Visible;
            AccountMenuPanel.Visibility = Visibility.Hidden;
        }

        static string CurrentLogin = "";
        static string CurrentRole = "";

        private void SwitchToAccountMenuWindow(string login)
        {
            CurrentLogin = login;
            AuthorizationPanel.Visibility = Visibility.Hidden;
            AccountMenuPanel.Visibility = Visibility.Visible;

            var account = new SelectAccountByLoginCommand(DbContext.SqlConnection).Execute(login);
            if (account == null)
            {
                ShowError("Can't fetch data about user", false);
                return;
            }

            DispatcherButton.SetResourceReference(BackgroundProperty, "DisableGradient");
            DispatcherButton.IsEnabled = false;
            DispatcherBorder.Visibility = Visibility.Collapsed;
            PlannerButton.SetResourceReference(BackgroundProperty, "DisableGradient");
            PlannerButton.IsEnabled = false;
            PlannerBorder.Visibility = Visibility.Collapsed;
            AdminOptionsText.Visibility = Visibility.Collapsed;
            AddNewAccountButton.Visibility = Visibility.Collapsed;
            ManageAccountsButton.Visibility = Visibility.Collapsed;

            CurrentRole = (string)account["role"];

            if ((string)account["role"] != "planner")
            {
                DispatcherButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");
                DispatcherButton.IsEnabled = true;
                DispatcherBorder.Visibility = Visibility.Visible;
            }
            if ((string)account["role"] != "dispatcher")
            {
                PlannerButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");
                PlannerButton.IsEnabled = true;
                PlannerBorder.Visibility = Visibility.Visible;
            }
            if ((string)account["role"] == "admin" || (string)account["role"] == "superadmin")
            {
                AdminOptionsText.Visibility = Visibility.Visible;
                AddNewAccountButton.Visibility = Visibility.Visible;
                ManageAccountsButton.Visibility = Visibility.Visible;
            }

            GreetingText.Text = $"Hello, {login}! ({(string)account["role"]})";
        }

        private void ShowSuccessMessage(string text)
        {
            SuccessText.Text = text;
            SuccessGrid.Visibility = Visibility.Visible;
            SuccessBackground.Visibility = Visibility.Visible;
        }

        private void HideSuccessMessage()
        {
            SuccessGrid.Visibility = Visibility.Hidden;
            SuccessBackground.Visibility = Visibility.Hidden;
        }

        private void ShowError(string text, bool retryButton)
        {
            ErrorText.Text = text;
            RetryButton.Visibility = (retryButton) ? Visibility.Visible : Visibility.Hidden;
            OkayButton.Visibility = (!retryButton) ? Visibility.Visible : Visibility.Hidden;
            ErrorBackground.Visibility = Visibility.Visible;
            ErrorGrid.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorBackground.Visibility = Visibility.Hidden;
            ErrorGrid.Visibility = Visibility.Hidden;
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
                    ShowError("Wrong login", false);
                    return;
                }

                ShowSuccessMessage("Logged in via token as " + (string)account["login"]);
                SwitchToAccountMenuWindow((string)account["login"]);
            }
            catch (Exception)
            {
            }
        }

        private bool ValidatePassword(string password)
        {
            if (password.Length < 8)
            {
                ShowError("Password should be at least 8 symbols long", false);
                return false;
            }
            else
            {
                bool num = false;
                bool letter = false;
                foreach (var c in password)
                {
                    num |= Char.IsDigit(c);
                    letter |= Char.IsLetter(c);
                }
                if (!num || !letter)
                {
                    ShowError("Password should contain at least one letter and one digit", false);
                    return false;
                }
            }

            return true;
        }

        private bool CheckPasswordFor(string login, string password)
        {
            var account = new SelectAccountByLoginCommand(DbContext.SqlConnection).Execute(login);
            if (account == null)
            {
                ShowError("Wrong login", false);
                return false;
            }
            if (Hasher.GetHashString(password + (string)account["salt"]) != (string)account["passhash"])
            {
                ShowError("Wrong password", false);
                return false;
            }
            return true;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckPasswordFor(LoginBox.Text, PasswordBox.Password))
            {
                return;
            }
            if (RememberMeCheckBox.IsChecked ?? false)
            {
                if (new DeleteTokenByLoginCommand(DbContext.SqlConnection).Execute(LoginBox.Text) != "")
                {
                    ShowError("Connection error", false);
                    return;
                }

                string token = new SaltCreator(new Random()).GetSalt(40);

                if (new AddTokenEntryCommand(DbContext.SqlConnection).Execute(new TokenDto(Hasher.GetHashString(token), LoginBox.Text)) != "")
                {
                    ShowError("Token saving error", false);
                    return;
                }

                StreamWriter sw = new("token");
                sw.Write(token);
                sw.Close();
            }

            ShowSuccessMessage("Successfully logged in!");
            SwitchToAccountMenuWindow(LoginBox.Text);

            return;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            HideError();
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            HideError();
            Connect();
        }

        private void SuccessOkayButton_Click(object sender, RoutedEventArgs e)
        {
            HideSuccessMessage();
        }

        private void PlannerButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new PlannerWindow(DbContext);
            w.Show();
            Close();
        }

        private void DispatcherButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new GraphWindow();
            w.Show();
            Close();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordBackground.Visibility = Visibility.Visible;
            ChangePassword1Box.Password = "";
            ChangePassword2Box.Password = "";
            ChangePassword3Box.Password = "";
        }

        private void LogOffButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists("token"))
                {
                    new DeleteTokenByLoginCommand(DbContext.SqlConnection).Execute(CurrentLogin);
                    File.Delete("token");
                }
                PasswordBox.Password = "";
                LoginBox.Text = "";
                CurrentLogin = "";
                SwitchToAuthorizationWindow();
            }
            catch (Exception)
            {
                ShowError("Error while logging off", false);
            }
        }

        private void ManageAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            AccountsStack.Children.Clear();
            ManageAccountPanel.Visibility = Visibility.Visible;
            try
            {
                var table = new SelectAllAccountEntriesCommand(DbContext.SqlConnection).Execute();
                int i = 1;
                foreach (DataRow row in table.Tables[0].Rows)
                {
                    AccountsStack.Children.Add(ManageAccountsRowCreator.GetObject(i++, (string)row["login"], (string)row["role"], ManageAccountClick, CurrentRole));
                }
            }
            catch (Exception)
            {
                ShowError("Error while displaying data", false);
            }
        }

        string CurrentAccountToManage = "";

        private void ManageAccountClick(string name)
        {
            CurrentAccountToManage = name;
            AccountOptionsLabel.Text = name + "'s options";
            AccountOptionsPanel.Visibility = Visibility.Visible;
        }

        private void AddNewAccountButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewAccountBackground.Visibility = Visibility.Visible;
            CreateAccountName.Text = "";
            CreateAccountRole.SelectedIndex = -1;
            CreateAccountGeneratedPassword.Text = new SaltCreator(new Random()).GetSalt(7, true);
            CreateAccountGeneratedPassword.Text += (char)new Random().Next('0', '9');
        }

        private void CloseChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordBackground.Visibility = Visibility.Hidden;
        }

        private void SubmitChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckPasswordFor(CurrentLogin, ChangePassword1Box.Password))
            {
                return;
            }
            if (ChangePassword2Box.Password != ChangePassword3Box.Password)
            {
                ShowError("Passwords are not equal", false);
                return;
            }
            if (ValidatePassword(ChangePassword2Box.Password))
            {
                try
                {
                    string salt = new SaltCreator(new Random()).GetSalt(10);
                    string password = ChangePassword2Box.Password + salt;
                    
                    if (new UpdatePasswordCommand(DbContext.SqlConnection).Execute(new UpdatePasswordDto(CurrentLogin, Hasher.GetHashString(password), salt)) == "")
                    {
                        ChangePasswordBackground.Visibility = Visibility.Hidden;
                        ShowSuccessMessage("Password has changed");
                    }
                    else
                    {
                        ShowError("Database query error", false);
                    }
                }
                catch (Exception)
                {
                    ShowError("Error while changing passwords", false);
                }
            }
        }

        private void CreateAccountBack_Click(object sender, RoutedEventArgs e)
        {
            CreateNewAccountBackground.Visibility = Visibility.Hidden;
        }

        private void CreateAccountSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (CreateAccountName.Text.Length == 0)
            {
                ShowError("Name can't be empty", false);
                return;
            }
            if (CreateAccountName.Text.Length > 32)
            {
                ShowError("Name can't have more than 32 symbols", false);
                return;
            }

            var account = new SelectAccountByLoginCommand(DbContext.SqlConnection).Execute(CreateAccountName.Text);
            if (account != null)
            {
                ShowError("Account with this login already exists", false);
                return;
            }

            if (!ValidatePassword(CreateAccountGeneratedPassword.Text))
            {
                return;
            }
            if (CreateAccountRole.SelectedIndex == -1)
            {
                ShowError("Role should be selected", false);
                return;
            }

            string salt = new SaltCreator(new Random()).GetSalt(10);
            string password = Hasher.GetHashString(CreateAccountGeneratedPassword.Text + salt);
            if (new AddAccountEntryCommand(DbContext.SqlConnection).Execute(new PasswordDto(CreateAccountName.Text, password, salt, CreateAccountRole.Text.ToLower())) != "")
            {
                ShowError("Error while creating account", false);
                return;
            }
            CreateNewAccountBackground.Visibility = Visibility.Hidden;
            ShowSuccessMessage($"Account with login '{CreateAccountName.Text}' has been created. Authorization data has been copied to your clipboard");
            Clipboard.SetText($"{CreateAccountName.Text} {CreateAccountGeneratedPassword.Text}");
        }

        private void ManageAccountsBack_Click(object sender, RoutedEventArgs e)
        {
            ManageAccountPanel.Visibility = Visibility.Hidden;
        }

        private void AccountOptionsResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string password = "";
            try
            {
                password = new SaltCreator(new Random()).GetSalt(7, true);
                password += (char)new Random().Next('0', '9');
                string salt = new SaltCreator(new Random()).GetSalt(10);
                string passhash = Hasher.GetHashString(password + salt);

                if (new UpdatePasswordCommand(DbContext.SqlConnection).Execute(new UpdatePasswordDto(CurrentAccountToManage, passhash, salt)) != "")
                {
                    ShowError("Error while updating row entry", false);
                }
                Clipboard.SetText(password);
            }
            catch (Exception)
            {
                ShowError("Error while updating row entry", false);
            }
            AccountOptionsPanel.Visibility = Visibility.Hidden;
            ShowSuccessMessage($"Password has been reset. New password has been copied to your clipboard");
        }

        private void AccountOptionsDeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            YesNoPanel.Visibility = Visibility.Visible;
        }

        private void AccountOptionsCloseButton_Click(object sender, RoutedEventArgs e)
        {
            AccountOptionsPanel.Visibility = Visibility.Hidden;
        }

        private void YesNoBack_Click(object sender, RoutedEventArgs e)
        {
            YesNoPanel.Visibility = Visibility.Hidden;
        }

        private void YesNoAgree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (new DeleteAccountEntryCommand(DbContext.SqlConnection).Execute(CurrentAccountToManage) != "")
                {
                    ShowError("Error while deleting account", false);
                }
            }
            catch (Exception)
            {
                ShowError("Error while deleting account", false);
            }
            YesNoPanel.Visibility = Visibility.Hidden;
            AccountOptionsPanel.Visibility = Visibility.Hidden;
            ShowSuccessMessage($"Account has been deleted");
            ManageAccountsButton_Click(new object(), new RoutedEventArgs());
        }
    }
}
