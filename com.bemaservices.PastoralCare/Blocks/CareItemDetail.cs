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
    [DisplayName( "Care Item Detail" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "Displays the details of the given care item for editing." )]
    [IconCssClass( "fa fa-user" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile.", false, Key = AttributeKey.PersonProfilePage, Order = 0 )]
    [BadgesField( "Badges", "The person badges to display in this block.", false, "", "", 1, Key = AttributeKey.Badges )]
    [BooleanField( "Default Request Time to Current Time", "Defaults the Request Date and Time to current date and time.", false, "", 2, Key = AttributeKey.DefaultRequestTimetoCurrentTime )]
    [BooleanField( "Allow Requester Self Selection", "Allows the person filling out the form to select themselves as the requestor.", false, "", 3, Key = AttributeKey.AllowRequesterSelfSelection )]

    [Rock.SystemGuid.EntityTypeGuid( "f426d8bf-5387-452d-b264-0075cd5865d2" )]
    [Rock.SystemGuid.BlockTypeGuid( "c2a7dc2e-81a9-4ce8-86d1-a90f87df36e2" )]
    public class CareItemDetail : RockBlockType
    {
        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
            public const string Badges = "Badges";
            public const string DefaultRequestTimetoCurrentTime = "DefaultRequestTimetoCurrentTime";
            public const string AllowRequesterSelfSelection = "AllowRequesterSelfSelection";
        }

        private static class PageParameterKey
        {
            public const string CareItemId = "CareItemId";
            public const string CareTypeId = "CareTypeId";
            public const string PersonId = "PersonId";
        }

        public override string ObsidianFileUrl => "~/Plugins/com_bemaservices/PastoralCare/careItemDetail.obs";

        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var careItemId = PageParameter( PageParameterKey.CareItemId ).AsIntegerOrNull() ?? 0;
                var careTypeId = PageParameter( PageParameterKey.CareTypeId ).AsIntegerOrNull();
                var personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

                var careTypeOptions = new CareTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .OrderBy( ct => ct.Name )
                    .ToList()
                    .Where( ct => UserCanEdit || ct.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    .Select( ct => new ListItemBag
                    {
                        Value = ct.Id.ToString(),
                        Text = ct.Name
                    } )
                    .ToList();

                var careItem = careItemId > 0
                    ? new CareItemService( rockContext )
                        .Queryable()
                        .Include( ci => ci.PersonAlias.Person )
                        .Include( ci => ci.ContactorPersonAlias.Person )
                        .Include( ci => ci.CareTypeItems.Select( cti => cti.CareType ) )
                        .Include( ci => ci.CareContacts.Select( cc => cc.ContactorPersonAlias.Person ) )
                        .FirstOrDefault( ci => ci.Id == careItemId )
                    : null;

                var defaultDate = GetAttributeValue( AttributeKey.DefaultRequestTimetoCurrentTime ).AsBoolean()
                    ? RockDateTime.Now
                    : (DateTime?) null;

                var bag = careItem != null
                    ? new CareItemBag
                    {
                        Id = careItem.Id,
                        IdKey = careItem.IdKey,
                        PersonId = careItem.PersonAlias?.PersonId,
                        PersonName = careItem.PersonAlias?.Person?.FullName,
                        ContactorId = careItem.ContactorPersonAlias?.PersonId,
                        ContactorName = careItem.ContactorPersonAlias?.Person?.FullName,
                        ContactDateTime = careItem.ContactDateTime,
                        Description = careItem.Description,
                        IsActive = careItem.IsActive,
                        CareTypeIds = careItem.CareTypeItems.Select( cti => cti.CareTypeId ).ToList(),
                        Contacts = BuildContactBags( careItem, careItem.CareContacts.ToList() )
                    }
                    : new CareItemBag
                    {
                        Id = 0,
                        PersonId = personId,
                        ContactDateTime = defaultDate,
                        IsActive = true,
                        CareTypeIds = careTypeId.HasValue ? new List<int> { careTypeId.Value } : new List<int>(),
                        Contacts = new List<CareContactBag>()
                    };

                return new
                {
                    entity = bag,
                    options = new
                    {
                        careTypes = careTypeOptions,
                        allowRequesterSelfSelection = GetAttributeValue( AttributeKey.AllowRequesterSelfSelection ).AsBoolean()
                    },
                    isEditable = true,
                    parentPageUrl = this.GetParentPageUrl()
                };
            }
        }

        [BlockAction]
        public BlockActionResult Save( CareItemBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var careItemService = new CareItemService( rockContext );
                var careTypeItemService = new CareTypeItemService( rockContext );
                var personAliasService = new Rock.Model.PersonAliasService( rockContext );

                CareItem careItem;
                if ( bag.Id <= 0 && bag.IdKey.IsNullOrWhiteSpace() )
                {
                    careItem = new CareItem();
                    careItemService.Add( careItem );
                }
                else
                {
                    careItem = bag.Id > 0
                        ? careItemService.Get( bag.Id )
                        : careItemService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );
                }

                if ( careItem == null )
                {
                    return ActionBadRequest( "Care Item not found." );
                }

                if ( !bag.PersonId.HasValue )
                {
                    return ActionBadRequest( "A person is required." );
                }

                if ( !bag.ContactorId.HasValue )
                {
                    return ActionBadRequest( "A requester is required." );
                }

                var personAliasId = personAliasService.GetPrimaryAliasId( bag.PersonId.Value );
                var contactorAliasId = personAliasService.GetPrimaryAliasId( bag.ContactorId.Value );

                if ( !personAliasId.HasValue || !contactorAliasId.HasValue )
                {
                    return ActionBadRequest( "Could not resolve one or more person aliases." );
                }

                careItem.PersonAliasId = personAliasId.Value;
                careItem.ContactorPersonAliasId = contactorAliasId.Value;
                careItem.ContactDateTime = bag.ContactDateTime ?? RockDateTime.Now;
                careItem.Description = bag.Description?.SanitizeHtml();
                careItem.IsActive = bag.IsActive;

                var selectedCareTypeIds = bag.CareTypeIds ?? new List<int>();
                if ( !selectedCareTypeIds.Any() )
                {
                    return ActionBadRequest( "At least one care type is required." );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    var existingTypeItems = careTypeItemService.Queryable()
                        .Where( cti => cti.CareItemId == careItem.Id )
                        .ToList();

                    foreach ( var existingTypeItem in existingTypeItems.Where( eti => !selectedCareTypeIds.Contains( eti.CareTypeId ) ).ToList() )
                    {
                        careTypeItemService.Delete( existingTypeItem );
                    }

                    foreach ( var careTypeId in selectedCareTypeIds.Distinct() )
                    {
                        if ( !existingTypeItems.Any( eti => eti.CareTypeId == careTypeId ) )
                        {
                            careTypeItemService.Add( new CareTypeItem
                            {
                                CareItemId = careItem.Id,
                                CareTypeId = careTypeId
                            } );
                        }
                    }

                    rockContext.SaveChanges();
                } );

                return ActionOk( new { id = careItem.Id, idKey = careItem.IdKey } );
            }
        }

        [BlockAction]
        public BlockActionResult GetContacts( string careItemKey )
        {
            using ( var rockContext = new RockContext() )
            {
                var careItem = new CareItemService( rockContext )
                    .Queryable()
                    .Include( ci => ci.CareContacts.Select( cc => cc.ContactorPersonAlias.Person ) )
                    .FirstOrDefault( ci => ci.IdKey == careItemKey || ci.Id.ToString() == careItemKey );

                if ( careItem == null )
                {
                    return ActionBadRequest( "Care Item not found." );
                }

                return ActionOk( BuildContactBags( careItem, careItem.CareContacts.ToList() ) );
            }
        }

        [BlockAction]
        public BlockActionResult SaveContact( string careItemKey, CareContactBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var careItemService = new CareItemService( rockContext );
                var contactService = new CareContactService( rockContext );
                var personAliasService = new Rock.Model.PersonAliasService( rockContext );

                var careItem = careItemService.Get( careItemKey, !PageCache.Layout.Site.DisablePredictableIds );
                if ( careItem == null )
                {
                    return ActionBadRequest( "Care Item not found." );
                }

                var isEditAllowed = UserCanEdit || careItem.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

                var contact = bag.Id > 0
                    ? contactService.Get( bag.Id )
                    : new CareContact { CareItemId = careItem.Id };

                if ( contact == null )
                {
                    return ActionBadRequest( "Contact not found." );
                }

                if ( contact.Id > 0 && !CanEditContact( contact, isEditAllowed ) )
                {
                    return ActionBadRequest( "Not authorized to edit this contact." );
                }

                if ( bag.ContactorId.HasValue )
                {
                    var contactorAliasId = personAliasService.GetPrimaryAliasId( bag.ContactorId.Value );
                    if ( contactorAliasId.HasValue )
                    {
                        contact.ContactorPersonAliasId = contactorAliasId.Value;
                    }
                }

                contact.ContactDateTime = bag.ContactDateTime ?? RockDateTime.Now;
                contact.Description = bag.Description?.SanitizeHtml();

                if ( contact.Id == 0 )
                {
                    contactService.Add( contact );
                }

                rockContext.SaveChanges();

                var reloaded = contactService.Queryable()
                    .Include( cc => cc.ContactorPersonAlias.Person )
                    .FirstOrDefault( cc => cc.Id == contact.Id );

                var result = new CareContactBag
                {
                    Id = reloaded.Id,
                    ContactorId = reloaded.ContactorPersonAlias?.PersonId,
                    ContactorName = reloaded.ContactorPersonAlias?.Person?.FullNameReversed,
                    ContactDateTime = reloaded.ContactDateTime,
                    Description = reloaded.Description,
                    CanEdit = CanEditContact( reloaded, isEditAllowed )
                };

                return ActionOk( result );
            }
        }

        [BlockAction]
        public BlockActionResult DeleteContact( string careItemKey, int contactId )
        {
            using ( var rockContext = new RockContext() )
            {
                var contactService = new CareContactService( rockContext );
                var contact = contactService.Get( contactId );
                var careItem = new CareItemService( rockContext ).Get( careItemKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( contact == null )
                {
                    return ActionBadRequest( "Contact not found." );
                }

                if ( careItem == null )
                {
                    return ActionBadRequest( "Care Item not found." );
                }

                var isEditAllowed = UserCanEdit || careItem.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                if ( !CanEditContact( contact, isEditAllowed ) )
                {
                    return ActionBadRequest( "Not authorized to delete this contact." );
                }

                contactService.Delete( contact );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        [BlockAction]
        public BlockActionResult SearchPeople( string search )
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new Rock.Model.PersonService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( p => !p.IsDeceased );

                if ( search.IsNotNullOrWhiteSpace() )
                {
                    qry = qry.Where( p => p.NickName.Contains( search ) || p.LastName.Contains( search ) );
                }

                var result = qry
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

                return ActionOk( result );
            }
        }

        private List<CareContactBag> BuildContactBags( CareItem careItem, List<CareContact> contacts )
        {
            var isEditAllowed = UserCanEdit || careItem.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            return contacts
                .OrderByDescending( cc => cc.ContactDateTime )
                .Select( cc => new CareContactBag
                {
                    Id = cc.Id,
                    ContactorId = cc.ContactorPersonAlias?.PersonId,
                    ContactorName = cc.ContactorPersonAlias?.Person?.FullNameReversed,
                    Description = cc.Description,
                    ContactDateTime = cc.ContactDateTime,
                    CanEdit = CanEditContact( cc, isEditAllowed )
                } )
                .ToList();
        }

        private bool CanEditContact( CareContact contact, bool isEditAllowed )
        {
            if ( isEditAllowed )
            {
                return true;
            }

            var currentPersonId = RequestContext.CurrentPerson?.Id;

            return currentPersonId.HasValue &&
                ( contact.CreatedByPersonAlias?.PersonId == currentPersonId.Value
                    || contact.ContactorPersonAlias?.PersonId == currentPersonId.Value );
        }

        public class CareItemBag
        {
            public int Id { get; set; }
            public string IdKey { get; set; }
            public int? PersonId { get; set; }
            public string PersonName { get; set; }
            public int? ContactorId { get; set; }
            public string ContactorName { get; set; }
            public DateTime? ContactDateTime { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
            public List<int> CareTypeIds { get; set; }
            public List<CareContactBag> Contacts { get; set; }
        }

        public class CareContactBag
        {
            public int Id { get; set; }
            public int? ContactorId { get; set; }
            public string ContactorName { get; set; }
            public DateTime? ContactDateTime { get; set; }
            public string Description { get; set; }
            public bool CanEdit { get; set; }
        }
    }
}
