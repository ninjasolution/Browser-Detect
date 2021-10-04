using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Input;

namespace BrowserDetect.Helpers
{
    public class Detect
    {
        public string GetChromeUrl()
        {
            string outURL = "";
            AutomationElement elm = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1"));
            if (elm == null) return outURL;

            AutomationElement elmUrlBar = elm.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));
            if (elmUrlBar == null) return "";

            if (!(bool)elmUrlBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty))
            {
                AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
                if (patterns.Length == 1)
                {
                    try
                    {
                        outURL = ((ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0])).Current.Value;
                    }
                    catch { }
                    if (outURL != "")
                    {
                        // must match a domain name (and possibly "https://" in front)
                        if (Regex.IsMatch(outURL, @"^(https:\/\/)?[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,4}).*$"))
                        {
                            // prepend http:// to the url, because Chrome hides it if it's not SSL
                            if (!outURL.StartsWith("http"))
                            {
                                outURL = "http://" + outURL;
                            }
                            return outURL;
                        }
                    }
                }
            }

            return outURL;
        }


        public string GetFirefoxUrl()
        {
            string outURL = "";
            try
            {
                AutomationElement root = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ClassNameProperty, "MozillaWindowClass"));
                if (root == null) return outURL;

                Condition toolBar = new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar),
                new PropertyCondition(AutomationElement.NameProperty, "Browser tabs"));
                if (toolBar == null) return outURL;

                var tool = root.FindFirst(TreeScope.Children, toolBar);
                if (tool == null) return outURL;

                var tool2 = TreeWalker.ControlViewWalker.GetNextSibling(tool);
                if (tool2 == null) return outURL;

                var children = tool2.FindAll(TreeScope.Children, Condition.TrueCondition);
                if (children == null) return outURL;

                foreach (AutomationElement item in children)
                {
                    foreach (AutomationElement i in item.FindAll(TreeScope.Children, Condition.TrueCondition))
                    {
                        foreach (AutomationElement ii in i.FindAll(TreeScope.Element, Condition.TrueCondition))
                        {
                            if (ii.Current.LocalizedControlType == "edit")
                            {
                                //   if (!ii.Current.BoundingRectangle.X.ToString().Contains("empty"))
                                {
                                    ValuePattern activeTab = ii.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                                    outURL = activeTab.Current.Value;
                                    return outURL;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return outURL;
            }
            return outURL;

        }



        public string GetEdgeUrl(AutomationElement edgeCommandsWindow)
        {
            var adressEditBox = edgeCommandsWindow.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "addressEditBox"));

            return ((TextPattern)adressEditBox.GetCurrentPattern(TextPattern.Pattern)).DocumentRange.GetText(int.MaxValue);
        }


    }



    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter ?? "<N/A>");
        }

    }
}