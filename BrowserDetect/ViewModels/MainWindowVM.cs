using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Threading;
using BrowserDetect.Helpers;
using BrowserDetect.Models;
using ReactiveUI;

namespace BrowserDetect.ViewModels
{
    public class MainWindowVM : ReactiveObject
    {
        public Detect detect = new Detect();
        private IntPtr winHook;
        private WinEventProc listener;
        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;

        public MainWindowVM()
        {

            //timer = new Timer(
            //            new TimerCallback(DetectBrowser),
            //            null,
            //            0,
            //            3000);

            CloseCommand = new RelayCommand(o => closeClick(o));

            listener = new WinEventProc(EventCallback);
            //setting the window hook
            winHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, listener, 0, 0, WINEVENT_OUTOFCONTEXT);

        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, uint dwflags);
        [DllImport("user32.dll")]
        internal static extern int UnhookWinEvent(IntPtr hWinEventHook);
        internal delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);

        public Timer timer;

        private ObservableCollection<BrowserURL> urls = new ObservableCollection<BrowserURL>();
        public ObservableCollection<BrowserURL> Urls
        {
            get => urls;
            set => this.RaiseAndSetIfChanged(ref urls, value);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out Int32 lpdwProcessId);
        [DllImport("user32")]
        public static extern IntPtr GetDesktopWindow();
        public static AutomationElement GetEdgeCommandsWindow(AutomationElement edgeWindow)
        {
            return edgeWindow.FindFirst(TreeScope.Children, new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                new PropertyCondition(AutomationElement.NameProperty, "Microsoft Edge")));
        }

        private void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {

            if (hWnd != IntPtr.Zero)
            {
                Int32 pid = 0;
                GetWindowThreadProcessId(hWnd, out pid);
                Process pro = Process.GetProcessById(pid);

                string url;
                switch (pro.ProcessName)
                {
                    case "chrome":
                        url = detect.GetChromeUrl();
                        if (url == "") return;
                        var chromeUrl = Urls.FirstOrDefault(u => u.BrowserName == "chrome" && u.URL == url);
                        if (chromeUrl == null)
                        {
                            Urls.Add(new BrowserURL(Urls.Count + 1, "chrome", url));
                        }
                        break;
                    case "firefox":
                        url = detect.GetFirefoxUrl();
                        if (url == "") return;
                        var firefoxUrl = Urls.FirstOrDefault(u => u.BrowserName == "firefox" && u.URL == url);
                        if (firefoxUrl == null)
                        {
                            Urls.Add(new BrowserURL(Urls.Count + 1, "firefox", url));
                        }
                        break;
                    case "msedge":
                        AutomationElement main = AutomationElement.FromHandle(GetDesktopWindow());
                        foreach (AutomationElement child in main.FindAll(TreeScope.Children, PropertyCondition.TrueCondition))
                        {
                            AutomationElement window = GetEdgeCommandsWindow(child);
                            if (window == null) continue;
                            url = detect.GetEdgeUrl(window);
                            Urls.Add(new BrowserURL(Urls.Count + 1, "msedge", url));
                            break;
                        }
                        break;
                        
                }

            }


        }
        //public void DetectBrowser(object state)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //      {

        //      });
        //}

        public ICommand CloseCommand { get; set; }
        private void closeClick(object sender)
        {
            UnhookWinEvent(winHook);

            Environment.Exit(Environment.ExitCode);
        }


    }
}
