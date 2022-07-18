using Tech.business;
using Tech.business.AccountApis;
using Tech.business.AccountApis.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Tech.Helpers
{
    public class EmailHelper
    {
        private readonly IOjcAccountApiCollectionService _ojcAccountApiCollectionService;
        public EmailHelper()
        {
          
        }
        public void SendEmail(EmailTypes emailType, string sendTo, string subject, string body, bool bccOJC = true, bool isHtml = false, string displayAs = "", string fileAttachment = "", List<Attachment> attachments = null, string cc = "")
        {

            var companyInfo = _ojcAccountApiCollectionService.GetCompanyInfo();

            SmtpClient mailClient = new SmtpClient(companyInfo.BulkSmtp);
            mailClient.Credentials = new NetworkCredential(companyInfo.SmtpUserName, companyInfo.SmtpPassword);
            mailClient.Port = 587;
            mailClient.EnableSsl = companyInfo.EnabelSsl;

            MailMessage message = new MailMessage();

            //if (string.IsNullOrWhiteSpace(fromEmailAddress))
            message.From = new MailAddress(companyInfo.BulkEmailAddress, string.IsNullOrWhiteSpace(displayAs) ? "The Tech Fund" : displayAs);
            //else
            //    message.From = new MailAddress(fromEmailAddress);

            
            string mailAddress = "";
            var replyTo = "";
            switch (emailType)
            {
               
                case EmailTypes.DonationToOJC:                    
                case EmailTypes.Recommendation:
                    mailAddress = companyInfo.EmailAddress;
                    break;
                case EmailTypes.None:
                    mailAddress = sendTo;
                    break;
                case EmailTypes.SendToClient_DonationConfirmation:
                case EmailTypes.SendToClient_RecommendationConfirmation:
                    mailAddress = sendTo;
                    break;
                default:
                    break;
            }

            switch (emailType)
            {
                case EmailTypes.None:
                    break;
                case EmailTypes.DonationToOJC:
                    replyTo = _ojcAccountApiCollectionService.GetReplyToEmailAddress("Receipts");
                    break;
                case EmailTypes.Recommendation:                    
                    break;
                case EmailTypes.SendToClient_DonationConfirmation:
                    replyTo = _ojcAccountApiCollectionService.GetReplyToEmailAddress("Receipts");
                    break;
                case EmailTypes.SendToClient_RecommendationConfirmation:
                    break;
                default:
                    break;
            }

            var mailAddessList = mailAddress.Split(',');

            foreach (var address in mailAddessList)
            {
                message.To.Add(address);
            }

            var ccEmailAddressLit = cc.Split(',');

            foreach (var address in ccEmailAddressLit.Where(a=> !string.IsNullOrWhiteSpace(a)) )
            {
                message.CC.Add(address);
            }

            //message.To.Add(mailAddress);

            if (bccOJC)
                message.Bcc.Add(companyInfo.EmailAddress);
            //message.Bcc.Add("samlebowitz@gmail.com");

            message.Subject = subject;
            message.Body = body;
            if (!string.IsNullOrWhiteSpace(fileAttachment))
                message.Attachments.Add(new Attachment(fileAttachment));

            if (string.IsNullOrWhiteSpace(replyTo))
                replyTo = companyInfo.EmailAddress;

            if (!string.IsNullOrWhiteSpace(replyTo))
                message.ReplyToList.Add(replyTo);

            if (attachments != null)
                foreach (var att in attachments)
                {
                    message.Attachments.Add(att);
                }

           

            message.IsBodyHtml = isHtml;

            if(emailType == EmailTypes.DonationToOJC)
                message.Bcc.Add("statements@ojcfund.org");

            mailClient.EnableSsl = companyInfo.EnabelSsl;
            mailClient.SendAsync(message, null);
            
        }

        public enum EmailTypes
        {
            None = 0,
            DonationToOJC = 1,
            Recommendation,
            SendToClient_DonationConfirmation,
            SendToClient_RecommendationConfirmation
        }      
    }
}
