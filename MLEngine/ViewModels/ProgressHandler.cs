using System;
using System.Windows.Forms;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

namespace MLEngine
{
    public delegate void UpdterBinaryExperimentProgressHandler(object obj, EventUpdateBinaryExperimentArgs e);
    public delegate void UpdterMulticlassExperimentProgressHandler(object obj, EventUpdateMulticlassExperimentArgs e);
    public delegate void UpdterRegressionExperimentProgressHandler(object obj, EventUpdateRegressionExperimentArgs e);

    public class EventUpdateBinaryExperimentArgs
    {
        public RunDetail<BinaryClassificationMetrics> IterationResult { get; }
        public int IterationIndex { get; }
        public double AllRunTime { get; }
        public double BinExpAccuracy { get; }
        public double F1score { get; }
        public string Algorythm { get; }
        public double BinExpAccuracyF1 { get; }
        public double F1scoreF1 { get; }
        public string AlgorythmF1 { get; }
        public EventUpdateBinaryExperimentArgs(RunDetail<BinaryClassificationMetrics> iterationResult, int iterationIndex, double allRunTime, double binExpAccuracy, double f1score, string algorythm, double binExpAccuracyf1, double f1scoref1, string algorythmf1)
        {
            IterationResult = iterationResult;
            IterationIndex = iterationIndex;
            AllRunTime = allRunTime;
            BinExpAccuracy = binExpAccuracy;
            F1score = f1score;
            Algorythm = algorythm;
            BinExpAccuracyF1 = binExpAccuracyf1;
            F1scoreF1 = f1scoref1;
            AlgorythmF1 = algorythmf1;
        }
    }

    public class EventUpdateMulticlassExperimentArgs
    {
        public RunDetail<MulticlassClassificationMetrics> IterationResult { get; }
        public int IterationIndex { get; }
        public double AllRunTime { get; }
        public double MacroAccuracy { get; }
        public double MicroAccuracy { get; }
        public double LogLoss { get; }
        public string Algorythm { get; }
        public EventUpdateMulticlassExperimentArgs(RunDetail<MulticlassClassificationMetrics> iterationResult, int iterationIndex, double allRunTime, double macroAccuracy, double microAccuracy, string algorythm, double logLoss)
        {
            IterationResult = iterationResult;
            IterationIndex = iterationIndex;
            AllRunTime = allRunTime;
            MacroAccuracy = macroAccuracy;
            MicroAccuracy = microAccuracy;
            Algorythm = algorythm;
            LogLoss = logLoss;
        }
    }

    public class EventUpdateRegressionExperimentArgs
    {
        public RunDetail<RegressionMetrics> IterationResult { get; }
        public int IterationIndex { get; }
        public double AllRunTime { get; }
        public double BRSquared { get; }
        public double BMeanAError { get; }
        public string BAlgorythm { get; }

        public EventUpdateRegressionExperimentArgs(RunDetail<RegressionMetrics> iterationResult, int iterationIndex, double allRunTime, double bRSquared, double bMeanAError, string bAlgorythm)
        {
            IterationResult = iterationResult;
            IterationIndex = iterationIndex;
            AllRunTime = allRunTime;
            BRSquared = bRSquared;
            BMeanAError = bMeanAError;
            BAlgorythm = bAlgorythm;
        }
    }


    /// <summary>
    /// Progress handler that AutoML will invoke after each model it produces and evaluates.
    /// </summary>
    public class BinaryExperimentProgressHandler : IProgress<RunDetail<BinaryClassificationMetrics>>
    {
        public event UpdterBinaryExperimentProgressHandler EventUpdateBinaryExperiment;

        private int iterationIndex = 0;
        public double allRunTime = 0;
        public double binExpAccuracy = 0;
        public double f1score = 0;
        public string algorythm = "";
        public double binExpAccuracyf1 = 0;
        public double f1scoref1 = 0;
        public string algorythmf1 = "";

        public void Report(RunDetail<BinaryClassificationMetrics> iterationResult)
        {
            iterationIndex++;
            allRunTime = allRunTime + iterationResult.RuntimeInSeconds;


            if (iterationResult.ValidationMetrics.Accuracy > binExpAccuracy)
            {
                binExpAccuracy = iterationResult.ValidationMetrics.Accuracy;
                algorythm = iterationResult.TrainerName;
                f1score = iterationResult.ValidationMetrics.F1Score;
            }

            if (iterationResult.ValidationMetrics.F1Score > f1score)
            {
                binExpAccuracyf1 = iterationResult.ValidationMetrics.Accuracy;
                algorythmf1 = iterationResult.TrainerName;
                f1scoref1 = iterationResult.ValidationMetrics.F1Score;
            }

            if (iterationResult.Exception != null)
            {
                MessageBox.Show(iterationResult.Exception.ToString());
            }
            else
            {
                if (EventUpdateBinaryExperiment != null)
                    EventUpdateBinaryExperiment(this, new EventUpdateBinaryExperimentArgs(iterationResult, iterationIndex, allRunTime, binExpAccuracy, f1score, algorythm, binExpAccuracyf1, f1scoref1, algorythmf1));
            }
        }
    }

    /// <summary>
    /// Progress handler that AutoML will invoke after each model it produces and evaluates.
    /// </summary>
    public class MulticlassExperimentProgressHandler : IProgress<RunDetail<MulticlassClassificationMetrics>>
    {
        public event UpdterMulticlassExperimentProgressHandler EventUpdateMulticlassExperiment;

        private int iterationIndex = 0;
        public double allRunTime = 0;
        public double macroAccuracy = 0;
        public double microAccuracy = 0;
        public double logLoss = 0;
        public string algorythm = "";

        public void Report(RunDetail<MulticlassClassificationMetrics> iterationResult)
        {
            iterationIndex++;
            allRunTime = allRunTime + iterationResult.RuntimeInSeconds;


            if (iterationResult.ValidationMetrics.MacroAccuracy > macroAccuracy)
            {
                macroAccuracy = iterationResult.ValidationMetrics.MacroAccuracy;
                algorythm = iterationResult.TrainerName;
                microAccuracy = iterationResult.ValidationMetrics.MicroAccuracy;
                logLoss = iterationResult.ValidationMetrics.LogLoss;
            }

            if (iterationResult.Exception != null)
            {
                MessageBox.Show(iterationResult.Exception.ToString());
            }
            else
            {
                if (EventUpdateMulticlassExperiment != null)
                    EventUpdateMulticlassExperiment(this, new EventUpdateMulticlassExperimentArgs(iterationResult, iterationIndex, allRunTime, macroAccuracy, microAccuracy, algorythm, logLoss));
            }
        }
    }

    /// <summary>
    /// Progress handler that AutoML will invoke after each model it produces and evaluates.
    /// </summary>
    public class RegressionExperimentProgressHandler : IProgress<RunDetail<RegressionMetrics>>
    {
        public event UpdterRegressionExperimentProgressHandler EventUpdateRegressionExperiment;

        private int iterationIndex = 0;
        public double allRunTime = 0;
        public double bRSquared = 0;
        public double bMeanAError = 0;
        public string bAlgorythm = "";

        public void Report(RunDetail<RegressionMetrics> iterationResult)
        {
                iterationIndex++;
                allRunTime = allRunTime + iterationResult.RuntimeInSeconds;

                if (iterationResult.ValidationMetrics != null && iterationResult.ValidationMetrics.RSquared > bRSquared)
                {
                    bRSquared = iterationResult.ValidationMetrics.RSquared;
                    bAlgorythm = iterationResult.TrainerName;
                    bMeanAError = iterationResult.ValidationMetrics.MeanAbsoluteError;
                }

                if (iterationResult.Exception != null)
                {
                    MessageBox.Show(iterationResult.Exception.ToString());
                }
                else
                {
                    if (EventUpdateRegressionExperiment != null)
                        EventUpdateRegressionExperiment(this, new EventUpdateRegressionExperimentArgs(iterationResult, iterationIndex, allRunTime, bRSquared, bMeanAError, bAlgorythm));
                }
        }
    }
}

