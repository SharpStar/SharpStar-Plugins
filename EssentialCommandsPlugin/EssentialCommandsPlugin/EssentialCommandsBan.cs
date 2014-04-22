using SQLite;

namespace EssentialCommandsPlugin
{
    public class EssentialCommandsBan
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string UUID { get; set; }

        public int? UserAccountId { get; set; }

    }
}
