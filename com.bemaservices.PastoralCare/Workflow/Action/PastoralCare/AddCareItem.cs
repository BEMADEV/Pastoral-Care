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
    [Description("Creates a Care Item.")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Care Item Create")]

    // Reservation Property Fields
    [WorkflowAttribute("Person", "The attribute that contains the person the care item is for.",
        false, "", "", 0, null, new string[] { "rock.Field.Types.PersonFieldType" })]

    [WorkflowAttribute("Care Types", "The attribute that contains the care types the care item is for.",
        false, "", "", 1, null, new string[] { "com.bemaservices.PastoralCare.Field.Types.CareTypesFieldType" })]


    [WorkflowAttribute("Requester", "The attribute that contains the requester of the care item.",
        false, "", "", 0, null, new string[] { "rock.Field.Types.PersonFieldType" })]

    [WorkflowTextOrAttribute("Request Date", "Attribute Value", "The request date or an attribute that contains the request date of the care item. <span class='tip tip-lava'></span>",
        false, "", "", 0, "RequestDate", new string[] { "Rock.Field.Types.DateFieldType" })]

    [WorkflowTextOrAttribute("Description", "Attribute Value", "The description or an attribute that contains the description of the care item. <span class='tip tip-lava'></span>",
        false, "", "", 0, "Description", new string[] { "Rock.Field.Types.TextFieldType" })]

    // New Reservation Attribute
    [WorkflowAttribute("Care Item", "The care item attribute to set the value to the care item created.", true, "", "", 6, null,
        new string[] { "com.bemaservices.PastoralCare.Field.Types.CareItemFieldType" })]

    public class CreateCareItem : ActionComponent
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
            var attribute = AttributeCache.Get(GetAttributeValue(action, "CareItem").AsGuid(), rockContext);
            if (attribute != null)
            {
                var mergeFields = GetMergeFields(action);

                // Get Description
                string description = GetAttributeValue(action, "Description", true).ResolveMergeFields(mergeFields);

                // Get Request Date
                DateTime? requestDate = GetAttributeValue(action, "RequestDate", true).ResolveMergeFields(mergeFields).AsDateTime();

                if (requestDate == null)
                {
                    errorMessages.Add("Invalid Request Date Value!");
                    return false;
                }

                // Get care types
                List<CareType> careTypeList = new List<CareType>();
                var careTypeService = new CareTypeService(rockContext);
                Guid? careTypeAttributeGuid = GetAttributeValue(action, "CareTypes").AsGuidOrNull();
                if (careTypeAttributeGuid.HasValue)
                {
                    careTypeList = careTypeService.GetByGuids(action.GetWorkflowAttributeValue(careTypeAttributeGuid.Value).SplitDelimitedValues().AsGuidList()).ToList();
                }

                if (!careTypeList.Any())
                {
                    errorMessages.Add("Invalid Care Type Attribute!");
                    return false;
                }

                // Get care item person 
                PersonAlias personAlias = null;
                Guid? personAttributeGuid = GetAttributeValue(action, "Person").AsGuidOrNull();
                if (personAttributeGuid.HasValue)
                {
                    personAlias = new PersonAliasService(rockContext).Get(action.GetWorkflowAttributeValue(personAttributeGuid.Value).AsGuid());
                }

                if (personAlias == null)
                {
                    errorMessages.Add("Invalid Person Attribute!");
                    return false;
                }

                // Get care item requester 
                PersonAlias requesterAlias = null;
                Guid? requesterAttributeGuid = GetAttributeValue(action, "Requester").AsGuidOrNull();
                if (requesterAttributeGuid.HasValue)
                {
                    requesterAlias = new PersonAliasService(rockContext).Get(action.GetWorkflowAttributeValue(requesterAttributeGuid.Value).AsGuid());
                }

                if (requesterAlias == null)
                {
                    errorMessages.Add("Invalid Requester Attribute!");
                    return false;
                }

                var careItemService = new CareItemService(rockContext);
                var careTypeItemService = new CareTypeItemService(rockContext);

                var careItem = new CareItem { Id = 0 };
                careItem.IsActive = true;
                careItem.PersonAlias = personAlias;
                careItem.ContactorPersonAlias = requesterAlias;
                careItem.ContactDateTime = requestDate.Value;
                careItem.Description = description;
                careItemService.Add(careItem);
                rockContext.SaveChanges();


                careItem.CareTypeItems = new List<CareTypeItem>();
                foreach( var careType in careTypeList)
                {
                    var careTypeItem = new CareTypeItem();
                    careTypeItem.CareType = careType;
                    careTypeItem.CareItemId = careItem.Id;
                    careTypeItemService.Add(careTypeItem);
                }

                rockContext.SaveChanges();

                if (careItem != null)
                {
                    SetWorkflowAttributeValue(action, attribute.Guid, careItem.Guid.ToString());
                    action.AddLogEntry(string.Format("Set '{0}' attribute to '{1}'.", attribute.Name, careItem.Name));
                    return true;
                }
                else
                {
                    errorMessages.Add("Care Item could not be determined!");
                }
            }
            else
            {
                errorMessages.Add("Care Item Attribute could not be found!");
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