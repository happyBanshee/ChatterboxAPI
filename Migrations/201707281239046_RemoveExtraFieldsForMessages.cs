namespace ChatterboxAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveExtraFieldsForMessages : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Messages", "AuthorId");
            DropColumn("dbo.Messages", "RoomId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Messages", "RoomId", c => c.Byte(nullable: false));
            AddColumn("dbo.Messages", "AuthorId", c => c.Byte(nullable: false));
        }
    }
}
