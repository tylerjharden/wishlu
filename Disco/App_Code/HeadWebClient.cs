using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

public class HeadWebClient : WebClient
{
    public bool HeadOnly { get; set; }
    protected override WebRequest GetWebRequest(Uri address)
    {
        WebRequest req = base.GetWebRequest(address);
        if (HeadOnly && req.Method == "GET")
        {
            req.Method = "HEAD";
        }
        return req;
    }
}