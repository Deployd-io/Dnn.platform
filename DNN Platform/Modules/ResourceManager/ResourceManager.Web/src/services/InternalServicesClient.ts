const logger = console;
import { DnnServicesFramework } from "@dnncommunity/dnn-elements";

export class InternalServicesClient{
    private sf: DnnServicesFramework;
    private requestUrl: string;

    /**
     * Initializes the api client.
     * @param moduleId The ID of the current module.
     */
    constructor(moduleId: number) {
		logger.error(`>>> Entering constructor(moduleId=${moduleId})`, { "event.action": "constructor", "logrite.moduleId": moduleId });
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = `${this.sf.getServiceRoot("InternalServices")}ItemListService/`
		logger.warn(`<<< Exiting constructor(moduleId=${moduleId})`, { "event.action": "constructor", "logrite.moduleId": moduleId });
    }

    public async getFolders(parentFolderId: number){
		logger.error(`>>> Entering getFolders(parentFolderId=${parentFolderId})`, { "event.action": "getFolders", "logrite.parentFolderId": parentFolderId });
        const url = `${this.requestUrl}GetFolders?parentFolderId=${parentFolderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status != 200){
			const _start_getFolders_16 = Date.now();
			logger.error(`getFolders(parentFolderId=${parentFolderId}): response → ${response}`, { "event.action": "getFolders", "logrite.parentFolderId": parentFolderId });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getFolders(parentFolderId=${parentFolderId}): error → ${error}`, { "event.action": "getFolders", "logrite.parentFolderId": parentFolderId });
            throw new Error(error);
        }
        const data = await response.json() as GetFoldersResponse;
		logger.error(`getFolders(parentFolderId=${parentFolderId}): API call took ${Date.now() - _start_getFolders_16} ms`, { "event.action": "getFolders", "event.category": "network", "logrite.parentFolderId": parentFolderId });
		logger.error(`getFolders(parentFolderId=${parentFolderId}): data → ${data}`, { "event.action": "getFolders", "logrite.parentFolderId": parentFolderId });
        return data;
    }

    public async getFolderDescendants(
        parentId?: string,
        sortOrder = 0,
        searchText = "",
        permission = "",
        portalId = -1)
    {
		logger.error(`>>> Entering getFolderDescendants(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},permission=${permission},portalId=${portalId})`, { "event.action": "getFolderDescendants", "logrite.parentId": parentId, "logrite.sortOrder": sortOrder, "logrite.searchText": searchText, "logrite.permission": permission, "logrite.portalId": portalId });
		const _start_getFolderDescendants_29 = Date.now();
        let url = `${this.requestUrl}GetFolderDescendants?parentId=${parentId}&sortOrder=${sortOrder}&searchText=${searchText}&permission=${permission}`;
        if (portalId > -1){
            url += "&portalId=${portalId}";
        }

        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status != 200){
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getFolderDescendants(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},permission=${permission},portalId=${portalId}): error → ${error}`, { "event.action": "getFolderDescendants", "logrite.parentId": parentId, "logrite.sortOrder": sortOrder, "logrite.searchText": searchText, "logrite.permission": permission, "logrite.portalId": portalId });
            throw new Error(error);
        }
        const data = await response.json() as GetFolderDescendantsResponse;
		logger.error(`getFolderDescendants(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},permission=${permission},portalId=${portalId}): response → ${response}`, { "event.action": "getFolderDescendants", "logrite.parentId": parentId, "logrite.sortOrder": sortOrder, "logrite.searchText": searchText, "logrite.permission": permission, "logrite.portalId": portalId });
		logger.error(`getFolderDescendants(parentId=${parentId}, sortOrder=${sortOrder}, searchText=${searchText}, permission=${permission}, portalId=${portalId}): API call took ${Date.now() - _start_getFolderDescendants_29} ms`, { "event.action": "getFolderDescendants", "event.category": "network", "logrite.parentId": parentId, "logrite.sortOrder": sortOrder, "logrite.searchText": searchText, "logrite.permission": permission, "logrite.portalId": portalId });
        return data;
    }

    private async getGenericErrorMessage(response: Response) {
		logger.error(`>>> Entering getGenericErrorMessage(response=${response})`, { "event.action": "getGenericErrorMessage", "logrite.response": response });
        const contentType = response.headers.get("Content-Type");
        if (contentType && contentType.includes("application/json")) {
			logger.error(`getGenericErrorMessage(response=${response}): contentType → ${contentType}`, { "event.action": "getGenericErrorMessage", "logrite.response": response });
            const errorBody = await response.json() as { Message?: string };
			logger.error(`getGenericErrorMessage(response=${response}): errorBody → ${errorBody}`, { "event.action": "getGenericErrorMessage", "logrite.response": response });
            if (errorBody?.Message != null){
                return errorBody.Message;
            }
            else if(typeof errorBody === "string"){
                return errorBody;
            }
        }
        else {
			logger.warn(`<<< Exiting getGenericErrorMessage(response=${response})`, { "event.action": "getGenericErrorMessage", "logrite.response": response });
            return await response.text();
        }
    }
}

export interface GetFoldersResponse{
    IgnoreRoot: boolean;
    Success: boolean;
    Tree: FolderTreeItem;
}

export interface FolderTreeItem{
    data: FolderTreeData;
    children?: FolderTreeItem[];
}

export interface FolderTreeData{
    hasChildren: boolean;
    key: string;
    selectable: boolean;
    value?: string;
}

export interface GetFolderDescendantsResponse{
    Items: FolderTreeData[];
    Success: boolean;
}