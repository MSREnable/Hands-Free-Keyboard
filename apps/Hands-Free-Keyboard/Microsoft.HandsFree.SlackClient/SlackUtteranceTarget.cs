using Microsoft.HandsFree.Settings;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.SlackClient
{
    class SlackUtteranceTarget : IUtteranceTarget
    {
        static readonly string _username = CreateUsername();

        internal SlackUtteranceTarget()
        {
        }

        static string CreateUsername()
        {
            string username = string.Empty;

            //var accountName = MsrSettings.GetAccountName();
            //var atPosition = accountName.IndexOf('@');
            //if (atPosition != -1)
            //{
            //    username = accountName.Substring(atPosition);
            //}
            //else
            //{

            //    var slashPosition = accountName.IndexOf('\\');
            //    if (slashPosition != -1)
            //    {
            //        username = accountName.Substring(slashPosition + 1);
            //    }
            //    else
            //    {
            //        username = accountName;
            //    }
            //}

            return username;
        }

        static string SlackEscape(string unescaped)
        {
            var escaped = unescaped.Replace("&", "&amp;");
            escaped = escaped.Replace("<", "&lt;");
            escaped = escaped.Replace(">", "&gt;");
            return escaped;
        }

        public void Send(string utterance)
        {
            var escapedUtterance = SlackEscape(utterance);

            var payload = new Payload
            {
                Channel = AppSettings.Instance.General.SlackChannel,
                Username = _username,
                Text = escapedUtterance
            };

            var payloadJson = JsonConvert.SerializeObject(payload);

            using (var client = new WebClient())
            {
                var data = new NameValueCollection();
                data["payload"] = payloadJson;

                var uri = AppSettings.Instance.General.SlackHostUri;

                try
                {
                    var response = client.UploadValues(uri, "POST", data);

                    var responseString = Encoding.UTF8.GetString(response);
                    Debug.Assert(responseString == "ok");
                }
                catch
                {
                    Debug.Assert(false, "TODO: Need better error feedback");
                }
            }
        }

        public Task SendAsync(string utterance)
        {
            Send(utterance);
            return Task.FromResult(true);
        }
    }
}
