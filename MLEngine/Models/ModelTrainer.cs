using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace MLEngine.Models
{
    public delegate void StatusChanged(string status);
    public delegate void SlaveChanged(string slave);

    class ModelTrainer 
    {
        private CancellationTokenSource cts;

        public CancellationTokenSource Cts { set { cts = value; } }

        private string fname_train;
        private string fname_test;
        private string fname_valid;
        private string fname_model;

        private string baseFileName;
        public string BaseFileName
        {
            set
            {
                baseFileName = value;
                fname_train = GetNewFileName(baseFileName, "-train.csv");
                fname_test = GetNewFileName(baseFileName, "-test.csv");
                fname_valid = GetNewFileName(baseFileName, "-valid.csv");
                fname_model = GetNewFileName(baseFileName, "-model.zip");
            }
        }

        private string GetNewFileName(string baseFileName, string postFix)
        {
            List<string> words = baseFileName.Split('\\').ToList();

            string name = words.Last().Split('.').First() + postFix;
            words.Reverse();
            words.Remove(words.First());
            words.Reverse();
            words.Add(name);
            return string.Join("\\", words);
        }

        public UInt32 MyExperimentTime { get; set; }
        public string OptimizerResult { get; set; } = "";

        public event StatusChanged statusChanged;
        public event SlaveChanged slaveChanged;

        private string slaveResult;
        public string SlaveResult
        {
            get { return slaveResult; }
            set
            {
                slaveResult = value;
                slaveChanged?.Invoke(slaveResult);
            }
        }

        private string statusResult;
        public string StatusResult
        {
            get { return statusResult; }
            set
            {
                statusResult = value;
                statusChanged?.Invoke(statusResult);
            }
        }


        public void DoModel(string taskType)
        {
            if (taskType == "Binary Classification")
                DoBinaryModel(fname_train, fname_test, fname_model, fname_valid, MyExperimentTime);
            if (taskType == "Multi-class Classification")
                DoMultiModel(fname_train, fname_test, fname_model, fname_valid, MyExperimentTime);
            if (taskType == "Regression")
                DoRegressionModel(fname_train, fname_test, fname_model, fname_valid, MyExperimentTime);
        }

        void DoBinaryModel(string fname_train, string fname_test, string fname_model, string fname_valid, UInt32 MyExperimentTime)
        {
            MLContext mlContext = new MLContext();
            ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(fname_train, labelColumnIndex: 0, separatorChar: ';', groupColumns: false);

            TextLoader textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
            IDataView trainDataView = textLoader.Load(fname_train);
            IDataView testDataView = textLoader.Load(fname_test);
            IDataView validDataView = textLoader.Load(fname_valid);

            ColumnInformation columnInformation = columnInference.ColumnInformation;

            var experimentSettings = new BinaryExperimentSettings();

            experimentSettings.MaxExperimentTimeInSeconds = MyExperimentTime;
            experimentSettings.CancellationToken = cts.Token;

            experimentSettings.OptimizingMetric = BinaryClassificationMetric.Accuracy;

            experimentSettings.CacheDirectoryName = null;
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.AveragedPerceptron);
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.FastForest);
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.FastTree);
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.LbfgsLogisticRegression);
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.LightGbm);
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.LinearSvm);
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.SdcaLogisticRegression);
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.SgdCalibrated);
            //experimentSettings.Trainers.Remove(BinaryClassificationTrainer.SymbolicSgdLogisticRegression);

            var progressHandler = new BinaryExperimentProgressHandler();
            progressHandler.EventUpdateBinaryExperiment += TextBoxNewDataBinaryExperiment;

            ExperimentResult<BinaryClassificationMetrics> experimentResult = mlContext.Auto()
                    .CreateBinaryClassificationExperiment(experimentSettings)
                    .Execute(trainDataView, validDataView, columnInformation, progressHandler: progressHandler);
            
            RunDetail<BinaryClassificationMetrics> bestRun = experimentResult.BestRun;
            BinaryClassificationMetrics metrics = bestRun.ValidationMetrics;

            IDataView testDataViewWithBestScore = bestRun.Model.Transform(testDataView);
            BinaryClassificationMetrics testMetrics = mlContext.BinaryClassification.Evaluate(testDataViewWithBestScore);

            OptimizerResult = OptimizerResult + 
                (Environment.NewLine + Environment.NewLine) +
                ($"Total modeling passes:              {experimentResult.RunDetails.Count()} " + Environment.NewLine) +
                ($"The best results for the model:     {bestRun.TrainerName}" + Environment.NewLine + Environment.NewLine) +
                (Environment.NewLine + $"Characteristics of the best model:" + Environment.NewLine) +
                ($"------------------------------------------------------------------" + Environment.NewLine) +
                ($"Accuracy:                                                   {string.Format("{0:0.000}", metrics.Accuracy)}" + Environment.NewLine) +
                ($"F1Score            (Usefull):                               {string.Format("{0:0.000}", metrics.F1Score)}" + Environment.NewLine) +
                ($"Negative Precision (How many selected items are relevant?): {string.Format("{0:0.000}", metrics.NegativePrecision)}" + Environment.NewLine) +
                ($"Negative Recall    (How many relevant items are selected?): {string.Format("{0:0.000}", metrics.NegativeRecall)}" + Environment.NewLine) +
                ($"Positive Precision (How many selected items are relevant?): {string.Format("{0:0.000}", metrics.PositivePrecision)}" + Environment.NewLine) +
                ($"Positive Recall    (How many relevant items are selected?): {string.Format("{0:0.000}", metrics.PositiveRecall)}" + Environment.NewLine) +
                ($"Area Under Precision Recall Curve:                          {string.Format("{0:0.000}", metrics.AreaUnderPrecisionRecallCurve)}" + Environment.NewLine) +
                ($"Area Under Roc Curve:                                       {string.Format("{0:0.000}", metrics.AreaUnderRocCurve)}" + Environment.NewLine) +
                (Environment.NewLine + Environment.NewLine) +
                ($"Characteristics of the best model on the test sample:" + Environment.NewLine) +
                ($"------------------------------------------------------------------" + Environment.NewLine) +
                ($"Accuracy:                                                   {string.Format("{0:0.000}", testMetrics.Accuracy)}" + Environment.NewLine) +
                ($"F1Score            (Usefull):                               {string.Format("{0:0.000}", testMetrics.F1Score)}" + Environment.NewLine) +
                ($"Negative Precision (How many selected items are relevant?): {string.Format("{0:0.000}", testMetrics.NegativePrecision)}" + Environment.NewLine) +
                ($"Negative Recall    (How many relevant items are selected?): {string.Format("{0:0.000}", testMetrics.NegativeRecall)}" + Environment.NewLine) +
                ($"Positive Precision (How many selected items are relevant?): {string.Format("{0:0.000}", testMetrics.PositivePrecision)}" + Environment.NewLine) +
                ($"Positive Recall    (How many relevant items are selected?): {string.Format("{0:0.000}", testMetrics.PositiveRecall)}" + Environment.NewLine) +
                ($"Area Under Precision Recall Curve:                          {string.Format("{0:0.000}", testMetrics.AreaUnderPrecisionRecallCurve)}" + Environment.NewLine) +
                ($"Area Under Roc Curve:                                       {string.Format("{0:0.000}", testMetrics.AreaUnderRocCurve)}" + Environment.NewLine) +
                (Environment.NewLine + Environment.NewLine) +
                ($"Characteristics of the best model on the test sample:" + Environment.NewLine);


            SaveModelAsFile(mlContext, bestRun.Model, testDataViewWithBestScore, fname_model);
        }


        void DoMultiModel(string fname_train, string fname_test, string fname_model, string fname_valid, UInt32 MyExperimentTime)
        {
            MLContext mlContext = new MLContext();
            ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(fname_train, labelColumnIndex: 0, separatorChar: ';', groupColumns: false);

            TextLoader textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
            IDataView trainDataView = textLoader.Load(fname_train);
            IDataView testDataView = textLoader.Load(fname_test);
            IDataView validDataView = textLoader.Load(fname_valid);
            
            ColumnInformation columnInformation = columnInference.ColumnInformation;

            var experimentSettings = new MulticlassExperimentSettings();
            experimentSettings.MaxExperimentTimeInSeconds = MyExperimentTime;
            experimentSettings.CancellationToken = cts.Token;          
            experimentSettings.OptimizingMetric = MulticlassClassificationMetric.MacroAccuracy;
            experimentSettings.CacheDirectoryName = null;
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.AveragedPerceptronOva);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.LbfgsLogisticRegressionOva);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.LbfgsMaximumEntropy);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.SdcaMaximumEntropy);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.SgdCalibratedOva);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.SymbolicSgdLogisticRegressionOva);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.LinearSupportVectorMachinesOva);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.LightGbm);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.FastTreeOva);
            //experimentSettings.Trainers.Remove(MulticlassClassificationTrainer.FastForestOva);





            var progressHandler = new MulticlassExperimentProgressHandler();
            progressHandler.EventUpdateMulticlassExperiment += TextBoxNewDataMulticlassExperiment;            
            
            ExperimentResult<MulticlassClassificationMetrics> experimentResult = mlContext.Auto()
                .CreateMulticlassClassificationExperiment(experimentSettings)
                .Execute(trainDataView, validDataView, columnInformation, progressHandler: progressHandler);

            RunDetail<MulticlassClassificationMetrics> bestRun = experimentResult.BestRun;
            MulticlassClassificationMetrics metrics = bestRun.ValidationMetrics;
            IDataView testDataViewWithBestScore = bestRun.Model.Transform(testDataView);
            MulticlassClassificationMetrics testMetrics = mlContext.MulticlassClassification.Evaluate(testDataViewWithBestScore);
            
            
            OptimizerResult = OptimizerResult + 
                (Environment.NewLine + Environment.NewLine + Environment.NewLine) +
                ($"Total modeling passes:              {experimentResult.RunDetails.Count()} " + Environment.NewLine) +
                ($"The best results for the model:     {bestRun.TrainerName}" + Environment.NewLine + Environment.NewLine) +
                (Environment.NewLine + $"Characteristics of the best model:" + Environment.NewLine) +
                ($"------------------------------------------------------------------" + Environment.NewLine) +
                ($"MacroAccuracy:                                              {string.Format("{0:0.000}", metrics.MacroAccuracy)}" + Environment.NewLine) +
                ($"MicroAccuracy:                                              {string.Format("{0:0.000}", metrics.MicroAccuracy)}" + Environment.NewLine) +
                ($"LogLoss:                                                    {string.Format("{0:0.000}", metrics.LogLoss)}" + Environment.NewLine) +
                ($"LogLossReduction:                                           {string.Format("{0:0.000}", metrics.LogLossReduction)}" + Environment.NewLine) +
                (Environment.NewLine + Environment.NewLine) + 
                ($"Characteristics of the best model on the test sample:" + Environment.NewLine) +
                ($"------------------------------------------------------------------" + Environment.NewLine) +
                ($"MacroAccuracy:                                              {string.Format("{0:0.000}", testMetrics.MacroAccuracy)}" + Environment.NewLine) +
                ($"MicroAccuracy:                                              {string.Format("{0:0.000}", testMetrics.MicroAccuracy)}" + Environment.NewLine) +
                ($"LogLoss:                                                    {string.Format("{0:0.000}", testMetrics.LogLoss)}" + Environment.NewLine) +
                ($"LogLossReduction:                                           {string.Format("{0:0.000}", testMetrics.LogLossReduction)}" + Environment.NewLine) +
                (Environment.NewLine + Environment.NewLine) + 
                ($"Characteristics of the best model on the test sample:" + Environment.NewLine);
            
            
            SaveModelAsFile(mlContext, bestRun.Model, testDataViewWithBestScore, fname_model);
        }


        void DoRegressionModel(string fname_train, string fname_test, string fname_model, string fname_valid, UInt32 MyExperimentTime)
        {
            try
            {
                MLContext mlContext = new MLContext(seed: 0);
                ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(fname_train, labelColumnIndex: 0, separatorChar: ';', groupColumns: false) ;

                TextLoader textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
                IDataView trainDataView = textLoader.Load(fname_train);
                IDataView testDataView = textLoader.Load(fname_test);
                IDataView validDataView = textLoader.Load(fname_valid);

                ColumnInformation columnInformation = columnInference.ColumnInformation;

                var experimentSettings = new RegressionExperimentSettings();

                experimentSettings.MaxExperimentTimeInSeconds = MyExperimentTime;
                experimentSettings.OptimizingMetric = RegressionMetric.RSquared;
                experimentSettings.CancellationToken = cts.Token;
                experimentSettings.CacheDirectoryName = null;

                //experimentSettings.Trainers.Remove(RegressionTrainer.FastTreeTweedie);
                experimentSettings.Trainers.Remove(RegressionTrainer.LbfgsPoissonRegression);
                //experimentSettings.Trainers.Remove(RegressionTrainer.OnlineGradientDescent);
                //experimentSettings.Trainers.Remove(RegressionTrainer.StochasticDualCoordinateAscent);
                //experimentSettings.Trainers.Remove(RegressionTrainer.Ols);
                


                var progressHandler = new RegressionExperimentProgressHandler();
                progressHandler.EventUpdateRegressionExperiment += TextBoxNewRegressionExperiment;

                ExperimentResult<RegressionMetrics> experimentResult = mlContext.Auto()
                        .CreateRegressionExperiment(experimentSettings)
                        .Execute(trainDataView, validDataView, columnInformation, progressHandler: progressHandler);
                RunDetail<RegressionMetrics> bestRun = experimentResult.BestRun;
                RegressionMetrics metrics = bestRun.ValidationMetrics;
                IDataView testDataViewWithBestScore = bestRun.Model.Transform(testDataView);
                RegressionMetrics testMetrics = mlContext.Regression.Evaluate(testDataViewWithBestScore);

                OptimizerResult = OptimizerResult +
                (Environment.NewLine + Environment.NewLine + Environment.NewLine) +
                ($"Total modeling passes:              {experimentResult.RunDetails.Count()} " + Environment.NewLine) +
                ($"The best results for the model:     {bestRun.TrainerName}" + Environment.NewLine + Environment.NewLine) +
                (Environment.NewLine + $"Characteristics of the best model:" + Environment.NewLine) +
                ($"------------------------------------------------------------------" + Environment.NewLine) +
                ($"RSquared:                                     {string.Format("{0:0.00000}", metrics.RSquared)}" + Environment.NewLine) +
                ($"Loss Function:                                {string.Format("{0:0.00000}", metrics.LossFunction)}" + Environment.NewLine) +
                ($"Mean Absolute Error:                          {string.Format("{0:0.00000}", metrics.MeanAbsoluteError)}" + Environment.NewLine) +
                ($"Mean Squared Error:                           {string.Format("{0:0.00000}", metrics.MeanSquaredError)}" + Environment.NewLine) +
                ($"Root Mean Squared Error:                      {string.Format("{0:0.00000}", metrics.RootMeanSquaredError)}" + Environment.NewLine) +
                (Environment.NewLine + Environment.NewLine) +
                ($"Characteristics of the best model on the test sample:" + Environment.NewLine) +
                ($"------------------------------------------------------------------" + Environment.NewLine) +
                ($"RSquared:                                     {string.Format("{0:0.00000}", testMetrics.RSquared)}" + Environment.NewLine) +
                ($"Loss Function:                                {string.Format("{0:0.00000}", testMetrics.LossFunction)}" + Environment.NewLine) +
                ($"Mean Absolute Error:                          {string.Format("{0:0.00000}", testMetrics.MeanAbsoluteError)}" + Environment.NewLine) +
                ($"Mean Squared Error:                           {string.Format("{0:0.00000}", testMetrics.MeanSquaredError)}" + Environment.NewLine) +
                ($"Root Mean Squared Error:                      {string.Format("{0:0.00000}", testMetrics.RootMeanSquaredError)}" + Environment.NewLine);

                SaveModelAsFile(mlContext, bestRun.Model, testDataViewWithBestScore, fname_model);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            
        }

        

        void TextBoxNewDataBinaryExperiment(object obj, EventUpdateBinaryExperimentArgs e)
        {
            double counter = Math.Round(e.AllRunTime, 0);
            if (counter < 0)
                counter = 0;

            if (counter > 0)
            {
                StatusResult = ($"Performing a binary classification experiment. {counter.ToString()} seconds have passed");
            }
            else
            {
                StatusResult = ($"Modeling is completed, saving the model");
            }


            SlaveResult = $"Best Accuracy:   {e.BinExpAccuracy:P3}" + Environment.NewLine +
                             $"Best F1-Score:   {e.F1score:F5}" + Environment.NewLine +
                             $"Best Algorythm:  {e.Algorythm}" + Environment.NewLine +
                             $"Last Algorythm:  {e.IterationResult.TrainerName}" + Environment.NewLine +
                             $"Modeling passes: {e.IterationIndex}";
        }

        void TextBoxNewDataMulticlassExperiment(object obj, EventUpdateMulticlassExperimentArgs e)
        {
            double counter = Math.Round(e.AllRunTime, 0);
            if (counter < 0)
                counter = 0;

            if (counter > 0)
            {
                StatusResult = ($"Performing a Multiclass classification experiment. {counter.ToString()} seconds have passed");
            }
            else
            {
                StatusResult = ($"Modeling is completed, saving the model");
            }

            SlaveResult = $"Best MacroAccuracy:   {e.MacroAccuracy:P3}" + Environment.NewLine +
                             $"Best MicroAccuracy:   {e.MicroAccuracy:F5}" + Environment.NewLine +
                             $"Best Algorythm:       {e.Algorythm}" + Environment.NewLine +
                             $"Last Algorythm:       {e.IterationResult.TrainerName}" + Environment.NewLine +
                             $"Modeling passes:      {e.IterationIndex}";
        }

        void TextBoxNewRegressionExperiment(object obj, EventUpdateRegressionExperimentArgs e)
        {
            double counter = Math.Round(e.AllRunTime, 0);
            if (counter < 0)
                counter = 0;

            if (counter > 0)
            {
                StatusResult = ($"Performing a regression experiment. {counter.ToString()} seconds have passed");
            }
            else
            {
                StatusResult = ($"Modeling is completed, saving the model");
            }

            SlaveResult = $"Best RSquared:   {e.BRSquared:F5}" + Environment.NewLine +
                             $"Best MeanAError: {e.BMeanAError:F5}" + Environment.NewLine +
                             $"Best Algorythm:  {e.BAlgorythm}" + Environment.NewLine +
                             $"Last Algorythm:  {e.IterationResult.TrainerName}" + Environment.NewLine +
                             $"Modeling passes: {e.IterationIndex}";
        }

        private static void SaveModelAsFile(MLContext mlContext, ITransformer model, IDataView dataView, string fname_model)
        {
            using (var fileStream = new FileStream(fname_model, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                mlContext.Model.Save(model, dataView.Schema, fileStream);
            }
        }
    }
}
