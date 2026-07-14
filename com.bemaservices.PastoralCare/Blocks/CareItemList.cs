using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Security;
using Rock.ViewModels.Utility;

using com.bemaservices.PastoralCare.Model;

namespace com.bemaservices.PastoralCare.Blocks
{
    [DisplayName( "Care Item List" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "Block to display the care items." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [ContextAware]
    [LinkedPage( "Configuration Page", "Page used to modify and create care types.", false, Key = AttributeKey.ConfigurationPage, Order = 0 )]
    [LinkedPage( "Detail Page", "Page used to view details of a care item.", true, Key = AttributeKey.DetailPage, Order = 1 )]
    [BooleanField( "Hide Inactive Requests By Default", Key = AttributeKey.HideInactiveRequestsByDefault, Description = "If this is enabled, inactive requests will be hidden from the list by default.", DefaultBooleanValue = false, Order = 2 )]

    [Rock.SystemGuid.EntityTypeGuid( "f2b5f793-45f6-46bd-a28c-d66b4f8cabbe" )]
    [Rock.SystemGuid.BlockTypeGuid( "a7a6ffab-fd70-4bdb-8948-cae86727be7a" )]
    public class CareItemList : RockBlockType
    {
        private static class AttributeKey
        {
            public const string ConfigurationPage = "ConfigurationPage";
            public const string DetailPage = "DetailPage";
            public const string HideInactiveRequestsByDefault = "HideInactiveRequestsByDefault";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string ConfigurationPage = "ConfigurationPage";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
            public const string CareTypeId = "CareTypeId";
            public const string CareItemId = "CareItemId";
        }

        public override string ObsidianFileUrl => "~/Plugins/com_bemaservices/PastoralCare/careItemList.obs";

        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var allCareTypes = new CareTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .OrderBy( ct => ct.Name )
                    .ToList();

                var typeOptions = allCareTypes
                    .Where( ct => UserCanEdit || ct.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .Select( ct => new
                    {
                        id = ct.Id,
                        idKey = ct.IdKey,
                        name = ct.Name,
                        isActive = ct.IsActive,
                        canEdit = UserCanEdit || ct.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                    } )
                    .ToList();

                return new
                {
                    options = new
                    {
                        careTypes = typeOptions,
                        defaultCareTypeId = typeOptions.FirstOrDefault()?.id,
                        defaultStatus = GetAttributeValue( AttributeKey.HideInactiveRequestsByDefault ).AsBoolean() ? "Active" : string.Empty,
                        personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull()
                    },
                    navigationUrls = new Dictionary<string, string>
                    {
                        [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.CareItemId, "((CareItemId))", PageParameterKey.CareTypeId, "((CareTypeId))", PageParameterKey.PersonId, "((PersonId))" ),
                        [NavigationUrlKey.ConfigurationPage] = this.GetLinkedPageUrl( AttributeKey.ConfigurationPage )
                    }
                };
            }
        }

        [BlockAction]
        public BlockActionResult GetGridData( int? careTypeId, int? personId, int? contactorId, string status )
        {
            if ( !careTypeId.HasValue || careTypeId.Value <= 0 )
            {
                return ActionOk( new List<object>() );
            }

            using ( var rockContext = new RockContext() )
            {
                var qry = new CareTypeItemService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( cti => cti.CareType )
                    .Include( cti => cti.CareItem )
                    .Include( cti => cti.CareItem.PersonAlias.Person )
                    .Include( cti => cti.CareItem.ContactorPersonAlias.Person )
                    .Include( cti => cti.CareItem.CareContacts.Select( cc => cc.ContactorPersonAlias.Person ) )
                    .Where( cti => cti.CareTypeId == careTypeId.Value );

                var contextPersonId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
                if ( contextPersonId.HasValue )
                {
                    qry = qry.Where( cti => cti.CareItem.PersonAlias.PersonId == contextPersonId.Value );
                }

                if ( personId.HasValue && personId.Value > 0 )
                {
                    qry = qry.Where( cti => cti.CareItem.PersonAlias.PersonId == personId.Value );
                }

                if ( contactorId.HasValue && contactorId.Value > 0 )
                {
                    qry = qry.Where( cti => cti.CareItem.ContactorPersonAlias.PersonId == contactorId.Value );
                }

                if ( status == "Active" )
                {
                    qry = qry.Where( cti => cti.CareItem.IsActive );
                }
                else if ( status == "Inactive" )
                {
                    qry = qry.Where( cti => !cti.CareItem.IsActive );
                }

                var rows = qry
                    .OrderByDescending( cti => cti.CareItem.ContactDateTime )
                    .Take( 500 )
                    .ToList()
                    .Select( cti =>
                    {
                        var latestContact = cti.CareItem.CareContacts
                            .OrderByDescending( cc => cc.ContactDateTime )
                            .FirstOrDefault();

                        return (object) new
                        {
                            id = cti.CareItem.Id,
                            idKey = cti.CareItem.IdKey,
                            careTypeId = cti.CareTypeId,
                            requestDate = cti.CareItem.ContactDateTime,
                            personName = cti.CareItem.PersonAlias?.Person?.FullNameReversed ?? string.Empty,
                            requesterName = cti.CareItem.ContactorPersonAlias?.Person?.FullNameReversed ?? string.Empty,
                            isActive = cti.CareItem.IsActive,
                            lastContactDate = latestContact?.ContactDateTime,
                            lastContactor = latestContact?.ContactorPersonAlias?.Person?.FullNameReversed ?? string.Empty,
                            lastContact = latestContact?.Description ?? string.Empty
                        };
                    } )
                    .ToList();

                return ActionOk( rows );
            }
        }

        [BlockAction]
        public BlockActionResult GetPeople( string search )
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new Rock.Model.PersonService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( p => p.IsDeceased == false );

                if ( search.IsNotNullOrWhiteSpace() )
                {
                    qry = qry.Where( p => p.NickName.Contains( search ) || p.LastName.Contains( search ) );
                }

                var items = qry
                    .OrderBy( p => p.LastName )
                    .ThenBy( p => p.NickName )
                    .Take( 25 )
                    .ToList()
                    .Select( p => new ListItemBag
                    {
                        Value = p.Id.ToString(),
                        Text = p.FullNameReversed
                    } )
                    .ToList();

                return ActionOk( items );
            }
        }
    }
}
