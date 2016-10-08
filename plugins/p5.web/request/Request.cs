/*
 * Phosphorus Five, copyright 2014 - 2016, Thomas Hansen, phosphorusfive@gmail.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the Affero GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using System.IO;
using System.Web;
using p5.exp;
using p5.core;

/// <summary>
///     Main namespace for everything related to the current HTTP request
/// </summary>
namespace p5.web.ui.request {
    /// <summary>
    ///     Class wrapping Active Events related to the HTTP request
    /// </summary>
    public static class Request
    {
        /// <summary>
        ///     Retrieves type of request
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "get-http-method")]
        public static void get_http_method (ApplicationContext context, ActiveEventArgs e)
        {
            e.Args.Value = HttpContext.Current.Request.HttpMethod;
        }

        /// <summary>
        ///     Returns the current HTTP request's body
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "get-request-body")]
        public static void get_request_body (ApplicationContext context, ActiveEventArgs e)
        {
            if (HttpContext.Current.Request.InputStream.Length == 0) {
                e.Args.Value = ""; // Defaulting to string.Empty!
                return; // Nothing to do here ...
            }

            if (RequestIsText (e.Args, context)) {
                
                // Some sort of "textual" based type of request
                StreamReader reader = new StreamReader (HttpContext.Current.Request.InputStream);
                e.Args.Value = reader.ReadToEnd ();
            } else {

                // Some sort of "binary" type of request, we assume
                var rawBytes = new byte [HttpContext.Current.Request.InputStream.Length];
                HttpContext.Current.Request.InputStream.Read (rawBytes, 0, rawBytes.Length);
                e.Args.Value = rawBytes;
            }
        }

        /// <summary>
        ///     Saves the current HTTP request's body to a specified file
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "save-request-body")]
        public static void save_request_body (ApplicationContext context, ActiveEventArgs e)
        {
            // Getting filename
            var filename = e.Args.GetExValue<string> (context);

            // Making sure we transform filename into actual filename in case it uses our "~" logic
            filename = context.Raise (".p5.io.unroll-path", new Node ("", filename)).Get<string> (context);

            // Verifying user is authorized writing to the file
            context.Raise (".p5.io.authorize.modify-file", new Node ("", filename).Add ("args", e.Args));

            // Retrieving root node of web application
            var rootFolder = context.Raise (".p5.core.application-folder").Get<string> (context);

            // Creating a file stream, copying the request stream to our filestream
            using (FileStream fs = File.Create (rootFolder + filename)) {
                HttpContext.Current.Request.InputStream.CopyTo (fs);
            }
        }

        /// <summary>
        ///     Returns true if the user is coming in from a mobile device
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "request-is-mobile-device")]
        public static void request_is_mobile_device (ApplicationContext context, ActiveEventArgs e)
        {
            var userAgent = HttpContext.Current.Request.UserAgent.ToLower ();
            if (userAgent.Contains ("blackberry") || 
                userAgent.Contains ("iphone") || 
                userAgent.Contains ("ppc") || 
                userAgent.Contains ("windows ce") || 
                userAgent.Contains ("mobile") || 
                userAgent.Contains ("palm") || 
                userAgent.Contains ("portable") || 
                userAgent.Contains ("opera mobi") || 
                userAgent.Contains ("android")) {

                // This is a mobile device, probably not on .Net's list of devices
                e.Args.Value = true;
            } else {

                // This "might" be a mobile device, checking the .Net Framework's property
                e.Args.Value = HttpContext.Current.Request.Browser.IsMobileDevice;
            }
        }

        /*
         * Determines if current request is "text"
         */
        private static bool RequestIsText (Node node, ApplicationContext context)
        {
            // Checking if Content-Type starts with "text/" ...
            if (HttpContext.Current.Request.ContentType.StartsWith ("text/"))
                return true;

            // Not text type
            return false;
        }
    }
}