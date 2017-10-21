using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace LogRead
{
    public sealed class ThreadStream
    {
        //最大线程数
        private int MAXBLOCK = 1024 * 1024 * 2;

        private FileStream fileStream;
        private FileReadPoint fPoint;
        private long currentCount = 0L;

        public FileReadPoint FPoint { get => fPoint; }

        private ThreadStream()
        {
        }

        public ThreadStream(FileStream stream, FileReadPoint point)
        {
            this.fileStream = stream;
            this.fPoint = point;
        }


        /// <summary>
        /// 读取剩余的所有字节
        /// </summary>
        public string ReadAll()
        {
            if (currentCount < fPoint.ReadCount)
            {
                long lastCount = fPoint.ReadCount - currentCount;
                byte[] data = new byte[lastCount];
                long currentDataIndex = 0L;
                while (lastCount > MAXBLOCK)
                {
                    AddData(MAXBLOCK, data, currentDataIndex);
                    lastCount = lastCount - MAXBLOCK;
                    currentDataIndex += MAXBLOCK;
                }
                if (lastCount > 0)
                {
                    AddData((int)lastCount, data, currentDataIndex);
                }
                currentCount = fPoint.ReadCount;
                return data.ToString();
            }
            else
            {
                return null;
            }
        }


        public byte[] Read(int block)
        {
            if (currentCount < fPoint.ReadCount)
            {
                int currentBlock = block;
                if (currentCount + block > fPoint.ReadCount)
                {
                    currentBlock = (int)(fPoint.ReadCount - currentCount);
                }
                byte[] data = new byte[currentBlock];
                fileStream.Read(data, 0, data.Length);
                currentCount += currentBlock;
                return data;

            }
            else
            {
                return null;
            }
        }

        private void AddData(int block, byte[] data, long currentDataIndex)
        {
            byte[] cutData = Read(block);
            Array.Copy(cutData,0,data,currentDataIndex,cutData.Length);
         }
    }
}
