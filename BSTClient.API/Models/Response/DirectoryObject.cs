using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BSTClient.API.Models.Response
{
    public class MyDirectoryObject : INotifyPropertyChanged
    {
        private string _relativePath;
        public char DirectorySeparatorChar { get; set; } = Path.DirectorySeparatorChar;
        public bool CanCreateFiles { get; set; } = true;
        public List<ExplorerDesc> Items { get; set; } = new List<ExplorerDesc>();

        public string RelativePath
        {
            get => _relativePath;
            set
            {
                _relativePath = value;
                OnPropertyChanged();
            }
        }

        public static MyDirectoryObject FromDirectoryObject(DirectoryObject directoryObject)
        {
            var my = new MyDirectoryObject
            {
                DirectorySeparatorChar = directoryObject.DirectorySeparatorChar,
                CanCreateFiles = directoryObject.CanCreateFiles,
                Items = directoryObject.Directories.Cast<ExplorerDesc>().Concat(directoryObject.Files).ToList(),
                RelativePath = directoryObject.RelativePath.Replace(directoryObject.DirectorySeparatorChar,
                    Path.DirectorySeparatorChar)
            };
            return my;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DirectoryObject
    {
        public char DirectorySeparatorChar { get; set; } = Path.DirectorySeparatorChar;
        public bool CanCreateFiles { get; set; } = true;
        public List<DirectoryDesc> Directories { get; set; } = new List<DirectoryDesc>();
        public List<FileDesc> Files { get; set; } = new List<FileDesc>();
        public string RelativePath { get; set; }
    }

    public class ExplorerDesc
    {
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public long? Size { get; set; }
    }

    public class FileDesc : ExplorerDesc
    {
        public bool CanDisable { get; set; } = true;
        public bool CanDelete { get; set; } = true;
        public bool CanEdit { get; set; } = true;
    }

    public class DirectoryDesc : ExplorerDesc
    {
    }
}
