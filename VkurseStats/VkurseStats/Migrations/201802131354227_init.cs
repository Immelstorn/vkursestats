namespace VkurseStats.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "vkurse.VkurseRates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UsdBuy = c.Double(nullable: false),
                        UsdSell = c.Double(nullable: false),
                        EurBuy = c.Double(nullable: false),
                        EurSell = c.Double(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("vkurse.VkurseRates");
        }
    }
}
