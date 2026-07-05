using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BetterWindowsTerminal;

public partial class MainWindow : Window
{
    private readonly Dictionary<TabItem, TerminalSession> _sessions = new();
    private readonly Random _rng = new();
    private DispatcherTimer? _particleTimer;

    public MainWindow()
    {
        InitializeComponent();
        Closing += (_, _) => KillAllSessions();
        Loaded += (_, _) => StartParticleSystem();
    }

    // --- Session Model ---

    private sealed class TerminalSession
    {
        public Process Proc { get; init; } = null!;
        public TextBox Output { get; init; } = null!;
        public CancellationTokenSource Cts { get; } = new();
    }

    // --- Window Chrome ---

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else
            DragMove();
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    // --- Tab Lifecycle ---

    private void NewTabBtn_Click(object sender, RoutedEventArgs e)
    {
        ShellMenu.PlacementTarget = NewTabBtn;
        ShellMenu.IsOpen = true;
    }

    private void NewTab_PowerShell(object sender, RoutedEventArgs e) => CreateTab("PowerShell", "powershell.exe");
    private void NewTab_Cmd(object sender, RoutedEventArgs e) => CreateTab("CMD", "cmd.exe");

    private void CreateTab(string title, string executable)
    {
        var (panel, outputBox, inputBox) = BuildTerminalPanel();

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executable,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        var tab = new TabItem
        {
            Header = title,
            Style = (Style)FindResource("ChromeTabItem"),
            Content = panel
        };

        var session = new TerminalSession { Proc = proc, Output = outputBox };
        _sessions[tab] = session;

        proc.Start();
        proc.Exited += (_, _) => Dispatcher.InvokeAsync(() => AppendOutput(session, "\n[Process exited]\n"));

        inputBox.KeyDown += (s, args) =>
        {
            if (args.Key != Key.Enter) return;
            var text = inputBox.Text;
            inputBox.Clear();
            args.Handled = true;

            var trimmed = text.Trim().ToLowerInvariant();
            if (trimmed is "cls" or "clear")
            {
                session.Output.Clear();
                return;
            }

            if (!proc.HasExited)
                proc.StandardInput.WriteLine(text);
        };

        StartReadLoop(session, proc.StandardOutput);
        StartReadLoop(session, proc.StandardError);

        TerminalTabs.Items.Add(tab);
        TerminalTabs.SelectedItem = tab;
        inputBox.Focus();
    }

    private void StartReadLoop(TerminalSession session, StreamReader reader)
    {
        var token = session.Cts.Token;
        Task.Run(async () =>
        {
            var buffer = new char[4096];
            while (!token.IsCancellationRequested)
            {
                int count;
                try { count = await reader.ReadAsync(buffer, 0, buffer.Length); }
                catch { break; }
                if (count == 0) break;
                var text = new string(buffer, 0, count);
                await Dispatcher.InvokeAsync(() => AppendOutput(session, text));
            }
        }, token);
    }

    private void AppendOutput(TerminalSession session, string text)
    {
        var box = session.Output;
        box.AppendText(text);
        box.ScrollToEnd();

        // Ring-buffer trim: keep last ~8000 chars
        if (box.Text.Length > 10000)
            box.Text = box.Text[^8000..];
    }

    // --- Tab Close / Cleanup ---

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        var tab = FindParent<TabItem>((Button)sender);
        if (tab != null) DestroyTab(tab);
    }

    private void DestroyTab(TabItem tab)
    {
        if (_sessions.Remove(tab, out var session))
        {
            session.Cts.Cancel();
            if (!session.Proc.HasExited)
                session.Proc.Kill(true);
            session.Proc.Dispose();
        }
        TerminalTabs.Items.Remove(tab);
    }

    private void KillAllSessions()
    {
        foreach (var tab in _sessions.Keys.ToList())
            DestroyTab(tab);
    }

    // --- Particle System ---

    private static readonly Color[] ParticleColors = new[]
    {
        Color.FromRgb(0xC0, 0x84, 0xFC),
        Color.FromRgb(0xB4, 0x7A, 0xFF),
        Color.FromRgb(0xA8, 0x55, 0xF7),
        Color.FromRgb(0xD8, 0xB4, 0xFE),
    };

    private void StartParticleSystem()
    {
        _particleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _particleTimer.Tick += (_, _) => SpawnParticle();
        _particleTimer.Start();
    }

    private void SpawnParticle()
    {
        var h = ParticleCanvas.ActualHeight;
        var w = ParticleCanvas.ActualWidth;
        if (h < 10 || w < 10) return;

        var color = ParticleColors[_rng.Next(ParticleColors.Length)];
        var leftSide = _rng.Next(2) == 0;
        var coreSize = 6.0 + _rng.NextDouble() * 6.0;
        var duration = 4.0 + _rng.NextDouble() * 5.0;

        var core = new Ellipse
        {
            Width = coreSize,
            Height = coreSize,
            Fill = new SolidColorBrush(color),
            IsHitTestVisible = false
        };

        var glowSize = coreSize * 5;
        var glow = new Ellipse
        {
            Width = glowSize,
            Height = glowSize,
            Fill = new RadialGradientBrush(
                Color.FromArgb(0xCC, color.R, color.G, color.B),
                Color.FromArgb(0x00, color.R, color.G, color.B)),
            Effect = new BlurEffect { Radius = 20 },
            IsHitTestVisible = false
        };

        var x = leftSide
            ? _rng.NextDouble() * 25
            : w - 25 + _rng.NextDouble() * 25;

        var startY = -20.0;
        var endY = h + 20;

        Canvas.SetLeft(core, x);
        Canvas.SetTop(core, startY);
        Canvas.SetLeft(glow, x - (glowSize - coreSize) / 2);
        Canvas.SetTop(glow, startY - (glowSize - coreSize) / 2);
        ParticleCanvas.Children.Add(glow);
        ParticleCanvas.Children.Add(core);

        var coreAnim = new DoubleAnimation(startY, endY, TimeSpan.FromSeconds(duration));
        var glowAnim = new DoubleAnimation(startY - (glowSize - coreSize) / 2, endY - (glowSize - coreSize) / 2, TimeSpan.FromSeconds(duration));

        coreAnim.Completed += (_, _) =>
        {
            ParticleCanvas.Children.Remove(core);
            ParticleCanvas.Children.Remove(glow);
        };

        core.BeginAnimation(Canvas.TopProperty, coreAnim);
        glow.BeginAnimation(Canvas.TopProperty, glowAnim);
    }

    // --- Helpers ---

    private (Grid panel, TextBox output, TextBox input) BuildTerminalPanel()
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var outputBox = new TextBox
        {
            IsReadOnly = true,
            Background = System.Windows.Media.Brushes.Transparent,
            Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary"),
            FontFamily = (System.Windows.Media.FontFamily)FindResource("TerminalFont"),
            FontSize = 13,
            BorderThickness = new Thickness(0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            TextWrapping = TextWrapping.NoWrap,
            AcceptsReturn = true,
            Padding = new Thickness(8)
        };
        Grid.SetRow(outputBox, 0);

        var inputBox = new TextBox
        {
            Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000")!),
            Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary"),
            FontFamily = (System.Windows.Media.FontFamily)FindResource("TerminalFont"),
            FontSize = 13,
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = (System.Windows.Media.Brush)FindResource("BorderThin"),
            Padding = new Thickness(8, 6, 8, 6),
            CaretBrush = (System.Windows.Media.Brush)FindResource("TextPrimary")
        };
        Grid.SetRow(inputBox, 1);

        grid.Children.Add(outputBox);
        grid.Children.Add(inputBox);
        return (grid, outputBox, inputBox);
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
        return parent as T;
    }
}
