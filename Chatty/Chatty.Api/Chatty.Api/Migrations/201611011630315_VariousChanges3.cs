namespace Chatty_ApiService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VariousChanges3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "FacebookId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "FacebookId");
        }
    }
}
