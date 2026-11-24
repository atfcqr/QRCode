using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using System.Data;

using System.IO;
using Models;
using System.Web.Security; //FormsAuthentication

//Claims
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;

namespace QRCodeAdmin.Controllers
{
    public class MainController : BaseController
    {
        [HttpGet]
ㄆ        public ActionResult Login()
        {
            return View();
            
        }

        [HttpPost]
        public ActionResult Login(FormCollection fc)
        {
            string acc = fc["acc"].ToString();
            //string pwd = fc["pwd"].ToString();
            Jsn j = null;

            QRCodeEntities dbq = new QRCodeEntities();
            int cnt = dbq.Users.Where(x => x.GID == acc).ToList().Count;
            if (cnt == 0)
            {
                j = new Jsn() { msg = "帳號或密碼錯誤", url = "javascript:void();" };
                return Json(j, JsonRequestBehavior.AllowGet);
            }

            Users u = dbq.Users.Where(x => x.GID == acc).FirstOrDefault();
            if (u.Enabled == 0)
            {
                j = new Jsn() { msg = "帳號停用", url = "javascript:void();" };
                return Json(j, JsonRequestBehavior.AllowGet);
            }

            String RoleName = dbq.SysCode.Where(x => x.SysType == "角色" && x.Value == u.Roles).FirstOrDefault().Text;
            Session.Add("Name", u.Name);
            Session.Add("Role", u.Roles);
            Session.Add("RoleName", RoleName);


            j = new Jsn() { msg = "登入成功", url = "../Prj/List" };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            //清除所有的 session
            Session.RemoveAll();

            //AAD登出
            AAD_SignOut();

            return null;
            //return Redirect("../Main/Login");
        }

        public ActionResult AAD_Login()
        {
            //AAD登入
            //SignIn();
            
            return View();
        }

        public ActionResult AAD_Transfer()
        {
            var userClaims = User.Identity as System.Security.Claims.ClaimsIdentity;

            var email = userClaims.Name;
            QRCodeEntities dbq = new QRCodeEntities();
            int cnt = dbq.Users.Where(x => x.Email == email).ToList().Count;
            if (cnt == 0)
            {
                //帳號密碼錯誤
                //Response.Write("<script>alert('您非系統使用者，請洽系統管理員');</script>");
                Response.Redirect("../Main/AAD_Login?errormessage=您非系統使用者，請洽系統管理員");
                Response.End();
            }

            Users u = dbq.Users.Where(x => x.Email == email).FirstOrDefault();
            if (u.Enabled == 0)
            {
                //帳號停用
                //Response.Write("<script>alert('您的帳號已經停用了，請洽系統管理員');</script>");
                Response.Redirect("../Main/AAD_Login?errormessage=您的帳號已經停用了，請洽系統管理員");
                Response.End();
            }

            string RoleName = dbq.SysCode.Where(x => x.SysType == "角色" && x.Value == u.Roles).FirstOrDefault().Text;
            Session.Add("Name", u.Name);
            Session.Add("Role", u.Roles);
            Session.Add("RoleName", RoleName);

            Response.Redirect("/Prj/List");
            Response.End();

            return null;
        }

        //Add user's claims to viewbag
        public async Task<ActionResult> AAD_Claims()
        {
            var userClaims = User.Identity as System.Security.Claims.ClaimsIdentity;

            // You get the user’s first and last name below:
            ViewBag.Name = userClaims?.FindFirst("name")?.Value;

            // The subject/ NameIdentifier claim can be used to uniquely identify the user across the web
            ViewBag.Subject = userClaims?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // TenantId is the unique Tenant Id - which represents an organization in Azure AD
            ViewBag.TenantId = userClaims?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            // You can also call Microsoft Graph (with incremental consent)
            try
            {
                var me = await this.GetGraphServiceClient().Me.Request().GetAsync();
                ViewBag.Username = me.DisplayName;
            }
            catch (ServiceException graphEx) when (graphEx.InnerException is MicrosoftIdentityWebChallengeUserException)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                return View();
            }

            return View();
        }
    }
}