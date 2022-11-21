using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace BSTClient
{
    public class VmBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public static class Execute
    {
        private static SynchronizationContext _uiContext;

        public static void SetMainThreadContext()
        {
            if (_uiContext != null) Console.WriteLine("Current SynchronizationContext may be replaced.");

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                var fileName = Path.GetFileName(assembly.Location);
                if (fileName == "System.Windows.Forms.dll")
                {
                    var type = assembly.DefinedTypes.First(k => k.Name.StartsWith("WindowsFormsSynchronizationContext"));
                    _uiContext = (SynchronizationContext)Activator.CreateInstance(type);
                    break;
                }
                else if (fileName == "WindowsBase.dll")
                {
                    var type = assembly.DefinedTypes.First(k => k.Name.StartsWith("DispatcherSynchronizationContext"));
                    _uiContext = (SynchronizationContext)Activator.CreateInstance(type);
                    break;
                }
            }

            if (_uiContext == null) _uiContext = SynchronizationContext.Current;
        }

        public static void OnUiThread(this Action action)
        {
            if (_uiContext == null)
            {
                if (Application.Current?.Dispatcher != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        action?.Invoke();

                    });
                else
                {
                    action?.Invoke();
                }
            }
            else
            {
                _uiContext.Send(obj => { action?.Invoke(); }, null);
            }
        }

        public static void ToUiThread(this Action action)
        {
            if (_uiContext == null)
            {
                Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                {
                    action?.Invoke();
                }), DispatcherPriority.Normal);
            }
            else
            {
                _uiContext.Post(obj => { action?.Invoke(); }, null);
            }
        }

        public static bool CheckDispatcherAccess() => Thread.CurrentThread.ManagedThreadId == 1;
    }
}