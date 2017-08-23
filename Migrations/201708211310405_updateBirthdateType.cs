namespace ChatterboxAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateBirthdateType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Members", "Birthdate", c => c.DateTime(nullable: false, precision: 0, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Members", "Birthdate", c => c.DateTime(nullable: false));
        }
    }
}
