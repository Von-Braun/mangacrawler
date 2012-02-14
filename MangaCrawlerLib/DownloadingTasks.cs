using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YAXLib;
using System.Collections.Concurrent;
using System.Diagnostics;
using TomanuExtensions.Utils;
using System.IO;
using System.Threading.Tasks;

namespace MangaCrawlerLib
{
    internal class DownloadingTasks
    {
        private static readonly string DOWNLOADING_XML = "downloading.xml";
        private static readonly string VERSION = "2012-02-08";

        [YAXAttributeForClass]
        private string Version = VERSION;

        [YAXCollection(ElementName = "Task", Name = "Tasks", 
            SerializationType = YAXCollectionSerializationTypes.InnerCollectionRecursiveInElement)]
        public List<TaskInfo> Tasks = new List<TaskInfo>();

        private string m_file_path;

        public static DownloadingTasks Load(string a_file_path)
        {

            a_file_path += DOWNLOADING_XML;
            DownloadingTasks result = null;

            try
            {
                if (File.Exists(a_file_path))
                    result = YAXSerializer.LoadFromFile<DownloadingTasks>(a_file_path);
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error(ex);
                result = new DownloadingTasks();
            }

            Debug.Assert(result.Version == VERSION);

            result.m_file_path = a_file_path;

            return result;
        }

        public void Save()
        {
            DateTime dt = DateTime.Now;

            Task.Factory.StartNew(() =>
            {
                if ((DateTime.Now - dt).TotalMilliseconds > 100)
                    Loggers.MangaCrawler.Warn("Save task idle more than 100ms");

                try
                {
                    Tasks = DownloadManager.Tasks.ToList();
                    Tasks.RemoveAll(ti => ti.State == TaskState.Deleting);
                    YAXSerializer.SaveToFile<DownloadingTasks>(m_file_path, this);
                }
                catch (Exception ex)
                {
                    Loggers.MangaCrawler.Error(ex);
                }
            });
        }

        public void Restore()
        {
            foreach (var task in Tasks)
            {
                if ((task.State == TaskState.Downloading) ||
                    (task.State == TaskState.Waiting) ||
                    (task.State == TaskState.Zipping))
                {
                    DownloadManager.StartTask(task);
                }
            }
        }
    }
}
