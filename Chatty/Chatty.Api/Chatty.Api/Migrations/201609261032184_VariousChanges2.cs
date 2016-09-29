namespace Chatty_ApiService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VariousChanges2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Ticket", c => c.String());
            AddColumn("dbo.Users", "IsConfirmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IsConfirmed");
            DropColumn("dbo.Users", "Ticket");
        }
    }
}
