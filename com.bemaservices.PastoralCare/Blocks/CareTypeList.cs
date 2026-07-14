using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Security;

using com.bemaservices.PastoralCare.Model;

namespace com.bemaservices.PastoralCare.Blocks
{
    [DisplayName( "Care Type List" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "Block to display the care types." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Detail Page", "Page used to view details of a care type.", Key = AttributeKey.DetailPage, Order = 0 )]
    [BooleanField( "Allow Shared Care Item Attributes", "Displays a link to a page for care item attributes shared across care types", false, Key = AttributeKey.AllowSharedCareItemAttributes, Order = 1 )]
    [LinkedPage( "Shared Care Item Attribute Page", "Page used to view universal care item attributes.", false, Key = AttributeKey.SharedCareItemAttributePage, Order = 2 )]
    [BooleanField( "Allow Shared Care Contact Attributes", "Displays a link to a page for care contact attributes shared across care types", false, Key = AttributeKey.AllowSharedCareContactAttributes, Order = 3 )]
    [LinkedPage( "Shared Care Contact Attribute Page", "Page used to view universal care contact attributes.", false, Key = AttributeKey.SharedCareContactAttributePage, Order = 4 )]

    [Rock.SystemGuid.EntityTypeGuid( "f86385f8-a153-4de8-8cf4-6ce92c37b056" )]
    [Rock.SystemGuid.BlockTypeGuid( "252ef3e6-876a-40ba-9f7a-0eeec9a50200" )]
    public class CareTypeList : RockBlockType
    {
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string AllowSharedCareItemAttributes = "AllowSharedCareItemAttributes";
            public const string SharedCareItemAttributePage = "SharedCareItemAttributePage";
            public const string AllowSharedCareContactAttributes = "AllowSharedCareContactAttributes";
            public const string SharedCareContactAttributePage = "SharedCareContactAttributePage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string SharedCareItemAttributePage = "SharedCareItemAttributePage";
            public const string SharedCareContactAttributePage = "SharedCareContactAttributePage";
        }

        public override string ObsidianFileUrl => "~/Plugins/com_bemaservices/PastoralCare/careTypeList.obs";

        public override object GetObsidianBlockInitialization()
        {
            return new
            {
                isAddEnabled = UserCanAdministrate,
                options = new
                {
                    showSharedCareItemAttributes = GetAttributeValue( AttributeKey.AllowSharedCareItemAttributes ).AsBoolean(),
                    showSharedCareContactAttributes = GetAttributeValue( AttributeKey.AllowSharedCareContactAttributes ).AsBoolean()
                },
                navigationUrls = new Dictionary<string, string>
                {
                    [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "CareTypeId", "((Key))" ),
                    [NavigationUrlKey.SharedCareItemAttributePage] = this.GetLinkedPageUrl( AttributeKey.SharedCareItemAttributePage ),
                    [NavigationUrlKey.SharedCareContactAttributePage] = this.GetLinkedPageUrl( AttributeKey.SharedCareContactAttributePage )
                }
            };
        }

        [BlockAction]
        public BlockActionResult GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                var allCareTypes = new CareTypeService( rockContext )
                    .Queryable()
                    .OrderBy( ct => ct.Name )
                    .ToList();

                var rows = allCareTypes
                    .Where( ct => UserCanEdit || ct.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .Select( ct => (object) new
                    {
                        id = ct.Id,
                        idKey = ct.IdKey,
                        name = ct.Name,
                        isActive = ct.IsActive,
                        canEdit = UserCanAdministrate || ct.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
                    } )
                    .ToList();

                return ActionOk( rows );
            }
        }
    }
}
