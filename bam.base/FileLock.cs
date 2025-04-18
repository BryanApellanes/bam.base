namespace Bam
{
    internal static class FileLock
    {
        static readonly Dictionary<string, object> _locks = new Dictionary<string, object>();

        public static object Named(string name)
        {
            lock (_locks)
            {
                if (!_locks.ContainsKey(name))
                {
                    _locks.Add(name, new object());
                }
                return _locks[name];
            }
        }  
        
        public static void ClearLocks()
        {
            lock (_locks)
            {
                _locks.Clear();
            }
        }
    }
}
