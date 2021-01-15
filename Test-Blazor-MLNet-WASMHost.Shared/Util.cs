using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Lucene.Net;
using Lucene.Net.Documents;

namespace Test_Blazor_MLNet_WASMHost.Shared
{
    public class Util
    {
        private static MLContext mlContext = new MLContext();
        private static Dictionary<string, ITransformer> _itransfomerModels = new Dictionary<string, ITransformer>();
        private static Dictionary<string, PredictionEngine<MLBBaseballBatter, MLBHOFPrediction>> _predictionEngines = new Dictionary<string, PredictionEngine<MLBBaseballBatter, MLBHOFPrediction>>();

        public static ITransformer GetModel(MLContext mlContext, string predictionType, string algorithmName)
        {
            var _transformerModelKey = $"{predictionType}-{algorithmName}";
            ITransformer transformerModel;

            if (!_itransfomerModels.TryGetValue(_transformerModelKey, out transformerModel))
            {
                DataViewSchema schema;
                var modelStream = Util.GetModelStream(predictionType, algorithmName);
                transformerModel = mlContext.Model.Load(modelStream, out schema);

                _itransfomerModels.Add(_transformerModelKey, transformerModel);
            }

            return transformerModel;
        }

        public static Stream GetModelStream(string predictionType, string algorithmName)
        {
            var assembly = typeof(Test_Blazor_MLNet_WASMHost.Shared.Util).Assembly;
            Stream resource = assembly.GetManifestResourceStream($"Test_Blazor_MLNet_WASMHost.Shared.Models.{predictionType}-{algorithmName}.mlnet");

            return resource;
        }

        public static Stream GetBaseballDataStream()
        {
            var assembly = typeof(Test_Blazor_MLNet_WASMHost.Shared.Util).Assembly;

            // var test = assembly.GetManifestResourceNames();
            // taskkill /IM dotnet.exe /F /T 2>nul 1>nul

            Stream resource = assembly.GetManifestResourceStream($"Test_Blazor_MLNet_WASMHost.Shared.Data.MLBBaseballBattersHistorical.csv");

            return resource;
        }

        public static PredictionEngine<MLBBaseballBatter, MLBHOFPrediction> GetPredictionEngine(MLContext mlContext, string predictionType, string algorithmName)
        {
            var _predictionEngineKey = $"{predictionType}-{algorithmName}";
            PredictionEngine<MLBBaseballBatter, MLBHOFPrediction> _predictionEngine;

            if (!_predictionEngines.TryGetValue(_predictionEngineKey, out _predictionEngine))
            {
                DataViewSchema schema;
                var modelStream = Util.GetModelStream(predictionType, algorithmName);
                ITransformer _model = mlContext.Model.Load(modelStream, out schema);

                _predictionEngine = mlContext.Model.CreatePredictionEngine<MLBBaseballBatter, MLBHOFPrediction>(_model);
                _predictionEngines.Add(_predictionEngineKey, _predictionEngine);
            }

            return _predictionEngine;
        }

        public static PredictionData GetMLBBaseballBatterSeasonPredictions(string algorithmName, MLBBaseballBatter mLBBaseballBatter, List<MLBBaseballBatter> selectedBatterSeasons)
        {
            // Object to return
            var predictionData = new PredictionData();

            var _predictionEngineInductedToHallOfFameKey = $"Inducted-{algorithmName}";
            var _predictionEngineOnHallOfFameBallotKey = $"OnHallOfFameBallot-{algorithmName}";

            var mlbBaseballBatterSeasonPredictions = new List<MLBBaseballBatterSeasonPrediction>();

            var chartData = new List<PredictionChartData>();

            var algorithNamesForEnsemble = new List<string> { "FastTree", "GeneralizedAdditiveModels", "LightGbm",
                    "LogisticRegression", "StochasticGradientDescentCalibrated" };

            // If algorithm is selected that does not return probabilities add it
            var exists = algorithNamesForEnsemble.Exists(a => a.Contains(algorithmName));

            if ((!exists) && (algorithmName != "StackedEnsemble"))
            {
                algorithNamesForEnsemble.Add(algorithmName);
            }

            for (int i = 0; i != mLBBaseballBatter.YearsPlayed; i++)
            {
                var season = i + 1;
                var batterSeason = selectedBatterSeasons.Where(s => Convert.ToInt32(s.YearsPlayed) == season).First();
                MLBBaseballBatterSeasonPrediction seasonPrediction;

                var probabilitiesInducted = new List<AlgorithmPrediction>();
                var probabilitiesOnHallOfFameBallot = new List<AlgorithmPrediction>();

                foreach (var algorithmNameEnsemble in algorithNamesForEnsemble)
                {
                    PredictionEngine<MLBBaseballBatter, MLBHOFPrediction> _predictionEngineInductedToHallOfFameEnsemble =
                        Util.GetPredictionEngine(mlContext, "InductedToHallOfFame", algorithmNameEnsemble);
                    PredictionEngine<MLBBaseballBatter, MLBHOFPrediction> _predictionEngineOnHallOfFameBallotEnsemble =
                        Util.GetPredictionEngine(mlContext, "OnHallOfFameBallot", algorithmNameEnsemble);

                    // Note: Cannot do this in Blazor client-side, Transform method starts multiple threads which is not supported
                    //var modelTest = Util.GetModel(mlContext, "OnHallOfFameBallot", algorithmNameEnsemble);
                    //var seasonsView = mlContext.Data.LoadFromEnumerable(selectedBatterSeasons);
                    //var preview = modelTest.Transform(seasonsView).Preview();

                    var onHallOfFameBallotPredictionEnsemble = _predictionEngineOnHallOfFameBallotEnsemble.Predict(batterSeason);
                    var inductedToHallOfFamePredictionEnsemble = _predictionEngineInductedToHallOfFameEnsemble.Predict(batterSeason);

                    probabilitiesInducted.Add(
                        new AlgorithmPrediction
                        {
                            AlgorithmName = algorithmNameEnsemble,
                            Prediction = inductedToHallOfFamePredictionEnsemble.Prediction,
                            Probability = inductedToHallOfFamePredictionEnsemble.Probability
                        });
                    probabilitiesOnHallOfFameBallot.Add(
                        new AlgorithmPrediction
                        {
                            AlgorithmName = algorithmNameEnsemble,
                            Prediction = onHallOfFameBallotPredictionEnsemble.Prediction,
                            Probability = onHallOfFameBallotPredictionEnsemble.Probability
                        });

                    // Only add probabilities for algorithms that return probabilities
                    if (algorithmName == "FastTree" || algorithmName == "GeneralizedAdditiveModels" || algorithmName == "LightGbm" || algorithmName == "LogisticRegression" ||
                    algorithmName == "StochasticGradientDescentCalibrated" || algorithmName == "StackedEnsemble")
                    {
                        chartData.Add(new PredictionChartData
                        {
                            Algorithm = algorithmNameEnsemble,
                            InductedToHallOfFameProbability = inductedToHallOfFamePredictionEnsemble.Probability,
                            OnHallOfFameBallotProbability = onHallOfFameBallotPredictionEnsemble.Probability,
                            SeasonPlayed = season
                        });
                    }

                } // EOF Foreach Algorithm Ensemble


                if (algorithmName == "StackedEnsemble")
                {
                    // Average out predictions for ensemble
                    var probabilityInducted = probabilitiesInducted.Select(a => a.Probability).Sum() / 5;
                    var probabilityOnHallOfFameBallot = probabilitiesOnHallOfFameBallot.Select(a => a.Probability).Sum() / 5;

                    seasonPrediction = new MLBBaseballBatterSeasonPrediction
                    {
                        SeasonNumber = season,
                        FullPlayerName = mLBBaseballBatter.FullPlayerName,
                        InductedToHallOfFamePrediction = (probabilityInducted > 0.5f) ? true : false,
                        InductedToHallOfFameProbability = probabilityInducted,
                        OnHallOfFameBallotPrediction = (probabilityOnHallOfFameBallot > 0.5f) ? true : false,
                        OnHallOfFameBallotProbability = probabilityOnHallOfFameBallot
                    };
                }
                else
                {
                    // Average out predictions for ensemble
                    var probabilityInducted = probabilitiesInducted.Where(a => a.AlgorithmName == algorithmName).FirstOrDefault()?.Probability ?? 0f;
                    var probabilityOnHallOfFameBallot = probabilitiesOnHallOfFameBallot.Where(a => a.AlgorithmName == algorithmName).FirstOrDefault()?.Probability ?? 0f;

                    seasonPrediction = new MLBBaseballBatterSeasonPrediction
                    {
                        SeasonNumber = season,
                        FullPlayerName = mLBBaseballBatter.FullPlayerName,
                        InductedToHallOfFamePrediction = probabilitiesInducted.Where(a => a.AlgorithmName == algorithmName).FirstOrDefault().Prediction,
                        InductedToHallOfFameProbability = probabilityInducted,
                        OnHallOfFameBallotPrediction = probabilitiesOnHallOfFameBallot.Where(a => a.AlgorithmName == algorithmName).FirstOrDefault().Prediction,
                        OnHallOfFameBallotProbability = probabilityOnHallOfFameBallot
                    };
                }

                seasonPrediction.InductedToHallOfFameProbabilityLabel = (seasonPrediction.InductedToHallOfFameProbability == 0f) ? "N/A" :
                    Math.Round(seasonPrediction.InductedToHallOfFameProbability, 6, MidpointRounding.AwayFromZero).ToString("0.000");
                seasonPrediction.OnHallOfFameBallotProbabilityLabel = (seasonPrediction.OnHallOfFameBallotProbability == 0f) ? "N/A" :
                    Math.Round(seasonPrediction.OnHallOfFameBallotProbability, 6, MidpointRounding.AwayFromZero).ToString("0.000");
                mlbBaseballBatterSeasonPredictions.Add(seasonPrediction);

                // Add StackedEnsemble always to the ChartData
                chartData.Add(new PredictionChartData
                {
                    Algorithm = "StackedEnsemble",
                    InductedToHallOfFameProbability = seasonPrediction.InductedToHallOfFameProbability,
                    OnHallOfFameBallotProbability = seasonPrediction.OnHallOfFameBallotProbability,
                    SeasonPlayed = season
                });
                //}

                // Get the min/max for each season
                var chardDataMin =
                    chartData
                        .GroupBy(c => new
                        {
                            c.SeasonPlayed
                        })
                        .Select(gcs => new PredictionChartDataMinMax()
                        {
                            Algorithm = "OnHallOfFameBallot",
                            SeasonPlayed = gcs.Key.SeasonPlayed,
                            Min = gcs.Min(g => g.OnHallOfFameBallotProbability),
                            Max = gcs.Max(g => g.OnHallOfFameBallotProbability)
                        }).ToList();
                var chardDataMax =
                    chartData
                        .GroupBy(c => new
                        {
                            c.SeasonPlayed
                        })
                        .Select(gcs => new PredictionChartDataMinMax()
                        {
                            Algorithm = "InductedToHallOfFame",
                            SeasonPlayed = gcs.Key.SeasonPlayed,
                            Min = gcs.Min(g => g.InductedToHallOfFameProbability),
                            Max = gcs.Max(g => g.InductedToHallOfFameProbability)
                        }).ToList();

                predictionData.ChartData = chartData;

                chardDataMin.AddRange(chardDataMax);
                predictionData.ChartDataMinMax = chardDataMin;
            }

            predictionData.MLBBaseballBatterSeasonPredictions = mlbBaseballBatterSeasonPredictions;

            return predictionData;
        }

        public static void LoadLuceneIndexIntoDirectory()
        {
            var assembly = typeof(Test_Blazor_MLNet_WASMHost.Shared.Util).Assembly;
            // var resources = assembly.GetManifestResourceNames();

            Stream resource = assembly.GetManifestResourceStream($"Test_Blazor_MLNet_WASMHost.Shared.LuceneIndex.LuceneIndex.zip");
            Console.WriteLine("LoadLuceneIndexIntoDirectory - Retrieved Stream");

            var indexPath = Path.Combine(Environment.CurrentDirectory, "LuceneIndex.zip");
            Console.WriteLine("LoadLuceneIndexIntoDirectory - Retrieved Index");

            var fileStream = File.Create(indexPath);
            Console.WriteLine("LoadLuceneIndexIntoDirectory - Created file stream");

            resource.CopyTo(fileStream);
            Console.WriteLine("LoadLuceneIndexIntoDirectory - Copied To Stream");

            ZipFile.ExtractToDirectory(indexPath, Environment.CurrentDirectory, true);
            Console.WriteLine("LoadLuceneIndexIntoDirectory - Extracted index to Current Directory: " + Environment.CurrentDirectory);
        }

        public static MLBBaseballBatter GetBaseballBatterFromLuceneDocument(Document document)
        {
            var mlbBaseballBatter = new MLBBaseballBatter
            {
                ID = document.GetField("Id").GetStringValue(),
                FullPlayerName = document.GetField("FullPlayerName").GetStringValue(),
                YearsPlayed = (float)document.GetField("YearsPlayed").GetSingleValue(),
                AB = (float)document.GetField("AB").GetSingleValue(),
                R = (float)document.GetField("R").GetSingleValue(),
                H = (float)document.GetField("H").GetSingleValue(),
                Doubles = (float)document.GetField("Doubles").GetSingleValue(),
                Triples = (float)document.GetField("Triples").GetSingleValue(),
                HR = (float)document.GetField("HR").GetSingleValue(),
                RBI = (float)document.GetField("RBI").GetSingleValue(),
                SB = (float)document.GetField("SB").GetSingleValue(),
                BattingAverage = (float)document.GetField("BattingAverage").GetSingleValue(),
                SluggingPct = (float)document.GetField("SluggingPct").GetSingleValue(),
                AllStarAppearances = (float)document.GetField("AllStarAppearances").GetSingleValue(),
                MVPs = (float)document.GetField("MVPs").GetSingleValue(),
                TripleCrowns = (float)document.GetField("TripleCrowns").GetSingleValue(),
                GoldGloves = (float)document.GetField("GoldGloves").GetSingleValue(),
                MajorLeaguePlayerOfTheYearAwards = (float)document.GetField("MajorLeaguePlayerOfTheYearAwards").GetSingleValue(),
                TB = (float)document.GetField("TB").GetSingleValue(),
                TotalPlayerAwards = (float)document.GetField("TotalPlayerAwards").GetSingleValue(),
                LastYearPlayed = (float)document.GetField("LastYearPlayed").GetSingleValue(),
            };

            return mlbBaseballBatter;
        }
    }
}
