import { languageEditor as ActionTypes } from "../constants/actionTypes";
import utilities from "utils";
const logger = console;

function insertRecords(folders, newFolders, clearArray) {
	logger.debug(`>>> Entering insertRecords(folders=${folders},newFolders=${newFolders},clearArray=${clearArray})`);
    let newFolderList = !clearArray ? utilities.utilities.getObjectCopy(folders) : [];
    newFolders.forEach((folder) => {
        let alreadyThere = folders.find((_folder) => {
            return _folder.NewValue === folder.NewValue;
        });
        if (!alreadyThere || clearArray) {
            newFolderList.push(folder);
        }
    });
	logger.debug(`insertRecords(folders=${folders},newFolders=${newFolders},clearArray=${clearArray}): newFolderList → ${newFolderList}`);
	logger.debug(`<<< Exiting insertRecords(folders=${folders},newFolders=${newFolders},clearArray=${clearArray})`);
    return newFolderList;
}

export default function languageEditor(state = {
    languageBeingEdited: {},
    languageFiles: [],
    languageFolders: [],
    translations: [],
    resxBeingEdited: "",
    resxBeingEditedDisplay: "",
    resxFormIsDirty: false
}, action) {
    switch (action.type) {
        case ActionTypes.SET_LANGUAGE_BEING_EDITED:
            return {...state,
                languageBeingEdited: action.payload
            };
        case ActionTypes.RETRIEVED_ROOT_RESOURCES_FOLDER:
            return {...state,
                languageFolders: insertRecords(state.languageFolders, action.payload.Folders, true),
                resxBeingEdited: "",
                resxBeingEditedDisplay: "",
                translations: []
            };
        case ActionTypes.RETRIEVED_SUBROOT_RESOURCES_FOLDER:
            return {...state,
                languageFolders: insertRecords(state.languageFolders, action.payload.Folders),
                languageFiles: insertRecords(state.languageFiles, action.payload.Files)
            };
        case ActionTypes.RETRIEVED_RESX_ENTRIES:
            return {...state,
                translations: action.payload.Translations,
                resxBeingEdited: action.payload.resourceFileActual,
                resxBeingEditedDisplay: action.payload.File
            };
        case ActionTypes.UPDATED_RESX_ENTRIES:
            return {...state,
                translations: action.payload
            };
        default:
            return {...state
            };
    }
}