using System.Windows;
using TFusion.Foundation;

namespace TFusion.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        VersionText = $"Version {BuildInfo.Current.InformationalVersion}";
        DataContext = this;
    }

    public string VersionText { get; }
}
