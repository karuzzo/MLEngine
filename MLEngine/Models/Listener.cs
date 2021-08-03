using Microsoft.ML;
using Microsoft.ML.AutoML;
using MLEngine.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MLEngine.Models
{
    public class Listener
    {
        public ObservableCollection<MLmodel> MLmodels { get; set; }
        private MLContext mlContext = new MLContext();
        DataViewSchema modelInputSchema;

        List<RPCBus> IPCMessageBuses = new List<RPCBus>();

        private Dictionary<string, PredictionEngine<BarLoadBinary, BarLoadBinaryPrediction>> binaryDictionary = new Dictionary<string, PredictionEngine<BarLoadBinary, BarLoadBinaryPrediction>>();
        private Dictionary<string, PredictionEngine<BarLoadMulti, BarLoadMultiPrediction>> multiDictionary = new Dictionary<string, PredictionEngine<BarLoadMulti, BarLoadMultiPrediction>>();
        private Dictionary<string, PredictionEngine<BarLoadRegression, BarLoadRegressionPrediction>> regressionDictionary = new Dictionary<string, PredictionEngine<BarLoadRegression, BarLoadRegressionPrediction>>();
        private Dictionary<string, PredictionEngine<BarLoadTimeSeries, BarLoadTimeSeriesPrediction>> timeSeriesDictionary = new Dictionary<string, PredictionEngine<BarLoadTimeSeries, BarLoadTimeSeriesPrediction>>();

        public Listener (ObservableCollection<MLmodel> mlModels)
        {
            MLmodels = mlModels;

            try
            {
                foreach (var model in MLmodels)
                {
                    ITransformer loadedModel = mlContext.Model.Load(model.FileNameModel, out DataViewSchema modelInputSchema);

                    if (model.TitleModel == "Binary Classification")
                        binaryDictionary.Add(model.PostfixModel, mlContext.Model.CreatePredictionEngine<BarLoadBinary, BarLoadBinaryPrediction>(loadedModel));

                    if (model.TitleModel == "Multi-class Classification")
                        multiDictionary.Add(model.PostfixModel, mlContext.Model.CreatePredictionEngine<BarLoadMulti, BarLoadMultiPrediction>(loadedModel));

                    if (model.TitleModel == "Regression")
                        regressionDictionary.Add(model.PostfixModel, mlContext.Model.CreatePredictionEngine<BarLoadRegression, BarLoadRegressionPrediction>(loadedModel));

                    IPCMessageBuses.Add(new RPCBus(model.PostfixModel));
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private CancellationTokenSource m_Cts;
        private Thread m_Thread;
        private readonly object m_SyncObject = new object();

        public void Start()
        {
            lock (m_SyncObject)
            {
                if (m_Thread == null || !m_Thread.IsAlive)
                {
                    m_Cts = new CancellationTokenSource();
                    m_Thread = new Thread(() => Listen(m_Cts.Token))
                    {
                        IsBackground = true
                    };
                    m_Thread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (m_SyncObject)
            {
                m_Cts.Cancel();
                foreach (var bus in IPCMessageBuses)
                    bus.Stop();
            }
        }

        private void Listen(CancellationToken token)
        {
            foreach (var bus in IPCMessageBuses)
            {
                //bus.Start();
                bus.get_answer += Bus_NewMessageRecived;
            }
        }

        private string Bus_NewMessageRecived(string channelName, string message)
        {
            string outMessage = Convert.ToString(ServerDataPrediction(message
                                                                      .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                                                                      .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) + 
                                                                      ";" + channelName));

            foreach (var bus in IPCMessageBuses)
                if (bus.channelName == channelName)
                    return outMessage;

            return double.NaN.ToString();
        }

        object locker = new object();

        public string ServerDataPrediction(string request)
        {
            lock (locker)
            {
                var barLoadBinary = new BarLoadBinary();
                var barLoadMulti = new BarLoadMulti();
                var barLoadRegression = new BarLoadRegression();

                List<string> words = request.Split(';').ToList();
                string checker = words.Last();
                words.RemoveAt(words.Count() - 1);

                if (binaryDictionary.ContainsKey(checker))
                {
                    for (int i = 0; i < words.Count(); i++)
                    {
                        typeof(BarLoadBinary).GetProperty($"col{i + 1}").SetValue(barLoadBinary, float.Parse(words[i]));
                    }

                    var prediction = binaryDictionary[checker].Predict(barLoadBinary);
                    return string.Format("{0}", prediction.Prediction);
                }
                else if (multiDictionary.ContainsKey(checker))
                {
                    for (int i = 0; i < words.Count(); i++)
                    {
                        typeof(BarLoadMulti).GetProperty($"col{i + 1}").SetValue(barLoadMulti, float.Parse(words[i]));
                    }

                    var prediction = multiDictionary[checker].Predict(barLoadMulti);
                    return string.Format("{0}", prediction.Prediction);
                }
                else if (regressionDictionary.ContainsKey(checker))
                {
                    for (int i = 0; i < words.Count(); i++)
                    {
                        typeof(BarLoadRegression).GetProperty($"col{i + 1}").SetValue(barLoadRegression, float.Parse(words[i]));
                    }

                    var prediction = regressionDictionary[checker].Predict(barLoadRegression);
                    return string.Format("{0}", prediction.Score);
                }

                return "Something went wrong";
            }
        }
    }
}
