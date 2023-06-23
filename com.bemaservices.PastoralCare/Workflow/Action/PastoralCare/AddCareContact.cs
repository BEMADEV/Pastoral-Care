// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using com.bemaservices.PastoralCare.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.PastoralCare.Workflow.Actions.PastoralCare
{
    /// <summary>
    /// Creates a reservation.
    /// </summary>
    [ActionCategory("Pastoral Care")]
    [Description("Creates a Care Contact.")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Care Contact Create")]

    // Reservation Property Fields
    [WorkflowAttribute("Care Item", "The care item attribute to add the care contact to.", true, "", "", 6, null,
        new string[] { "com.bemaservices.PastoralCare.Field.Types.CareItemFieldType" })]

    [WorkflowAttribute("Contactor", "The attribute that contains the contactor.",
        false, "", "", 0, null, new string[] { "rock.Field.Types.PersonFieldType" })]

    [WorkflowTextOrAttribute("Contact Date", "Attribute Value", "The contact date or an attribute that contains the contact date of the care item. <span class='tip tip-lava'></span>",
        false, "", "", 0, "ContactDate", new string[] { "Rock.Field.Types.DateFieldType" })]

    [WorkflowTextOrAttribute("Description", "Attribute Value", "The description or an attribute that contains the description of the care contact. <span class='tip tip-lava'></span>",
        false, "", "", 0, "Description", new string[] { "Rock.Field.Types.TextFieldType" })]

    public class AddCareContact : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var careItemService = new CareItemService(rockContext);

            var mergeFields = GetMergeFields(action);

            // Get Description
            string description = GetAttributeValue(action, "Description", true).ResolveMergeFields(mergeFields);

            // Get Name
            DateTime? contactDate = GetAttributeValue(action, "ContactDate", true).ResolveMergeFields(mergeFields).AsDateTime();

            if (contactDate == null)
            {
                errorMessages.Add("Invalid Contact Date Value!");
                return false;
            }

            // Get care item
            CareItem careItem = null;
            Guid careItemGuid = action.GetWorkflowAttributeValue(GetAttributeValue(action, "CareItem").AsGuid()).AsGuid();
            careItem = careItemService.Get(careItemGuid);
            if (careItem == null)
            {
                errorMessages.Add("Invalid Care Item Attribute!");
                return false;
            }

            // Get care contact contactor 
            PersonAlias personAlias = null;
            Guid? personAttributeGuid = GetAttributeValue(action, "Contactor").AsGuidOrNull();
            if (personAttributeGuid.HasValue)
            {
                personAlias = new PersonAliasService(rockContext).Get(action.GetWorkflowAttributeValue(personAttributeGuid.Value).AsGuid());
            }

            if (personAlias == null)
            {
                errorMessages.Add("Invalid Person Attribute!");
                return false;
            }

            var careContact = new CareContact();
            careContact.ContactorPersonAlias = personAlias;
            careContact.ContactDateTime = contactDate.Value;
            careContact.CareItemId = careItem.Id;
            careContact.Description = description;
            careItem.CareContacts.Add(careContact);
            rockContext.SaveChanges();

            if (careContact == null)
            {
                errorMessages.Add("Care Contact could not be created!");
            }

            if (errorMessages.Any())
            {
                errorMessages.ForEach(m => action.AddLogEntry(m, true));
                return false;
            }

            return true;
        }
    }
}