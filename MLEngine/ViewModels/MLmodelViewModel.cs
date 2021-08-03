using MLEngine.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MLEngine.ViewModels
{
    class MLmodelViewModel : INotifyPropertyChanged
    {
        private MLmodel mlModelInctance;

        public MLmodelViewModel(MLmodel m)
        {
            mlModelInctance = m;
        }

        public string TitleModel
        {
            get { return mlModelInctance.TitleModel; }
            set
            {
                mlModelInctance.TitleModel = value;
                OnPropertyChanged("TitleModel");
            }
        }

        public string FileNameModel
        {
            get { return mlModelInctance.FileNameModel; }
            set
            {
                mlModelInctance.FileNameModel = value;
                OnPropertyChanged("FileModel");
            }
        }

        public string PostfixModel
        {
            get { return mlModelInctance.PostfixModel; }
            set
            {
                mlModelInctance.PostfixModel = value;
                OnPropertyChanged("PostfixModel");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
