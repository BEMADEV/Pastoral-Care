using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 7, "1.11.2" )]
    public class AddWorkflowActions : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType("Care Item", "", "com.bemaservices.PastoralCare", "com.bemaservices.PastoralCare.Field.Types.CareItemFieldType", "377B0FBE-D53C-4306-B179-5810E02B9A3F");
            RockMigrationHelper.UpdateFieldType("Care Types", "", "com.bemaservices.PastoralCare", "com.bemaservices.PastoralCare.Field.Types.CareTypesFieldType", "E9EA88C6-BAFB-4A2E-9471-CC2FD7951744");

            RockMigrationHelper.UpdateEntityType("com.bemaservices.PastoralCare.Web.Cache.CareTypeCache", "Care Type Cache", "com.bemaservices.PastoralCare.Web.Cache.CareTypeCache, com.bemaservices.PastoralCare, Version=1.0.0.7, Culture=neutral, PublicKeyToken=null", false, true, "30C997BB-B0FA-484C-989E-888016DDB9D7");
            RockMigrationHelper.UpdateEntityType("com.bemaservices.PastoralCare.Workflow.Actions.PastoralCare.AddCareContact", "Add Care Contact", "com.bemaservices.PastoralCare.Workflow.Actions.PastoralCare.AddCareContact, com.bemaservices.PastoralCare, Version=1.0.0.7, Culture=neutral, PublicKeyToken=null", false, true, "A6E9385A-2E33-4312-868C-032C29B9AA56");
            RockMigrationHelper.UpdateEntityType("com.bemaservices.PastoralCare.Workflow.Actions.PastoralCare.AddCareItemAttribute", "Add Care Item Attribute", "com.bemaservices.PastoralCare.Workflow.Actions.PastoralCare.AddCareItemAttribute, com.bemaservices.PastoralCare, Version=1.0.0.7, Culture=neutral, PublicKeyToken=null", false, true, "3691A2D7-3824-497B-A226-F49F51225049");
            RockMigrationHelper.UpdateEntityType("com.bemaservices.PastoralCare.Workflow.Actions.PastoralCare.CreateCareItem", "Create Care Item", "com.bemaservices.PastoralCare.Workflow.Actions.PastoralCare.CreateCareItem, com.bemaservices.PastoralCare, Version=1.0.0.7, Culture=neutral, PublicKeyToken=null", false, true, "EF30C40E-79EB-48AE-B2F3-D8D21364443D");

        
        }
        public override void Down()
        {
          
        }
    }
}
