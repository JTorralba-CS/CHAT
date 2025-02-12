//OK

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

        public override string ToString() => $"{ID} {Alias}";
    }
}
