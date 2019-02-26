using System.IO;
using UnityEngine;

namespace Assets
{
    class FileLog
    {
        FileStream fs { get; set; }
        StreamWriter writer { get; set; }

        public FileLog(string filename)
        {
            //fs = File.(filename);
            fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            fs.SetLength(0);
            fs.Seek(0, SeekOrigin.Begin);
            writer = new StreamWriter(fs);
        }

        public void LogLine(string line)
        {
            if (line.EndsWith("\n"))
            {
                var bs = System.Text.Encoding.ASCII.GetBytes(line);
                fs.Write(bs, 0, bs.Length);
                //writer.Write(line);
            }
            else
            {
                var bs = System.Text.Encoding.ASCII.GetBytes(line + "\n");
                fs.Write(bs, 0, bs.Length);
                //writer.Write(line + "\n");
            }
        }
        
        public void close()
        {
            fs.Close();
        }
    }
}
