using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using com.bemaservices.PastoralCare.Model;
using com.bemaservices.PastoralCare.ViewModels;

namespace com.bemaservices.PastoralCare.Blocks
{
    /// <summary>
    /// Displays the details of a particular care item.
    /// </summary>

    [DisplayName( "Care Item Detail" )]
    [Category( "BEMA Software Services > Pastoral Care" )]
    [Description( "Displays the details of a particular care item." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [BooleanField( "Default Request Time to Current Time",
        Description = "Defaults the request date and time to the current date and time.",
        DefaultBooleanValue = false,
        Key = AttributeKey.DefaultRequestTimeToCurrentTime )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "c883850c-d9d8-4fa9-a337-0b2ba639a3c3" )]
    [Rock.SystemGuid.BlockTypeGuid( "8e91198e-ba25-48ba-acd7-1c339ea328a0" )]
    public class CareItemDetail : RockEntityDetailBlockType<CareItem, CareItemBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CareItemId = "CareItemId";
            public const string CareTypeId = "CareTypeId";
        }

        private static class AttributeKey
        {
            public const string DefaultRequestTimeToCurrentTime = "DefaultRequestTimeToCurrentTime";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Plugins/com_bemaservices/PastoralCare/careItemDetail.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<CareItemBag, CareItemDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private CareItemDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new CareItemDetailOptionsBag
            {
                DefaultRequestTimeToCurrentTime = GetAttributeValue( AttributeKey.DefaultRequestTimeToCurrentTime ).AsBoolean()
            };

            return options;
        }

        /// <summary>
        /// Validates the CareItem for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="careItem">The CareItem to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the CareItem is valid, <c>false</c> otherwise.</returns>
        private bool ValidateCareItem( CareItem careItem, out string errorMessage )
        {
            if ( !careItem.PersonAliasId.HasValue )
            {
                errorMessage = "A person is required.";
                return false;
            }

            if ( !careItem.ContactorPersonAliasId.HasValue )
            {
                errorMessage = "A requester is required.";
                return false;
            }

            if ( careItem.ContactDateTime <= DateTime.MinValue.AddYears( 1 ) )
            {
                errorMessage = "A valid request date is required.";
                return false;
            }

            if ( !careItem.CareTypeItems.Any() )
            {
                errorMessage = "At least one care type is required.";
                return false;
            }

            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<CareItemBag, CareItemDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {CareItem.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( CareItem.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( CareItem.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="CareItemBag"/> that represents the entity.</returns>
        private CareItemBag GetCommonEntityBag( CareItem entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CareItemBag
            {
                IdKey = entity.IdKey,
                CareTypeNames = entity.CareTypeItems.Select( cti => cti.CareType != null ? cti.CareType.Name : string.Empty ).Where( n => n.IsNotNullOrWhiteSpace() ).ToList(),
                ContactDateTime = entity.ContactDateTime > DateTime.MinValue.AddYears( 1 ) ? entity.ContactDateTime : ( DateTime? ) null,
                ContactorPersonAlias = entity.ContactorPersonAlias.ToListItemBag(),
                Description = entity.Description,
                IsActive = entity.IsActive,
                LastCareContact = entity.LastCareContact.ToListItemBag(),
                PersonAlias = entity.PersonAlias.ToListItemBag()
            };
        }

        /// <inheritdoc/>
        protected override CareItemBag GetEntityBagForView( CareItem entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override CareItemBag GetEntityBagForEdit( CareItem entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( CareItem entity, ValidPropertiesBox<CareItemBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.ContactDateTime ),
                () => entity.ContactDateTime = box.Bag.ContactDateTime ?? RockDateTime.Now );

            box.IfValidProperty( nameof( box.Bag.ContactorPersonAlias ),
                () => entity.ContactorPersonAliasId = box.Bag.ContactorPersonAlias.GetEntityId<PersonAlias>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.PersonAlias ),
                () => entity.PersonAliasId = box.Bag.PersonAlias.GetEntityId<PersonAlias>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override CareItem GetInitialEntity()
        {
            var entity = GetInitialEntity<CareItem, CareItemService>( RockContext, PageParameterKey.CareItemId );

            if ( entity != null )
            {
                LoadEntityIncludes( entity );
            }

            return entity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out CareItem entity, out BlockActionResult error )
        {
            var entityService = new CareItemService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new CareItem();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{CareItem.FriendlyTypeName} not found." );
                return false;
            }

            LoadEntityIncludes( entity );

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${CareItem.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<CareItemBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<CareItemBag> box )
        {
            var entityService = new CareItemService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            EnsureCareTypeAssociation( entity );
            if ( !ValidateCareItem( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.CareItemId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            LoadEntityIncludes( entity );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<CareItemBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new CareItemService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Adds the selected care type association to new entities when one was provided on the page.
        /// </summary>
        /// <param name="entity">The entity being saved.</param>
        private void EnsureCareTypeAssociation( CareItem entity )
        {
            if ( entity.CareTypeItems.Any() )
            {
                return;
            }

            var careTypeId = RequestContext.GetPageParameter( PageParameterKey.CareTypeId ).AsIntegerOrNull();

            if ( careTypeId.HasValue && careTypeId.Value > 0 )
            {
                entity.CareTypeItems.Add( new CareTypeItem
                {
                    CareTypeId = careTypeId.Value
                } );
            }
        }

        /// <summary>
        /// Loads related entities required by the Obsidian view/edit bags.
        /// </summary>
        /// <param name="entity">The care item entity.</param>
        private void LoadEntityIncludes( CareItem entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return;
            }

            RockContext.Entry( entity )
                .Collection( c => c.CareTypeItems )
                .Query()
                .Include( cti => cti.CareType )
                .Load();
        }

        #endregion
    }
}
