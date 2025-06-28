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

namespace QRCodeAdmin.Controllers
{
    public class Jss
    {
        public string msg { get; set; }
        public string url { get; set; }
    }

    public class AttController : Controller
    {
        public ActionResult Upload(string PrjSNO, string HouSNO)
        {
            AllVM vm = new AllVM();

            //產生附件資料
            //GenAtts(int.Parse(HouSNO));

            //取得附件標題
            QRCodeEntities dbq = new QRCodeEntities();
            vm.AttTitles = dbq.SysCode.Where(x => x.SysType == "上傳項目").ToList();

            //取得附件資料
            ViewBag.qy_PrjSNO = PrjSNO;
            string sql = "";
            sql += "select aa.*, ss.Text as AttTitle from ";
            sql += "( ";
            sql += "select* from Attachs where PrjSNO = " + PrjSNO + " and AttCode = 'a' ";
            sql += "union ";
            sql += "select* from Attachs where HouSNO = " + HouSNO + " ";
            sql += ") aa ";
            sql += "inner join SysCode ss on ss.SysType = N'上傳項目' and ss.Code = aa.AttCode ";

            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            DbCommand cmd = db.GetSqlStringCommand(sql);

            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    AttPro pro = new AttPro();
                    pro.AttSNO = int.Parse(dr["AttSNO"].ToString());
                    pro.HouSNO = int.Parse(dr["HouSNO"].ToString());
                    pro.AttFileName = dr["AttFileName"].ToString();
                    pro.AttTitle = dr["AttTitle"].ToString();
                    pro.AttNote = dr["AttNote"].ToString();
                    pro.AttSort = int.Parse(dr["AttSort"].ToString());
                    vm.AttProList.Add(pro);
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();

            vm.PrjInfo.PrjSNO = int.Parse(PrjSNO);
            vm.HouInfo.HouSNO = int.Parse(HouSNO);


            int P = int.Parse(PrjSNO);
            int H = int.Parse(HouSNO);
            vm.PrjInfo.PrjCode = dbq.Project.Where(x => x.PrjSNO == P).FirstOrDefault().PrjCode;
            vm.PrjInfo.PrjName = dbq.Project.Where(x => x.PrjSNO == P).FirstOrDefault().PrjName;
            vm.HouInfo.HouCode = dbq.House.Where(x => x.HouSNO == H).FirstOrDefault().HouCode; 

            return View(vm);
        }

        [HttpPost]
        public void Upload(FormCollection fc, HttpPostedFileBase Att02, HttpPostedFileBase Att03, HttpPostedFileBase Att04, HttpPostedFileBase Att05, HttpPostedFileBase Att06, HttpPostedFileBase Att07)
        {
            save(int.Parse(fc["PrjSNO"]), int.Parse(fc["HouSNO"]), int.Parse(fc["AttSNO02"]), fc["AttTitle02"], fc["AttNote02"], "b", 2, Att02);
            save(int.Parse(fc["PrjSNO"]), int.Parse(fc["HouSNO"]), int.Parse(fc["AttSNO03"]), fc["AttTitle03"], fc["AttNote03"], "c", 3, Att03);
            save(int.Parse(fc["PrjSNO"]), int.Parse(fc["HouSNO"]), int.Parse(fc["AttSNO04"]), fc["AttTitle04"], fc["AttNote04"], "d", 4, Att04);
            save(int.Parse(fc["PrjSNO"]), int.Parse(fc["HouSNO"]), int.Parse(fc["AttSNO05"]), fc["AttTitle05"], fc["AttNote05"], "e", 5, Att05);
            save(int.Parse(fc["PrjSNO"]), int.Parse(fc["HouSNO"]), int.Parse(fc["AttSNO06"]), fc["AttTitle06"], fc["AttNote06"], "f", 6, Att06);
            save(int.Parse(fc["PrjSNO"]), int.Parse(fc["HouSNO"]), int.Parse(fc["AttSNO07"]), fc["AttTitle07"], fc["AttNote07"], "g", 7, Att07);
            Response.Write("<script>alert('儲存成功');location.href='../Att/Upload?PrjSNO=" + fc["PrjSNO"] + "&HouSNO=" + fc["HouSNO"] + "'</script>");

            //Jss j = new Jss() { msg = "success", url = "" };
            //return Json(j, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Manual(string PrjSNO)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            int P = int.Parse(PrjSNO);

            Project pInfo = dbq.Project.Where(x => x.PrjSNO == P).FirstOrDefault();
            SysCode sInfo = dbq.SysCode.Where(x => x.SysType == "上傳項目" && x.Value == "1").FirstOrDefault();
            Attachs aInfo = dbq.Attachs.Where(x => x.PrjSNO == P && x.AttCode == "a").FirstOrDefault();
            AllVM vm = new AllVM();
            vm.PrjInfo.PrjSNO = pInfo.PrjSNO;
            vm.PrjInfo.PrjCode = pInfo.PrjCode;
            vm.PrjInfo.PrjName = pInfo.PrjName;
            vm.SysCodeInfo.Text = sInfo.Text;

            if (aInfo != null)
            {
                vm.AttProInfo.AttSNO = aInfo.AttSNO;
                vm.AttProInfo.AttFileName = aInfo.AttFileName;
                vm.AttProInfo.AttNote = aInfo.AttNote;
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult Manual(FormCollection fc, HttpPostedFileBase Att01)
        {
            save(int.Parse(fc["PrjSNO"]), 0, int.Parse(fc["AttSNO"]), fc["AttTitle"], fc["AttNote"], "a", 1, Att01);

            Jss j = new Jss() { msg = "儲存成功", url = "" };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Batch(string PrjSNO)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            AllVM vm = new AllVM();
            vm.PrjInfo.PrjSNO = int.Parse(PrjSNO);
            vm.PrjInfo.PrjCode = dbq.Project.Where(x => x.PrjSNO == vm.PrjInfo.PrjSNO).FirstOrDefault().PrjCode;
            vm.PrjInfo.PrjName = dbq.Project.Where(x => x.PrjSNO == vm.PrjInfo.PrjSNO).FirstOrDefault().PrjName;
            vm.AttTitles = dbq.SysCode.Where(x => x.SysType == "上傳項目").OrderBy(x=>x.Sort).ToList();

            //建立目錄
            string ff = Server.MapPath("../Upload/" + vm.PrjInfo.PrjCode);
            if (!System.IO.Directory.Exists(ff))
            {
                System.IO.Directory.CreateDirectory(ff);
            }

            DirectoryInfo di = new DirectoryInfo(Server.MapPath("../Upload/" + vm.PrjInfo.PrjCode));

            FileInfo[] files = di.GetFiles("*.*");
            vm.files = files;


            return View(vm);
        }

        [HttpPost]
        public ActionResult Batch(FormCollection fc, List<HttpPostedFileBase> postedFiles)
        {
            string path = Server.MapPath("../Upload/" + fc["PrjCode"]);
            string msg = "";
            string err = "";
            QRCodeEntities dbq = new QRCodeEntities();

            //建立目錄
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //檔案上傳
            foreach (HttpPostedFileBase postedFile in postedFiles)
            {
                if (postedFile == null)
                {
                    err += "<div>檔案損毀</div>";
                    continue;
                }

                if (postedFile != null)
                {
                    string fileName = Path.GetFileName(postedFile.FileName);

                    //拆解fileName
                    string[] ext = fileName.Split('.');
                    if (ext.Length <= 0)
                    {
                        err += string.Format("<div>{0} 檔名格式錯誤</div>", fileName);
                        continue;
                    }

                    string[] nnn = ext[0].Split('_');
                    if (nnn.Length != 3)
                    {
                        err += string.Format("<div>{0} 檔名格式錯誤</div>", fileName);
                        continue;
                    }

                    string ppp = nnn[0].ToString();
                    string hhh = nnn[1].ToString();
                    string abc = nnn[2].ToString();

                    int PrjSNO = 0;
                    int HouSNO = 0;
                    bool match = false;
                    string AttCode = "";
                    int? AttSort = 0;

                    Project pInfo = dbq.Project.Where(x => x.PrjCode == ppp).FirstOrDefault();
                    if (pInfo == null)
                    {
                        err += string.Format("<div>{0} 專案代號不存在</div>", fileName);
                        continue;
                    }
                    PrjSNO = pInfo.PrjSNO;

                    if (hhh == "000")
                    {
                        if (abc != dbq.SysCode.Where(x => x.SysType == "上傳項目" && x.Code == "a").FirstOrDefault().Text)
                        {
                            err += string.Format("<div>{0} 中文字不符合規定</div>", fileName);
                            continue;
                        }
                        HouSNO = 0;
                        AttCode = "a";
                        AttSort = 1;
                    }
                    else
                    {
                        House hInfo = dbq.House.Where(x => x.PrjCode == ppp && x.HouCode == hhh).FirstOrDefault();
                        if (hInfo == null)
                        {
                            err += string.Format("<div>{0} 戶號不存在</div>", fileName);
                            continue;
                        }
                        HouSNO = hInfo.HouSNO;

                        foreach (SysCode sInfo in dbq.SysCode.Where(x => x.SysType == "上傳項目" && x.Code != "a").ToList())
                        {
                            if (sInfo.Text == abc)
                            {
                                match = true;
                                AttCode = sInfo.Code;
                                AttSort = sInfo.Sort;
                                break;
                            }
                        }

                        if (!match)
                        {
                            err += string.Format("<div>{0} 中文字不符合規定</div>", fileName);
                            continue;
                        }
                    }


                    Attachs aInfo = dbq.Attachs.Where(x => x.PrjSNO == PrjSNO && x.HouSNO == HouSNO && x.AttCode == AttCode).FirstOrDefault();
                    //新增記錄
                    if (aInfo == null)
                    {
                        aInfo = new Attachs();
                        aInfo.PrjSNO = PrjSNO;
                        aInfo.HouSNO = HouSNO;
                        aInfo.AttCode = AttCode;
                        aInfo.AttFileName = fileName;
                        aInfo.AttSort = AttSort;
                        aInfo.AttTime = DateTime.Now;
                        dbq.Attachs.Add(aInfo);
                    }
                    else
                    {
                        aInfo.AttFileName = fileName;
                        aInfo.AttTime = DateTime.Now;
                    }
                    dbq.SaveChanges();

                    //儲存檔案
                    postedFile.SaveAs(path + @"\" + fileName);

                    err += string.Format("<div>{0} 上傳成功</div>", fileName);
                }
            }

            Jsn j = new Jsn() { msg = "上傳完成，詳情請見下方說明", err = err, url = "../Att/Batch?PrjSNO=" + fc["PrjSNO"].ToString() };
            return Json(j, JsonRequestBehavior.AllowGet);

            //return View();
            //Response.Write("<script>alert('" + msg + "');location.href='../Att/Batch?PrjCode=" + fc["PrjCode"] + "'</script>");
        }

        public void GenAtts(int HouSNO)
        {
            QRCodeEntities dbq = new QRCodeEntities();

            string pcode = dbq.House.Where(x => x.HouSNO == HouSNO).FirstOrDefault().PrjCode;

            if (dbq.Attachs.Where(x => x.HouSNO == HouSNO).ToList().Count() == 0)
            {
                List<SysCode> sysList = dbq.SysCode.Where(x => x.SysType == "上傳項目").OrderBy(x => x.Sort).ToList();

                foreach (SysCode sInfo in sysList)
                {
                    Attachs aInfo = new Attachs();
                    aInfo.HouSNO = HouSNO;
                    aInfo.AttNote = sInfo.Text;
                    aInfo.AttFileName = String.Format("{0}{1}.pdf", pcode, sInfo.Text);
                    aInfo.AttSort = sInfo.Sort;
                    dbq.Attachs.Add(aInfo);
                    dbq.SaveChanges();
                }
            }
        }

        public void save(int PrjSNO, int HouSNO, int AttSNO, string AttTitle, string AttNote, string AttCode, int AttSort, HttpPostedFileBase AttFile)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            Project prj = dbq.Project.Where(x => x.PrjSNO == PrjSNO).FirstOrDefault();
            House hou = dbq.House.Where(x => x.HouSNO == HouSNO).FirstOrDefault();
            Attachs att = null;

            string PrjCode = prj.PrjCode;
            string HouCode = (hou == null ? "000" : hou.HouCode);

            if (AttSNO == 0) //新增
            {
                att = new Attachs();
            }
            else
            {
                att = dbq.Attachs.Where(x => x.AttSNO == AttSNO).FirstOrDefault();
            }

            att.PrjSNO = PrjSNO;
            att.HouSNO = HouSNO;
            att.AttCode = AttCode;
            att.AttNote = AttNote;
            att.AttSort = AttSort;
            att.AttTime = DateTime.Now;

            //建立目錄
            string ff = Server.MapPath("../Upload/" + PrjCode);
            if (!System.IO.Directory.Exists(ff))
            {
                System.IO.Directory.CreateDirectory(ff);
            }

            //檔案上傳
            if (AttFile != null && AttFile.ContentLength > 0)
            {
                string AttFileName = "";
                if (!String.IsNullOrEmpty(att.AttFileName))
                {
                    FileInfo f = new FileInfo(Path.Combine(Server.MapPath("../Upload" + PrjCode), att.AttFileName));
                    if (f.Exists)
                        f.Delete();
                }

                AttFileName = String.Format("{0}_{1}_{2}{3}", PrjCode, HouCode, AttTitle, Path.GetExtension(AttFile.FileName));
                AttFile.SaveAs(Path.Combine(Server.MapPath("../Upload/" + PrjCode), AttFileName));
                att.AttFileName = AttFileName;
            }

            if (AttSNO == 0) //新增
                dbq.Attachs.Add(att);

            dbq.SaveChanges();
        }

        public ActionResult Del(string PrjSNO, string PrjCode, string AttFileName)
        {
            FileInfo f = new FileInfo(Path.Combine(Server.MapPath("../Upload/" + PrjCode), AttFileName));
            if (f.Exists)
                f.Delete();

            int P = int.Parse(PrjSNO);
            QRCodeEntities dbq = new QRCodeEntities();
            Attachs aInfo = dbq.Attachs.Where(x => x.PrjSNO == P && x.AttFileName == AttFileName).FirstOrDefault();
            if (aInfo != null)
            {
                aInfo.AttFileName = null;
                dbq.SaveChanges();
            }
            Jss j = new Jss() { msg = "刪除成功", url = "../Att/Batch?PrjSNO=" + PrjSNO };
            return Json(j, JsonRequestBehavior.AllowGet);
        }
    }
}