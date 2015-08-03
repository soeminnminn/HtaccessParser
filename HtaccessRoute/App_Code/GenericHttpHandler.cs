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
    private Uri m_uri = null;
    private string m_url = "";
    private int m_errorCode = 200;
    private Exception m_exception = null;
    #endregion

    #region Static Methods
    public static GenericHttpHandler Create(string url)
    {
        return new GenericHttpHandler(url);
    }

    public static GenericHttpHandler Create(string url, Uri baseUri)
    {
        return new GenericHttpHandler(url, baseUri);
    }

    public static GenericHttpHandler Create(int errorCode, Exception ex)
    {
        return new GenericHttpHandler(errorCode, ex);
    }
    #endregion

    #region Constructor
    public GenericHttpHandler(string requestUrl)
	{
        if (!string.IsNullOrEmpty(requestUrl))
        {
            try
            {
                this.m_uri = new Uri(requestUrl);
            }
            catch
            { }

            if (this.m_uri == null)
                this.m_url = requestUrl;
        }
	}

    public GenericHttpHandler(string requestUrl, Uri baseUri)
    {
        if (!string.IsNullOrEmpty(requestUrl))
        {
            if (baseUri != null)
            {
                string url = baseUri.Scheme + "://" + baseUri.Authority;
                for (int i = 0; i < baseUri.Segments.Length; i++)
                {
                    url += baseUri.Segments[i];
                }
                url = url.TrimEnd('/');

                if (requestUrl.StartsWith("~/"))
                    url += "/" + requestUrl.Substring(2);
                else if (requestUrl.StartsWith("/"))
                    url += requestUrl;
                else
                    url += "/" + requestUrl;

                try
                {
                    this.m_uri = new Uri(url);
                }
                catch
                { }
            }

            if (this.m_uri == null)
                this.m_url = requestUrl;
        }
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
            if (this.m_uri == null)
            {
                if (!string.IsNullOrEmpty(this.m_url))
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
                        url = url.TrimEnd('/');

                        if (this.m_url.StartsWith("/"))
                            url += this.m_url;
                        else
                            url += "/" + this.m_url;
                    }

                    uri = new Uri(url);
                }
                else
                {
                    this.m_errorCode = 400;
                    this.m_exception = new System.Exception("Bad request!");
                }
            }
            else
            {
                uri = this.m_uri;
            }
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