using System.Text;
using System.Text.Json;

namespace Microsoft.Research.RankWriter.Library.Test
{
    /// <summary>
    /// Helper for JSON serialization.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Convert predictor to JSON.
        /// </summary>
        /// <param name="predictor"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static byte[] ToJsonUtf8(this TokenPredictor predictor, StringTokens tokens)
        {
            var dictionary = predictor.ToJsonDictionary(tokens);

            var utf8 = JsonSerializer.SerializeToUtf8Bytes(dictionary);

            return utf8;
        }

        /// <summary>
        /// Convert predictor to JSON.
        /// </summary>
        /// <param name="predictor"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string ToJson(this TokenPredictor predictor, StringTokens tokens)
        {
            var utf8 = predictor.ToJsonUtf8(tokens);

            var json = Encoding.UTF8.GetString(utf8);

            return json;
        }
    }
}
