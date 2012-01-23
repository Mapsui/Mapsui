// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;

namespace BruTile.Web
{
    public static class RequestHelper
    {
        public static byte[] FetchImage(Uri uri)
        {
            return FetchImage(uri, String.Empty, String.Empty, true);
        }

#if SILVERLIGHT
        //I agree this '#if' is rather ugly, but it is a lot simpler like this than using abstraction layers like I did before.
        public static byte[] FetchImage(Uri uri, string userAgent, string referer, bool keepAlive)
        {
            HttpWebRequest webClient = (HttpWebRequest)WebRequest.Create(uri);

            //todo: figure out what to do with keepAlive here.
            if (!String.IsNullOrEmpty(userAgent)) webClient.Headers["user-agent"] = userAgent;
            if (!String.IsNullOrEmpty(referer)) webClient.Headers["Referer"] = referer;

            //we use a waithandle to fake a synchronous call
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            IAsyncResult result = webClient.BeginGetResponse(webClient_OpenReadCompleted, waitHandle);

            //This trick works because the this is called on a worker thread. In SL it wont work if you call
            //it from the main thread because the main thead dispatches the worker threads and it starts waiting 
            //before it dispatches the worker thread.
            //!!!waitHandle.WaitOne();

            WebResponse response = webClient.EndGetResponse(result);

            return BruTile.Utilities.ReadFully(response.GetResponseStream());
        }

        private static void webClient_OpenReadCompleted(IAsyncResult e)
        {
            //Call Set() so that WaitOne can proceed.
            ((AutoResetEvent)e.AsyncState).Set();
        }

#else

        public static byte[] FetchImage(Uri uri, string userAgent, string referer, bool keepAlive)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);

#if !PocketPC
            IWebProxy proxy = WebRequest.GetSystemWebProxy();
            proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            webRequest.Proxy = proxy;
            webRequest.PreAuthenticate = true;
#endif
            webRequest.KeepAlive = keepAlive;
            if (!String.IsNullOrEmpty(userAgent)) webRequest.UserAgent = userAgent;
            if (!String.IsNullOrEmpty(referer)) webRequest.Referer = referer;

            WebResponse webResponse = webRequest.GetResponse();
            if (webResponse.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                using (Stream responseStream = webResponse.GetResponseStream())
                {
                    return Utilities.ReadFully(responseStream);
                }
            }
            else
            {
                string message = CreateErrorMessage(webResponse, uri.AbsoluteUri);
                throw (new WebResponseFormatException(message, null));
            }
        }

#endif

        private static string CreateErrorMessage(WebResponse webResponse, string uri)
        {
            string message = String.Format(
                CultureInfo.InvariantCulture,
              "Failed to retrieve tile from this uri:\n{0}\n.An image was expected but the received type was '{1}'.",
              uri,
              webResponse.ContentType
              );

            if (webResponse.ContentType.StartsWith("text", StringComparison.OrdinalIgnoreCase))
            {
                using (Stream stream = webResponse.GetResponseStream())
                {
                    message += String.Format(CultureInfo.InvariantCulture,
                      "\nThis was returned:\n{0}", ReadAllText(stream));
                }
            }
            return message;
        }

        private static string ReadAllText(Stream responseStream)
        {
            using (StreamReader streamReader = new StreamReader(responseStream, true))
            {
                using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
                {
                    stringWriter.Write(streamReader.ReadToEnd());
                    return stringWriter.ToString();
                }
            }
        }
    }
}
