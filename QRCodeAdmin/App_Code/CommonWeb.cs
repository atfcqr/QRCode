using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Globalization;



public static class CommonWeb
{
     public static string dt2str(DateTime? dt, string format, string cultureName)
    {
        if (dt == null)
            return "";
        else
            return Convert.ToDateTime(dt).ToString(format, new CultureInfo(cultureName)); //zh-TW en-US
    }
}
