using System.ComponentModel;
using System.Runtime.CompilerServices;
using MLEngine.Entities;

namespace MLEngine.ViewModels
{
    class TrainingInstanceViewModel : INotifyPropertyChanged
    {
        private TrainingInstance trainingInstance;

        public TrainingInstanceViewModel(TrainingInstance t)
        {
            trainingInstance = t;
        }

        public string Title
        {
            get { return trainingInstance.Title; }
            set
            {
                trainingInstance.Title = value;
                OnPropertyChanged("Title");
            }
        }

        public string FileName
        {
            get { return trainingInstance.FileName; }
            set
            {
                trainingInstance.FileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
