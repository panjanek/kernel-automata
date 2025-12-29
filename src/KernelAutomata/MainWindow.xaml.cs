using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using KernelAutomata.Gpu;
using KernelAutomata.Gui;
using KernelAutomata.Models;
using AppContext = KernelAutomata.Models.AppContext;
using Application = System.Windows.Application;

namespace KernelAutomata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppContext app;

        private bool uiPending;

        private DateTime lastCheckTime;

        private long lastCheckFrameCount;

        private int recNr;

        private bool prevRecState;
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void parent_Loaded(object sender, RoutedEventArgs e)
        {
            app = new Models.AppContext()
            {
                mainWindow = this,
                placeholder = this.placeholder
            };

            var initRecipe = RecipeFactory.LoadFromResource("orbs-ch1.json");
            app.StartNewSimulation(initRecipe);

            app.configWindow = new ConfigWindow(app);
            app.configWindow.Show();
            app.configWindow.Activate();
            Closing += (s, e) => { };

            KeyDown += MainWindow_KeyDown;

            System.Timers.Timer systemTimer = new System.Timers.Timer() { Interval = 10 };
            systemTimer.Elapsed += SystemTimer_Elapsed;
            systemTimer.Start();

            DispatcherTimer infoTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.0) };
            infoTimer.Tick += InfoTimer_Tick;
            infoTimer.Start();
        }


        private void SystemTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!uiPending)
            {
                uiPending = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        app.renderer.Step();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        uiPending = false;
                    }

                    uiPending = false;
                }), DispatcherPriority.Render);
            }
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space: case Key.P:
                    app.renderer.Paused = !app.renderer.Paused;
                    e.Handled = true;
                    break;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.Z)
                {
                    app.Undo();
                    app.configWindow.UpdateAllControls();
                    e.Handled = true;
                }
                else if (e.Key == Key.Y)
                {
                    app.Redo();
                    app.configWindow.UpdateAllControls();
                    e.Handled = true;
                }
            }
        }

        private void InfoTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var timespan = now - lastCheckTime;
            double frames = app.renderer.FrameCounter - lastCheckFrameCount;
            if (timespan.TotalSeconds >= 0.0001)
            {
                double fps = frames / timespan.TotalSeconds;
                Title = $"KernelAutomata. " +
                        $"fps:{fps.ToString("0.0")} ";

                if (!string.IsNullOrWhiteSpace(app.configWindow.recordDir))
                {
                    Title += $"[recording to {app.configWindow.RecordDir}] ";
                }

                if (app.renderer.Paused)
                {
                    Title += $"[pause] ";
                }

                lastCheckFrameCount = app.renderer.FrameCounter;
                lastCheckTime = now;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;

            // Determine maximum square size
            double size = Math.Min(workArea.Width, workArea.Height);

            // Apply square dimensions
            Width = size;
            Height = size;

            // Optional: center manually if needed
            Left = workArea.Left + (workArea.Width - size) / 2;
            Top = workArea.Top + (workArea.Height - size) / 2;
        }
    }
}