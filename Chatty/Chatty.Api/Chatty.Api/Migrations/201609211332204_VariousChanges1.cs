namespace Chatty_ApiService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VariousChanges1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "LoginDate", c => c.DateTime());
            AddColumn("dbo.Users", "LastActiveDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "LastActiveDate");
            DropColumn("dbo.Users", "LoginDate");
        }
    }
}
