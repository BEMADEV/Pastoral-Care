using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.Web.Cache;

using com.bemaservices.PastoralCare.Model;
using com.bemaservices.PastoralCare.ViewModels;

namespace com.bemaservices.PastoralCare.Blocks
{
    /// <summary>
    /// Displays a list of care types.
    /// </summary>

    [DisplayName( "Care Type List" )]
    [Category( "BEMA Software Services > Pastoral Care" )]
    [Description( "Displays a list of care types." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the care type details.",
        Key = AttributeKey.DetailPage )]
    [BooleanField( "Allow Shared Care Item Attributes",
        Description = "Displays a link to a page for care item attributes shared across care types.",
        DefaultBooleanValue = false,
        Key = AttributeKey.AllowSharedCareItemAttributes )]
    [LinkedPage( "Shared Care Item Attribute Page",
        Description = "Page used to view universal care item attributes.",
        Key = AttributeKey.SharedCareItemAttributePage )]
    [BooleanField( "Allow Shared Care Contact Attributes",
        Description = "Displays a link to a page for care contact attributes shared across care types.",
        DefaultBooleanValue = false,
        Key = AttributeKey.AllowSharedCareContactAttributes )]
    [LinkedPage( "Shared Care Contact Attribute Page",
        Description = "Page used to view universal care contact attributes.",
        Key = AttributeKey.SharedCareContactAttributePage )]

    [Rock.SystemGuid.EntityTypeGuid( "39577b84-5e6f-4582-93ae-0707e56cc4fc" )]
    [Rock.SystemGuid.BlockTypeGuid( "e3e46273-ce72-43da-a130-2f91f97ef21e" )]
    [CustomizedGrid]
    public class CareTypeList : RockEntityListBlockType<CareType>
    {
        #region Keys

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

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Plugins/com_bemaservices/PastoralCare/careTypeList.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CareTypeListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CareTypeListOptionsBag GetBoxOptions()
        {
            var options = new CareTypeListOptionsBag
            {
                AllowSharedCareItemAttributes = GetAttributeValue( AttributeKey.AllowSharedCareItemAttributes ).AsBoolean(),
                AllowSharedCareContactAttributes = GetAttributeValue( AttributeKey.AllowSharedCareContactAttributes ).AsBoolean()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// </summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new CareType();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "CareTypeId", "((Key))" ),
                [NavigationUrlKey.SharedCareItemAttributePage] = this.GetLinkedPageUrl( AttributeKey.SharedCareItemAttributePage ),
                [NavigationUrlKey.SharedCareContactAttributePage] = this.GetLinkedPageUrl( AttributeKey.SharedCareContactAttributePage )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<CareType> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext );
        }

        /// <inheritdoc/>
        protected override GridBuilder<CareType> GetGridBuilder()
        {
            return new GridBuilder<CareType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "description", a => a.Description )
                .AddTextField( "name", a => a.Name )
                .AddAttributeFields( GetGridAttributes() );
        }

        #endregion

        #region Block Actions

        #endregion
    }
}
