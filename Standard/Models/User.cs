namespace Standard.Models
{
    public class User
    {
        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
            }
        }

        private int _ID { get; set; }

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
            }
        }

        private string _Name { get; set; }

        public User()
        {
            _ID = 0;
            _Name = string.Empty;
        }
    }
}
