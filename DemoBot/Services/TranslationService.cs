using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace DemoBot.Services
{
    public static class TranslationService
    {
        private const string SUBSCRIPTION_KEY = "<subscription_key>";
        private const string AUTHENTICATION_ENDPOINT = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private const string TRANSLATION_ENDPOINT = "https://api.microsofttranslator.com/v2/http.svc/Translate";

        public static async Task<string> TranslateAsync(string textToTranslate, string targetLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(textToTranslate) || string.IsNullOrWhiteSpace(targetLanguageCode))
            {
                return null;
            }

            // get access token first
            var bearerToken = await (await AUTHENTICATION_ENDPOINT
                .WithHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY)
                .PostStringAsync(string.Empty))
                .Content
                .ReadAsStringAsync();

            var translatedTextInXml = await TRANSLATION_ENDPOINT
                .SetQueryParam("text", textToTranslate)
                .SetQueryParam("to", targetLanguageCode)
                .WithHeader("Authorization", $"Bearer {bearerToken}")
                .GetStringAsync();

            var indexEndOpeningTag = translatedTextInXml.IndexOf(">");
            var indexStartClosingTag = translatedTextInXml.LastIndexOf("<");
            var translatedText = translatedTextInXml.Substring(indexEndOpeningTag + 1, indexStartClosingTag - indexEndOpeningTag - 1);

            return translatedText;
        }
    }
}
