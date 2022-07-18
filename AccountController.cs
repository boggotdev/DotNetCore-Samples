using BOL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Tech.Models.OrganizationRegisterModel;

namespace Tech.Controllers
{
    public class AccountController : Controller
    {
        private readonly IOjcAccountApiCollectionService _ojcAccountApiCollectionService;
        public static string alertmessage { get; set; }
        public static string SomethingWrongalertmessage { get; set; }
        public AccountController(IOjcAccountApiCollectionService ojcAccountApiCollectionService)
        {
            _ojcAccountApiCollectionService = ojcAccountApiCollectionService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.SuccessAlert = alertmessage;
            alertmessage = null;
            ViewBag.SomethingWrong = SomethingWrongalertmessage;
            SomethingWrongalertmessage = null;
            return View();
        }

        /// <summary>
        /// Login for the Registered Organization
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Verify(UserLogin userLogin)
        {
            var user = AuthenticateUser(userLogin.EmailAddress, userLogin.password);

            if (user == null)
            {
                SomethingWrongalertmessage = "Dont find any account with provided cred";
                //return RedirectToAction("dashboard","home");
                //ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                ViewData["NotAuthenticated"] = true;
                return View("login");
            }


            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.PrimarySid, user.OrgId.ToString()),
                    new Claim(ClaimTypes.Name, user.AccountNo),
                    new Claim(ClaimTypes.Email, user.EmailAddress),
                    new Claim("OrgName", user.OrgName),
                    new Claim(ClaimTypes.Role, "Org"),
                    new Claim("OrgLoginId", user.OrgLoginId.ToString())
                };



            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                // Refreshing the authentication session should be allowed.

                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.                    

                IsPersistent = false,
                // Whether the authentication session is persisted across 
                // multiple requests. Required when setting the 
                // ExpireTimeSpan option of CookieAuthenticationOptions 
                // set with AddCookie. Also required when setting 
                // ExpiresUtc.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            HttpContext.SignInAsync(
               CookieAuthenticationDefaults.AuthenticationScheme,
                           new ClaimsPrincipal(claimsIdentity),
                           authProperties);
            // Something failed. Redisplay the form.
            //return Page();
            return RedirectToAction("dashboard", "home");
        }
        /// <summary>
        /// Authenticate User with Backend
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private UserLogin AuthenticateUser(string email, string password)
        {
            // For demonstration purposes, authenticate a user
            // with a static email address. Ignore the password.
            // Assume that checking the database takes 500ms
            var login = _ojcAccountApiCollectionService.GetOrgLogin(email, password);

            if (login != null)
            {
                var org = _ojcAccountApiCollectionService.GetOrganizationFull(login.ForOrgId, "", "").FirstOrDefault();
                return new Models.UserLogin()
                {
                    EmailAddress = email,
                    Username = login.LoginEmailAddress,
                    OrgName = org.OrgName,
                    OrgId = login.ForOrgId,
                    OrgLoginId = login.OrgLoginInfoId,
                    AccountNo = login.ForOrgId.ToString()
                };
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// get method for RegisterOrganization view
        /// </summary>
        /// <returns></returns>
        public IActionResult RegisterOrganization()
        {
            return View();

        }
        /// <summary>
        /// organization Sign_Up
        /// </summary>
        /// <param name="organizationSignup"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SignUpOrg(OrganizationSignup organizationSignup)
        {
            var taskDetails = $"Tax Id: {organizationSignup.TaxId}\n" +
                $"Org. Name: {organizationSignup.OrgName}\n" +
                $"Address: {organizationSignup.Address}\n" +
                $"City: {organizationSignup.City}\n" +
                $"State: {organizationSignup.State}\n" +
                $"Postal Code: {organizationSignup.PostalCode}\n" +
                $"Phone no: {organizationSignup.PhoneNo}\n" +
                $"Email address: {organizationSignup.EmailAddress}\n" +
                $"Contact person: {organizationSignup.ContactName}";

            var rslt = _ojcAccountApiCollectionService.OrganizationSignup(new UserTask()
            {
                IsClient = true,
                Client = new ClientAccount() { ClientAccountID = 101 },
                ForId = 101,
                Subject = "New organization",
                Datails = taskDetails,
                CreatedByUserId = 39,
                SignedForUserId = 19,
                TaskType = TaskTypes.NewOrganizationApplication
            });
            if (rslt.IsSuccessStatusCode)
            {
                var taskId = rslt.Content.ReadAsStringAsync().Result;

                if (organizationSignup.AddressVerificationImage != null)
                {
                    byte[] file;
                    using (var memStream = new MemoryStream())
                    {
                        organizationSignup.AddressVerificationImage.CopyTo(memStream);
                        file = memStream.ToArray();
                    }

                    var imageId = _ojcAccountApiCollectionService.AddFileImage(new ImageInfo()
                    {
                        FileName = organizationSignup.AddressVerificationImage.FileName,
                        ImageExt = Path.GetExtension(organizationSignup.AddressVerificationImage.FileName),
                        ImageData = file,
                        InformationType = InformationTypes.Task,
                        ReferenceId = Convert.ToInt32(taskId)
                    });
                }

                TempData["Message"] = "Your application was received. Please allow up to 2 business days to process.Thanks";

                //return Redirect("message");
                return Redirect("MessageBox");

            }
            return RedirectToAction("SignUpOrganization");
        }

       /// <summary>
       /// reset password using email
       /// </summary>
       /// <param name="EmailAddress"></param>
       /// <returns></returns>
        //[AllowAnonymous]
        [HttpPost]
        public IActionResult forgotPassword(string EmailAddress)
        {
            if (!string.IsNullOrWhiteSpace(EmailAddress))
            {
                if (_ojcAccountApiCollectionService.ResetOrgLoginPassword(EmailAddress))
                    return Json(new { Success = true, ResponseMessage = "A temporary password will be emailed shortly to " + EmailAddress + "." });
                else
                    return Json(new { Success = false, ResponseMessage = "The account email address that you entered could not be found in the system, please check it and try again" });
            }
            else
                return Json(new { Success = false, ResponseMessage = "Please complete the form." });
        }
      

        public IActionResult SignUpOrganization()
        {
            return View();
        }
        public void OnGet(OrganizationRegisterModel organizationRegisterModel)
        {
            organizationRegisterModel.MultipleOrgs = new SelectList(new List<orgId_Name>(), nameof(orgId_Name.OrgenizationID), nameof(orgId_Name.OrgName));
        }
        /// <summary>
        /// organization register
        /// </summary>
        /// <param name="organizationRegisterModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult OrgRegister(OrganizationRegisterModel organizationRegisterModel)
        {
            if (!ModelState.IsValid)
            {
                organizationRegisterModel.MultipleOrgs = new SelectList(new List<orgId_Name>(), nameof(orgId_Name.OrgenizationID), nameof(orgId_Name.OrgName));
                return View("RegisterOrganization");
            }

            if (!string.IsNullOrWhiteSpace(organizationRegisterModel.TaxId))
                organizationRegisterModel.TaxId = organizationRegisterModel.TaxId.Replace("-", "");

            var orgId = organizationRegisterModel.SelectedOrg != null ? new int?(organizationRegisterModel.SelectedOrg.Value) : new int?();

            //reload values in org list
            if (orgId > 0)
            {
                var rslts = _ojcAccountApiCollectionService.VerifyOrganization(organizationRegisterModel.TaxId, organizationRegisterModel.LastCheckAmount, null);
                var fill_orgs = rslts.Content.ReadAsStringAsync().Result;
                FillMultipleOrg(fill_orgs);
            }

            var response = _ojcAccountApiCollectionService.VerifyOrganization(organizationRegisterModel.TaxId, organizationRegisterModel.LastCheckAmount, orgId > 0 ? orgId : null);

            if (response.IsSuccessStatusCode)
            {
                Orgenization orgenization = null;

                if (orgId > 0)
                    orgenization = _ojcAccountApiCollectionService.GetOrganizationFull(orgId.Value, "", "").FirstOrDefault();
                else
                    orgenization = _ojcAccountApiCollectionService.GetOrganizationFull(0, "", organizationRegisterModel.TaxId).FirstOrDefault();

                if (orgenization == null)
                {
                    organizationRegisterModel.ErrorMessage = "This organization is not active. Please call us at: 718-599-1400 Ext. 106 to reactivate this organization";
                    return View("RegisterOrganization");
                    //return Page();
                }

                var orglogins = _ojcAccountApiCollectionService.GetOrgLogin(orgenization.OrgenizationID);

                if (orglogins != null && orglogins.Length > 0)
                {
                    organizationRegisterModel.ErrorMessage = $"This organization was already registered. Please <a href='login'>Click here</a> to go the login page";

                    return View("RegisterOrganization");
                }


                return RedirectToPage("organizationregistrationInfo", "WithPassedOrg", new { orgId = Helpers.Encoder.Encode(orgenization.OrgenizationID) });
            }
            else
            {
                var code = response.StatusCode;
                if (code == System.Net.HttpStatusCode.MultipleChoices)
                {
                    var orgs = response.Content.ReadAsStringAsync().Result;
                    var orgslist = FillMultipleOrg(orgs);
                    ViewBag.MultipleOrgs = orgslist;

                    return View("RegisterOrganization");
                }
                organizationRegisterModel.ErrorMessage = response.Content.ReadAsStringAsync().Result;
                organizationRegisterModel.ErrorMessage = organizationRegisterModel.ErrorMessage.Replace("\"", "");
                ViewBag.ErrorMessage = "Your organization was not found in our system. Please check your Tax ID and try again";
                return View("RegisterOrganization");//need view to add 
            }
        }
        /// <summary>
        /// ContactUs
        /// </summary>
        /// <returns></returns>
        public IActionResult ContactUs()
        {
            return View();
        }
        /// <summary>
        /// Account,payment,device,change password page
        /// </summary>
        /// <returns></returns>
        public IActionResult AccountSetting()
        {
            return View();
        }
        /// <summary>
        /// filling multiple organization need to confirm the flow once 
        /// </summary>
        /// <param name="json"></param>
        SelectList FillMultipleOrg(string json)
        {
            OrganizationRegisterModel registerOrganization = new OrganizationRegisterModel();
            var list = JsonConvert.DeserializeObject<List<orgId_Name>>(json);

            list.Insert(0, new orgId_Name() { OrgenizationID = 0, OrgName = "Please select your organization" });
            return registerOrganization.MultipleOrgs = new SelectList(list, nameof(orgId_Name.OrgenizationID), nameof(orgId_Name.OrgName));

        }

        public IActionResult OrganizationSetting()
        {
            return View();
        }
        /// <summary>
        /// send verification code on given number
        /// </summary>
        /// <param name="phoneNo"></param>
        /// <returns></returns>
        public IActionResult OnGetVerifyPhone([FromBody] RegisterPhoneModel model)
        {
            var rslt = TwilioSMS.Verify.SendCode(model.MobileNumber);
            ViewBag.Number = model.MobileNumber;
            if (rslt)
                return new OkResult();
            else
                return BadRequest();

        }
        /// <summary>
        /// register device with verifycode
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Verifycode([FromBody] RegisterPhoneModel model)
        {
            if (model.MobileNumber != null && !model.MobileNumber.All(char.IsDigit))
                ModelState.AddModelError("MobileNumber", "Only digits are allowed in phone number");
            if (string.IsNullOrWhiteSpace(model.VerificationCode))
                ModelState.AddModelError("VerificationCode", "Verification Code is required");
            var rslt2 = TwilioSMS.Verify.VerifyCode(model.MobileNumber, model.VerificationCode);

            if (!rslt2)
                ModelState.AddModelError("VerificationCode", "Verification Code is NOT valid");

            if (!ModelState.IsValid)
            {
                //return Page();
            }

            var id = Convert.ToInt32(User.Identity.Name);

            var mobile = new OrgMobileCharge() { ForOrgId = id, IsActive = true, MobileNumber = model.MobileNumber, NameOfOwner = model.NameOfOwner };


            var rslt = _ojcAccountApiCollectionService.AddMobileCharge(mobile);

            if (rslt)
                return RedirectToPage("orginformation");
            else
            {
                //ErrorMessage = $"Mobile number {MobileNumber} was not registerd";
                //return Page();
            }
            return null;
        }

        /// <summary>
        /// logout
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            //HttpContext.Session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("login");
        }
        public IActionResult MessageBox()
        {
            return View();
        }
        public IActionResult Back()
        {
            return Redirect("login");
        }
    }
}

