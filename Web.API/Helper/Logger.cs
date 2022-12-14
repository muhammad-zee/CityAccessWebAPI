using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Web.API.Helper
{
    public class Logger
    {
        private IWebHostEnvironment _hostEnvironment;
        IConfiguration _config;
        private string _RootPath;
        public Logger(IWebHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _config = configuration;
            _hostEnvironment = hostEnvironment;
            _RootPath = _config["FilePath:Path"].ToString();
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
                //FileAppender fileAppender = new FileAppender();
                var RootPath = this._RootPath; //this._hostEnvironment.WebRootPath;
                var folderName = "Logs";
                RootPath = Path.Combine(RootPath, folderName);
                if (!Directory.Exists(RootPath))
                {
                    Directory.CreateDirectory(RootPath);
                }
                string FileName = $"log4net_{DateTime.UtcNow.ToString("dd-MM-yyyy")}.txt";
                var targetPath = Path.Combine(RootPath, FileName);
                //fileAppender.File = targetPath;
                if (!File.Exists(targetPath))
                {
                    File.Create(targetPath);
                }
                log4net.GlobalContext.Properties["LogFileName"] = targetPath;
                log4net.Config.XmlConfigurator.ConfigureAndWatch(logFileInfo);
                //targetPath = Path.GetFullPath(targetPath);
                //var filePath = File.ReadAllBytes(targetPath);
            }
        }

        #region Methods
        public void LogInfo(string entry)
        {
            try
            {
                //configure();
                log.Info(entry);
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        public void LogError(string entry)
        {
            try
            {
                //configure();
                log.Error(entry);
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        public void LogExceptions(Exception ex)
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
