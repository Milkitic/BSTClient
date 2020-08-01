using BSTClient.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BSTClient
{
    public class UploadManager : VmBase
    {
        public UploadManager()
        {
            Default = this;
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
                StatusMessage = "not valid file type",
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
            var taskObj = new TaskObj(() => cts.Cancel())
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
                        taskObj.TotalSize = total;
                        taskObj.TransferredSize = current;
                    }, cts);
                taskObj.RunStatus = false;
                if (!success) taskObj.StatusMessage = message;
                _tasks.TryRemove(path, out _);
            }, TaskCreationOptions.LongRunning);
            taskObj.Task = task;
            taskObj.Cts = cts;
            taskObj.Task.Start();
            _tasks.TryAdd(path, taskObj);

            ObservableTasks = new ObservableCollection<TaskObj>(_tasks.Values);
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

    public class TaskObj
    {
        public TaskObj(Action cancelAction)
        {
            CancelAction = cancelAction;
            TaskId = Guid.NewGuid();
        }

        public Guid TaskId { get; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long TransferredSize { get; set; }
        public long TotalSize { get; set; }
        /// <summary>
        /// true: running; false: stopped; null: not started
        /// </summary>
        public bool? RunStatus { get; set; }
        public string StatusMessage { get; set; }
        public long BytePerSecond { get; set; }
        public Action CancelAction { get; }
        public Task Task { get; set; }
        public CancellationTokenSource Cts { get; set; }
    }
}
