using Ionic.Zlib;
using Squid.Log;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace Disco.Controllers
{
    //================================================================================================//
    public
    class CJController : BaseController
    {
        public static byte[] ReadAllBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        //---------------------------------------------------------------------------------------------//                        
        [AllowAnonymous]
        [HttpPost]        
        public ActionResult Index()
        {
            if (Request.ContentLength > 0)
            {
                //string result = "nothing";

                Request.InputStream.Position = 0;
                GZipStream zipStream = new GZipStream(Request.InputStream, CompressionMode.Decompress);
                
                XmlDocument xml = new XmlDocument();
                xml.Load(zipStream);

                xml.Save("C:\\milkshake\\cj\\" + DateTime.Now.Ticks + ".xml");
            }

            return View();
        }
    }

    public static class HttpExtensions
    {
        public static bool HasFile(this HttpPostedFileBase file)
        {
            return (file != null && file.ContentLength > 0) ? true : false;
        }
    }
    //================================================================================================//
}