using System.ComponentModel;
using SQLite;

namespace Yamanote
{
    [Table("questions")]
    public class YamanoteQuestion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Content { get; set; }
        public string Category { get; set; }

        public string Theme { get; set; }

        public string Difficulty { get; set; }
    }

    public enum Difficulty
    {
        [Description("低")] Easy,
        [Description("中")] Medium,
        [Description("高")] Hard
    }
}