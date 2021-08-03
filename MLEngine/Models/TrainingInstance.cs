using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MLEngine.Entities
{
    public class TrainingInstance : INotifyPropertyChanged
    {
        private string title;
        private string fileName;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged(nameof(title));
            }
        }

        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                OnPropertyChanged(nameof(fileName));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
