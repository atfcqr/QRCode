using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace QRCodeCustomer.Models
{
    //選單專用
    public class SelectInfo
    {
        public string txt { get; set; }
        public string val { get; set; }
        public string cod { get; set; }
    }

    public class HousePro : Models.House
    {
        public string PrjCode { get; set; }
        public string PrjName { get; set; }
        public string PrjWelcome { get; set; }
        public string PrjBgColor { get; set; }
        public string PrjFontColor { get; set; }
        public string PrjPic { get; set; }
    }

    public class AttPro : Models.Attach
    {

    }

    public class UsersPro : Models.Users
    {

    }

    public class AllVM
    {

        public Models.Project PrjInfo { get; set; }
        public IList<Models.Project> PrjList { get; set; }

        public HousePro HouInfo { get; set; }
        public IList<HousePro> HouList { get; set; }

        public AttPro AttInfo { get; set; }
        public IList<AttPro> AttList { get; set; }

        public UsersPro UsersInfo { get; set; }
        public IList<UsersPro> UsersList { get; set; }

        public IList<SelectInfo> PrjOption { get; set; }
        public IList<SelectInfo> UnitOption { get; set; }

        public AllVM()
        {
            PrjInfo = new Models.Project();
            PrjList = new List<Models.Project>();
            HouInfo = new HousePro();
            HouList = new List<HousePro>();
            AttInfo = new AttPro();
            AttList = new List<AttPro>();
            UsersInfo = new UsersPro();
            UsersList = new List<UsersPro>();

            PrjOption = new List<SelectInfo>();
            UnitOption = new List<SelectInfo>();
        }
    }
}