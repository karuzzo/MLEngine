using System;
using System.Windows;

namespace MLEngine
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => SystemCommands.CloseWindow(w));
        }

        private void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => SystemCommands.MinimizeWindow(w));
        }

        private void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w =>
            {
                if (w.WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(w);
                else SystemCommands.MaximizeWindow(w);
            });
        }
    }

    internal static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            Window window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
            if (window != null) action(window);
        }

        //public static IntPtr GetWindowHandle(this Window window)
        //{
        //    WindowInteropHelper helper = new WindowInteropHelper(window);
        //    return helper.Handle;
        //}
    }
}