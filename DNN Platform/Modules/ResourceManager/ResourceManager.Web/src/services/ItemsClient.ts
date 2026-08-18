const logger = console;
import { DnnServicesFramework } from "@dnncommunity/dnn-elements";
import { IPermissions } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/permissions-interface";
import { IRoleGroup } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-group-interface";
import { IRole } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-interface";
import { ISearchedUser } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/searched-user-interface";
import { SortFieldInfo } from "../enums/SortField";
import { SortOrderInfo } from "../enums/SortOrder";

export class ItemsClient{
    private sf: DnnServicesFramework;
    private requestUrl: string;
    private abortController: AbortController;

    /**
     * Initializes the api client.
     * @param moduleId The ID of the current module.
     */
    constructor(moduleId: number) {
		logger.debug(`>>> Entering constructor(moduleId=${moduleId})`);
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = `${this.sf.getServiceRoot("ResourceManager")}Items/`
		logger.debug(`<<< Exiting constructor(moduleId=${moduleId})`);
    }

    public async getSettings() {
		logger.debug('>>> Entering getSettings()'); const _start = Date.now();
        const url =`${this.requestUrl}GetSettings`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`getSettings(): headers → ${headers}`);
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, { headers });
		logger.debug(`getSettings(): response → ${response}`);
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getSettings(): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as GetSettingsResponse;
		logger.info(`getSettings(): API call took ${Date.now() - _start} ms`);
		logger.debug(`getSettings(): data → ${data}`);
		logger.debug('<<< Exiting getSettings()');
        return data;
    }

    /**
     * Gets a specific folder contents.
     * @param folderId Gets the content of a folder.
     * @param startIndex Which item to start at in the paging mechanism.
     * @param numItems How many items to return.
     * @param sorting How to sort the items returned.
     * @returns
     */
    public async getFolderContent(
        folderId: number,
        startIndex = 0,
        numItems = 20,
        sortingField: SortFieldInfo = new SortFieldInfo("ItemName"),
        sortingOrder: SortOrderInfo = new SortOrderInfo()
    ) {
		logger.debug(`>>> Entering getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder})`);
		const _start = Date.now();
        const url = `${this.requestUrl}GetFolderContent?folderId=${folderId}&startIndex=${startIndex}&numItems=${numItems}&sorting=${sortingField.sortKey}&sortingOrder=${sortingOrder.sortOrderKey}`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder}): headers → ${headers}`);
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
			logger.debug(`getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as GetFolderContentResponse;
		logger.info(`getFolderContent(folderId=${folderId}, startIndex=${startIndex}, numItems=${numItems}, sortingField=${sortingField}, sortingOrder=${sortingOrder}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder})`);
		logger.debug(`getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder}): data → ${data}`);
        return data;
    }

    public async getFolderIconUrl(folderId: number) {
		logger.debug(`>>> Entering getFolderIconUrl(folderId=${folderId})`); const _start = Date.now();
        const url = `${this.requestUrl}GetFolderIconUrl?folderId=${folderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			logger.debug(`getFolderIconUrl(folderId=${folderId}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getFolderIconUrl(folderId=${folderId}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as GetFolderIconUrlResponse;
		logger.info(`getFolderIconUrl(folderId=${folderId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`getFolderIconUrl(folderId=${folderId}): data → ${data}`);
		logger.debug(`<<< Exiting getFolderIconUrl(folderId=${folderId})`);
        return data.url;
    }

    public async getAllowedFileExtensions() {
		logger.debug('>>> Entering getAllowedFileExtensions()'); const _start = Date.now();
        const url = `${this.requestUrl}GetAllowedFileExtensions`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			logger.debug(`getAllowedFileExtensions(): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getAllowedFileExtensions(): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as GetAllowedFileExtensionsResponse;
		logger.info(`getAllowedFileExtensions(): API call took ${Date.now() - _start} ms`);
		logger.debug(`getAllowedFileExtensions(): data → ${data}`);
		logger.debug('<<< Exiting getAllowedFileExtensions()');
        return data;
    }

    /**
     * Syncs the folder content.
     * @param folderId The folder id.
     * @param numItems The number of items.
     * @param sorting The sorting.
     * @param recursive If true sync recursively.
     * @returns
     */
     public async syncFolderContent(
        folderId: number,
        numItems = 20,
        sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
        recursive = false){
		logger.debug(`>>> Entering syncFolderContent(folderId=${folderId},numItems=${numItems},sorting=${sorting},recursive=${recursive})`);
        const url = `${this.requestUrl}SyncFolderContent?folderId=${folderId}&numItems=${numItems}&sorting=${sorting.sortKey}&recursive=${recursive}`;
        const headers = this.sf.getModuleHeaders();
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
		const _start = Date.now();
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`syncFolderContent(folderId=${folderId},numItems=${numItems},sorting=${sorting},recursive=${recursive}): headers → ${headers}`);
            throw new Error(error);
        }
        const data = await response.json() as GetFolderContentResponse;
		logger.debug(`<<< Exiting syncFolderContent(folderId=${folderId},numItems=${numItems},sorting=${sorting},recursive=${recursive})`);
		logger.info(`syncFolderContent(folderId=${folderId}, numItems=${numItems}, sorting=${sorting}, recursive=${recursive}): API call took ${Date.now() - _start} ms`);
        return data;
    }

    /**
     * Downloads a file.
     * @param fileId The id of the file to download.
     * @param forceDownload A value indicating whether to force the download.
     *                      When true, will download the file as an attachment and ensures the browser won't just render the file if supported.
     *                      When false, the browser may render the file instead of downloading it for some formats like pdf or images.
     * @returns The actual requested file.
     */
     public async download(
        fileId: number,
        forceDownload = true
    ){
		logger.debug(`>>> Entering download(fileId=${fileId},forceDownload=${forceDownload})`);
		const _start = Date.now();
        const url = `${this.requestUrl}Download?fileId=${fileId}&forceDownload=${forceDownload}`;
        const headers = this.sf.getModuleHeaders();
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
			logger.info(`download(fileId=${fileId}, forceDownload=${forceDownload}): API call took ${Date.now() - _start} ms`);
			logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): headers → ${headers}`);
			logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }

        let filename = response.headers.get("Content-Disposition").split("filename=")[1];
        filename = this.decodeRFC5987ContentDisposition(filename);
		logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): filename → ${filename}`);

        const blob = await response.blob()        
        const oldDownloadLink = document.querySelector("#downloadLink");
		logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): blob → ${blob}`);
		logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): oldDownloadLink → ${oldDownloadLink}`);
        if (oldDownloadLink !== null) {
            oldDownloadLink.remove();
        }
        const downloadLink = document.createElement('a');
		logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): downloadLink → ${downloadLink}`);
        downloadLink.id = "downloadLink";
        downloadLink.href = window.URL.createObjectURL(blob);
		logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): downloadLink.id → ${downloadLink.id}`);
        downloadLink.download = filename;
		logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): downloadLink.download → ${downloadLink.download}`);
        document.body.appendChild(downloadLink);
        downloadLink.click();
		logger.debug(`<<< Exiting download(fileId=${fileId},forceDownload=${forceDownload})`);
    }

    public async search(
        folderId: number,
        search: string,
        pageIndex: number,
        pageSize = 20,
        sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
        culture = "")
    {
        const url = `${this.requestUrl}Search?folderId=${folderId}&search=${search}&pageIndex=${pageIndex}&pageSize=${pageSize}&sorting=${sorting.sortKey}&culture=${culture}`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture}): headers → ${headers}`);
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
			logger.debug(`>>> Entering search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture})`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture}): error → ${error}`);
			logger.debug(`<<< Exiting search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture})`);
            throw new Error(error);
        }
        const data = await response.json() as SearchResponse;
		logger.info(`search(folderId=${folderId}, search=${search}, pageIndex=${pageIndex}, pageSize=${pageSize}, sorting=${sorting}, culture=${culture}): API call took ${Date.now() - _start} ms`);
		logger.debug(`search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture}): data → ${data}`);
        return data;
    }

    public async getFolderMappings(){
		logger.debug('>>> Entering getFolderMappings()'); const _start = Date.now();
        const url = `${this.requestUrl}GetFolderMappings`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			logger.debug(`getFolderMappings(): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getFolderMappings(): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as FolderMappingInfo[];
		logger.info(`getFolderMappings(): API call took ${Date.now() - _start} ms`);
		logger.debug(`getFolderMappings(): data → ${data}`);
		logger.debug('<<< Exiting getFolderMappings()');
        return data;
    }

    public async canManageFolderTypes(){
		logger.debug('>>> Entering canManageFolderTypes()'); const _start = Date.now();
        const url = `${this.requestUrl}CanManageFolderTypes`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			logger.debug(`canManageFolderTypes(): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`canManageFolderTypes(): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as boolean;
		logger.info(`canManageFolderTypes(): API call took ${Date.now() - _start} ms`);
		logger.debug(`canManageFolderTypes(): data → ${data}`);
		logger.debug('<<< Exiting canManageFolderTypes()');
        return data;
    }

    public async getAddFolderTypeUrl(){
		logger.debug('>>> Entering getAddFolderTypeUrl()'); const _start = Date.now();
        const url = `${this.requestUrl}GetAddFolderTypeUrl`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			logger.debug(`getAddFolderTypeUrl(): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getAddFolderTypeUrl(): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as string;
		logger.info(`getAddFolderTypeUrl(): API call took ${Date.now() - _start} ms`);
		logger.debug(`getAddFolderTypeUrl(): data → ${data}`);
		logger.debug('<<< Exiting getAddFolderTypeUrl()');
        return data;
    };

    public async createNewFolder(request: CreateNewFolderRequest){
		logger.debug(`>>> Entering createNewFolder(request=${request})`); const _start = Date.now();
        const url = `${this.requestUrl}CreateNewFolder`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`createNewFolder(request=${request}): headers → ${headers}`);
        headers.append("groupId", this.getGroupId());
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.debug(`createNewFolder(request=${request}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`createNewFolder(request=${request}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as CreateNewFolderResponse;
		logger.info(`createNewFolder(request=${request}): API call took ${Date.now() - _start} ms`);
		logger.debug(`createNewFolder(request=${request}): data → ${data}`);
		logger.debug(`<<< Exiting createNewFolder(request=${request})`);
        return data;
    }

    public async getFolderItem(folderId: number){
		logger.debug(`>>> Entering getFolderItem(folderId=${folderId})`); const _start = Date.now();
        const url = `${this.requestUrl}GetFolderItem?folderId=${folderId}`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`getFolderItem(folderId=${folderId}): headers → ${headers}`);
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            headers,
        });
        if (response.status !== 200){
			logger.debug(`getFolderItem(folderId=${folderId}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getFolderItem(folderId=${folderId}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as Item;
		logger.info(`getFolderItem(folderId=${folderId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`getFolderItem(folderId=${folderId}): data → ${data}`);
		logger.debug(`<<< Exiting getFolderItem(folderId=${folderId})`);
        return data;
    }

    public async getFolderDetails(folderId: number){
		logger.debug(`>>> Entering getFolderDetails(folderId=${folderId})`); const _start = Date.now();
        const url = `${this.requestUrl}GetFolderDetails?folderId=${folderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			logger.debug(`getFolderDetails(folderId=${folderId}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getFolderDetails(folderId=${folderId}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as FolderDetails;
		logger.info(`getFolderDetails(folderId=${folderId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`getFolderDetails(folderId=${folderId}): data → ${data}`);
		logger.debug(`<<< Exiting getFolderDetails(folderId=${folderId})`);
        return data;
    }

    public async getFileDetails(fileId: number){
		logger.debug(`>>> Entering getFileDetails(fileId=${fileId})`); const _start = Date.now();
        const url = `${this.requestUrl}GetFileDetails?fileId=${fileId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status === 404)
		logger.debug(`getFileDetails(fileId=${fileId}): response → ${response}`);
        {
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getFileDetails(fileId=${fileId}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as FileDetails;
		logger.info(`getFileDetails(fileId=${fileId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`getFileDetails(fileId=${fileId}): data → ${data}`);
		logger.debug(`<<< Exiting getFileDetails(fileId=${fileId})`);
        return data;
    }

    public async getRoleGroups(){
		logger.debug('>>> Entering getRoleGroups()'); const _start = Date.now();
        const url = `${this.requestUrl}GetRoleGroups`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status === 401){
			logger.debug(`getRoleGroups(): response → ${response}`);
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getRoleGroups(): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as IRoleGroup[];
		logger.info(`getRoleGroups(): API call took ${Date.now() - _start} ms`);
		logger.debug(`getRoleGroups(): data → ${data}`);
		logger.debug('<<< Exiting getRoleGroups()');
        return data;
    }

    public async getRoles(){
		logger.debug('>>> Entering getRoles()'); const _start = Date.now();
        const url = `${this.requestUrl}GetRoles`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			logger.debug(`getRoles(): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`getRoles(): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as IRole[];
		logger.info(`getRoles(): API call took ${Date.now() - _start} ms`);
		logger.debug(`getRoles(): data → ${data}`);
		logger.debug('<<< Exiting getRoles()');
        return data;
    }

    public async searchUsers(query: string) {
		logger.debug(`>>> Entering searchUsers(query=${query})`); const _start = Date.now();
        const url = `${this.requestUrl}GetSuggestionUsers?keyword=${query}&count=50`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			logger.debug(`searchUsers(query=${query}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`searchUsers(query=${query}): error → ${error}`);
            throw new Error(error);
        }
        const data = await response.json() as ISearchedUser[];
		logger.info(`searchUsers(query=${query}): API call took ${Date.now() - _start} ms`);
		logger.debug(`searchUsers(query=${query}): data → ${data}`);
		logger.debug(`<<< Exiting searchUsers(query=${query})`);
        return data;
    }

    public async saveFolderDetails(request: SaveFolderDetailsRequest){
		logger.debug(`>>> Entering saveFolderDetails(request=${request})`); const _start = Date.now();
        const url = `${this.requestUrl}SaveFolderDetails`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`saveFolderDetails(request=${request}): headers → ${headers}`);
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status === 404){
			logger.info(`saveFolderDetails(request=${request}): API call took ${Date.now() - _start} ms`);
			logger.debug(`saveFolderDetails(request=${request}): response → ${response}`);
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
			logger.debug(`<<< Exiting saveFolderDetails(request=${request})`);
            throw new Error(response.statusText);
        }
    }

    public async saveFileDetails(request: SaveFileDetailsRequest){
		logger.debug(`>>> Entering saveFileDetails(request=${request})`); const _start = Date.now();
        const url = `${this.requestUrl}SaveFileDetails`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`saveFileDetails(request=${request}): headers → ${headers}`);
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.info(`saveFileDetails(request=${request}): API call took ${Date.now() - _start} ms`);
			logger.debug(`<<< Exiting saveFileDetails(request=${request})`);
            throw new Error(response.statusText);
        }
    }

    public async moveFile(request: MoveFileRequest){
		logger.debug(`>>> Entering moveFile(request=${request})`); const _start = Date.now();
        const url = `${this.requestUrl}MoveFile`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`moveFile(request=${request}): headers → ${headers}`);
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.info(`moveFile(request=${request}): API call took ${Date.now() - _start} ms`);
			logger.debug(`moveFile(request=${request}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`moveFile(request=${request}): error → ${error}`);
		logger.debug(`<<< Exiting moveFile(request=${request})`);
            throw new Error(error);
        }
    }

    public async moveFolder(request: MoveFolderRequest){
		logger.debug(`>>> Entering moveFolder(request=${request})`); const _start = Date.now();
        const url = `${this.requestUrl}MoveFolder`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`moveFolder(request=${request}): headers → ${headers}`);
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.info(`moveFolder(request=${request}): API call took ${Date.now() - _start} ms`);
			logger.debug(`moveFolder(request=${request}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`moveFolder(request=${request}): error → ${error}`);
		logger.debug(`<<< Exiting moveFolder(request=${request})`);
            throw new Error(error);
        }
    }

    public async deleteFolder(request: DeleteFolderRequest){
		logger.debug(`>>> Entering deleteFolder(request=${request})`); const _start = Date.now();
        const url = `${this.requestUrl}DeleteFolder`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`deleteFolder(request=${request}): headers → ${headers}`);
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.info(`deleteFolder(request=${request}): API call took ${Date.now() - _start} ms`);
			logger.debug(`deleteFolder(request=${request}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`deleteFolder(request=${request}): error → ${error}`);
		logger.debug(`<<< Exiting deleteFolder(request=${request})`);
            throw new Error(error);
        }
    }

    public async deleteFile(request: DeleteFileRequest){
		logger.debug(`>>> Entering deleteFile(request=${request})`); const _start = Date.now();
        const url = `${this.requestUrl}DeleteFile`;
        const headers = this.sf.getModuleHeaders();
		logger.debug(`deleteFile(request=${request}): headers → ${headers}`);
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.info(`deleteFile(request=${request}): API call took ${Date.now() - _start} ms`);
			logger.debug(`deleteFile(request=${request}): response → ${response}`);
            const error = await this.getGenericErrorMessage(response);
			logger.debug(`deleteFile(request=${request}): error → ${error}`);
		logger.debug(`<<< Exiting deleteFile(request=${request})`);
            throw new Error(error);
        }
    }

    private decodeRFC5987ContentDisposition(filename: string): string {
        filename = filename.replace(/(^")|("$)/g, '');

		logger.debug(`decodeRFC5987ContentDisposition(filename=${filename}): filename → ${filename}`);
        if (filename.startsWith("=?utf-8?B?") && filename.endsWith("?=")) {
			logger.debug(`>>> Entering decodeRFC5987ContentDisposition(filename=${filename})`);
            const encoded = filename.slice(10, -2);
            return decodeURIComponent(
              atob(encoded)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
            );
          }
          return filename;
    }

    private getGroupId(): string {
        let result = "-1";
        if (window.location.pathname.indexOf("/groupid/") > -1){
			logger.debug('>>> Entering getGroupId()');
            const parts = window.location.pathname.split("/");
            const groupIdIndex = parts.indexOf("groupid");
			logger.debug(`getGroupId(): parts → ${parts}`);
			logger.debug(`getGroupId(): groupIdIndex → ${groupIdIndex}`);
            result = parts[groupIdIndex + 1];
			logger.debug('<<< Exiting getGroupId()');
            return result;
        }

        const searchParams = new URLSearchParams(window.location.search);
		logger.debug(`getGroupId(): searchParams → ${searchParams}`);
        const groupId = searchParams.get("groupid");
        if (groupId != null){
            result = groupId;
        }

		logger.debug(`getGroupId(): groupId → ${groupId}`);
        return result;
    }

    private async getGenericErrorMessage(response: Response) {
        const contentType = response.headers.get("Content-Type");
        if (contentType && contentType.includes("application/json")) {
            const errorBody = await response.json() as { Message?: string };
            if (errorBody?.Message != null){
                return errorBody.Message;
            }
            else if(typeof errorBody === "string"){
                return errorBody;
            }
        }
        else {
            return await response.text();
        }
    }
}

/** Represents the module settings. */
export interface GetSettingsResponse{
        /** The name of the root folder (could be an actual folder name or "Site Assets" or "Global Assets") */
        HomeFolderName: string;

        /** The ID of the home folder configures in the settings */
        HomeFolderId: number;

        /** The current module mode configured in the settings */
        Mode: ModuleMode;

        /** Whether folder is global or for current portal */
        IsHostPortal: boolean;
}

enum ModuleMode{
    /** Normal mode is when the module is used to manage site or host files. */
    Normal = 0,

    /** User mode is for when a module is used for a specific user to manage his files. */
    User = 1,

    /** Group mode is for when the module is used by a social group. */
    Group = 2,
}

export interface GetFolderContentResponse{
    folder: FolderInfo;
    hasAddFilesPermission: boolean;
    hasAddFoldersPermission: boolean;
    hasDeletePermission: boolean;
    hasManagePermission: boolean;
    items: Item[];
    totalCount: number;
}

export interface GetFolderIconUrlResponse{
    url: string;
}

export interface GetAllowedFileExtensionsResponse {
    /** A coma delimited list of the allowed file extensions. */
    allowedExtensions: string;
    validationCode: string;
    maxUploadFileSize: number;
}

export interface FolderInfo{
    folderId: number;
    folderMappingId: number;
    folderName: string;
    folderParentId: number;
    folderPath: string;
}

export interface Item{
    /** The file icon (not the image thumbnail) */
    iconUrl: string;
    /** If true, the item is a folder and the itemId represents a folderId, if false then the item is a file and the id is the fileId. */
    isFolder: boolean;
    /** The folder or file id. */
    itemId: number;
    /** The folder or file name. */
    itemName: string;
    /** The relative url to the file (no present on folders) */
    path?: string;
    /** If true, a thumbnail is available for this item. */
    thumbnailAvailable?: boolean | undefined;
    /** The relative url to the item thumbnail. */
    thumbnailUrl?: string;
    /** And ISO 8601 string representing the created date of the item. */
    createdOn: string;
    /** And ISO 8601 string representing the last modified date of the item. */
    modifiedOn: string;
    /** The size of the file (only available for file Items) */
    fileSize?: number,
    /** Defines if a folder has the possibility of being unlinked instead of deleted. */
    unlinkAllowedStatus?: "true" | "false" | "unlinkOnly",
}

export interface SearchResponse{
    items: Item[],
    totalCount: number,
}

export interface FolderMappingInfo{
    /** The ID of the folder mapping. */
    FolderMappingID: number;
    /** The provider type name such as "AzureFolderProvider" */
    FolderProviderType: string;
    /** True if this is the default provider type. */
    IsDefault: boolean;
    /** A friendly name for this mapping type. */
    MappingName: string;
    /** A url that allows editing this folder mapping. */
    editUrl: string;
}

export interface CreateNewFolderRequest{
    /** Gets or sets the new folder name. */
    FolderName: string;

    /** Gets or sets he parent folder id for the new folder. */
    ParentFolderId: number;

    /** Gets or sets the folder mapping id. */
    FolderMappingId: number;

    /** Gets or sets the optional mapped path. */
    MappedName?: string;
}

export interface CreateNewFolderResponse{
    /** The ID of the recently created folder. */
    FolderID: number;

    /** The created folder name. */
    FolderName: string,

    /** The url to the folder icon. */
    IconUrl: string;

    /** The ID of the folder mapping. */
    FolderMappingID: number,
}

export interface FolderDetails{
    folderId: number;
    folderName: string;
    createdOnDate: string;
    createdBy: string;
    lastModifiedOnDate: string;
    lastModifiedBy: string;
    type: string;
    isVersioned: boolean;
    permissions: IPermissions;
}

export interface SaveFolderDetailsRequest{
    folderId: number;
    folderName: string;
    permissions: IPermissions;
}

export interface FileDetails{
    fileId: number;
    fileName: string;
    title: string;
    description: string;
    size: string;
    createdOnDate: string;
    createdBy: string;
    lastModifiedOnDate: string;
    lastModifiedBy: string;
    url: string;
    iconUrl: string;
}

export interface SaveFileDetailsRequest{
    fileId: number;
    fileName: string;
    title: string;
    description: string;
}

export interface MoveFileRequest{
    SourceFileId: number;
    DestinationFolderId: number;
}

export interface MoveFolderRequest{
    SourceFolderId: number;
    DestinationFolderId: number;
}

export interface DeleteFolderRequest{
    FolderId: number;
    UnlinkAllowedStatus: boolean;
}

export interface DeleteFileRequest{
    FileId: number;
}

