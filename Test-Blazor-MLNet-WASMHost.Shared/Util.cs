﻿using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Test_Blazor_MLNet_WASMHost.Shared
{
    public class Util
    {
        private static MLContext mlContext = new MLContext();
        private static Dictionary<string, PredictionEngine<MLBBaseballBatter, MLBHOFPrediction>> _predictionEngines = new Dictionary<string, PredictionEngine<MLBBaseballBatter, MLBHOFPrediction>>();

        public static Stream GetModel(string predictionType, string algorithmName)
        {
            var assembly = typeof(Test_Blazor_MLNet_WASMHost.Shared.Util).Assembly;
            Stream resource = assembly.GetManifestResourceStream($"Test_Blazor_MLNet_WASMHost.Shared.Models.{predictionType}-{algorithmName}.mlnet");

            return resource;
        }

        public static Stream GetModelOnnx()
        {
            var assembly = typeof(Test_Blazor_MLNet_WASMHost.Shared.Util).Assembly;
            Stream resource = assembly.GetManifestResourceStream($"Test_Blazor_MLNet_WASMHost.Shared.Models.InductedToHallOfFame-FastTree.onnx");

            return resource;
        }

        public static Stream GetBaseballData()
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
                var modelStream = Util.GetModel(predictionType, algorithmName);
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

            //if (algorithmName != "StackedEnsemble")
            //{
            //    PredictionEngine<MLBBaseballBatter, MLBHOFPrediction> _predictionEngineInductedToHallOfFame =
            //        Util.GetPredictionEngine(mlContext, "InductedToHallOfFame", algorithmName);
            //    PredictionEngine<MLBBaseballBatter, MLBHOFPrediction> _predictionEngineOnHallOfFameBallot =
            //        Util.GetPredictionEngine(mlContext, "OnHallOfFameBallot", algorithmName);

            //    for (int i = 0; i != mLBBaseballBatter.YearsPlayed; i++)
            //    {
            //        var season = i + 1;
            //        var batterSeason = selectedBatterSeasons.Where(s => Convert.ToInt32(s.YearsPlayed) == season).First();
            //        var onHallOfFameBallotPrediction = _predictionEngineOnHallOfFameBallot.Predict(batterSeason);
            //        var inductedToHallOfFamePrediction = _predictionEngineInductedToHallOfFame.Predict(batterSeason);

            //        var seasonPrediction = new MLBBaseballBatterSeasonPrediction
            //        {
            //            SeasonNumber = season,
            //            FullPlayerName = mLBBaseballBatter.FullPlayerName,
            //            InductedToHallOfFamePrediction = inductedToHallOfFamePrediction.Prediction,
            //            InductedToHallOfFameProbability = Math.Round(inductedToHallOfFamePrediction.Probability, 5, MidpointRounding.AwayFromZero),
            //            OnHallOfFameBallotPrediction = onHallOfFameBallotPrediction.Prediction,
            //            OnHallOfFameBallotProbability = Math.Round(onHallOfFameBallotPrediction.Probability, 5, MidpointRounding.AwayFromZero)
            //        };

            //        seasonPrediction.InductedToHallOfFameProbabilityLabel = (seasonPrediction.InductedToHallOfFameProbability == 0f) ? "N/A" : seasonPrediction.InductedToHallOfFameProbability.ToString();
            //        seasonPrediction.OnHallOfFameBallotProbabilityLabel = (seasonPrediction.OnHallOfFameBallotProbability == 0f) ? "N/A" : seasonPrediction.OnHallOfFameBallotProbability.ToString();


            //        mlbBaseballBatterSeasonPredictions.Add(seasonPrediction);
            //    }
            //}
            //else
            //{
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
    }
}
