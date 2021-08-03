using System.Windows;
using System.Windows.Input;


namespace MLEngine
{
    public partial class MachineLearningUi : Window
    {
        

        public MachineLearningUi()
        {
            InitializeComponent();
        }

        private void LabelOsa_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://binom.ml");
        }
    }
}
