namespace ChatterboxAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedMembersRooms : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Members (Name,Birthdate) VALUES ('Lososik Superstar','2007-10-01')");
            Sql("INSERT INTO Members (Name,Birthdate) VALUES ('Dima Dima','1989-12-16')");
            Sql("INSERT INTO Members (Name,Birthdate) VALUES ('Ivan Durak',1955-12-03)");
            Sql("INSERT INTO Members (Name,Birthdate) VALUES ('MPJ','1984-12-12')");

            Sql("INSERT INTO Rooms (Name,Description) VALUES ('SPAM','All track here')");
            Sql("INSERT INTO Rooms (Name,IsPrivate) VALUES ('VIP',1)");


        }
        
        public override void Down()
        {
        }
    }
}
