using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using System.Data;

namespace QRCodeCustomer.Controllers
{
    public class Jsn
    {
        public string msg { get; set; }
        public string url { get; set; }
    }

    public class MainController : Controller
    {
        public ActionResult Index(string tkn)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            AllVM vm = new AllVM();
            ViewBag.ErrMsg = "";

            //token不合法
            if (tkn == null)
            {
                ViewBag.ErrMsg = "您非本建案之客戶，請重新讀取條碼！";
                return View(vm);
            }

            //token不存在
            int cnt = dbq.House.Where(x => x.Token == tkn).ToList().Count();
            if (cnt == 0)
            {
                ViewBag.ErrMsg = "您非本建案之客戶，請重新讀取條碼！";
                return View(vm);
            }

            //條碼過期
            int? psno = dbq.House.Where(x => x.Token == tkn).FirstOrDefault().PrjSNO;
            string lim = dbq.House.Where(x => x.Token == tkn).FirstOrDefault().Limited;
            if (DateTime.Parse(lim) < DateTime.Now)
            {
                ViewBag.ErrMsg = "此條碼已經過期了！";
                return View(vm);
            }

            //取得客戶資訊
            vm.HouInfo.HouCode = dbq.House.Where(x => x.Token == tkn).FirstOrDefault().HouCode;
            vm.HouInfo.Token = dbq.House.Where(x => x.Token == tkn).FirstOrDefault().Token;

            Session.Add("HouToken", vm.HouInfo.Token);


            //撈專案
            Project pInfo = dbq.Project.Where(x => x.PrjSNO == psno).FirstOrDefault();

            vm.HouInfo.PrjSNO = pInfo.PrjSNO;
            vm.HouInfo.PrjCode = pInfo.PrjCode;
            vm.HouInfo.PrjName = pInfo.PrjName;
            vm.HouInfo.PrjWelcome = pInfo.PrjWelcome;
            vm.HouInfo.PrjBgColor = pInfo.PrjBgColor;
            vm.HouInfo.PrjFontColor = pInfo.PrjFontColor;
            vm.HouInfo.PrjBannerColor = pInfo.PrjBannerColor;
            vm.HouInfo.PrjButFtColor = pInfo.PrjButFtColor;
            vm.HouInfo.PrjButBgColor = pInfo.PrjButBgColor;
            vm.HouInfo.PrjPic = pInfo.PrjPic;
            vm.HouInfo.PrjLogo = pInfo.PrjLogo;

            return View(vm);
        }

        public ActionResult PWD(FormCollection fc)
        {
            if (Session["HouToken"] == null)
            {
                //Response.Write("<script>alert('客戶不存在')</script>");
                Response.End();
            }

            QRCodeEntities dbq = new QRCodeEntities();
            string hou = fc["hou"].ToString();
            string tkn = fc["tkn"].ToString();
            string pwd = fc["pwd"].ToString();
            int cnt = dbq.House.Where(x => x.HouPWD == pwd && x.HouCode == hou).ToList().Count;
            if (cnt == 0)
            {
                Jsn j = new Jsn() { msg = "密碼錯誤", url = "" };
                return Json(j, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Jsn j = new Jsn() { msg = "驗證通過", url = "../Main/Show?tkn=" + tkn };
                return Json(j, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Show", "Main", new { tkn = tkn });
            }
        }

        public ActionResult Show(string tkn)
        {
            if (Session["HouToken"] == null)
            {
                //Response.Write("<script>alert('客戶不存在')</script>");
                //Response.End();
                Response.Redirect("../Main/Index");
            }

            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            DbCommand cmd = null;
            IDataReader dr = null;
            string sql = "";
            int PrjSNO = 0;
            int HouSNO = 0;
            AllVM vm = new AllVM();

            //撈戶號
            sql = "select * from House where Token = '" + tkn + "'";
            cmd = db.GetSqlStringCommand(sql);
            using (dr = db.ExecuteReader(cmd))
            {
                if (dr.Read())
                {
                    vm.HouInfo.HouSNO = int.Parse(dr["HouSNO"].ToString());
                    vm.HouInfo.HouCode = dr["HouCode"].ToString();
                    vm.HouInfo.QRCode = dr["QRCode"].ToString();
                    PrjSNO = int.Parse(dr["PrjSNO"].ToString());
                    HouSNO = int.Parse(dr["HouSNO"].ToString());
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();


            //撈專案
            sql = "select * from Project where PrjSNO = " + PrjSNO;
            cmd = db.GetSqlStringCommand(sql);
            using (dr = db.ExecuteReader(cmd))
            {
                if (dr.Read())
                {
                    vm.HouInfo.PrjSNO = int.Parse(dr["PrjSNO"].ToString());
                    vm.HouInfo.PrjCode = dr["PrjCode"].ToString();
                    vm.HouInfo.PrjName = dr["PrjName"].ToString();
                    vm.HouInfo.PrjWelcome = dr["PrjWelcome"].ToString();
                    vm.HouInfo.PrjBgColor = dr["PrjBgColor"].ToString();
                    vm.HouInfo.PrjFontColor = dr["PrjFontColor"].ToString();
                    vm.HouInfo.PrjBannerColor = dr["PrjBannerColor"].ToString();
                    vm.HouInfo.PrjButFtColor = dr["PrjButFtColor"].ToString();
                    vm.HouInfo.PrjButBgColor = dr["PrjButBgColor"].ToString();
                    vm.HouInfo.PrjPic = dr["PrjPic"].ToString();
                    vm.HouInfo.PrjLogo = dr["PrjLogo"].ToString();
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();


            //撈附件
            sql = "";
            sql += "select aa.*, ss.Text as AttTitle ";
            sql += "from SysCode ss ";
            sql += "left join ";
            sql += "( ";
            sql += "select* from Attachs where PrjSNO = " + vm.HouInfo.PrjSNO + " and AttCode = 'a' ";
            sql += "union ";
            sql += "select* from Attachs where HouSNO = " + HouSNO + " ";
            sql += ") aa on ss.Code = aa.AttCode ";
            sql += "where 0 = 0 ";
            sql += "and ss.SysType = '顯示項目' ";
            if (vm.HouInfo.PrjCode == "1180B") //鐫月
                sql += "and (ss.Code not in ('e')) ";
            if (vm.HouInfo.PrjCode == "1190G") //鐫萃
                sql += "and (ss.Code not in ('b')) ";
            sql += "order by ss.Sort ";
            cmd = db.GetSqlStringCommand(sql);
            using (dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    AttPro info = new AttPro();
                    info.AttSNO = (dr["AttSNO"] == DBNull.Value ? 0 : int.Parse(dr["AttSNO"].ToString()));
                    info.AttTitle = dr["AttTitle"].ToString();
                    info.AttNote = dr["AttNote"].ToString();
                    info.AttFileName = dr["AttFileName"].ToString();
                    vm.AttProList.Add(info);
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();


            return View(vm);
        }
    }
}