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
		logger.debug(`>>> Entering constructor(moduleId=${moduleId})`);
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = `${this.sf.getServiceRoot("InternalServices")}ItemListService/`
		logger.debug(`<<< Exiting constructor(moduleId=${moduleId})`);
    }

    public async getFolders(parentFolderId: number){
		logger.debug(`>>> Entering getFolders(parentFolderId=${parentFolderId})`); const _start = Date.now();
        const url = `${this.requestUrl}GetFolders?parentFolderId=${parentFolderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status != 200){
			logger.debug(`getFolders(parentFolderId=${parentFolderId}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getFolders(parentFolderId=${parentFolderId}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as GetFoldersResponse;
		logger.info(`getFolders(parentFolderId=${parentFolderId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`getFolders(parentFolderId=${parentFolderId}): data → ${data}`);
		logger.debug(`<<< Exiting getFolders(parentFolderId=${parentFolderId})`);
        return data;
    }

    public async getFolderDescendants(
        parentId?: string,
        sortOrder = 0,
        searchText = "",
        permission = "",
        portalId = -1)
    {
        let url = `${this.requestUrl}GetFolderDescendants?parentId=${parentId}&sortOrder=${sortOrder}&searchText=${searchText}&permission=${permission}`;
        if (portalId > -1){
			logger.debug(`>>> Entering getFolderDescendants(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},permission=${permission},portalId=${portalId})`);
			logger.debug(`<<< Exiting getFolderDescendants(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},permission=${permission},portalId=${portalId})`);
            url += "&portalId=${portalId}";
        }

        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status != 200){
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getFolderDescendants(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},permission=${permission},portalId=${portalId}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as GetFolderDescendantsResponse;
		logger.debug(`getFolderDescendants(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},permission=${permission},portalId=${portalId}): response → ${response}`);
		logger.info(`getFolderDescendants(parentId=${parentId}, sortOrder=${sortOrder}, searchText=${searchText}, permission=${permission}, portalId=${portalId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`getFolderDescendants(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},permission=${permission},portalId=${portalId}): data → ${data}`);
        return data;
    }

    private async getGenericErrorMessage(response: Response) {
		logger.debug(`>>> Entering getGenericErrorMessage(response=${response})`);
        const contentType = response.headers.get("Content-Type");
        if (contentType && contentType.includes("application/json")) {
			logger.debug(`getGenericErrorMessage(response=${response}): contentType → ${contentType}`);
            const errorBody = await response.json() as { Message?: string };
			logger.debug(`getGenericErrorMessage(response=${response}): errorBody → ${errorBody}`);
            if (errorBody?.Message != null){
                return errorBody.Message;
            }
            else if(typeof errorBody === "string"){
                return errorBody;
            }
        }
        else {
			logger.debug(`<<< Exiting getGenericErrorMessage(response=${response})`);
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