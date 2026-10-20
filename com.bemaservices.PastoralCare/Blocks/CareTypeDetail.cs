using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Displays the details of a particular care type.
    /// </summary>

    [DisplayName( "Care Type Detail" )]
    [Category( "BEMA Software Services > Pastoral Care" )]
    [Description( "Displays the details of a particular care type." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "44c63545-67b0-4a0e-bf3c-48901b0da31a" )]
    [Rock.SystemGuid.BlockTypeGuid( "490d0462-4bab-4c5b-a2ee-2b63531d466f" )]
    public class CareTypeDetail : RockEntityDetailBlockType<CareType, CareTypeBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CareTypeId = "CareTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Plugins/com_bemaservices/PastoralCare/careTypeDetail.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<CareTypeBag, CareTypeDetailOptionsBag>();

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
        private CareTypeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new CareTypeDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the CareType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="careType">The CareType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the CareType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateCareType( CareType careType, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<CareTypeBag, CareTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {CareType.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( CareType.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( CareType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="CareTypeBag"/> that represents the entity.</returns>
        private CareTypeBag GetCommonEntityBag( CareType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CareTypeBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsActive = entity.IsActive,
                Name = entity.Name
            };
        }

        /// <inheritdoc/>
        protected override CareTypeBag GetEntityBagForView( CareType entity )
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
        protected override CareTypeBag GetEntityBagForEdit( CareType entity )
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

            if ( entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                bag.CanManageCareItemAttributes = true;
                bag.CareItemAttributes = GetCareItemAttributes( entity.Id ).ConvertAll( attribute => new CareItemAttributeBag
                {
                    Attribute = PublicAttributeHelper.GetPublicEditableAttribute( attribute ),
                    FieldType = FieldTypeCache.Get( attribute.FieldTypeId )?.Name
                } );
            }

            return bag;
        }

        /// <summary>
        /// Gets the CareItem attribute definitions scoped to the specified CareType.
        /// </summary>
        private List<Rock.Model.Attribute> GetCareItemAttributes( int careTypeId )
        {
            if ( careTypeId == 0 )
            {
                return new List<Rock.Model.Attribute>();
            }

            var entityTypeId = new CareTypeItem().TypeId;

            return new AttributeService( RockContext ).GetByEntityTypeId( entityTypeId, true ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "CareTypeId", StringComparison.OrdinalIgnoreCase ) && a.EntityTypeQualifierValue.Equals( careTypeId.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( CareType entity, ValidPropertiesBox<CareTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override CareType GetInitialEntity()
        {
            return GetInitialEntity<CareType, CareTypeService>( RockContext, PageParameterKey.CareTypeId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out CareType entity, out BlockActionResult error )
        {
            var entityService = new CareTypeService( RockContext );
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
                entity = new CareType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{CareType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${CareType.FriendlyTypeName}." );
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

            return ActionOk( new ValidPropertiesBox<CareTypeBag>
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
        public BlockActionResult Save( ValidPropertiesBox<CareTypeBag> box )
        {
            var entityService = new CareTypeService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var shouldSaveCareItemAttributes = box.ValidProperties != null
                && box.ValidProperties.Contains( nameof( box.Bag.CareItemAttributes ) )
                && box.Bag.CareItemAttributes != null;

            List<PublicEditableAttributeBag> careItemAttributes = null;

            if ( shouldSaveCareItemAttributes )
            {
                if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( "Not authorized to manage Care Item attributes for this Care Type." );
                }

                if ( box.Bag.CareItemAttributes.Any( a => a?.Attribute == null ) )
                {
                    return ActionBadRequest( "Invalid Care Item attribute." );
                }

                careItemAttributes = box.Bag.CareItemAttributes.ConvertAll( a => a.Attribute );

                if ( !AreCareItemAttributeEditsAllowed( careItemAttributes, entity.Id ) )
                {
                    return ActionBadRequest( "Invalid attribute." );
                }

                if ( careItemAttributes.GroupBy( a => a.Key, StringComparer.OrdinalIgnoreCase ).Any( group => group.Count() > 1 ) )
                {
                    return ActionBadRequest( "Care Item attribute keys must be unique." );
                }
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateCareType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                if ( shouldSaveCareItemAttributes )
                {
                    SaveCareItemAttributes( entity.Id, careItemAttributes );
                }

                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.CareTypeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<CareTypeBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the staged CareItem attribute definitions for a CareType.
        /// </summary>
        private void SaveCareItemAttributes( int careTypeId, List<PublicEditableAttributeBag> careItemAttributes )
        {
            var entityTypeId = new CareTypeItem().TypeId;
            var attributeService = new AttributeService( RockContext );
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, "CareTypeId", careTypeId.ToString(), true ).ToList();
            var retainedAttributeGuids = new HashSet<Guid>( careItemAttributes.Where( a => a.Guid.HasValue ).Select( a => a.Guid.Value ) );
            var order = 0;

            foreach ( var attribute in existingAttributes.Where( a => !retainedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attribute );
            }

            RockContext.SaveChanges();

            foreach ( var attributeBag in careItemAttributes )
            {
                var attribute = Rock.Attribute.Helper.SaveAttributeEdits( attributeBag, entityTypeId, "CareTypeId", careTypeId.ToString(), RockContext );

                if ( attribute != null )
                {
                    attribute.Order = order++;
                }
            }

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Ensures posted attribute identifiers are new or already belong to this CareType's CareItem attributes.
        /// </summary>
        private bool AreCareItemAttributeEditsAllowed( List<PublicEditableAttributeBag> careItemAttributes, int careTypeId )
        {
            var attributeGuids = careItemAttributes
                .Where( a => a.Guid.HasValue )
                .Select( a => a.Guid.Value )
                .ToList();

            if ( attributeGuids.Count != attributeGuids.Distinct().Count() )
            {
                return false;
            }

            if ( !attributeGuids.Any() )
            {
                return true;
            }

            var entityTypeId = new CareTypeItem().TypeId;
            var qualifierValue = careTypeId == 0 ? null : careTypeId.ToString();
            var existingAttributes = new AttributeService( RockContext ).Queryable()
                .Where( a => attributeGuids.Contains( a.Guid ) )
                .Select( a => new
                {
                    a.EntityTypeId,
                    a.EntityTypeQualifierColumn,
                    a.EntityTypeQualifierValue
                } )
                .ToList();

            return existingAttributes.All( a =>
                a.EntityTypeId == entityTypeId
                && string.Equals( a.EntityTypeQualifierColumn, "CareTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue == qualifierValue );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new CareTypeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Copies the specified care type and redirects to the new entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to copy.</param>
        /// <returns>The URL for the new care type.</returns>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            var entityService = new CareTypeService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{CareType.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to copy {CareType.FriendlyTypeName}." );
            }

            var newEntityId = entityService.Copy( entity.Id );
            var newEntity = entityService.Get( newEntityId );

            if ( newEntity == null )
            {
                return ActionBadRequest( $"Unable to copy {CareType.FriendlyTypeName}." );
            }

            return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.CareTypeId] = newEntity.IdKey
            } ) );
        }

        #endregion
    }
}
