using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using System.Data;


using iTextSharp.text;
using iTextSharp.text.pdf;

using ZXing;                  // for BarcodeWriter
using ZXing.QrCode;           // for QRCode Engine

using Models;
using System.Web.Configuration;
using System.Web.Security;
using System.Text;
using System.IO;

namespace QRCodeAdmin.Controllers
{
    public class HouController : Controller
    {
        public ActionResult List(string PrjSNO)
        {
            if (Session["Name"] == null)
                Response.Redirect("../Main/Logout");

            string sql = "";
            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            QRCodeEntities dbq = new QRCodeEntities();
            DbCommand cmd = null;
            AllVM vm = new AllVM();

           //專案選單
            sql = "select PrjSNO as val, PrjName txt, PrjCode as cod from Project order by PrjCode ";
            cmd = db.GetSqlStringCommand(sql);
            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    SelectInfo sInfo = new SelectInfo();
                    sInfo.val = dr["val"].ToString();
                    sInfo.txt = dr["txt"].ToString();
                    sInfo.cod = dr["cod"].ToString();
                    vm.PrjOption.Add(sInfo);
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();

            if (PrjSNO == null || PrjSNO == "")
            {
                return View(vm);
            }
            else
            {
                vm.PrjInfo.PrjSNO = int.Parse(PrjSNO);
                vm.PrjInfo.PrjCode = dbq.Project.Where(x => x.PrjSNO == vm.PrjInfo.PrjSNO).FirstOrDefault().PrjCode;

                //產生專案資料夾
                GenQRCodeFolder(vm.PrjInfo.PrjCode);

                //產生戶號
                //GenWellanHou(PrjCode);

                //產生QRCode
                sql = "select h.*, p.PrjCode, p.PrjName, p.Limited, "; 
                sql += "(select count(*) from Attachs where (PrjSNO = p.PrjSNO and AttCode = 'a') or (PrjSNO = p.PrjSNO and HouSNO = h.HouSNO and AttFileName is not null)) as AttCount ";
                sql += "from House h, Project p ";
                sql += "where p.PrjSNO = h.PrjSNO ";
                sql += "and h.PrjSNO = '" + PrjSNO + "' ";
                sql += "order by p.PrjCode, h.HouCode ";
                cmd = db.GetSqlStringCommand(sql);
                using (IDataReader dr = db.ExecuteReader(cmd))
                {
                    while (dr.Read())
                    {
                        HousePro pro = new HousePro();
                        pro.HouSNO = int.Parse(dr["HouSNO"].ToString());
                        pro.PrjSNO = int.Parse(dr["PrjSNO"].ToString());
                        pro.HouCode = dr["HouCode"].ToString();
                        pro.HouPWD = (dr["HouPWD"] == null || dr["HouPWD"].ToString() == "" ? GenPWD(pro.HouSNO) : dr["HouPWD"].ToString()); //產生隨機密碼
                        pro.QRCode = dr["QRCode"].ToString();
                        pro.PrjCode = dr["PrjCode"].ToString();
                        pro.PrjName = dr["PrjName"].ToString();
                        pro.Limited = dr["Limited"].ToString();
                        pro.Token = dr["Token"].ToString();
                        pro.AttCount = dr["AttCount"].ToString();
                        vm.HouList.Add(pro);
                        GenQRCode(pro.PrjCode, pro.HouCode, pro.Token); //產生QRCode
                    }
                }
                cmd.Connection.Dispose();
                cmd.Dispose();
                return View(vm);
            }
        }

        public ActionResult PreView(string PrjSNO, string HouSNO)
        {
            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            DbCommand cmd = null;
            IDataReader dr = null;
            string sql = "";
            AllVM vm = new AllVM();
            HouSNO = (HouSNO == null || HouSNO == "" ? "0" : HouSNO);

            //撈戶號
            sql = "select * from House where HouSNO = " + HouSNO;
            cmd = db.GetSqlStringCommand(sql);
            using (dr = db.ExecuteReader(cmd))
            {
                if (dr.Read())
                {
                    vm.HouInfo.HouSNO = int.Parse(dr["HouSNO"].ToString());
                    vm.HouInfo.HouCode = dr["HouCode"].ToString();
                    vm.HouInfo.QRCode = dr["QRCode"].ToString();
                }
                else
                {
                    vm.HouInfo.HouSNO = 0;
                    vm.HouInfo.HouCode = "ＯＯＯ";
                    vm.HouInfo.QRCode = "";
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
            sql += "select* from Attachs where HouSNO <> 0 and HouSNO = " + HouSNO + " ";
            sql += ") aa on ss.Code = aa.AttCode ";
            sql += "where 0 = 0 ";
            sql += "and ss.SysType = '顯示項目' ";
            if (vm.HouInfo.PrjCode == "1180B") //鐫月
                sql += "and (ss.Code not in ('e')) ";
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

        public ActionResult Add(string PrjSNO)
        {
            QRCodeEntities dbq = new QRCodeEntities();

            AllVM vm = new AllVM();
            int P = int.Parse(PrjSNO);
            vm.PrjInfo.PrjSNO = P;
            vm.PrjInfo.PrjCode = dbq.Project.Where(x => x.PrjSNO == P).FirstOrDefault().PrjCode;
            vm.PrjInfo.PrjName = dbq.Project.Where(x => x.PrjSNO == P).FirstOrDefault().PrjName;
            return View(vm);
        }

        [HttpPost]
        public ActionResult Add(FormCollection fc)
        {
            int P = int.Parse(fc["PrjSNO"].ToString());

            QRCodeEntities dbq = new QRCodeEntities();
            string Limited = dbq.Project.Where(x => x.PrjSNO == P).FirstOrDefault().Limited;
            //string HouSNOs = RankHou(fc["HouSNOs"].ToString());
            string HouSNOs = fc["HouSNOs"].ToString();
            string err = "";
            string[] stringSeparators = new string[] { "\r\n" };
            if (HouSNOs.Split(stringSeparators, StringSplitOptions.None).Length > 0)
            {
                string[] row = HouSNOs.Split(stringSeparators, StringSplitOptions.None);
                foreach (string h in row)
                {
                    if (h.Split(',').Length > 0)
                    {
                        string HC = h.Split(',')[0].ToString().Trim();
                        string PW = h.Split(',')[1].ToString().Trim();
                        if (dbq.House.Where(x => x.PrjSNO == P && x.HouCode == HC).Count() == 0)
                        {
                            House hInfo = new House();
                            hInfo.PrjSNO = P;
                            hInfo.PrjCode = fc["PrjCode"].ToString();
                            hInfo.HouCode = HC;
                            hInfo.HouPWD = PW;
                            //hInfo.HouPWD = GetRandomString();
                            hInfo.Token = Guid.NewGuid().ToString();
                            hInfo.QRCode = fc["PrjCode"].ToString() + "_" + HC + ".jpg";
                            hInfo.Limited = Limited;
                            dbq.House.Add(hInfo);
                            dbq.SaveChanges();
                        }
                        else
                        {
                            House hInfo = dbq.House.Where(x => x.PrjSNO == P && x.HouCode == HC).FirstOrDefault();
                            hInfo.HouPWD = PW;
                            dbq.SaveChanges();
                        }
                    }
                }
            }
            Jsn j = new Jsn() { msg = "新增戶號成功", url = "../Hou/List?PrjSNO=" + fc["PrjSNO"].ToString() };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddPre(FormCollection fc)
        {
            int P = int.Parse(fc["PrjSNO"].ToString());
            string str = RankHou(fc["HouSNOs"].ToString());

            Jsn j = new Jsn() { msg = str };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Del(string PrjSNO, string PrjCode, string HouSNO) {

            try
            {
                //刪除附件
                FileInfo f = null;
                int H = int.Parse(HouSNO);
                QRCodeEntities dbq = new QRCodeEntities();
                House hInfo = dbq.House.Where(x => x.HouSNO == H).FirstOrDefault();
                IList<Attachs> aList = dbq.Attachs.Where(x => x.HouSNO == H).ToList();

                //刪QRCode檔
                f = new FileInfo(Path.Combine(Server.MapPath("../Pic/" + PrjCode), hInfo.QRCode));
                if (f.Exists)
                    f.Delete();

                //刪PDF檔
                foreach (Attachs aInfo in aList)
                {
                    f = new FileInfo(Path.Combine(Server.MapPath("../Upload/" + PrjCode), aInfo.AttFileName));
                    if (f.Exists)
                        f.Delete();
                }

                //刪除記錄
                dbq.House.Remove(hInfo);
                foreach (Attachs aInfo in aList)
                    dbq.Attachs.Remove(aInfo);
                dbq.SaveChanges();

                Jsn j = new Jsn() { msg = "刪除完成", url = "../Hou/List?PrjSNO=" + PrjSNO };
                return Json(j, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Jsn j = new Jsn() { msg = ex.Message, url = "../Hou/List?PrjSNO=" + PrjSNO };
                return Json(j, JsonRequestBehavior.AllowGet);
            }
        }

        public string RankHou(string HouSNOs)
        {
            HouSNOs = HouSNOs.Replace("\r\n", ",").Replace("\n", "").Replace("\r", "").Replace(" ", ",");
            HouSNOs = "," + HouSNOs + ",";
            string str = "";
            if (HouSNOs.Split(',').Length > 0)
            {
                Array.Sort(HouSNOs.Split(','));
                foreach (string h in HouSNOs.Split(','))
                {
                    if (h != "")
                        if (!("," + str.ToLower()).Contains("," + h.ToLower() + ","))
                            str += h + ",";
                }
            }

            if (str != "")
            {
                str = str.Substring(0, str.Length - (",").Length);
            }

            return str;
        }

        public void GenWellanHou(string PrjCode)
        {
            //讀取偉盟的戶號
            Database db = DatabaseFactory.CreateDatabase("DataContext_SALE_CDC");
            DbCommand cmd = null;
            string sql = "";
            sql += "select s.cus_idno, c.hou_pla_no, c.hou_bld_no, c.hou_now_custom ";
            sql += "from ctcustom c ";
            sql += "left join sacustom s on s.cus_pla_no = c.hou_pla_no and s.cus_bld_no = c.hou_bld_no and s.cus_no = c.hou_now_custom ";
            sql += "where 0 = 0 ";
            sql += "and hou_pla_no = '" + PrjCode + "' ";
            sql += "order by c.hou_pla_no ";

            cmd = db.GetSqlStringCommand(sql);
            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    //新增偉盟戶號
                    //AddWellanHou(dr["hou_pla_no"].ToString(), dr["hou_bld_no"].ToString());

                    //更新偉盟戶號
                    UpdWellanHou(dr["hou_pla_no"].ToString(), dr["hou_bld_no"].ToString());

                    //更新密碼
                    //UpdWellanPWD(dr["hou_pla_no"].ToString(), dr["hou_bld_no"].ToString(), dr["cus_idno"].ToString());

                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();
        }

        public void AddWellanHou(string hou_pla_no, string hou_bld_no)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            string HouCode = dbq.House.Where(x => x.PrjCode == hou_pla_no && x.HouCode == hou_bld_no).FirstOrDefault().HouCode;
            if (HouCode == null || HouCode == "")
            {
                Guid g = Guid.NewGuid();
                House hInfo = new House();
                hInfo.PrjCode = hou_pla_no;
                hInfo.HouCode = hou_bld_no;
                hInfo.Token = Guid.NewGuid().ToString();
                dbq.House.Add(hInfo);
                dbq.SaveChanges();
            }
        }

        public void UpdWellanHou(string hou_pla_no, string hou_bld_no)
        {
            QRCodeEntities dbq = new QRCodeEntities();

            Guid g = Guid.NewGuid();
            House hInfo = new House();
            hInfo = dbq.House.Where(x => x.PrjCode == hou_pla_no && x.HouCode == hou_bld_no).FirstOrDefault();
            if (hInfo != null && hInfo.Token == null)
                hInfo.Token = Guid.NewGuid().ToString();
            dbq.SaveChanges();
        }

        public void UpdWellanPWD(string hou_pla_no, string hou_bld_no, string cus_idno)
        {
 
        }

        public void GenQRCodeFolder(string PrjCode)
        {
            string ff = Server.MapPath("../Pic/" + PrjCode);
            if (!System.IO.Directory.Exists(ff))
            {
                System.IO.Directory.CreateDirectory(ff);
            }
        }

        public void GenQRCode(string PrjCode, string HouCode, string tkn)
        {
            string ff = Server.MapPath("../Pic/" + PrjCode + "/" + PrjCode + "_" + HouCode + ".jpg");
            
            if (!System.IO.File.Exists(ff))
            {
                string url = WebConfigurationManager.AppSettings["url"] + "?tkn=" + tkn;

                var writer = new BarcodeWriter  //dll裡面可以看到屬性
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions //設定大小
                    {
                        Height = 300,
                        Width = 300,
                    }
                };
                System.Drawing.Bitmap b = writer.Write(url); //轉QRcode的文字 
                b.Save(ff);
                b.Dispose();
            }
            //return View();
        }

        public string GenPWD(int HouSNO)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            House info = dbq.House.Where(x => x.HouSNO == HouSNO).FirstOrDefault();

            //info.HouPWD = Membership.GeneratePassword(8, 0);
            info.HouPWD = GetRandomString();
            dbq.SaveChanges();
            return info.HouPWD;
        }

        public ActionResult BlurPWD(string id, string HouPWD, string HouSNO)
        {
            try
            {
                QRCodeEntities dbq = new QRCodeEntities();
                int H = int.Parse(HouSNO);
                House hInfo = dbq.House.Where(x => x.HouSNO == H).FirstOrDefault();
                hInfo.HouPWD = HouPWD;
                dbq.SaveChanges();
                Jsn j = new Jsn() { msg = hInfo.HouPWD };
                return Json(j, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string res = ex.Message;
                return null;
            }

        }

        public ActionResult BlurLimited(string id, string Limited, string HouSNO)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            int H = int.Parse(HouSNO);
            House hInfo = dbq.House.Where(x => x.HouSNO == H).FirstOrDefault();
            hInfo.Limited = Limited;
            dbq.SaveChanges();

            Jsn j = new Jsn() { msg = hInfo.Limited };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        public static string GetRandomString()
        {
            var str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var next = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < 8; i++)
            {
                builder.Append(str[next.Next(0, str.Length)]);
            }
            return builder.ToString();
        }

        public ActionResult PrintQR(string PrjSNO)
        {
            string sql = "";
            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            QRCodeEntities dbq = new QRCodeEntities();
            DbCommand cmd = null;
            AllVM vm = new AllVM();

            //專案選單
            sql = "select PrjSNO as val, PrjName txt, PrjCode as cod from Project order by PrjCode ";
            cmd = db.GetSqlStringCommand(sql);
            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    SelectInfo sInfo = new SelectInfo();
                    sInfo.val = dr["val"].ToString();
                    sInfo.txt = dr["txt"].ToString();
                    sInfo.cod = dr["cod"].ToString();
                    vm.PrjOption.Add(sInfo);
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();

            vm.PrjInfo.PrjSNO = int.Parse(PrjSNO);
            vm.PrjInfo.PrjCode = dbq.Project.Where(x => x.PrjSNO == vm.PrjInfo.PrjSNO).FirstOrDefault().PrjCode;

            //產生專案資料夾
            GenQRCodeFolder(vm.PrjInfo.PrjCode);

            //產生戶號
            //GenWellanHou(PrjCode);

            //產生QRCode
            sql = "select h.*, p.PrjCode, p.PrjName, p.Limited from House h, Project p where p.PrjSNO = h.PrjSNO ";
            sql += "and h.PrjSNO = '" + PrjSNO + "' ";
            sql += "order by p.PrjCode, h.HouCode ";
            cmd = db.GetSqlStringCommand(sql);
            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    HousePro pro = new HousePro();
                    pro.HouSNO = int.Parse(dr["HouSNO"].ToString());
                    pro.PrjSNO = int.Parse(dr["PrjSNO"].ToString());
                    pro.HouCode = dr["HouCode"].ToString();
                    pro.HouPWD = (dr["HouPWD"] == null || dr["HouPWD"].ToString() == "" ? GenPWD(pro.HouSNO) : dr["HouPWD"].ToString()); //產生隨機密碼
                    pro.QRCode = dr["QRCode"].ToString();
                    pro.PrjCode = dr["PrjCode"].ToString();
                    pro.PrjName = dr["PrjName"].ToString();
                    pro.Limited = dr["Limited"].ToString();
                    pro.Token = dr["Token"].ToString();
                    vm.HouList.Add(pro);
                    GenQRCode(pro.PrjCode, pro.HouCode, pro.Token); //產生QRCode
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();
            return View(vm);
        }
    }
}