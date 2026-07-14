using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Security;

using com.bemaservices.PastoralCare.Model;

namespace com.bemaservices.PastoralCare.Blocks
{
    [DisplayName( "Care Type Detail" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "Displays the details of the given Care Type for editing." )]
    [IconCssClass( "fa fa-pencil" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "1a87c29e-bf65-4f7d-a39d-2cfdfed57fce" )]
    [Rock.SystemGuid.BlockTypeGuid( "7b44cb2c-67d2-4ad0-9cc5-975692cb6802" )]
    public class CareTypeDetail : RockBlockType
    {
        private static class PageParameterKey
        {
            public const string CareTypeId = "CareTypeId";
        }

        public override string ObsidianFileUrl => "~/Plugins/com_bemaservices/PastoralCare/careTypeDetail.obs";

        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var careType = GetCareTypeFromPage( rockContext ) ?? new CareType { IsActive = true };
                var canEdit = careType.Id == 0
                    ? UserCanAdministrate
                    : UserCanAdministrate || careType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

                var attributes = careType.Id > 0
                    ? GetCareTypeAttributes( careType.Id, rockContext )
                    : new List<CareTypeAttributeBag>();

                return new
                {
                    entity = new
                    {
                        id = careType.Id,
                        idKey = careType.IdKey,
                        name = careType.Name,
                        description = careType.Description,
                        isActive = careType.IsActive
                    },
                    attributes,
                    fieldTypes = FieldTypeCache.All()
                        .OrderBy( ft => ft.Name )
                        .Select( ft => new
                        {
                            value = ft.Id,
                            text = ft.Name
                        } )
                        .ToList(),
                    isEditable = canEdit,
                    parentPageUrl = this.GetParentPageUrl()
                };
            }
        }

        [BlockAction]
        public BlockActionResult Save( CareTypeBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new CareTypeService( rockContext );
                CareType careType;

                if ( bag.Id <= 0 && bag.IdKey.IsNullOrWhiteSpace() )
                {
                    if ( !UserCanAdministrate )
                    {
                        return ActionBadRequest( "Not authorized to add care types." );
                    }

                    careType = new CareType();
                    service.Add( careType );
                }
                else
                {
                    careType = bag.Id > 0
                        ? service.Get( bag.Id )
                        : service.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );

                    if ( careType == null )
                    {
                        return ActionBadRequest( "Care Type not found." );
                    }

                    if ( !( UserCanAdministrate || careType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) ) )
                    {
                        return ActionBadRequest( "Not authorized to edit this care type." );
                    }
                }

                careType.Name = bag.Name;
                careType.Description = bag.Description;
                careType.IsActive = bag.IsActive;

                if ( !careType.IsValid )
                {
                    return ActionBadRequest( careType.ValidationResults.Select( v => v.ErrorMessage ).FirstOrDefault() ?? "Care Type is not valid." );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    if ( !careType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        careType.AllowPerson( Authorization.VIEW, RequestContext.CurrentPerson, rockContext );
                    }

                    if ( !careType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        careType.AllowPerson( Authorization.EDIT, RequestContext.CurrentPerson, rockContext );
                    }

                    if ( !careType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                    {
                        careType.AllowPerson( Authorization.ADMINISTRATE, RequestContext.CurrentPerson, rockContext );
                    }
                } );

                return ActionOk( new { id = careType.Id, idKey = careType.IdKey } );
            }
        }

        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new CareTypeService( rockContext );
                var careType = service.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( careType == null )
                {
                    return ActionBadRequest( "Care Type not found." );
                }

                if ( !( UserCanAdministrate || careType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) ) )
                {
                    return ActionBadRequest( "Not authorized to delete this care type." );
                }

                if ( !service.CanDelete( careType, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                service.Delete( careType );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new CareTypeService( rockContext );
                var careType = service.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( careType == null )
                {
                    return ActionBadRequest( "Care Type not found." );
                }

                if ( !( UserCanAdministrate || careType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) ) )
                {
                    return ActionBadRequest( "Not authorized to copy this care type." );
                }

                var copiedId = service.Copy( careType.Id );
                var url = this.LinkedPageUrl( PageCache.Guid, new Dictionary<string, string>
                {
                    [PageParameterKey.CareTypeId] = copiedId.ToString()
                } );

                return ActionOk( url );
            }
        }

        [BlockAction]
        public BlockActionResult SaveAttribute( string careTypeKey, CareTypeAttributeBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var careTypeService = new CareTypeService( rockContext );
                var careType = careTypeService.Get( careTypeKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( careType == null )
                {
                    return ActionBadRequest( "Care Type not found." );
                }

                if ( !( UserCanAdministrate || careType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) ) )
                {
                    return ActionBadRequest( "Not authorized to edit care type attributes." );
                }

                var attributeService = new AttributeService( rockContext );
                var attribute = bag.Guid.AsGuidOrNull().HasValue
                    ? attributeService.Get( bag.Guid.AsGuid() )
                    : new Rock.Model.Attribute
                    {
                        EntityTypeId = new CareTypeItem().TypeId,
                        EntityTypeQualifierColumn = "CareTypeId",
                        EntityTypeQualifierValue = careType.Id.ToString()
                    };

                if ( attribute == null )
                {
                    return ActionBadRequest( "Attribute not found." );
                }

                attribute.Name = bag.Name;
                attribute.Key = bag.Key;
                attribute.Description = bag.Description;
                attribute.FieldTypeId = bag.FieldTypeId;
                attribute.IsRequired = bag.IsRequired;
                attribute.IsGridColumn = bag.IsGridColumn;
                attribute.AllowSearch = bag.AllowSearch;
                attribute.Order = bag.Order;

                if ( !attribute.IsValid )
                {
                    return ActionBadRequest( attribute.ValidationResults.Select( v => v.ErrorMessage ).FirstOrDefault() ?? "Attribute is not valid." );
                }

                if ( attribute.Id == 0 )
                {
                    attributeService.Add( attribute );
                }

                rockContext.SaveChanges();

                return ActionOk( GetCareTypeAttributes( careType.Id, rockContext ) );
            }
        }

        [BlockAction]
        public BlockActionResult DeleteAttribute( string careTypeKey, string attributeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var careTypeService = new CareTypeService( rockContext );
                var careType = careTypeService.Get( careTypeKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( careType == null )
                {
                    return ActionBadRequest( "Care Type not found." );
                }

                if ( !( UserCanAdministrate || careType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) ) )
                {
                    return ActionBadRequest( "Not authorized to edit care type attributes." );
                }

                var attribute = new AttributeService( rockContext ).Get( attributeGuid.AsGuid() );
                if ( attribute == null )
                {
                    return ActionBadRequest( "Attribute not found." );
                }

                if ( attribute.EntityTypeQualifierColumn != "CareTypeId" || attribute.EntityTypeQualifierValue != careType.Id.ToString() )
                {
                    return ActionBadRequest( "Attribute does not belong to this care type." );
                }

                new AttributeService( rockContext ).Delete( attribute );
                rockContext.SaveChanges();

                return ActionOk( GetCareTypeAttributes( careType.Id, rockContext ) );
            }
        }

        private List<CareTypeAttributeBag> GetCareTypeAttributes( int careTypeId, RockContext rockContext )
        {
            return new AttributeService( rockContext )
                .GetByEntityTypeId( new CareTypeItem().TypeId, true )
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "CareTypeId", System.StringComparison.OrdinalIgnoreCase )
                    && a.EntityTypeQualifierValue == careTypeId.ToString() )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new CareTypeAttributeBag
                {
                    Guid = a.Guid.ToString(),
                    Name = a.Name,
                    Key = a.Key,
                    Description = a.Description,
                    FieldTypeId = a.FieldTypeId,
                    FieldTypeName = FieldTypeCache.Get( a.FieldTypeId ) != null ? FieldTypeCache.Get( a.FieldTypeId ).Name : string.Empty,
                    IsRequired = a.IsRequired,
                    IsGridColumn = a.IsGridColumn,
                    AllowSearch = a.AllowSearch,
                    Order = a.Order
                } )
                .ToList();
        }

        private CareType GetCareTypeFromPage( RockContext rockContext )
        {
            var careTypeId = PageParameter( PageParameterKey.CareTypeId ).AsIntegerOrNull();
            if ( !careTypeId.HasValue || careTypeId.Value <= 0 )
            {
                return null;
            }

            return new CareTypeService( rockContext ).Get( careTypeId.Value );
        }

        public class CareTypeBag
        {
            public int Id { get; set; }
            public string IdKey { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
        }

        public class CareTypeAttributeBag
        {
            public string Guid { get; set; }
            public string Name { get; set; }
            public string Key { get; set; }
            public string Description { get; set; }
            public int FieldTypeId { get; set; }
            public string FieldTypeName { get; set; }
            public bool IsRequired { get; set; }
            public bool IsGridColumn { get; set; }
            public bool AllowSearch { get; set; }
            public int Order { get; set; }
        }
    }
}
