using System.Runtime.CompilerServices;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;


namespace Glass
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private const string PipeName = "GlassPipe";

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            Start();
            
        }

        public void Start()
        {
            _logger.LogInformation("Starting Pipe Server - GlassPipe");
            Thread pipeServerThread = new Thread(new ThreadStart(PipeServer));
            pipeServerThread.Start();
            _logger.LogInformation($"Server Thread started. | {pipeServerThread.IsAlive} ");
            WinTest();
           
        }

        

        private void PipeServer()
        {
         
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message ,PipeOptions.Asynchronous))
                {

                    pipeServer.WaitForConnection();

                        using (StreamReader reader = new StreamReader(pipeServer, leaveOpen: true))
                        using (StreamWriter writer = new StreamWriter(pipeServer, leaveOpen: true))
                        {
                            
                            while (true)
                            { 
                                if (pipeServer.IsConnected)
                                {   
                                    string message = reader.ReadLine();
                                    if (message != null) 
                                    {
                                        _logger.LogInformation($"Pipe Connection | {pipeServer.IsConnected}");
                                        _logger.LogInformation("Recieved from tray: " + message);
                                    }                            
                                }

                            }
                        }
                    
                }

            }
        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                   // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        public List<string> GetActiveWindows()
        {
            List<string> windows = new List<string>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder windowTitle = new StringBuilder(length + 1);
                        GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

                        if (GetWindowRect(hWnd, out RECT rect))
                        {
                            Rectangle windowBounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                            
                           

                            windows.Add($"Title: {windowTitle}, Position: {windowBounds}");
                        }
                    }
                }
                return true;

            }, IntPtr.Zero);

            return windows;
        }

        
        private void WinTest()
        {
            List<string> windows = GetActiveWindows();
            foreach (string win in windows)
            {
                _logger.LogCritical(win);
            }
        }
    }
}
