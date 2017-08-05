namespace ChatterboxAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateMessageModel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Messages", "Room_Id", "dbo.Rooms");
            DropIndex("dbo.Messages", new[] { "Room_Id" });
            RenameColumn(table: "dbo.Messages", name: "Author_Id", newName: "AuthorId");
            RenameColumn(table: "dbo.Messages", name: "Room_Id", newName: "RoomId");
            RenameIndex(table: "dbo.Messages", name: "IX_Author_Id", newName: "IX_AuthorId");
            AlterColumn("dbo.Messages", "RoomId", c => c.Int(nullable: true));
            CreateIndex("dbo.Messages", "RoomId");
            AddForeignKey("dbo.Messages", "RoomId", "dbo.Rooms", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Messages", "RoomId", "dbo.Rooms");
            DropIndex("dbo.Messages", new[] { "RoomId" });
            AlterColumn("dbo.Messages", "RoomId", c => c.Int());
            RenameIndex(table: "dbo.Messages", name: "IX_AuthorId", newName: "IX_Author_Id");
            RenameColumn(table: "dbo.Messages", name: "RoomId", newName: "Room_Id");
            RenameColumn(table: "dbo.Messages", name: "AuthorId", newName: "Author_Id");
            CreateIndex("dbo.Messages", "Room_Id");
            AddForeignKey("dbo.Messages", "Room_Id", "dbo.Rooms", "Id");
        }
    }
}
