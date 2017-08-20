namespace ChatterboxAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateRoomModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rooms", "MemberId", c => c.Int(nullable: false));
            DropColumn("dbo.Rooms", "CreatorId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Rooms", "CreatorId", c => c.String());
            DropColumn("dbo.Rooms", "MemberId");
        }
    }
}
