#region copyright
/**
 * Copyright © The MITRE Corporation.
 *
 * This program is licensed under the terms of the GNU General Public License Version 2, as published by the Free Software Foundation. This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  Please see the
 * GNU General Public License for more details.
 
 * This program or code contains content developed by The MITRE Corporation. If this program is used in a deployment or embedded within another project, MITRE requests that you send an email to opensource@mitre.org to let us know where and how this program is being used.
 
 * NOTICE
 * This (software/technical data) was produced for the U. S. Government under Contract Number HHSM-500-2012-00008I, and is subject to Federal Acquisition Regulation Clause 52.227-14, Rights in Data-General. No other use other than that granted to the U. S. Government, or to those acting on behalf of the U. S. Government under that Clause is authorized without the express written permission of The MITRE Corporation. For further information, please contact The MITRE Corporation, Contracts Management Office, 7515 Colshire Drive, McLean, VA  22102-7539, (703) 983-6000.
 */
#endregion

using System;
using System.Windows;
using System.Windows.Media;
using com.vtcsecure.ace.windows.Model;

namespace com.vtcsecure.ace.windows.Views
{
    public partial class AuthenticationView 
    {
        private string _un;
        private string _pwd;
        private bool _proceed;
        private bool _watermarkUn;
        private bool _watermarkPwd;
        private Color _watermarkColor;
        private Color _defaultColor;
        private int _prevPasswordLength;
        private int _currPasswordLength;

        public AuthenticationView() : base(VATRPWindowType.AUTHENTICATION_VIEW)
        {
            InitializeComponent();
            Proceed = false;
            _watermarkColor = new Color();
            _watermarkColor = Color.FromRgb(211, 211, 211);
            _defaultColor = new Color();
            _defaultColor = Color.FromRgb(0, 0, 0);
            _watermarkUn = true;
            _watermarkPwd = true;
            _prevPasswordLength = 0;

            // username box
            usernameBox.Text = "Username";
            usernameBox.GotFocus += new RoutedEventHandler(RemoveTextUn);
            usernameBox.LostFocus += new RoutedEventHandler(AddTextUn);
            usernameBox.Foreground = new SolidColorBrush(_watermarkColor);

            // password box
            PasswordBox.Text = "Password";
            PasswordBox.GotFocus += new RoutedEventHandler(RemoveTextPwd);
            PasswordBox.LostFocus += new RoutedEventHandler(AddTextPwd);
            PasswordBox.Foreground = new SolidColorBrush(_watermarkColor);
            PasswordBox.KeyUp += new System.Windows.Input.KeyEventHandler(username_keyup);
        }

        public string Username
        {
            get { return _un; }
            set { _un = value; }
        }

        public string Password
        {
            get { return _pwd; }
            set { _pwd = value; }
        }

        public bool Proceed
        {
            get { return _proceed; }
            set { _proceed = value; }
        }

        public bool WatermarkUn
        {
            get { return _watermarkUn; }
            set { _watermarkUn = value; }
        }

        public bool WatermarkPwd
        {
            get { return _watermarkPwd; }
            set { _watermarkPwd = value; }
        }

        public int PrevPasswordLength
        {
            get { return _prevPasswordLength; }
            set { _prevPasswordLength = value; }
        }

        public int CurrPasswordLength
        {
            get { return _currPasswordLength; }
            set { _currPasswordLength = value; }
        }

        public void RemoveTextUn(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(usernameBox.Text) || WatermarkUn)
            {
                usernameBox.Text = "";
                usernameBox.Foreground = new SolidColorBrush(_defaultColor);
                WatermarkUn = false;
            }
        }

        public void AddTextUn(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameBox.Text))
            {
                usernameBox.Foreground = new SolidColorBrush(_watermarkColor);
                usernameBox.Text = "Username";
                WatermarkUn = true;
            }       
        }

        public void RemoveTextPwd(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Text) || WatermarkPwd)
            {
                PasswordBox.Text = "";
                PasswordBox.Foreground = new SolidColorBrush(_defaultColor);
                WatermarkPwd = false;
            }      
        }

        public void AddTextPwd(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Text))
            {
                PasswordBox.Foreground = new SolidColorBrush(_watermarkColor);
                PasswordBox.Text = "Password";
                WatermarkPwd = true;
            }         
        }

        private void OnSubmitClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(usernameBox.Text) || string.IsNullOrEmpty(PasswordBox.Text) || WatermarkUn || WatermarkPwd)
            {
                Clear();
                return;
            }
            Proceed = true;
            Username = usernameBox.Text;
            this.Close();      
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            Proceed = false;
            this.Close();
        }

        private void Clear()
        {
            // clear built password
            Password = string.Empty;

            // reset the username
            usernameBox.Text = "Username";
            usernameBox.Foreground = new SolidColorBrush(_watermarkColor);
            
            // reset the password
            PasswordBox.Text = "Password";
            PasswordBox.Foreground = new SolidColorBrush(_watermarkColor);
            
            // reset the watermark states
            WatermarkUn = true;
            WatermarkPwd = true;
        }

        private void username_keyup(object sender, System.Windows.Input.KeyEventArgs e)
        {
           // build the password
            string temp = PasswordBox.Text;
            if (temp.Length < 1)
            {
                PrevPasswordLength = 0;
                Password = string.Empty;
                return;
            }
            foreach (char elem in temp)
            {
                if (elem != '*')
                {
                    Password = Password + elem;
                }
            }

            // handle deletion 
            CurrPasswordLength = PasswordBox.Text.Length;
            bool charRemoved = (CurrPasswordLength - PrevPasswordLength) < 0;
            if (charRemoved)
            {
                Password = Password.Substring(0, CurrPasswordLength);
            }
            PrevPasswordLength = CurrPasswordLength;

            // adjust the output
            temp = string.Empty;
            foreach (char elem in PasswordBox.Text)
            {
                temp = temp + '*';
            }
            PasswordBox.Text = temp;
            PasswordBox.SelectionStart = Math.Max(0, PasswordBox.Text.Length);
            PasswordBox.SelectionLength = 0;
        }
    }
}
