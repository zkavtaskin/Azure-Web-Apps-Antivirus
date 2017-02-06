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

            var httpRequest = HttpContext.Current.Request;
            HttpPostedFile file = httpRequest.Files[0];

            string fakeVirusText = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";

            string filePath = $"{WebConfigurationManager.AppSettings["UploadDirectory"]}\\{Guid.NewGuid()}_{file.FileName}";
            string filePathAUnknown = filePath + "A";
            string filePathBControl = filePath + "B";
            string filePathCUnknown = filePath + "C";
            string filePathDControl = filePath + "D";

            try
            {
                file.SaveAs(filePathAUnknown);

                using (StreamWriter writer = new StreamWriter(filePathBControl))
                {
                    writer.WriteLine(fakeVirusText);
                }

                file.SaveAs(filePathCUnknown);

                using (StreamWriter writer = new StreamWriter(filePathDControl))
                {
                    writer.WriteLine(fakeVirusText);
                }


                while(File.Exists(filePathBControl) && File.Exists(filePathDControl))
                {
                    Thread.Sleep(100);
                }

                if(!File.Exists(filePathAUnknown) || !File.Exists(filePathCUnknown))
                {
                    response = false;
                }
            }
            catch(Exception)
            {
                response = false;
                //Log error
            }
            finally
            {
                if(File.Exists(filePathAUnknown))
                    File.Delete(filePathAUnknown);

                if(File.Exists(filePathBControl))
                    File.Delete(filePathBControl);

                if (File.Exists(filePathCUnknown))
                    File.Delete(filePathCUnknown);

                if(File.Exists(filePathDControl))
                    File.Delete(filePathDControl);
            }

            return response;
        }
    }
}
