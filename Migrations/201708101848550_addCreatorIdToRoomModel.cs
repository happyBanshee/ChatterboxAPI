namespace ChatterboxAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCreatorIdToRoomModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rooms", "CreatorId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Rooms", "CreatorId");
        }
    }
}
