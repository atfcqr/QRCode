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
using System.Web.Configuration;

namespace QRCodeAdmin.Controllers
{
    public class PrjController : BaseController
    {
        // GET: Prj
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            if (Session["Name"] == null)
                AAD_SignIn();

            if (Session["Name"] == null)
                Response.Redirect("../Main/Logout");

            string sql = "select * from Project";

            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            DbCommand cmd = db.GetSqlStringCommand(sql);

            AllVM vm = new AllVM();
            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    Project info = new Project();
                    info.PrjSNO = int.Parse(dr["PrjSNO"].ToString());
                    info.PrjCode = dr["PrjCode"].ToString();
                    info.PrjName = dr["PrjName"].ToString();
                    info.Limited = dr["Limited"].ToString();
                    info.PrjWelcome = dr["PrjWelcome"].ToString();
                    info.PrjFontColor = dr["PrjFontColor"].ToString();
                    info.PrjBgColor = dr["PrjBgColor"].ToString();
                    info.PrjPic = dr["PrjPic"].ToString();
                    vm.PrjList.Add(info);
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();
            return View(vm);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(FormCollection fc)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            Project pInfo = new Project();
            pInfo.PrjCode = fc["PrjCode"].Trim();
            pInfo.PrjName = fc["PrjName"].Trim();
            dbq.Project.Add(pInfo);
            dbq.SaveChanges();

            Jsn j = new Jsn() { msg = "新增專案成功", url = "../Prj/List" };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Del(string PrjSNO)
        {
            int P = int.Parse(PrjSNO);
            Jsn j = null;
            QRCodeEntities dbq = new QRCodeEntities();
            if (dbq.House.Where(x => x.PrjSNO == P).ToList().Count > 0)
            {
                j = new Jsn() { msg = "此專案已新增戶號，無法刪除" };
                return Json(j, JsonRequestBehavior.AllowGet);
            }
            Project pInfo = dbq.Project.Where(x => x.PrjSNO == P).FirstOrDefault();
            dbq.Project.Remove(pInfo);
            dbq.SaveChanges();

            j = new Jsn() { msg = "已刪除" };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(string PrjSNO)
        {
            string sql = "select * from Project where prjsno = " + PrjSNO;

            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            DbCommand cmd = db.GetSqlStringCommand(sql);

            AllVM vm = new AllVM();
            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                if (dr.Read())
                {
                    vm.PrjInfo.PrjSNO = int.Parse(dr["PrjSNO"].ToString());
                    vm.PrjInfo.PrjCode = dr["PrjCode"].ToString();
                    vm.PrjInfo.PrjName = dr["PrjName"].ToString();
                    vm.PrjInfo.Limited = dr["Limited"].ToString();
                    vm.PrjInfo.PrjWelcome = dr["PrjWelcome"].ToString();
                    vm.PrjInfo.PrjFontColor = dr["PrjFontColor"].ToString();
                    vm.PrjInfo.PrjBgColor = dr["PrjBgColor"].ToString();
                    vm.PrjInfo.PrjBannerColor = dr["PrjBannerColor"].ToString();
                    vm.PrjInfo.PrjButFtColor = dr["PrjButFtColor"].ToString();
                    vm.PrjInfo.PrjButBgColor = dr["PrjButBgColor"].ToString();
                    vm.PrjInfo.PrjLogo = dr["PrjLogo"].ToString();
                    vm.PrjInfo.PrjPic = dr["PrjPic"].ToString();
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();
 
            return View(vm);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection fc, HttpPostedFileBase PrjLogo, HttpPostedFileBase PrjPic)
        {
            QRCodeEntities db = new QRCodeEntities();
            AllVM vm = new AllVM();
            int PrjSNO = int.Parse(fc["PrjSNO"]);

            Project pInfo = new Project();
            pInfo = db.Project.Where(x => x.PrjSNO == PrjSNO).FirstOrDefault();
            pInfo.Limited = fc["Limited"].ToString();
            pInfo.PrjWelcome = fc["PrjWelcome"].ToString();
            pInfo.PrjFontColor = fc["PrjFontColor"].ToString();
            pInfo.PrjBgColor = fc["PrjBgColor"].ToString();
            pInfo.PrjBannerColor = fc["PrjBannerColor"].ToString();
            pInfo.PrjButFtColor = fc["PrjButFtColor"].ToString();
            pInfo.PrjButBgColor = fc["PrjButBgColor"].ToString();

            //上傳目錄
            string ff = Server.MapPath(WebConfigurationManager.AppSettings["Pic"] + pInfo.PrjCode);
            if (!System.IO.Directory.Exists(ff))
            {
                System.IO.Directory.CreateDirectory(ff);
            }

            //檔案上傳
            string PrjLogoName = "";
            if (PrjLogo != null && PrjLogo.ContentLength > 0)
            {
                if (!String.IsNullOrEmpty(pInfo.PrjLogo))
                {
                    FileInfo f = new FileInfo(Path.Combine(ff, pInfo.PrjLogo));
                    f.Delete();
                }

                PrjLogoName = String.Format("{0}_Logo{1}", pInfo.PrjCode, Path.GetExtension(PrjLogo.FileName));
                PrjLogo.SaveAs(Path.Combine(ff, PrjLogoName));
                pInfo.PrjLogo = PrjLogoName;
            }
            string PrjPicName = "";
            if (PrjPic != null && PrjPic.ContentLength > 0)
            {
                if (!String.IsNullOrEmpty(pInfo.PrjPic))
                {
                    FileInfo f = new FileInfo(Path.Combine(ff, pInfo.PrjPic));
                    f.Delete();
                }

                PrjPicName = String.Format("{0}_Pic{1}", pInfo.PrjCode, Path.GetExtension(PrjPic.FileName));
                PrjPic.SaveAs(Path.Combine(ff, PrjPicName));
                pInfo.PrjPic = PrjPicName;
            }
            db.SaveChanges();

            Jsn j = new Jsn() { msg = "儲存成功" };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PreView(string PrjSNO)
        {
            string sql = "select * from Project where prjsno = " + PrjSNO;

            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            DbCommand cmd = db.GetSqlStringCommand(sql);

            AllVM vm = new AllVM();
            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                if (dr.Read())
                {
                    vm.PrjInfo.PrjSNO = int.Parse(dr["PrjSNO"].ToString());
                    vm.PrjInfo.PrjCode = dr["PrjCode"].ToString();
                    vm.PrjInfo.PrjName = dr["PrjName"].ToString();
                    vm.PrjInfo.Limited = dr["Limited"].ToString();
                    vm.PrjInfo.PrjWelcome = dr["PrjWelcome"].ToString();
                    vm.PrjInfo.PrjFontColor = dr["PrjFontColor"].ToString();
                    vm.PrjInfo.PrjBgColor = dr["PrjBgColor"].ToString();
                    vm.PrjInfo.PrjPic = dr["PrjPic"].ToString();
                    vm.PrjInfo.PrjLogo = dr["PrjLogo"].ToString();
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();


            return View(vm);
        }
    }
}