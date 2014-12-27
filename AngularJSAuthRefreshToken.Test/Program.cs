using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var smtp = new SmtpClient();

                smtp.SendMailAsync("test@test.com", "test@test.bg", "subject ...", "body ...").Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }
    }
}