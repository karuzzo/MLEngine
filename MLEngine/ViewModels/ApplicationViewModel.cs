using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MLEngine.Commands;
using MLEngine.Entities;
using MLEngine.Models;


namespace MLEngine.ViewModels
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        #region конструктор
        public ApplicationViewModel()
        {
            TrainingInstances = new ObservableCollection<TrainingInstance>
            {
                new TrainingInstance {Title = "Select Task", FileName = "Select Data File" },
            };

            MLmodels = new ObservableCollection<MLmodel>
            {
                new MLmodel {TitleModel = "Select Task", FileNameModel = "Select Model File", PostfixModel = "RPC Name"},
            };

            MyExperimentTime = "180";
            SplitParts = "0.25".Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

        }
        #endregion

        #region общий сервисный код
        private string status;
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged(nameof(status));
            }
        }

        private string mainInfo;
        public string MainInfo
        {
            get { return mainInfo; }
            set
            {
                mainInfo = value;
                OnPropertyChanged(nameof(mainInfo));
            }
        }

        private string slaveInfo;
        public string SlaveInfo
        {
            get { return slaveInfo; }
            set
            {
                slaveInfo = value;
                Properties.Settings.Default.trainTime = value;
                OnPropertyChanged(nameof(slaveInfo));
            }
        }
        #endregion

        #region сервисный код TrainingInstance
        public ObservableCollection<TrainingInstance> TrainingInstances { get; set; }
        private CancellationTokenSource cts = new CancellationTokenSource();
       // private bool useShuffle = true;

        private TrainingInstance selectedTrainingInstance;
        public TrainingInstance SelectedTrainingInstance
        {
            get { return selectedTrainingInstance; }
            set
            {
                selectedTrainingInstance = value;
                OnPropertyChanged(nameof(SelectedTrainingInstance));
            }
        }

        private UInt32 myExperimentTime;
        public string MyExperimentTime
        {
            get { return myExperimentTime.ToString(); }
            set
            {
                myExperimentTime = Convert.ToUInt32(value);
                OnPropertyChanged(nameof(status));
            }
        }

        private string splitParts;
        public string SplitParts
        {
            get { return splitParts.ToString(); }
            set
            {
                splitParts = value;
                Properties.Settings.Default.splitParts = value;
                OnPropertyChanged(nameof(status));
            }
        }

        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ?? // присваиваем если null
                  (addCommand = new RelayCommand(obj =>
                  {
                      TrainingInstance trainingInstance = new TrainingInstance() { Title = "Select Task", FileName = "Select Data File" };
                      TrainingInstances.Add(trainingInstance);
                      SelectedTrainingInstance = trainingInstance;
                  }));
            }
        }

        private RelayCommand removeCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return removeCommand ??
                  (removeCommand = new RelayCommand(obj =>
                  {
                      TrainingInstance trainingInstance = obj as TrainingInstance;
                      if (trainingInstance != null)
                      {
                          TrainingInstances.Remove(trainingInstance);
                      }
                  },
                 (obj) => TrainingInstances.Count > 0));
            }
        }

        private RelayCommand setFileNameCommand;
        public RelayCommand SetFileNameCommand
        {
            get
            {
                return setFileNameCommand ?? // присваиваем если null
                  (setFileNameCommand = new RelayCommand(obj =>
                  {
                      try
                      {
                          SelectedTrainingInstance.FileName = SelectPath();
                      }
                      catch { MessageBox.Show("Do not poke anywhere, first choose an iteration."); }
                  }));
            }
        }

        private bool useShuffle;
        public string UseShuffle
        {
            get { return Convert.ToString(useShuffle); }
            set
            {
                useShuffle = Convert.ToBoolean(value);
                OnPropertyChanged(nameof(useShuffle));
            }

        }

        private RelayCommand splitCommand;
        public RelayCommand SplitCommand
        {
            get
            {
                return splitCommand ?? // присваиваем если null
                  (splitCommand = new RelayCommand(obj =>
                  {
                      Task.Run(() =>
                      {
                          MainInfo = "";
                          Status = "";
                          foreach (var instance in TrainingInstances)
                          {
                              DataPreparator dataPreparatot = new DataPreparator();
                              dataPreparatot.SplitParts = splitParts;
                              dataPreparatot.BaseFileName = instance.FileName;
                              dataPreparatot.UseShufle = useShuffle;
                              dataPreparatot.MakeValidData();
                          }
                          Status = $"Data files successfully split.";
                      }); 
                  }));
            }
        }

        private RelayCommand trainCommand;
        public RelayCommand TrainCommand
        {
            get
            {
                return trainCommand ?? // присваиваем если null
                  (trainCommand = new RelayCommand(obj =>
                  {
                      Task.Run(() =>
                      {
                          MainInfo = "";
                          Status = $"The model is being built and optimized.";

                          foreach (var instance in TrainingInstances)
                          {
                              ModelTrainer modelTrainer = new ModelTrainer();
                              modelTrainer.Cts = cts;
                              modelTrainer.BaseFileName = instance.FileName;
                              modelTrainer.MyExperimentTime = myExperimentTime;
                              modelTrainer.slaveChanged += ModelTrainer_slaveChanged;
                              modelTrainer.statusChanged += ModelTrainer_statusChanged;
                              modelTrainer.DoModel(instance.Title);
                              MainInfo = modelTrainer.OptimizerResult;
                          }
                          Status = $"Modeling completed.";
                      });
                  }));
            }
        }

        private RelayCommand stopCommand;
        public RelayCommand StopCommand
        {
            get
            {
                return stopCommand ?? // присваиваем если null
                  (stopCommand = new RelayCommand(obj =>
                  {
                      Task.Run(() =>
                      {
                          cts.Cancel();
                          cts.Dispose();

                      });
                  }));
            }
        }

        #endregion

        #region сервисный код MLmodel
        public ObservableCollection<MLmodel> MLmodels { get; set; }
        private MLmodel selectedMLmodel;

        public MLmodel SelectedMLmodel
        {
            get { return selectedMLmodel; }
            set
            {
                selectedMLmodel = value;
                OnPropertyChanged(nameof(SelectedMLmodel));
            }
        }

        private string titleModel;
        public string TitleModel
        {
            get { return titleModel.ToString(); }
            set
            {
                titleModel = value;
                OnPropertyChanged(nameof(titleModel));
            }
        }

        private string fileNameModel;
        public string FileNameModel
        {
            get { return fileNameModel; }
            set
            {
                fileNameModel = value;
                OnPropertyChanged(nameof(fileNameModel));
            }
        }

        private string postfixModel;
        public string PostfixModel
        {
            get { return postfixModel; }
            set
            {
                postfixModel = value;
                OnPropertyChanged(nameof(postfixModel));
            }
        }

        private RelayCommand addModelCommand;
        public RelayCommand AddModelCommand
        {
            get
            {
                return addModelCommand ?? // присваиваем если null
                  (addModelCommand = new RelayCommand(obj =>
                  {
                      MLmodel mLmodel = new MLmodel() { TitleModel = "Select Task", FileNameModel = "Select Model File", PostfixModel = "RPC Name" };
                      MLmodels.Add(mLmodel);
                      SelectedMLmodel = mLmodel;
                  }));
            }
        }

        private RelayCommand removeModelCommand;
        public RelayCommand RemoveModelCommand
        {
            get
            {
                return removeModelCommand ??
                  (removeModelCommand = new RelayCommand(obj =>
                  {
                      MLmodel mlModel = obj as MLmodel;
                      if (mlModel != null)
                      {
                          MLmodels.Remove(mlModel);
                      }
                  },
                 (obj) => MLmodels.Count > 0));
            }
        }

        private RelayCommand setFileNameModelCommand;
        public RelayCommand SetFileNameModelCommand
        {
            get
            {
                return setFileNameModelCommand ?? // присваиваем если null
                  (setFileNameModelCommand = new RelayCommand(obj =>
                  {
                      try { 
                      SelectedMLmodel.FileNameModel = SelectPath();
                      }
                      catch { MessageBox.Show("Do not poke anywhere, first choose an model."); }
                  }));
            }
        }

        private bool serverIsRunning = false;
        public Listener modelExplorer;
        private RelayCommand startServer;
        public RelayCommand StartServer
        {
            get
            {  
                return startServer ?? // присваиваем если null
                  (startServer = new RelayCommand(obj =>
                  {
                      if (!serverIsRunning)
                      {
                          serverIsRunning = true;

                              modelExplorer = new Listener(MLmodels);
                              Status = $"The server is running.";
                              
                              modelExplorer.Start();
                      }
                      else
                      {
                          serverIsRunning = false;
                          modelExplorer.Stop();
                          Status = $"The server is Stoped.";
                      }
                  }));
            }
        }

        #endregion

        #region вспомогательные методы
        private void ModelTrainer_statusChanged(string status)
        {
            Status = status;
        }

        private void ModelTrainer_slaveChanged(string slave)
        {
            SlaveInfo = slave;
        }

        public string SelectPath()
        {
            string filePath = "";
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filePath = ofd.FileName;
            }
            return filePath;
        }
        #endregion

        #region реализация INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
