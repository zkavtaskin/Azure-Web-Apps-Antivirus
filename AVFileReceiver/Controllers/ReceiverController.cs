using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Configuration;
using System.Text;

namespace AVFileReceiver.Controllers
{
    public class ReceiverController : ApiController
    {
        public bool Post()
        {
            bool response = true;

            HttpRequest httpRequest = HttpContext.Current.Request;
            HttpPostedFile file = httpRequest.Files[0];

            string filePath = $"{WebConfigurationManager.AppSettings["UploadDirectory"]}\\{Guid.NewGuid()}_{file.FileName}";

            try
            {
                file.SaveAs(filePath);

                using (Process process = Process.Start(
                    "C:\\Program Files (x86)\\Symantec\\Symantec Endpoint Protection\\DoScan.exe",
                    "/ScanFile " + filePath))
                {
                    process.WaitForExit();
                }

                if (!File.Exists(filePath))
                    response = false; 
            }
            catch(Exception)
            {
                throw;
                //Log error
            }
            finally
            {
                if(File.Exists(filePath))
                    File.Delete(filePath);
            }

            return response;
        }
    }
}
