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
using Application = System.Windows.Application;

namespace KernelAutomata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConfigWindow configWindow;

        private OpenGlRenderer renderer;

        public Simulation simulation;

        public SimulationRecipe recipe;

        private bool uiPending;

        private DateTime lastCheckTime;

        private long lastCheckFrameCount;
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void parent_Loaded(object sender, RoutedEventArgs e)
        {
            //var recipe = RecipeFactory.TwoChannelsOrbs();
            //var recipe = RecipeFactory.OneChannelOrbs();
            //var recipe = RecipeFactory.TwoChannelsCaterpillar();

            //RecipeFactory.SaveToFile(recipe, "c://tmp//orbs-ch2.json");

            //var recipe = RecipeFactory.LoadFromResource("caterpillar1-ch2.json");
            var recipe = RecipeFactory.LoadFromResource("orbs-ch1.json");
            //var recipe = RecipeFactory.LoadFromResource("orbs-ch2.json");
            StartNewSimulation(recipe);
            /*
            var gpu = new GpuContext(recipe.size, placeholder);
            simulation = new Simulation(recipe, gpu);
            simulation.ResetFields();
            renderer = new OpenGlRenderer(placeholder, simulation);
            */
            KeyDown += MainWindow_KeyDown;
            System.Timers.Timer systemTimer = new System.Timers.Timer() { Interval = 10 };
            systemTimer.Elapsed += SystemTimer_Elapsed;
            systemTimer.Start();
            DispatcherTimer infoTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.0) };
            infoTimer.Tick += InfoTimer_Tick;
            infoTimer.Start();

            configWindow = new ConfigWindow(this);
            configWindow.Show();
            configWindow.Activate();
            Closing += (s, e) => { };
        }

        public void StartNewSimulation(SimulationRecipe recipe)
        {
            if (simulation!=null)
                simulation.Destroy();

            var gpu = new GpuContext(recipe.size, placeholder);
            simulation = new Simulation(recipe, gpu);
            simulation.ResetFields();
            renderer = new OpenGlRenderer(placeholder, simulation);
            this.recipe = recipe;
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
                        renderer.Step();
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
                case Key.Space:
                    renderer.Paused = !renderer.Paused;
                    break;
            }
        }

        private void InfoTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var timespan = now - lastCheckTime;
            double frames = renderer.FrameCounter - lastCheckFrameCount;
            if (timespan.TotalSeconds >= 0.0001)
            {
                double fps = frames / timespan.TotalSeconds;
                Title = $"KernelAutomata. " +
                        $"fps:{fps.ToString("0.0")} ";

                lastCheckFrameCount = renderer.FrameCounter;
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