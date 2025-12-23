using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace eBADetsisIntegration
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            string root = Path.GetDirectoryName(Path.GetDirectoryName(eBAAssemblyResolver.Resolver.CallingAssemblyDirectory));
            string pathCommon = Path.Combine(root, "Common");
            eBAAssemblyResolver.Resolver.AddPath(pathCommon);
            eBAAssemblyResolver.Resolver.AddPath(@"C:\Bimser2\eBA\Common");
            eBAAssemblyResolver.Resolver.AttachResolveEvent();

            GlobalAsaxHelper.SetInstance();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}