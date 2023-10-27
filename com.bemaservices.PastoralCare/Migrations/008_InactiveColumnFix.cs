using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 8, "1.11.2" )]
    public class InactiveColumnFix : Migration
    {
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] ALTER COLUMN [IsActive] BIT NULL

                Update [_com_bemaservices_PastoralCare_CareItem]
                Set IsActive = 1
                Where IsActive is null

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] ALTER COLUMN [IsActive] BIT Not Null
                " );
        }
        public override void Down()
        {
        }
    }
}
