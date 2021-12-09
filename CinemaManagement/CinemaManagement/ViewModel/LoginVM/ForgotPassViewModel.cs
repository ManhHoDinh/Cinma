﻿using CinemaManagement.Utils;
using CinemaManagement.Views.LoginWindow;
using System.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaManagement.Models.Services;
using CinemaManagement.Views;

namespace CinemaManagement.ViewModel
{

    public class ForgotPassViewModel : BaseViewModel
    {
        private bool isSendMail;
        public bool IsSendMail
        {
            get { return isSendMail; }
            set { isSendMail = value; OnPropertyChanged(); }
        }

        //Dành cho người quên mật khẩu
        public static string ForgotPasswordEmail = null;
        public static string RequestingStaffId = null;

        private int RandomCode;

        public Button SendmailBtn { get; set; }
        private string _usrename;
        public string usrename
        {
            get { return _usrename; }
            set { _usrename = value; OnPropertyChanged(); }
        }

        private string _code;
        public string code
        {
            get { return _code; }
            set { _code = value; OnPropertyChanged(); }
        }

        private string newPass;
        public string NewPass
        {
            get { return newPass; }
            set { newPass = value; OnPropertyChanged(); }
        }


        public ICommand ConfirmCM { get; set; }
        public ICommand CancelCM { get; set; }
        public ICommand SendMailCM { get; set; }
        public ICommand CodeChangedCM { get; set; }
        public ICommand PreviousPageCM { get; set; }
        public ICommand SaveNewPassCM { get; set; }
        public ICommand NewPassChanged { get; set; }
        public ICommand SaveSendmailBtnCM { get; set; }

        public ForgotPassViewModel()
        {
            CancelCM = new RelayCommand<object>((p) => { return p == null ? false : true; }, (p) =>
            {
                LoginViewModel.MainFrame.Content = new LoginPage();
            });
            ConfirmCM = new RelayCommand<PasswordBox>((p) => { return true; }, (p) =>
            {
                if (!string.IsNullOrEmpty(p.Password))
                {
                    if (p.Password == RandomCode.ToString())
                    {
                        LoginViewModel.MainFrame.Content = new ChangePassPage();
                    }
                    else
                    {
                        MessageBoxCustom mb = new MessageBoxCustom("", "Mã bảo mật sai", MessageType.Error, MessageButtons.OK);
                        mb.ShowDialog();
                        return;
                    }
                }

            });

            PreviousPageCM = new RelayCommand<Label>((p) => { return true; }, (p) =>
            {
                LoginViewModel.MainFrame.Content = new ForgotPassPage();
            });
            CodeChangedCM = new RelayCommand<PasswordBox>((p) => { return true; }, (p) =>
            {
                code = p.Password;
            });
            SendMailCM = new RelayCommand<Button>((p) => { return true; }, async (p) =>
             {
                 //field null then return
                 if (string.IsNullOrEmpty(usrename)) return;
                 // exists mail or not
                 if (ForgotPasswordEmail is null) return;


                 Random rd = new Random();
                 int MIN_VALUE = 11111;
                 int MAX_VALUE = 99999;
                 RandomCode = rd.Next(MIN_VALUE, MAX_VALUE);
                 try
                 {
                     await SendEmailForStaff(ForgotPasswordEmail, RandomCode);
                 }
                 catch (System.Data.Entity.Core.EntityException e)
                 {
                     Console.WriteLine(e);
                     MessageBoxCustom mb = new MessageBoxCustom("", "Mất kết nối cơ sở dữ liệu", MessageType.Error, MessageButtons.OK);
                     mb.ShowDialog();
                     throw;
                 }
                 catch (Exception e)
                 {
                     Console.WriteLine(e);
                     MessageBoxCustom mb = new MessageBoxCustom("", "Lỗi hệ thống", MessageType.Error, MessageButtons.OK);
                     mb.ShowDialog();
                     throw;
                 }

             });
            SaveNewPassCM = new RelayCommand<Label>((p) => { return true; }, async (p) =>
             {
                 if (string.IsNullOrEmpty(NewPass))
                 {
                     p.Content = "Không hợp lệ!";
                     p.Visibility = Visibility.Visible;
                 }
                 else
                 {
                     string newPass = NewPass;
                     (bool updatedSuccess, string messageFromUpdate) = await Task<(bool updatedSuccess, string messageFromUpdate)>.Run(() => StaffService.Ins.UpdatePassword(RequestingStaffId, newPass));

                     if (updatedSuccess)
                     {
                         p.Content = "";
                         MessageBoxCustom mb = new MessageBoxCustom("", "Đổi mật khẩu thành công!", MessageType.Success, MessageButtons.OK);
                         mb.ShowDialog();
                         LoginViewModel.MainFrame.Content = new LoginPage();
                     }
                     else
                     {
                         MessageBoxCustom mb = new MessageBoxCustom("", messageFromUpdate, MessageType.Error, MessageButtons.OK);
                         mb.ShowDialog();
                         return;
                     }
                 }
             });
            NewPassChanged = new RelayCommand<PasswordBox>((p) => { return true; }, (p) =>
            {
                NewPass = p.Password;
            });
            SaveSendmailBtnCM = new RelayCommand<Button>((p) => { return true; }, (p) =>
            {
                SendmailBtn = p;
            });
        }

        protected Task SendEmailForStaff(string customerEmail, int randomCode)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string APP_EMAIL = appSettings["APP_EMAIL"];
            string APP_PASSWORD = appSettings["APP_PASSWORD"];

            //SMTP CONFIG
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(APP_EMAIL, APP_PASSWORD);

            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(GetResetPasswordTemplate(randomCode), null, "text/html");

            //Add view to the Email Message
            mail.AlternateViews.Add(htmlView);

            mail.From = new MailAddress(APP_EMAIL, "Squadin Cinema");
            mail.To.Add(customerEmail);
            mail.Subject = "Lấy lại mật khẩu đăng nhập";


            return smtp.SendMailAsync(mail);

        }

        FrameworkElement GetParentWindow(FrameworkElement p)
        {
            FrameworkElement parent = p;

            while (parent.Parent != null)
            {
                parent = parent.Parent as FrameworkElement;
            }
            return parent;
        }

        private string GetResetPasswordTemplate(int randomCode)
        {
            string resetPasswordHTML = Helper.GetEmailTemplatePath(RESET_PASS_TEMPLATE_FILE);
            string HTML = File.ReadAllText(resetPasswordHTML).Replace("{RESET_PASSWORD_CODE}", randomCode.ToString());
            return HTML;
        }
        const string RESET_PASS_TEMPLATE_FILE = "reset_password_html.txt";
    }
}
