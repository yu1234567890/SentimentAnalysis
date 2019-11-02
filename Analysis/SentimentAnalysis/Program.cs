using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;

namespace SentimentAnalysis
{
    class Program
    {
        static readonly string _dataPath = Path.Combine(Environment.CurrentDirectory, "Data", "yelp_labelled.txt");
        static string fromText;
        static string toText;

        static void Main(string[] args)
        {
            Translator trans = new Translator();
            trans.FromLanguage = "ja";
            trans.ToLanguage = "en";
            trans.ModelType = "generalnn";
            // Azureで取得したキーを設定。
            trans.OcpApimSubscriptionKey = "";

            // 解析文字列設定
            fromText = Console.ReadLine();
            toText = trans.TranslatorText(fromText);

            // mlContextの初期化
            MLContext mlContext = new MLContext();

            // 学習用データの読み取り
            TrainTestData splitDataView = LoadData(mlContext);

            // モデル生成
            ITransformer model = BuildAndTrainModel(mlContext, splitDataView.TrainSet);

            // 文字列の解析
            UseModelWithBatchItems(mlContext, model);

        }

        public static TrainTestData LoadData(MLContext mlContext)
        {
            // テキストデータをIDataViewに格納
            IDataView dataView = mlContext.Data.LoadFromTextFile<SentimentData>(_dataPath, hasHeader: false);

            // testFraction:テストデータに入る割合
            TrainTestData splitDataView = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            return splitDataView;      
        }

        public static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView splitTrainSet)
        {
            var estimator = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentData.SentimentText))
            .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            var model = estimator.Fit(splitTrainSet);
            return model;
        }

        public static void UseModelWithBatchItems(MLContext mlContext, ITransformer model)
        {
            IEnumerable<SentimentData> sentiments = new[]
            {
                new SentimentData
                {
                    SentimentText = toText
                }
            };

            IDataView batchComments = mlContext.Data.LoadFromEnumerable(sentiments);

            IDataView predictions = model.Transform(batchComments);

            IEnumerable<SentimentPrediction> predictedResults = mlContext.Data.CreateEnumerable<SentimentPrediction>(predictions, reuseRowObject: false);

            foreach (SentimentPrediction prediction  in predictedResults)
            {
                Console.WriteLine("翻訳前:" + fromText);
                Console.WriteLine("翻訳後:" + toText);
                Console.WriteLine("結果:" + (Convert.ToBoolean(prediction.Prediction) ? "肯定的" : "否定的"));
                Console.WriteLine("数値:" + prediction.Probability.ToString());
            }  
        }

    }
}
