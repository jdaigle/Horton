using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Cridion.SchemaMigrator.SvnLoader
{
    public class Loader : IDisposable
    {

        public String Url { get; private set; }

        public String Username { get; private set; }

        public String Password { get; private set; }

        public Loader(String Url, String Username, String Password)
        {
            this.Url = Url;
            this.Username = Username;
            this.Password = Password;
        }


        private String _UniquePathID;

        public String UniquePathID
        {
            get
            {
                if (_UniquePathID == null)
                    _UniquePathID = System.Guid.NewGuid().ToString();
                return _UniquePathID;
            }
            private set
            {
                _UniquePathID = System.Guid.NewGuid().ToString();
            }

        }

        public String Path
        {
            get { return _Path; }
        }

        private String _Path;

        public void LoadTopDirectory(String ToPath)
        {
            //first create our temp subdirectory
            if (!ToPath.EndsWith(@"\"))
                ToPath += @"\";
            _Path += ToPath + UniquePathID;
            DirectoryInfo parentdirectory = new DirectoryInfo(_Path);
            if (!parentdirectory.Exists)
                parentdirectory.Create();

            //now run            
            String command = @"c:\Program Files\Subversion\bin\svn.exe";
            String args = String.Format(@"export --force --username {0} --password {1} -N --non-interactive {2} {3}", Username, Password, Url, _Path);
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(command, args);

            while (!proc.HasExited)
                System.Threading.Thread.Sleep(100);
        }

        public void LoadRecursiveDirectory(String ToPath)
        {
            //first create our temp subdirectory
            if (!ToPath.EndsWith(@"\"))
                ToPath += @"\";
            _Path += ToPath + UniquePathID;
            DirectoryInfo parentdirectory = new DirectoryInfo(_Path);
            if (!parentdirectory.Exists)
                parentdirectory.Create();

            //now run            
            String command = @"c:\Program Files\Subversion\bin\svn.exe";
            String args = String.Format(@"export --force --username {0} --password {1} --non-interactive {2} {3}", Username, Password, Url, _Path);
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(command, args);

            while (!proc.HasExited)
                System.Threading.Thread.Sleep(100);
        }

        #region IDisposable Members

        public void Dispose()
        {
            //delete all files            
            DirectoryInfo parentdirectory = new DirectoryInfo(_Path);
            if (!parentdirectory.Exists)
                return;
            parentdirectory.Delete(true);
        }

        #endregion
    }
}
