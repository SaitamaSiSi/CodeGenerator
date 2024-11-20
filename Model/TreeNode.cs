//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/19 14:57:26</date>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using Yitter.IdGenerator;

namespace CodeGenerator.Model
{
    public class TreeNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public TreeNode()
        {
            id = YitIdHelper.NextId();
        }

        private long id { get; set; }
        public long Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Id"));
            }
        }

        private bool isChecked { get; set; }
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }

        public bool IsCheckable
        {
            get
            {
                return level > 0;
            }
        }

        private string title { get; set; } = string.Empty;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));
            }
        }

        private int level { get; set; }
        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Level"));
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

        private List<TreeNode> subNodes { get; set; } = new List<TreeNode>();
        public List<TreeNode> SubNodes
        {
            get
            {
                return subNodes;
            }
            set
            {
                subNodes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SubNodes"));
            }
        }

        private List<ColumnParam> columns { get; set; } = new List<ColumnParam>();
        public List<ColumnParam> Columns
        {
            get
            {
                return columns;
            }
            set
            {
                columns = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Columns"));
            }
        }

    }
}
