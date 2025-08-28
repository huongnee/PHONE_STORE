namespace PHONE_STORE.Application.Options
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "";   // vd: smtp.gmail.com | smtp.office365.com | sandbox.smtp.mailtrap.io
        public int Port { get; set; } = 587;     // phần lớn là 587 (STARTTLS)
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = ""; // Gmail/Office: chính là địa chỉ email
        public string Password { get; set; } = ""; // App Password (Gmail) hoặc mật khẩu SMTP
        public string FromEmail { get; set; } = ""; // địa chỉ người gửi (nên trùng Username)
        public string FromName { get; set; } = "Phone Store";
    }
}
