//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/15 16:49:06</date>
//------------------------------------------------------------------------------

using System.ComponentModel;

namespace CodeGenerator.Model
{
    public class ColumnParam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string name { get; set; } = string.Empty;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        private string comments { get; set; } = string.Empty;
        public string Comments
        {
            get
            {
                return comments;
            }
            set
            {
                comments = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Comments"));
            }
        }

        private string type { get; set; } = string.Empty;
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Type"));
            }
        }

        /// <summary>
        /// N/Y
        /// </summary>
        private string isNullable { get; set; } = string.Empty;
        public string IsNullable
        {
            get
            {
                return isNullable;
            }
            set
            {
                isNullable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsNullable"));
            }
        }

        private int length { get; set; }
        public int Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Length"));
            }
        }

        private bool isPrimaryKey { get; set; }
        public bool IsPrimaryKey
        {
            get
            {
                return isPrimaryKey;
            }
            set
            {
                isPrimaryKey = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsPrimaryKey"));
            }
        }

        private bool isAutoIncrement { get; set; }
        public bool IsAutoIncrement
        {
            get
            {
                return isAutoIncrement;
            }
            set
            {
                isAutoIncrement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAutoIncrement"));
            }
        }

    }
}
