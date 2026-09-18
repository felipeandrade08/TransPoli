using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Transpoli.App.Services;

namespace Transpoli.App;

public partial class MainWindow : Window
{
    private readonly TelemetryService _telemetry = new();
    private readonly UpdateService _updates = new();
    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(1) };

    public MainWindow()
    {
        InitializeComponent();
        _telemetry.Updated += Telemetry_Updated;
        _telemetry.Start();
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
        UpdateClock();
    }

    private void Telemetry_Updated(object? sender, Models.TelemetrySnapshot e) => Dispatcher.Invoke(() =>
    {
        TruckNameText.Text = "Scania S 770";
        SpeedText.Text = e.SpeedKmh.ToString("0");
        RpmText.Text = e.Rpm.ToString("N0");
        GearText.Text = e.Gear;
        CruiseText.Text = e.CruiseControl ? "ON" : "OFF";
        EngineText.Text = e.EngineOn ? "MOTOR • LIGADO" : "MOTOR • DESLIGADO";
        FuelText.Text = $"{e.FuelLiters:N0} L";
        OdometerText.Text = $"{e.OdometerKm:N0} km";
        CargoText.Text = e.Cargo;
        DistanceText.Text = $"{e.TripDrivenKm:N0} km";
        DrivingText.Text = e.DrivingTime.ToString(@"hh\:mm\:ss");
        RestText.Text = e.RestTime == TimeSpan.Zero ? "--:--" : e.RestTime.ToString(@"hh\:mm");
        var range = e.FuelLiters * 2.8;
        RangeText.Text = $"{range:N0} km";
        EtaText.Text = e.EstimatedArrival.ToString("HH:mm");
    });

    private void UpdateClock()
    {
        ClockText.Text = DateTime.Now.ToString("HH:mm");
        TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        DateText.Text = DateTime.Now.ToString("dd/MM/yyyy");
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateButton.IsEnabled = false;
        UpdateButton.Content = "↻  VERIFICANDO...";
        try
        {
            var info = await _updates.CheckAsync();
            if (!info.Available)
            {
                MessageBox.Show($"Você já está usando a versão {info.CurrentVersion} do Transpoli.", "Transpoli", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var answer = MessageBox.Show($"Nova versão {info.LatestVersion} disponível.\n\nO Transpoli será fechado e atualizado automaticamente.", "Atualização disponível", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (answer == MessageBoxResult.OK && _updates.StartUpdate(info))
                Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível verificar atualizações agora.\n\n{ex.Message}", "Transpoli", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            UpdateButton.IsEnabled = true;
            UpdateButton.Content = "↻  ATUALIZAR";
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        _clock.Stop();
        _telemetry.Dispose();
        base.OnClosed(e);
    }
}
