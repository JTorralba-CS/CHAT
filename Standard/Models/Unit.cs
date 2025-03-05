using System.ComponentModel.DataAnnotations;

namespace Standard.Models
{
    public class Unit
    {
        [Key]
        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
            }
        }

        private int _ID { get; set; }

        public int Agency
        {
            get { return _Agency; }
            set
            {
                _Agency = value;
            }
        }

        private int _Agency { get; set; }

        public string Jurisdiction
        {
            get { return _Jurisdiction; }
            set
            {
                _Jurisdiction = value;
            }
        }

        private string _Jurisdiction { get; set; }

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
            }
        }

        private string _Name { get; set; }

        public string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
            }
        }

        private string _Status { get; set; }

        public string Location
        {
            get { return _Location; }
            set
            {
                _Location = value;
            }
        }

        private string _Location { get; set; }

        public Unit()
        {
            _ID = 0;
            _Agency = 0;
            _Jurisdiction = string.Empty;
            _Name = string.Empty;
            _Status = string.Empty;
            _Location = string.Empty;
        }

        public override string ToString() => $"{ID} {Agency} {Jurisdiction} {Name} {Status} {Location}";
    }
}
