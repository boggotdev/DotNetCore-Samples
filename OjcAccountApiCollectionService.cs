using BOL;
using Newtonsoft.Json;
using Tech.business.AccountApis.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebAPIFunctions;

namespace Tech.business.AccountApis.Service
{
    public class OjcAccountApiCollectionService : IOjcAccountApiCollectionService
    {

        private static HttpClient _httpClient = WebApiCollection._httpClient;

        /// <summary>
        /// login for organization
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="password"></param>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        public OrgLogin GetOrgLogin(string emailAddress, string password,string merchantId)
        {
            return CallWebAPI.GetOrgLogin(emailAddress, password, merchantId);
           
        }
        /// <summary>
        /// getting organization detail
        /// </summary>
        /// <param name="orgID"></param>
        /// <param name="orgName"></param>
        /// <param name="taxID"></param>
        /// <param name="paymentURL"></param>
        /// <returns></returns>
        public  Orgenization[] GetOrganizationFull(int orgID, string orgName, string taxID, string paymentURL = "")
        {
            return CallWebAPI.GetOrganizationFull(orgID,orgName,taxID,paymentURL);
           
        }
        /// <summary>
        /// Signup organization
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public  HttpResponseMessage OrganizationSignup(UserTask task)
        {
            return CallWebAPI.AddTask(task);
           
        }
        /// <summary>
        /// Add image at the time organization SignUp
        /// </summary>
        /// <param name="imageInfo"></param>
        /// <returns></returns>
        public  int AddFileImage(ImageInfo imageInfo)
        {
            return CallWebAPI.AddFileImage(imageInfo);
            
        }

        /// <summary>
        /// verify organization
        /// </summary>
        /// <param name="taxId"></param>
        /// <param name="lastCheckAmount"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public HttpResponseMessage VerifyOrganization(string taxId, decimal lastCheckAmount, int? orgId = null)
        {
            return CallWebAPI.VerifyOrganization(taxId,lastCheckAmount,orgId);
        }

        public  OrgLogin[] GetOrgLogin(int orgId)
        {
            return CallWebAPI.GetOrgLogin(orgId);
           
        }
        /// <summary>
        /// Forgot Password
        /// </summary>
        /// <param name="loginEmailAddress"></param>
        /// <returns></returns>
        public bool ResetOrgLoginPassword(string loginEmailAddress)
        {
            return CallWebAPI.ResetOrgLoginPassword(loginEmailAddress);
         
        }
        /// <summary>
        /// get Recomenmdor for the org
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public  List<RecurrRecommendation> GetOrganizationsRecurrRecommendations(int orgId)
        {
            return CallWebAPI.GetOrganizationsRecurrRecommendations(orgId);
        }
        /// <summary>
        /// Get Organization checks
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public  List<Check> GetOrgChecks(int orgId, DateTime fromDate, DateTime toDate)
        {
            return CallWebAPI.GetOrgChecks(orgId,fromDate,toDate);
        }
        /// <summary>
        /// Get submitted batches voucher
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<VoucherSubmissionBatch> GetVoucherSubmissionBatches(int orgId, BatchSubmissionStatusTypes status)
        {
            return CallWebAPI.GetVoucherSubmissionBatches(orgId,status);
        }
        /// <summary>
        /// Add Device
        /// </summary>
        /// <param name="orgMobileCharge"></param>
        /// <returns></returns>
        public bool AddMobileCharge(OrgMobileCharge orgMobileCharge)
        {
            string json = JsonConvert.SerializeObject(orgMobileCharge);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync("OrgMobileCharge", content).Result;

            return response.IsSuccessStatusCode;
        }
        /// <summary>
        /// reply to email
        /// </summary>
        /// <param name="emailType"></param>
        /// <returns></returns>
        public string GetReplyToEmailAddress(string emailType)
        {
            string replayEmailAddress = "";
            HttpResponseMessage response = _httpClient.GetAsync("CompanyInfo/ReplyEmailToAddress?EmailType=" + emailType).Result;
            if (response.IsSuccessStatusCode)
                replayEmailAddress = response.Content.ReadAsAsync<string>().Result;
            return replayEmailAddress;
        }

        /// <summary>
        /// Get Company Info
        /// </summary>
        /// <returns></returns>
        public CompanyInfo GetCompanyInfo()
        {
            CompanyInfo companyInfo = null;
            HttpResponseMessage response = _httpClient.GetAsync("CompanyInfo").Result;
            if (response.IsSuccessStatusCode)
                companyInfo = response.Content.ReadAsAsync<CompanyInfo>().Result;

            return companyInfo;
        }
    }
}
