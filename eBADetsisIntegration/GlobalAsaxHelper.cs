using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBADetsisIntegration
{
    public class GlobalAsaxHelper
    {
        public static void SetInstance()
        {
            eBAConfigurationHelper.Web.SetInstance();
        }
    }
}