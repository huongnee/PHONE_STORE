using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Options
{
    public class JwtKey
    {
        public string Kid { get; set; } = "";  // mã định danh khóa, ví dụ "2025-08"
        public string Key { get; set; } = "";  // secret (base64 hoặc plain string)
    }
    //Mục đích: gom toàn bộ cấu hình liên quan đến JWT lại để dễ bind từ appsettings.json hoặc secrets.
    public class JwtOptions
    {
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public int AccessTokenMinutes { get; set; } = 15;

        public string ActiveKid { get; set; } = "";     // Khóa đang dùng để KÝ
        public List<JwtKey> Keys { get; set; } = new(); // Danh sách KHÓA được chấp nhận để VERIFY
    }

}
