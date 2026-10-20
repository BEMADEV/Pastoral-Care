// This code was auto-generated, any manual changes made will be lost.

import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { PublicEditableAttributeBag } from "@Obsidian/ViewModels/Utility/publicEditableAttributeBag";

export type CareItemBag = {
    attributes?: Record<string, PublicAttributeBag> | null;

    attributeValues?: Record<string, string> | null;

    careTypeNames?: string[] | null;

    contactDateTime?: string | null;

    contactorPersonAlias?: ListItemBag | null;

    description?: string | null;

    idKey?: string | null;

    isActive: boolean;

    lastCareContact?: ListItemBag | null;

    personAlias?: ListItemBag | null;
};

export type CareItemDetailOptionsBag = {
    defaultRequestTimeToCurrentTime?: boolean | null;
};

export type CareTypeBag = {
    attributes?: Record<string, PublicAttributeBag> | null;

    attributeValues?: Record<string, string> | null;

    canManageCareItemAttributes: boolean;

    careItemAttributes?: CareItemAttributeBag[] | null;

    description?: string | null;

    idKey?: string | null;

    isActive: boolean;

    name?: string | null;
};

export type CareItemAttributeBag = {
    attribute?: PublicEditableAttributeBag | null;

    fieldType?: string | null;
};

export type CareTypeDetailOptionsBag = {
};

export type CareTypeItemListOptionsBag = {
};

export type CareTypeListOptionsBag = {
    allowSharedCareContactAttributes?: boolean | null;

    allowSharedCareItemAttributes?: boolean | null;
};
