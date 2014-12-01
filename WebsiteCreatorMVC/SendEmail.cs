using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteCreatorMVC
{
    public class SendEmail
    {
        public static void Send(string body, bool IsbodyHtml, string subject, string to)
    {
        //MailMessage msg = new MailMessage();
        //msg.Body = body;
        //msg.IsBodyHtml = IsbodyHtml;
        //msg.From = new MailAddress("no-reply@asmoney.com");
        //msg.Subject = subject;
        //msg.To.Add(to);

        //try
        //{
        //    SmtpClient client = new SmtpClient();
        //    client.Send(msg);
        //}
        //catch (System.Exception ex)
        //{

        RestSharp.RestClient client = new RestSharp.RestClient();
        client.BaseUrl = "https://api.mailgun.net/v2";
        client.Authenticator =
                new RestSharp.HttpBasicAuthenticator("api",
                                           "key-3g7l0iqd3ioxemch8-hqi04liuel3px9");
        RestSharp.RestRequest request = new RestSharp.RestRequest();
        request.AddParameter("domain",
                             "asmoney.com", RestSharp.ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter("from", "ManagerWar <no-reply@asmoney.com>");
        request.AddParameter("to", to);
        request.AddParameter("subject", subject);
        request.AddParameter("text", body);
        request.Method = RestSharp.Method.POST;
        client.Execute(request);

    }
    }
}