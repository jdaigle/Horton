using System;
using System.Runtime.Serialization;

namespace Horton.Scripts
{
    [Serializable]
    internal class DuplicateFilenameException : Exception
    {
        public DuplicateFilenameException()
        {
        }

        public DuplicateFilenameException(string message) : base(message)
        {
        }

        public DuplicateFilenameException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DuplicateFilenameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}