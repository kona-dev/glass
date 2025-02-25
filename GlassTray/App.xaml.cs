using System.Configuration;
using System.Data;
using System.Windows;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;
using System.IO.Pipes;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;


namespace GlassTray;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private NotifyIcon _notifyIcon;

    NamedPipeClientStream pipeClient;


   

    protected private void Application_Startup(object sender, StartupEventArgs e)
    {

        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon("HUH.ico"),
            Visible = true,
            Text = "Glass"
        };

        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Items.Add("Open Glass", null, OpenGlass);
        menu.Items.Add("Restart Glass", null, RestartService);
        menu.Items.Add("Exit Glass", null, ExitApp);

        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += OpenGlass;

        pipeClient = new NamedPipeClientStream(".", "GlassPipe", PipeDirection.InOut, PipeOptions.Asynchronous);

       

        SendMessageToWorker("GlassTray - 2025");

        
        

    }

    private void OpenGlass(object? sender, EventArgs e)
    {
        SendMessageToWorker("GlassTray Settings Opened");
        new SettingsWindow().Show();
    }


    private void RestartService(object? sender, EventArgs e)
    {
        Process.Start("sc", "stop Glass");
        Process.Start("sc", "start Glass");
        SendMessageToWorker("Restarting Glass Service");
    }

    private void ExitApp(object? sender, EventArgs e)
    {
        SendMessageToWorker("Closing Glass Service");
        _notifyIcon.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private void SendMessageToWorker(string message)
    {
            
      if (!pipeClient.IsConnected) pipeClient.Connect(500);


      try
      {
          

          Debug.WriteLine($"Pipeline Client Connected: {pipeClient.IsConnected}");


          using (StreamWriter writer = new StreamWriter(pipeClient, leaveOpen: true))
          using (StreamReader reader = new StreamReader(pipeClient, leaveOpen: true))
          {
              writer.WriteLine(message);
              writer.Flush();

          }
          Debug.WriteLine("Message sent to worker.");
        
      }
      catch (TimeoutException ex)
      {
          Debug.WriteLine($"Failed to connect to worker service. |  {ex.Message}");
     
      }

          
    }


  
}


