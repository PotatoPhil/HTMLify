using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HTMLify
{
    class FtpWebHandler
    {
        public void SendFile(Dictionary<string,string> attributes,string method, string path )
        {
            FtpWebRequest request = 
                (FtpWebRequest)WebRequest.Create(attributes["ftp_site_path"]+":"+attributes["ftp_site_port"]+"/"+attributes["ftp_containing_folder"]+attributes["ftp_dir_cut"]+"/"+path);
            request.Method = method;
            request.Credentials = new NetworkCredential(
                attributes["ftp_user"],
                attributes["ftp_pass"]);
            request.KeepAlive = false;
            StreamReader src = new StreamReader(path);
            byte[] fileContents = Encoding.UTF8.GetBytes(src.ReadToEnd());
            src.Close();
            request.ContentLength = fileContents.Length;
            
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(fileContents, 0, fileContents.Length);
            reqStream.Close();
            Console.WriteLine("Sent Request, Uploaded File.");
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Console.WriteLine("Upload File complete, status {0}", response.StatusDescription);
            response.Close();
        }
    }
}
