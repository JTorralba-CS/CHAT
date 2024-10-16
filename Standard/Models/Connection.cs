using System;

namespace Standard.Models
{
    public class Connection
    {
        public string ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
            }
        }

        private string _ID { get; set; }

        public string Alias
        {
            get { return _Alias; }
            set
            {
                _Alias = value;
            }
        }

        private string _Alias { get; set; }

        public Connection()
        {
            _ID = string.Empty;
            _Alias = string.Empty;
        }

        public Connection(string id, string alias)
        {
            _ID = id;
            _Alias = alias.ToUpper();
        }

        public void SetConnection(string id, string alias)
        {
            _ID = id;
            _Alias = alias;

            Console.WriteLine($"Connection.cs SetConnection {Alias}");
        }
    }
}
