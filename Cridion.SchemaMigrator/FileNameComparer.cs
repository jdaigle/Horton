using System;
using System.Collections.Generic;
using System.Text;

namespace Cridion.SchemaMigrator
{
    public class FileNameComparer : IComparable
    {

        private System.IO.FileInfo _FileInfo;
        public System.IO.FileInfo FileInfo
        {
            get { return _FileInfo; }
        }


        public FileNameComparer(System.IO.FileInfo ThisFile)
        {
            _FileInfo = ThisFile;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (!(obj is FileNameComparer))
                throw new Exception("Object is not comparable");
            return CompareTo((FileNameComparer)obj);
        }

        public int CompareTo(FileNameComparer file)
        {
            return String.Compare(_FileInfo.FullName, file.FileInfo.FullName);
        }

        #endregion
    }
}
