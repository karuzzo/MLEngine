using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MLEngine.Models
{
    public class MLmodel : INotifyPropertyChanged
    {
        private string titleModel;
        private string fileNameModel;
        private string postfixModel;

        public string TitleModel
        {
            get { return titleModel; }
            set
            {
                titleModel = value;
                OnPropertyChanged(nameof(titleModel));
            }
        }

        public string FileNameModel
        {
            get { return fileNameModel; }
            set
            {
                fileNameModel = value;
                OnPropertyChanged(nameof(fileNameModel));
            }
        }

        public string PostfixModel
        {
            get { return postfixModel; }
            set
            {
                postfixModel = value;
                OnPropertyChanged(nameof(postfixModel));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
