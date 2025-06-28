using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using System.Data;
using Models;

namespace QRCodeAdmin.Controllers
{
    public class UsersController : Controller
    {
        public ActionResult List()
        {
            if (Session["Name"] == null)
                Response.Redirect("../Main/Logout");

            string sql = "select u.*, s.Text as RolesStr ";
            sql += "from Users u ";
            sql += "inner join SysCode s on s.SysType = N'角色' and s.Value = u.Roles ";
            sql += "order by u.Roles, u.Name ";

            Database db = DatabaseFactory.CreateDatabase("DataContext_QRCode");
            DbCommand cmd = db.GetSqlStringCommand(sql);

            AllVM vm = new AllVM();

            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    UsersPro pro = new UsersPro();
                    pro.SNO = int.Parse(dr["SNO"].ToString());
                    pro.GID = dr["GID"].ToString();
                    pro.Name = dr["Name"].ToString();
                    pro.UnitCode = dr["UnitCode"].ToString();
                    pro.Roles = dr["RolesStr"].ToString();
                    pro.Email = dr["Email"].ToString();
                    pro.Enabled = int.Parse(dr["Enabled"].ToString());
                    vm.UsersProList.Add(pro);
                }
            }
            cmd.Connection.Dispose();
            cmd.Dispose();
            return View(vm);
        }

        public ActionResult Edit(string SNO)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            AllVM vm = new AllVM();

            //角色選單
            List<SysCode> rolesOpt = dbq.SysCode.Where(x => x.SysType == "角色").OrderBy(x => x.Sort).ToList();
            foreach (SysCode info in rolesOpt)
            {
                SelectInfo sInfo = new SelectInfo();
                sInfo.val = info.Value.ToString();
                sInfo.txt = info.Text.ToString();
                vm.RolesOption.Add(sInfo);
            };


            if (SNO == null || SNO == "")
                return View(vm);

            
            int S = int.Parse(SNO);


            if (dbq.Users.Where(x => x.SNO == S).ToList().Count == 0)
                return View(vm);


            Users uInfo = dbq.Users.Where(x => x.SNO == S).FirstOrDefault();
            
            vm.UsersProInfo.SNO = uInfo.SNO;
            vm.UsersProInfo.GID = uInfo.GID;
            vm.UsersProInfo.Name = uInfo.Name;
            vm.UsersProInfo.Email = uInfo.Email;
            vm.UsersProInfo.Roles = uInfo.Roles;
            vm.UsersProInfo.UnitCode = uInfo.UnitCode;
            vm.UsersProInfo.PWD = uInfo.PWD;
            vm.UsersProInfo.Enabled = uInfo.Enabled;
            return View(vm);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection fc)
        {
            QRCodeEntities dbq = new QRCodeEntities();
            Users uInfo = new Users();
            if (int.Parse(fc["SNO"]) == 0)
            {
                uInfo.Name = fc["Name"].ToString().Trim();
                uInfo.GID = fc["GID"].ToString().Trim();
                uInfo.UnitCode = fc["UnitCode"].ToString().Trim();
                uInfo.Roles = fc["Roles"].ToString().Trim();
                uInfo.Email = fc["Email"].ToString().Trim();
                uInfo.Enabled = int.Parse(fc["Enabled"].ToString());
                dbq.Users.Add(uInfo);
            }
            else
            {
                int S = int.Parse(fc["SNO"]);
                uInfo = dbq.Users.Where(x => x.SNO == S).FirstOrDefault();
                uInfo.Name = fc["Name"].ToString().Trim();
                uInfo.GID = fc["GID"].ToString().Trim();
                uInfo.UnitCode = fc["UnitCode"].ToString().Trim();
                uInfo.Roles = fc["Roles"].ToString().Trim();
                uInfo.Email = fc["Email"].ToString().Trim();
                uInfo.Enabled = int.Parse(fc["Enabled"].ToString());
            }
            dbq.SaveChanges();
            Jsn j = new Jsn() { msg = "儲存成功", url = "../Users/List" };
            return Json(j, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Del(FormCollection fc)
        {
            int SNO = int.Parse(fc["SNO"].ToString());
            QRCodeEntities dbq = new QRCodeEntities();
            dbq.Users.Remove(dbq.Users.Where(x => x.SNO == SNO).FirstOrDefault());
            dbq.SaveChanges();

            Jsn j = new Jsn() { msg = "刪除成功", url = "../Users/List" };
            return Json(j, JsonRequestBehavior.AllowGet);
        }
    }

}