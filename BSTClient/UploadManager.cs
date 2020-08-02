using BSTClient.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BSTClient
{
    public class UploadManager : VmBase
    {
        public UploadManager()
        {
            Default = this;

            Task.Factory.StartNew(() =>
            {
                var dic = new Dictionary<string, Queue<(DateTime dateTime, long size)>>();
                var refreshInterval = TimeSpan.FromMilliseconds(100);
                var buffCount = 30;
                Stopwatch sw = Stopwatch.StartNew();
                while (true)
                {
                    bool restartSw = false;
                    foreach (var (path, taskObj) in _tasks.ToList())
                    {
                        if (!dic.ContainsKey(path))
                        {
                            var queue = new Queue<(DateTime, long)>();
                            queue.Enqueue((DateTime.Now, taskObj.TransferredSize));
                            dic.Add(path, queue);
                        }
                        else
                        {
                            //taskObj.BytePerSecond =
                            //    (long)((taskObj.TransferredSize - dic[path]) / refreshInterval.TotalSeconds);
                            //Console.WriteLine(taskObj.BytePerSecond);
                            if (dic[path].Count > buffCount)
                                dic[path].Dequeue();
                            dic[path].Enqueue((DateTime.Now, taskObj.TransferredSize));
                        }

                        if (sw.Elapsed > TimeSpan.FromSeconds(1) && dic[path].Count > 1)
                        {
                            var list = dic[path].ToList();
                            var list2 = new List<double>();
                            for (int i = 0; i < list.Count - 1; i++)
                            {
                                var @this = list[i];
                                var next = list[i + 1];
                                list2.Add((next.size - @this.size) /
                                          (next.dateTime - @this.dateTime).TotalSeconds);
                            }

                            taskObj.BytePerSecond = (long)list2.Average();
                            restartSw = true;
                        }
                    }

                    if (restartSw) sw.Restart();

                    Thread.Sleep(refreshInterval);
                }
            });
        }

        public static UploadManager Default { get; private set; }

        public ObservableCollection<TaskObj> ObservableTasks
        {
            get => _observableTasks;
            set
            {
                if (Equals(value, _observableTasks)) return;
                _observableTasks = value;
                OnPropertyChanged();
            }
        }

        private ConcurrentDictionary<string, TaskObj> _tasks
            = new ConcurrentDictionary<string, TaskObj>();

        private ObservableCollection<TaskObj> _observableTasks = new ObservableCollection<TaskObj>(new[]
        {
            new TaskObj(() => { })
            {
                Name = "file.txt",
                Path = "C:\\file.txt",
                RunStatus = true,
                StatusMessage = "进行中",
                TotalSize = 114514,
                TransferredSize = 1919
            },
            new TaskObj(() => { })
            {
                Name = "pic.vpk",
                Path = "D:\\addons\\pic.vpk",
                RunStatus = false,
                StatusMessage = "无法从传输连接中读取数据: 连接已关闭。",
                TotalSize = 1919,
                TransferredSize = 810
            },
            new TaskObj(() => { })
            {
                Name = "HAHA.zip",
                Path = "/home/user/HAHA.zip",
                RunStatus = null,
                StatusMessage = "ready",
                TotalSize = 0,
                TransferredSize = 0
            },
            new TaskObj(() => { })
            {
                Name = "HAHA.zip",
                Path = "/home/user/gg.zip",
                RunStatus = false,
                StatusMessage = "上传成功",
                TotalSize = 2315450,
                TransferredSize = 2315450
            }
        });

        private static readonly string[] SupportedTypes = { ".vpk", ".zip" };

        public void AddTask(string path)
        {
            var fixedFilename = Path.GetFileName(path);

            string ext = Path.GetExtension(fixedFilename);
            if (!SupportedTypes.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                throw new NotSupportedException($"不支持的文件类型：\"{ext}\"");
            }

            char[] reverseChar = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToArray();
            if (fixedFilename?.Any(c => c > 127 && c < 32) != false ||
                fixedFilename.Any(c => reverseChar.Contains(c)))
            {
                throw new NotSupportedException("文件路径不合法，请检查路径是否包含特殊字符。目前不支持中文文件名。");
            }

            var cts = new CancellationTokenSource();
            var taskObj = new TaskObj(() =>
            {
                try
                {
                    cts.Cancel();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            })
            {
                Path = path,
                Name = fixedFilename,
            };
            var task = new Task(async () =>
            {
                var (success, message, data) = await UploadFile(
                    taskObj.Path,
                    null, (total, current) =>
                    {
                        taskObj.RunStatus = true;
                        taskObj.TotalSize = total;
                        taskObj.TransferredSize = current;
                    }, cts);
                Console.WriteLine("done await action");
                taskObj.RunStatus = false;
                if (!success) taskObj.StatusMessage = message;
                else taskObj.StatusMessage = "上传成功";
                _tasks.TryRemove(path, out _);
            }, TaskCreationOptions.LongRunning);
            taskObj.Task = task;
            taskObj.Cts = cts;
            taskObj.Task.Start();
            _tasks.TryAdd(path, taskObj);

            ObservableTasks.Add(taskObj);
        }

        private static async Task<(bool success, string message, string)> UploadFile(
            string path,
            string remark,
            FileUploadCallback callback,
            CancellationTokenSource cts)
        {
            try
            {
                var result = await Requester.Default.HttpClient.UploadFileAsync(
                    $"{Requester.Default.Host}api/explorer/upload", new Dictionary<string, string>
                    {
                        ["Remark"] = remark
                    }, new[] { path }, callback, cts
                );

                return (true, null, result);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
    }

    public class TaskObj : VmBase
    {
        private string _name;
        private string _path;
        private long _transferredSize;
        private long _totalSize;
        private bool? _runStatus;
        private string _statusMessage;
        private long _bytePerSecond;

        public TaskObj(Action cancelAction)
        {
            CancelAction = new DelegateCommand(obj => cancelAction());
            TaskId = Guid.NewGuid();
        }

        public Guid TaskId { get; }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                Execute.OnUiThread(() => OnPropertyChanged());
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                if (value == _path) return;
                _path = value;
                Execute.OnUiThread(() => OnPropertyChanged());
            }
        }

        public long TransferredSize
        {
            get => _transferredSize;
            set
            {
                if (value == _transferredSize) return;
                _transferredSize = value;
                Execute.OnUiThread(() => OnPropertyChanged());
            }
        }

        public long TotalSize
        {
            get => _totalSize;
            set
            {
                if (value == _totalSize) return;
                _totalSize = value;
                Execute.OnUiThread(() => OnPropertyChanged());
            }
        }

        /// <summary>
        /// true: running; false: stopped; null: not started
        /// </summary>
        public bool? RunStatus
        {
            get => _runStatus;
            set
            {
                if (value == _runStatus) return;
                _runStatus = value;
                Execute.OnUiThread(() => OnPropertyChanged());
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (value == _statusMessage) return;
                _statusMessage = value;
                Execute.OnUiThread(() => OnPropertyChanged());
            }
        }

        public long BytePerSecond
        {
            get => _bytePerSecond;
            set
            {
                if (value == _bytePerSecond) return;
                _bytePerSecond = value;
                Execute.OnUiThread(() => OnPropertyChanged());
            }
        }

        public ICommand CancelAction { get; }

        public ICommand CancelConfirmAction => new DelegateCommand(obj =>
        {
            if (RunStatus != false)
            {
                var result = MessageBox.Show(Application.Current.MainWindow, "任务正在下载，确定删除任务吗？", "提示",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.OK)
                {
                    CancelAction?.Execute(obj);
                    UploadManager.Default.ObservableTasks.Remove(this);
                }
            }
            else
            {
                CancelAction?.Execute(obj);
                UploadManager.Default.ObservableTasks.Remove(this);
            }
        });

        public Task Task { get; set; }
        public CancellationTokenSource Cts { get; set; }
    }
}