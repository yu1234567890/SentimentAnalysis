using System;
using System.IO;
using System.Net;

namespace SentimentAnalysis
{
    public class Translator
    {
        private string fromLanguage;
        /// <summary>
        /// 翻訳前の言語
        /// </summary>
        public string FromLanguage
        {
            get { return fromLanguage; }
            set { fromLanguage = value; }
        }

        private string toLanguage;
        /// <summary>
        /// 翻訳後の言語
        /// </summary>
        public string ToLanguage
        {
            get { return toLanguage; }
            set { toLanguage = value; }
        }

        private string modelType;
        /// <summary>
        /// 利用するモデル  ：空白 (=統計的機械翻訳) or generalnn(=ニューラルネットワーク)を指定
        /// </summary>
        public string ModelType
        {
            get { return modelType; }
            set { modelType = value; }
        }

        private string ocpApimSubscriptionKey;
        /// <summary>
        /// Translator Text API の Subscription Key（Azure Portal(https://portal.azure.com)から取得）
        /// </summary>
        public string OcpApimSubscriptionKey
        {
            get { return ocpApimSubscriptionKey; }
            set { ocpApimSubscriptionKey = value; }
        }

        public string TranslatorText(string fromText)
        {
            string toText = "";

            //URIの定義（Getでパラメータを送信する為、URLに必要なパラメーターを付加）
            string uri = "https://api.microsofttranslator.com/v2/http.svc/Translate?" +
            "&text=" + Uri.EscapeDataString(fromText) +
            "&from=" + fromLanguage +
            "&to=" + toLanguage +
            "&category=" + ModelType;

            //httpWebRequestの作成
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

            //Authorizationのためにhttpヘッダーにサブスクリプションキーを埋め込む
            httpWebRequest.Headers.Add("Ocp-Apim-Subscription-Key:" + ocpApimSubscriptionKey);

            WebResponse response = null;

            //Translator Text APIへのリクエストを実行して結果を取得
            response = httpWebRequest.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                System.Runtime.Serialization.DataContractSerializer dcs =
                new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                toText = (string)dcs.ReadObject(stream);
            }

            return toText;
        }
    }
}
