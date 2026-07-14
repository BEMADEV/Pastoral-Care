using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
	[MigrationNumber( 9, "1.17.6" )]
	public class ObsidianConversion : Migration
	{
		public override void Up()
		{
			AddOrUpdateEntityTypes();
			AddOrUpdateBlockTypes();
			AddOrUpdateBlockTypeAttributes();
		}

		public override void Down()
		{
		}

		private void AddOrUpdateEntityTypes()
		{
			RockMigrationHelper.UpdateEntityType( "com.bemaservices.PastoralCare.Blocks.CareTypeList", "Care Type List", "com.bemaservices.PastoralCare.Blocks.CareTypeList, com.bemaservices.PastoralCare, Version=1.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "F86385F8-A153-4DE8-8CF4-6CE92C37B056" );
			RockMigrationHelper.UpdateEntityType( "com.bemaservices.PastoralCare.Blocks.CareTypeDetail", "Care Type Detail", "com.bemaservices.PastoralCare.Blocks.CareTypeDetail, com.bemaservices.PastoralCare, Version=1.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "1A87C29E-BF65-4F7D-A39D-2CFDFED57FCE" );
			RockMigrationHelper.UpdateEntityType( "com.bemaservices.PastoralCare.Blocks.CareItemList", "Care Item List", "com.bemaservices.PastoralCare.Blocks.CareItemList, com.bemaservices.PastoralCare, Version=1.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "F2B5F793-45F6-46BD-A28C-D66B4F8CABBE" );
			RockMigrationHelper.UpdateEntityType( "com.bemaservices.PastoralCare.Blocks.CareItemDetail", "Care Item Detail", "com.bemaservices.PastoralCare.Blocks.CareItemDetail, com.bemaservices.PastoralCare, Version=1.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "F426D8BF-5387-452D-B264-0075CD5865D2" );
		}

		private void AddOrUpdateBlockTypes()
		{
			RockMigrationHelper.AddOrUpdateEntityBlockType( "Care Type List", "Block to display the care types.", "com.bemaservices.PastoralCare.Blocks.CareTypeList", "BEMA Services > Pastoral Care", "252EF3E6-876A-40BA-9F7A-0EEEC9A50200" );
			RockMigrationHelper.AddOrUpdateEntityBlockType( "Care Type Detail", "Displays the details of the given Care Type for editing.", "com.bemaservices.PastoralCare.Blocks.CareTypeDetail", "BEMA Services > Pastoral Care", "7B44CB2C-67D2-4AD0-9CC5-975692CB6802" );
			RockMigrationHelper.AddOrUpdateEntityBlockType( "Care Item List", "Block to display the care items.", "com.bemaservices.PastoralCare.Blocks.CareItemList", "BEMA Services > Pastoral Care", "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A" );
			RockMigrationHelper.AddOrUpdateEntityBlockType( "Care Item Detail", "Displays the details of the given care item for editing.", "com.bemaservices.PastoralCare.Blocks.CareItemDetail", "BEMA Services > Pastoral Care", "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2" );
		}

		private void AddOrUpdateBlockTypeAttributes()
		{
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page used to view details of a care type.", 0, @"", "009755D4-2EA8-49E1-B179-622AFFFDBAAB" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Shared Care Item Attributes", "AllowSharedCareItemAttributes", "Allow Shared Care Item Attributes", @"Displays a link to a page for care item attributes shared across care types", 1, @"False", "5DB27B1A-2E7D-4626-91B4-2087E9D622B2" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Shared Care Item Attribute Page", "SharedCareItemAttributePage", "Shared Care Item Attribute Page", @"Page used to view universal care item attributes.", 2, @"", "F7AC513C-6C6F-4072-967B-E0B12F663719" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Shared Care Contact Attributes", "AllowSharedCareContactAttributes", "Allow Shared Care Contact Attributes", @"Displays a link to a page for care contact attributes shared across care types", 3, @"False", "99E11ECA-A2C3-44A9-9C61-BCC5946BA007" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Shared Care Contact Attribute Page", "SharedCareContactAttributePage", "Shared Care Contact Attribute Page", @"Page used to view universal care contact attributes.", 4, @"", "116564E5-DA12-4D0A-8DB4-42A8758D5C22" );

			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "Configuration Page", @"Page used to modify and create care types.", 0, @"", "056A2CCA-86F7-4480-B8C6-6CB14827340E" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page used to view details of a care item.", 1, @"", "4FA7FEE2-8203-4001-89EA-3C796424469F" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Inactive Requests By Default", "HideInactiveRequestsByDefault", "Hide Inactive Requests By Default", @"If this is enabled, inactive requests will be hidden from the list by default.", 2, @"False", "7E5B93E2-7D0A-48CF-B9DA-0B90B29F0BC4" );

			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page used for viewing a person's profile. If set a view profile button will show for each group member.", 0, @"", "4BD88C6A-8D22-4466-B5C2-9DEF44170354" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2", "3F1AE891-7DC8-46D2-865D-11543B34FB60", "Badges", "Badges", "Badges", @"The person badges to display in this block.", 1, @"", "7ACA8972-EB2A-400C-8607-D7E185F85359" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Request Time to Current Time", "DefaultRequestTimetoCurrentTime", "Default Request Time to Current Time", @"Defaults the Request Date and Time to current date and time.", 2, @"False", "E2823C8F-F94A-46C4-B990-626B4AE273FA" );
			RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Requester Self Selection", "AllowRequesterSelfSelection", "Allow Requester Self Selection", @"Allows the person filling out the form to select themselves as the requestor.", 3, @"False", "BF5759FA-3628-4F11-82AA-A9174F3D1278" );
		}
	}
}
