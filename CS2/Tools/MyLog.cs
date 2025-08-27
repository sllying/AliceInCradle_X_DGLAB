using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradle.Tools
{

    public static class MyLog
    {
        public static void Log(string content)
        {
            string path = "LOG.txt";
            FileStream fs;
            if (File.Exists(path))
            {
                fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            }
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(content);
            sw.Close();
            fs.Close();
        }
    }
}


