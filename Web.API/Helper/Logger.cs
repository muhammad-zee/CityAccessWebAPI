using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Web.API.Helper
{
    public  class Logger
    {
        private IWebHostEnvironment _hostEnvironment;

        public  Logger(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        #region Memeber
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string logType = "";
        #endregion
         Logger()
        {
            try
            {
                configure();
            }

            catch (Exception exp)
            {
                throw exp;
            }
        }

        public void configure()
        {
            string logFile = _hostEnvironment.ContentRootPath + "\\log4net.config";// System.Web.HttpContext.Current.Server.MapPath("~/log4net.config");
            if (System.IO.File.Exists(logFile))
            {
                FileInfo logFileInfo = new FileInfo(logFile);
                log4net.Config.XmlConfigurator.ConfigureAndWatch(logFileInfo);
            }
        }

        #region Methods
        public  void LogInfo(string entry)
        {
            try
            {
                configure();
                log.Info(entry);
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        public  void LogError(string entry)
        {
            try
            {
                configure();
                log.Error(entry);
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        public  void LogExceptions(Exception ex)
        {
            configure();
            string complete = ex.ToString();
            complete += ex.InnerException + "\n\n";
            complete += ex.Message + "\n\n";

            this.LogError("Exception: \n\n" + complete);
        }

        #endregion
    }
}
