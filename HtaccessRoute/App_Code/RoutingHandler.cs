using System;
using System.Collections.Generic;
using System.Web.Routing;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.Security;
using System.Text.RegularExpressions;
using System.IO;

/// <summary>
/// Summary description for RoutingHandler
/// </summary>
public class RoutingHandler : IRouteHandler
{
    #region Variables
    private Dictionary<string, string> m_urlMatcher = new Dictionary<string, string>();
    private Dictionary<string, string> m_errorDocuments = new Dictionary<string, string>();
    #endregion

    #region Static Methods
    public static RoutingHandler LoadFromHtaccess(string defaultPage)
    {
        string htaccessPath = System.Web.HttpContext.Current.Server.MapPath("~/.htaccess");
        return RoutingHandler.LoadFromHtaccess(defaultPage, htaccessPath);
    }

    public static RoutingHandler LoadFromHtaccess(string defaultPage, string htaccessPath)
    {
        RoutingHandler handler = new RoutingHandler(defaultPage);

        if (string.IsNullOrEmpty(htaccessPath)) return handler;
        FileInfo file = new FileInfo(htaccessPath);
        if (file.Exists)
        {
            HtaccessParser.Htaccess htaccess = new HtaccessParser.Htaccess(file.Open(FileMode.Open, FileAccess.Read));

            HtaccessParser.HtaccessNode rewriteEngineNode = htaccess.Find((HtaccessParser.HtaccessNode node) => { return node.Name == "RewriteEngine"; });
            if (rewriteEngineNode != null)
            {
                HtaccessParser.Directive rewriteEngineDirective = (HtaccessParser.Directive)rewriteEngineNode;
                if (rewriteEngineDirective.HasArgumentValue("on"))
                {
                    List<HtaccessParser.HtaccessNode> listRewriteRule = htaccess.FindAll((HtaccessParser.HtaccessNode node) => { return node.Name == "RewriteRule"; });
                    if (listRewriteRule != null && listRewriteRule.Count > 0)
                    {
                        foreach (HtaccessParser.Directive directive in listRewriteRule)
                        {
                            if (directive != null)
                            {
                                string[] args = directive.Arguments;
                                if (args != null && args.Length > 1)
                                {
                                    handler.SetUrlMatcher(args[0].Trim(), args[1].Trim());
                                }
                            }
                        }
                    }
                }
            }

            List<HtaccessParser.HtaccessNode> listErrorDocument = htaccess.FindAll((HtaccessParser.HtaccessNode node) => { return node.Name == "ErrorDocument"; });
            if (listErrorDocument != null)
            {
                foreach (HtaccessParser.Directive directive in listErrorDocument)
                {
                    if (directive != null)
                    {
                        string[] args = directive.Arguments;
                        if (args != null && args.Length > 1)
                        {
                            handler.SetErrorDocument(args[0].Trim(), args[1].Trim());
                        }
                    }
                }
            }
        }

        return handler;
    }
    #endregion

    #region Constructor
    public RoutingHandler(string defaultPage)
    {
        this.DefaultPage = defaultPage;
    }
    #endregion

    #region Methods
    public IHttpHandler GetHttpHandler(HttpRequest request)
    {
        if (request != null && request.Url != null)
        {
            string requestUrl = request.Url.ToString();
            return this.HandlePage(requestUrl);
        }

        //foreach (var urlParm in requestContext.RouteData.Values)
        //{
        //    requestContext.HttpContext.Items[urlParm.Key] = urlParm.Value;
        //}

        //return BuildManager.CreateInstanceFromVirtualPath(this.VirtualPath, typeof(Page)) as IHttpHandler;
        return GenericHttpHandler.Create(this.DefaultPage) as IHttpHandler;
    }

    public IHttpHandler GetHttpHandler(RequestContext requestContext)
    {
        if (requestContext.RouteData.Values.ContainsKey("page"))
        {
            string requestUrl = requestContext.RouteData.Values["page"].ToString();
            return this.HandlePage(requestUrl);
        }

        foreach (var urlParm in requestContext.RouteData.Values)
        {
            requestContext.HttpContext.Items[urlParm.Key] = urlParm.Value;
        }

        //return BuildManager.CreateInstanceFromVirtualPath(this.VirtualPath, typeof(Page)) as IHttpHandler;
        return GenericHttpHandler.Create(this.DefaultPage) as IHttpHandler;
    }

    private IHttpHandler HandlePage(string requestUrl)
    {
        if (!string.IsNullOrEmpty(requestUrl))
        {
            foreach (string key in this.m_urlMatcher.Keys)
            {
                Match match = Regex.Match(requestUrl, key);
                if (match.Success)
                {
                    string url = Regex.Replace(requestUrl, key, this.m_urlMatcher[key]);
                    try
                    {
                        return GenericHttpHandler.Create(url) as IHttpHandler;
                    }
                    catch (HttpException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        return this.GetErrorPage(500, ex);
                    }
                }
            }
        }
        return GenericHttpHandler.Create(this.DefaultPage) as IHttpHandler;
    }

    private IHttpHandler GetErrorPage(int errorCode, Exception ex)
    {
        string errorPageUrl = this.m_errorDocuments[errorCode.ToString()];
        if (string.IsNullOrEmpty(errorPageUrl))
            return GenericHttpHandler.Create(errorPageUrl) as IHttpHandler;

        return GenericHttpHandler.Create(errorCode, ex) as IHttpHandler;
    }

    private void SetUrlMatcher(string pattern, string url)
    {
        if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(url)) return;
        if (this.m_urlMatcher.ContainsKey(pattern))
        {
            this.m_urlMatcher[pattern] = url;
        }
        else
        {
            this.m_urlMatcher.Add(pattern, url);
        }
    }

    private void SetErrorDocument(string code, string url)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(url)) return;
        if (this.m_urlMatcher.ContainsKey(code))
        {
            this.m_errorDocuments[code] = url;
        }
        else
        {
            this.m_errorDocuments.Add(code, url);
        }
    }
    #endregion

    #region Properties
    public bool IsReusable
    {
        get { return false; }
    }

    public string DefaultPage { get; private set; }
    #endregion
}