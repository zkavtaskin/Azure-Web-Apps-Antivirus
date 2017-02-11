using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace AVFilterIIS
{
    public class Filter : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;
            context.PreSendRequestContent += Context_PreSendRequestContent;
        }

        private void Context_PreSendRequestContent(object sender, EventArgs e)
        {
            HttpApplication httpApp = (HttpApplication)sender;
            HttpResponse httpResponse = httpApp.Context.Response;
            
            if(httpResponse.ContentType == "application/octet-stream")
            {
                ReadWriteProxyStream filter = httpResponse.Filter as ReadWriteProxyStream;

                if (filter != null)
                {
                    byte[] data = new byte[filter.Length];
                    filter.Position = 0;
                    filter.Read(data, 0, data.Length);

                    string outboundFilePath = ConfigurationManager.AppSettings["AVFilterIIS.Folder.TempUpload"] + Guid.NewGuid();

                    using (FileStream fileStream = new FileStream(outboundFilePath, FileMode.Create))
                    {
                        fileStream.Write(data, 0, data.Length);
                    }

                    try
                    {
                        uploadForScan(outboundFilePath);
                    }
                    finally
                    {
                        File.Delete(outboundFilePath);
                    }
                }
            }
        }
     

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication httpApplication = (HttpApplication)sender;
            HttpRequest httpRequest = httpApplication.Context.Request;
            
            if (httpRequest.RequestType == WebRequestMethods.Http.Post)
            {
                if (httpRequest.Files.Count != 0)
                {
                    for (int i = 0; i < httpRequest.Files.Count; i++)
                    {
                        HttpPostedFile incomingFile = httpRequest.Files[i];
                        string incomingFilePath = ConfigurationManager.AppSettings["AVFilterIIS.Folder.TempUpload"] + Guid.NewGuid();
                        incomingFile.SaveAs(incomingFilePath);

                        try
                        {
                            uploadForScan(incomingFilePath);
                        }
                        finally
                        {
                            File.Delete(incomingFilePath);
                        }

                    }
                }
            }

            httpApplication.Context.Response.Filter = new ReadWriteProxyStream(httpApplication.Context.Response.Filter);
        }

        private void uploadForScan(string filePath)
        {
            using (WebClient client = new WebClient())
            {
                byte[] response = client.UploadFile(
                    ConfigurationManager.AppSettings["AVFilterIIS.FileReceiverAPIURL"],
                    WebRequestMethods.Http.Post,
                    filePath
                    );

                string text = UTF8Encoding.UTF8.GetString(response);
                bool fileIsSafe = Convert.ToBoolean(text);

                if (!fileIsSafe)
                {
                    throw new HttpException(400, "Virus was detected");
                }
            }
        }

        public void Dispose() { }
    }
}
