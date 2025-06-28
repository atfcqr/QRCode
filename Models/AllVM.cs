using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace Models
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
        public string PrjName { get; set; }
        public string PrjWelcome { get; set; }
        public string PrjBgColor { get; set; }
        public string PrjFontColor { get; set; }
        public string PrjBannerColor { get; set; }
        public string PrjButFtColor { get; set; }
        public string PrjButBgColor { get; set; }
        public string PrjPic { get; set; }
        public string PrjLogo { get; set; }
        public string Limited { get; set; }
        public string AttCount { get; set; }
    }

    public class AttPro : Models.Attachs
    {
        public string AttTitle { get; set; }
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

        public House hInfo { get; set; }
        public IList<House> hList { get; set; }

        public AttPro AttProInfo { get; set; }
        public IList<AttPro> AttProList { get; set; }

        public UsersPro UsersProInfo { get; set; }
        public IList<UsersPro> UsersProList { get; set; }

        public IList<SelectInfo> PrjOption { get; set; }
        public IList<SelectInfo> UnitOption { get; set; }
        public IList<SelectInfo> RolesOption { get; set; }

        public SysCode SysCodeInfo { get; set; }
        public IList<SysCode> SysCodeList { get; set; }
        public IList<SysCode> AttTitles { get; set; }

        public FileInfo[] files { get; set; }

        public AllVM()
        {
            PrjInfo = new Models.Project();
            PrjList = new List<Models.Project>();
            HouInfo = new HousePro();
            HouList = new List<HousePro>();
            AttProInfo = new AttPro();
            AttProList = new List<AttPro>();
            UsersProInfo = new UsersPro();
            UsersProList = new List<UsersPro>();
            
            SysCodeInfo = new SysCode();
            SysCodeList = new List<SysCode>();
            AttTitles = new List<SysCode>();

            PrjOption = new List<SelectInfo>();
            UnitOption = new List<SelectInfo>();
            RolesOption = new List<SelectInfo>();

            hInfo = new House();
            hList = new List<House>();
        }
    }
}
