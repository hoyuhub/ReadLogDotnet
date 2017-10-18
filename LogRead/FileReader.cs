using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace LogRead
{
    public abstract class FileReader
    {
        private string filePath;
        private List<FileReadPoint> readPoint = new List<FileReadPoint>();
        private bool isStart;
        private int threadCompleteCount;
        public event EventHandler FileReadEnd;//文件读取完成执行的委托事件
        public bool IsStart
        {
            get { return isStart; }
        }
        public string FilePath
        {
            get { return filePath; }
        }
        private FileReader()
        {
        }
        public FileReader(string filePath)
        {
            this.filePath = filePath;
        }
        /// <summary>
        /// 获取读取文件的起始点和结束点
        /// 文件起始点会在参数point中给出
        /// </summary>
        /// <param name="point">读取文件的起始点和结束点</param>
        /// <param name="stream">文件流</param>
        /// <param name="length">文件长度</param>
        protected abstract void GetPoint(FileReadPoint point, FileStream stream, long length);
        /// <summary>
        /// 设置文件读取起始点
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual int SetStartPoint(FileStream stream)
        {
            return 0;
        }
        /// <summary>
        /// 对已用多线程分块读取的文件做的处理
        /// </summary>
        /// <param name="threadStream"></param>
        protected abstract void DoFileRead(ThreadStream threadStream);

        /// <summary>
        /// 初始化分块读取文件的点
        /// </summary>
        /// <returns></returns>
        public bool Create()
        {
            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.Refresh();
            if (fileInfo.Exists)
            {
                filePath = fileInfo.FullName;
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    if (readPoint.Count != 0)
                    {
                        readPoint.Clear();
                    }
                    long startPoint = SetStartPoint(stream);
                    long length = stream.Length;
                    while (startPoint < length)
                    {
                        stream.Position = startPoint;
                        FileReadPoint fPoint = new FileReadPoint();
                        fPoint.StartPoint = startPoint;
                       // GetPoint(fPoint, stream, length);
                        if (fPoint.StartPoint + fPoint.ReadCount > length)
                        {
                            fPoint.ReadCount = length - fPoint.StartPoint;
                        }
                        readPoint.Add(fPoint);
                        startPoint = fPoint.StartPoint + fPoint.ReadCount;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 启动多线程文件读取
        /// </summary>
        public void StartRead()
        {
            if (!isStart)
            {
                threadCompleteCount = 0;
                foreach (FileReadPoint fp in readPoint)
                {
                    Thread thread = new Thread(OnReadFile);
                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.MTA);
                    thread.Start(fp);
                }
                isStart = true;
            }
        }


        [MTAThread()]
        private void OnReadFile(object obj)
        {
            FileReadPoint fp = obj as FileReadPoint;
            if (fp != null)
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    stream.Position = fp.StartPoint;
                    ThreadStream threadStream = new ThreadStream(stream, fp);
                    DoFileRead(threadStream);
                }
            }
            if (FileReadEnd != null)
            {
                lock (readPoint)
                {
                    threadCompleteCount++;
                    if (threadCompleteCount == readPoint.Count)
                    {
                        FileReadEnd(this, new EventArgs());
                    }
                }
            }
        }


    }
}
