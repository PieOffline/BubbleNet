using System.Windows;
using BubbleNet.ViewModels;

namespace BubbleNet.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.Dispose();
            }
        }
    }
}
