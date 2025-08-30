using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WinFormsApp5
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();

            button1.Click += Button1_Click;
        }

        private ChaoMemoryInstance _instance = new();
        private bool _running = false;

        private async void Button1_Click(object? sender, EventArgs e)
        {
            button1.Enabled = false;
            while (true)
            {
                toolStripStatusLabel2.Text = "Memorizer running...";
                bool solutionAvailable;
                do
                {
                    await Task.Delay(10);
                    var map = ChaoMemory.ReadImage(out var handPosition);
                    solutionAvailable = _instance.Proc(map, handPosition);
                    var bitmap = solutionAvailable
                        ? ChaoMemory.Render(_instance.Solution, -1)
                        : ChaoMemory.Render(map, handPosition);

                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = bitmap;
                } while (!solutionAvailable);

                toolStripStatusLabel2.Text = "Solver running...";
                var solution = _instance.Solution;
                ChaoMemory.ReadImage(out var hand);
                await ChaoMemoryInput.Run(solution, hand, ChaoMemory.GetWindowHandle());
                toolStripStatusLabel2.Text = "Waiting 10s to restart.";
                await Task.Delay(10000);
                await ChaoMemoryInput.KeyDown((VK)'X', ChaoMemory.GetWindowHandle());
                
            }
        }
    }
}
