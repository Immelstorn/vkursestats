namespace VkurseStats.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDkk : DbMigration
    {
        public override void Up()
        {
            AddColumn("vkurse.VkurseRates", "DkkRate", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("vkurse.VkurseRates", "DkkRate");
        }
    }
}
