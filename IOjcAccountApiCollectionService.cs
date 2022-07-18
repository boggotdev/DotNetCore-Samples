using BOL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tech.business.AccountApis.Interface
{
    public interface IOjcAccountApiCollectionService
    {
        OrgLogin GetOrgLogin(string emailAddress, string password,string merchantId = "");
        Orgenization[] GetOrganizationFull(int orgID, string orgName, string taxID, string paymentURL = "");
        HttpResponseMessage OrganizationSignup(UserTask task);
        int AddFileImage(ImageInfo imageInfo);
        HttpResponseMessage VerifyOrganization(string taxId, decimal lastCheckAmount, int? orgId = null);
        OrgLogin[] GetOrgLogin(int orgId);
        bool ResetOrgLoginPassword(string loginEmailAddress);
        List<RecurrRecommendation> GetOrganizationsRecurrRecommendations(int orgId);
        List<Check> GetOrgChecks(int orgId, DateTime fromDate, DateTime toDate);
        List<VoucherSubmissionBatch> GetVoucherSubmissionBatches(int orgId, BatchSubmissionStatusTypes status);
        bool AddMobileCharge(OrgMobileCharge orgMobileCharge);
        string GetReplyToEmailAddress(string emailType);
         CompanyInfo GetCompanyInfo();
    }
}
