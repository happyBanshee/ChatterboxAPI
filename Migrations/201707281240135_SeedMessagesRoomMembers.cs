namespace ChatterboxAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedMessagesRoomMembers : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Messages (Author_Id,Content,Room_Id,Date) VALUES (4,'Hello there!',1,GETDATE())");
            Sql("INSERT INTO Messages (Author_Id,Content,Room_Id,Date) VALUES (4,'Anybody here?',1,GETDATE())");
            Sql("INSERT INTO Messages (Author_Id,Content,Room_Id,Date) VALUES (7,'Good monday morning!',2,GETDATE())");
            Sql("INSERT INTO Messages (Author_Id,Content,Room_Id,Date) VALUES (6,'Woop!Woop!',2,GETDATE())");

            Sql("INSERT INTO RoomMembers (Member_Id,Room_Id) VALUES (4,1)");
            Sql("INSERT INTO RoomMembers (Member_Id,Room_Id) VALUES (5,1)");
            Sql("INSERT INTO RoomMembers (Member_Id,Room_Id) VALUES (7,1)");

            Sql("INSERT INTO RoomMembers (Member_Id,Room_Id) VALUES (5,2)");
            Sql("INSERT INTO RoomMembers (Member_Id,Room_Id) VALUES (4,2)");
            Sql("INSERT INTO RoomMembers (Member_Id,Room_Id) VALUES (6,2)");
        }
        
        public override void Down()
        {
        }
    }
}
