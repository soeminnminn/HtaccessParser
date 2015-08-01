using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for GenericHttpHandler
/// </summary>
public class GenericHttpHandler : IHttpHandler
{
    #region Variables
    private string m_url = "";
    private int m_errorCode = 200;
    private Exception m_exception = null;
    #endregion

    #region Static Methods
    public static GenericHttpHandler Create(string url)
    {
        return new GenericHttpHandler(url);
    }

    public static GenericHttpHandler Create(int errorCode, Exception ex)
    {
        return new GenericHttpHandler(errorCode, ex);
    }
    #endregion

    #region Constructor
    public GenericHttpHandler(string url)
	{
        this.m_url = url;
	}

    public GenericHttpHandler(int errorCode, Exception ex)
    {
        this.m_errorCode = errorCode;
        this.m_exception = ex;
    }
    #endregion

    #region Methods
    public void ProcessRequest(HttpContext context)
    {
        Uri uri = null;
        if (this.m_errorCode == 200)
        {
            Uri requestUri = context.Request.Url;
            string url = this.m_url;
            if (url.StartsWith("~/"))
            {
                string baseUrl = requestUri.GetLeftPart(UriPartial.Authority) + context.Request.ApplicationPath;
                url = baseUrl + "/" + this.m_url.Substring(2);
            }
            else
            {
                url = requestUri.Scheme + "://" + requestUri.Authority;
                for (int i = 0; i < requestUri.Segments.Length; i++)
                {
                    url += requestUri.Segments[i];
                }
                if (this.m_url.StartsWith("/"))
                    url += this.m_url.Substring(1);
                else
                    url += this.m_url;
            }

            uri = new Uri(url);
        }

        if (uri != null)
        {
            string url = uri.ToString();
            string physicalPath = context.Server.MapPath(uri.LocalPath);
            if (!System.IO.File.Exists(physicalPath))
            {
                this.m_errorCode = 404;
                this.m_exception = new System.IO.FileNotFoundException(string.Format("The file '{0}' does not exist.", uri.LocalPath), physicalPath);
                this.ProcessError(context);
            }
            else
            {
                context.Server.ClearError();
                context.Response.Redirect(url);
                context.ApplicationInstance.CompleteRequest();
            }
        }
        else
        {
            this.ProcessError(context);
        }
    }

    private void ProcessError(HttpContext context)
    {
        string pageHtml = this.BuildErrorPage(this.m_errorCode, this.m_exception);
        context.Server.ClearError();
        context.Response.Write(pageHtml);
        context.ApplicationInstance.CompleteRequest();
    }

    private string BuildErrorPage(int errorCode, Exception ex)
    {
        string pageHtml = string.Empty;
        if (ex != null)
        {
            pageHtml = "<html>";
            pageHtml += "<title>" + errorCode.ToString() + "</title>";
            pageHtml += "<body>";
            pageHtml += "<div>";
            pageHtml += "<h2>" + ex.Message + "</h2>";
            pageHtml += "<div>" + ex.StackTrace + "</div>";
            if (ex.InnerException != null)
            {
                pageHtml += "<h2>Inner Exception</h2>";
                pageHtml += "<h2>" + ex.InnerException.Message + "<h2>";
                pageHtml += "<div>" + ex.InnerException.StackTrace + "<div>";
            }
            pageHtml += "</div>";
            pageHtml += "</body>";
            pageHtml += "</html>";
        }

        return pageHtml;
    }
    #endregion

    #region Properties
    public bool IsReusable
    {
        get { return false; }
    }
    #endregion
}