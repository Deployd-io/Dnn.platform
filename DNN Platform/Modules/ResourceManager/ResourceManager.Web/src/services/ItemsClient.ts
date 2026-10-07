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
		logger.error(`>>> Entering constructor(moduleId=${moduleId})`, { "event.action": "constructor", "logrite.moduleId": moduleId });
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = `${this.sf.getServiceRoot("ResourceManager")}Items/`
		logger.warn(`<<< Exiting constructor(moduleId=${moduleId})`, { "event.action": "constructor", "logrite.moduleId": moduleId });
    }

    public async getSettings() {
		logger.error('>>> Entering getSettings()', { "event.action": "getSettings" });
        const url =`${this.requestUrl}GetSettings`;
        const headers = this.sf.getModuleHeaders();
		const _start_getSettings_23 = Date.now();
		logger.error(`getSettings(): headers → ${headers}`, { "event.action": "getSettings" });
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, { headers });
		logger.error(`getSettings(): response → ${response}`, { "event.action": "getSettings" });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getSettings(): error → ${error}`, { "event.action": "getSettings" });
            throw new Error(error);
        }
        const data = await response.json() as GetSettingsResponse;
		logger.error(`getSettings(): API call took ${Date.now() - _start_getSettings_23} ms`, { "event.action": "getSettings", "event.category": "network" });
		logger.error(`getSettings(): data → ${data}`, { "event.action": "getSettings" });
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
		logger.error(`>>> Entering getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder})`, { "event.action": "getFolderContent", "logrite.folderId": folderId, "logrite.startIndex": startIndex, "logrite.numItems": numItems, "logrite.sortingField": sortingField, "logrite.sortingOrder": sortingOrder });
		const _start_getFolderContent_44 = Date.now();
        const url = `${this.requestUrl}GetFolderContent?folderId=${folderId}&startIndex=${startIndex}&numItems=${numItems}&sorting=${sortingField.sortKey}&sortingOrder=${sortingOrder.sortOrderKey}`;
        const headers = this.sf.getModuleHeaders();
		logger.error(`getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder}): headers → ${headers}`, { "event.action": "getFolderContent", "logrite.folderId": folderId, "logrite.startIndex": startIndex, "logrite.numItems": numItems, "logrite.sortingField": sortingField, "logrite.sortingOrder": sortingOrder });
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
			logger.error(`getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder}): response → ${response}`, { "event.action": "getFolderContent", "logrite.folderId": folderId, "logrite.startIndex": startIndex, "logrite.numItems": numItems, "logrite.sortingField": sortingField, "logrite.sortingOrder": sortingOrder });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder}): error → ${error}`, { "event.action": "getFolderContent", "logrite.folderId": folderId, "logrite.startIndex": startIndex, "logrite.numItems": numItems, "logrite.sortingField": sortingField, "logrite.sortingOrder": sortingOrder });
            throw new Error(error);
        }
        const data = await response.json() as GetFolderContentResponse;
		logger.error(`getFolderContent(folderId=${folderId}, startIndex=${startIndex}, numItems=${numItems}, sortingField=${sortingField}, sortingOrder=${sortingOrder}): API call took ${Date.now() - _start_getFolderContent_44} ms`, { "event.action": "getFolderContent", "event.category": "network", "logrite.folderId": folderId, "logrite.startIndex": startIndex, "logrite.numItems": numItems, "logrite.sortingField": sortingField, "logrite.sortingOrder": sortingOrder });
		logger.error(`getFolderContent(folderId=${folderId},startIndex=${startIndex},numItems=${numItems},sortingField=${sortingField},sortingOrder=${sortingOrder}): data → ${data}`, { "event.action": "getFolderContent", "logrite.folderId": folderId, "logrite.startIndex": startIndex, "logrite.numItems": numItems, "logrite.sortingField": sortingField, "logrite.sortingOrder": sortingOrder });
        return data;
    }

    public async getFolderIconUrl(folderId: number) {
		logger.error(`>>> Entering getFolderIconUrl(folderId=${folderId})`, { "event.action": "getFolderIconUrl", "logrite.folderId": folderId });
        const url = `${this.requestUrl}GetFolderIconUrl?folderId=${folderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			const _start_getFolderIconUrl_68 = Date.now();
			logger.error(`getFolderIconUrl(folderId=${folderId}): response → ${response}`, { "event.action": "getFolderIconUrl", "logrite.folderId": folderId });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getFolderIconUrl(folderId=${folderId}): error → ${error}`, { "event.action": "getFolderIconUrl", "logrite.folderId": folderId });
            throw new Error(error);
        }
        const data = await response.json() as GetFolderIconUrlResponse;
		logger.error(`getFolderIconUrl(folderId=${folderId}): API call took ${Date.now() - _start_getFolderIconUrl_68} ms`, { "event.action": "getFolderIconUrl", "event.category": "network", "logrite.folderId": folderId });
		logger.error(`getFolderIconUrl(folderId=${folderId}): data → ${data}`, { "event.action": "getFolderIconUrl", "logrite.folderId": folderId });
        return data.url;
    }

    public async getAllowedFileExtensions() {
		logger.error('>>> Entering getAllowedFileExtensions()', { "event.action": "getAllowedFileExtensions" });
        const url = `${this.requestUrl}GetAllowedFileExtensions`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			const _start_getAllowedFileExtensions_81 = Date.now();
			logger.error(`getAllowedFileExtensions(): response → ${response}`, { "event.action": "getAllowedFileExtensions" });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getAllowedFileExtensions(): error → ${error}`, { "event.action": "getAllowedFileExtensions" });
            throw new Error(error);
        }
        const data = await response.json() as GetAllowedFileExtensionsResponse;
		logger.error(`getAllowedFileExtensions(): API call took ${Date.now() - _start_getAllowedFileExtensions_81} ms`, { "event.action": "getAllowedFileExtensions", "event.category": "network" });
		logger.error(`getAllowedFileExtensions(): data → ${data}`, { "event.action": "getAllowedFileExtensions" });
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
		logger.error(`>>> Entering syncFolderContent(folderId=${folderId},numItems=${numItems},sorting=${sorting},recursive=${recursive})`, { "event.action": "syncFolderContent", "logrite.folderId": folderId, "logrite.numItems": numItems, "logrite.sorting": sorting, "logrite.recursive": recursive });
        const url = `${this.requestUrl}SyncFolderContent?folderId=${folderId}&numItems=${numItems}&sorting=${sorting.sortKey}&recursive=${recursive}`;
        const headers = this.sf.getModuleHeaders();
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
			const _start_syncFolderContent_102 = Date.now();
			logger.error(`syncFolderContent(folderId=${folderId},numItems=${numItems},sorting=${sorting},recursive=${recursive}): headers → ${headers}`, { "event.action": "syncFolderContent", "logrite.folderId": folderId, "logrite.numItems": numItems, "logrite.sorting": sorting, "logrite.recursive": recursive });
            throw new Error(error);
        }
        const data = await response.json() as GetFolderContentResponse;
		logger.error(`syncFolderContent(folderId=${folderId}, numItems=${numItems}, sorting=${sorting}, recursive=${recursive}): API call took ${Date.now() - _start_syncFolderContent_102} ms`, { "event.action": "syncFolderContent", "event.category": "network", "logrite.folderId": folderId, "logrite.numItems": numItems, "logrite.sorting": sorting, "logrite.recursive": recursive });
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
		logger.error(`>>> Entering download(fileId=${fileId},forceDownload=${forceDownload})`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
		const _start_download_132 = Date.now();
        const url = `${this.requestUrl}Download?fileId=${fileId}&forceDownload=${forceDownload}`;
        const headers = this.sf.getModuleHeaders();
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
			logger.error(`download(fileId=${fileId}, forceDownload=${forceDownload}): API call took ${Date.now() - _start_download_132} ms`, { "event.action": "download", "event.category": "network", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
			logger.error(`download(fileId=${fileId},forceDownload=${forceDownload}): headers → ${headers}`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
			logger.error(`download(fileId=${fileId},forceDownload=${forceDownload}): response → ${response}`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }

        let filename = response.headers.get("Content-Disposition").split("filename=")[1];
        filename = this.decodeRFC5987ContentDisposition(filename);
		logger.error(`download(fileId=${fileId},forceDownload=${forceDownload}): filename → ${filename}`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });

        const blob = await response.blob()        
        const oldDownloadLink = document.querySelector("#downloadLink");
		logger.error(`download(fileId=${fileId},forceDownload=${forceDownload}): blob → ${blob}`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
		logger.error(`download(fileId=${fileId},forceDownload=${forceDownload}): oldDownloadLink → ${oldDownloadLink}`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
        if (oldDownloadLink !== null) {
            oldDownloadLink.remove();
        }
        const downloadLink = document.createElement('a');
		logger.error(`download(fileId=${fileId},forceDownload=${forceDownload}): downloadLink → ${downloadLink}`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
		logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): downloadLink.download → ${downloadLink.download}`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
        downloadLink.id = "downloadLink";
		logger.debug(`download(fileId=${fileId},forceDownload=${forceDownload}): downloadLink.id → ${downloadLink.id}`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
        downloadLink.href = window.URL.createObjectURL(blob);
        downloadLink.download = filename;
        document.body.appendChild(downloadLink);
        downloadLink.click();
		logger.warn(`<<< Exiting download(fileId=${fileId},forceDownload=${forceDownload})`, { "event.action": "download", "logrite.fileId": fileId, "logrite.forceDownload": forceDownload });
    }

    public async search(
        folderId: number,
        search: string,
        pageIndex: number,
        pageSize = 20,
        sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
        culture = "")
    {
		logger.error(`>>> Entering search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture})`, { "event.action": "search", "logrite.folderId": folderId, "logrite.search": search, "logrite.pageIndex": pageIndex, "logrite.pageSize": pageSize, "logrite.sorting": sorting, "logrite.culture": culture });
		const _start_search_165 = Date.now();
        const url = `${this.requestUrl}Search?folderId=${folderId}&search=${search}&pageIndex=${pageIndex}&pageSize=${pageSize}&sorting=${sorting.sortKey}&culture=${culture}`;
        const headers = this.sf.getModuleHeaders();
		logger.error(`search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture}): headers → ${headers}`, { "event.action": "search", "logrite.folderId": folderId, "logrite.search": search, "logrite.pageIndex": pageIndex, "logrite.pageSize": pageSize, "logrite.sorting": sorting, "logrite.culture": culture });
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
			logger.error(`search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture}): response → ${response}`, { "event.action": "search", "logrite.folderId": folderId, "logrite.search": search, "logrite.pageIndex": pageIndex, "logrite.pageSize": pageSize, "logrite.sorting": sorting, "logrite.culture": culture });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture}): error → ${error}`, { "event.action": "search", "logrite.folderId": folderId, "logrite.search": search, "logrite.pageIndex": pageIndex, "logrite.pageSize": pageSize, "logrite.sorting": sorting, "logrite.culture": culture });
            throw new Error(error);
        }
        const data = await response.json() as SearchResponse;
		logger.error(`search(folderId=${folderId}, search=${search}, pageIndex=${pageIndex}, pageSize=${pageSize}, sorting=${sorting}, culture=${culture}): API call took ${Date.now() - _start_search_165} ms`, { "event.action": "search", "event.category": "network", "logrite.folderId": folderId, "logrite.search": search, "logrite.pageIndex": pageIndex, "logrite.pageSize": pageSize, "logrite.sorting": sorting, "logrite.culture": culture });
		logger.error(`search(folderId=${folderId},search=${search},pageIndex=${pageIndex},pageSize=${pageSize},sorting=${sorting},culture=${culture}): data → ${data}`, { "event.action": "search", "logrite.folderId": folderId, "logrite.search": search, "logrite.pageIndex": pageIndex, "logrite.pageSize": pageSize, "logrite.sorting": sorting, "logrite.culture": culture });
        return data;
    }

    public async getFolderMappings(){
		logger.error('>>> Entering getFolderMappings()', { "event.action": "getFolderMappings" });
        const url = `${this.requestUrl}GetFolderMappings`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			const _start_getFolderMappings_190 = Date.now();
			logger.error(`getFolderMappings(): response → ${response}`, { "event.action": "getFolderMappings" });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getFolderMappings(): error → ${error}`, { "event.action": "getFolderMappings" });
            throw new Error(error);
        }
        const data = await response.json() as FolderMappingInfo[];
		logger.error(`getFolderMappings(): API call took ${Date.now() - _start_getFolderMappings_190} ms`, { "event.action": "getFolderMappings", "event.category": "network" });
		logger.error(`getFolderMappings(): data → ${data}`, { "event.action": "getFolderMappings" });
        return data;
    }

    public async canManageFolderTypes(){
		logger.error('>>> Entering canManageFolderTypes()', { "event.action": "canManageFolderTypes" });
        const url = `${this.requestUrl}CanManageFolderTypes`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			const _start_canManageFolderTypes_203 = Date.now();
			logger.error(`canManageFolderTypes(): response → ${response}`, { "event.action": "canManageFolderTypes" });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`canManageFolderTypes(): error → ${error}`, { "event.action": "canManageFolderTypes" });
            throw new Error(error);
        }
        const data = await response.json() as boolean;
		logger.error(`canManageFolderTypes(): API call took ${Date.now() - _start_canManageFolderTypes_203} ms`, { "event.action": "canManageFolderTypes", "event.category": "network" });
		logger.error(`canManageFolderTypes(): data → ${data}`, { "event.action": "canManageFolderTypes" });
        return data;
    }

    public async getAddFolderTypeUrl(){
		logger.error('>>> Entering getAddFolderTypeUrl()', { "event.action": "getAddFolderTypeUrl" });
        const url = `${this.requestUrl}GetAddFolderTypeUrl`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			const _start_getAddFolderTypeUrl_216 = Date.now();
			logger.error(`getAddFolderTypeUrl(): response → ${response}`, { "event.action": "getAddFolderTypeUrl" });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getAddFolderTypeUrl(): error → ${error}`, { "event.action": "getAddFolderTypeUrl" });
            throw new Error(error);
        }
        const data = await response.json() as string;
		logger.error(`getAddFolderTypeUrl(): API call took ${Date.now() - _start_getAddFolderTypeUrl_216} ms`, { "event.action": "getAddFolderTypeUrl", "event.category": "network" });
		logger.error(`getAddFolderTypeUrl(): data → ${data}`, { "event.action": "getAddFolderTypeUrl" });
        return data;
    };

    public async createNewFolder(request: CreateNewFolderRequest){
		logger.error(`>>> Entering createNewFolder(request=${request})`, { "event.action": "createNewFolder", "logrite.request": request });
        const url = `${this.requestUrl}CreateNewFolder`;
        const headers = this.sf.getModuleHeaders();
		const _start_createNewFolder_229 = Date.now();
		logger.error(`createNewFolder(request=${request}): headers → ${headers}`, { "event.action": "createNewFolder", "logrite.request": request });
        headers.append("groupId", this.getGroupId());
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.error(`createNewFolder(request=${request}): response → ${response}`, { "event.action": "createNewFolder", "logrite.request": request });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`createNewFolder(request=${request}): error → ${error}`, { "event.action": "createNewFolder", "logrite.request": request });
            throw new Error(error);
        }
        const data = await response.json() as CreateNewFolderResponse;
		logger.error(`createNewFolder(request=${request}): API call took ${Date.now() - _start_createNewFolder_229} ms`, { "event.action": "createNewFolder", "event.category": "network", "logrite.request": request });
		logger.error(`createNewFolder(request=${request}): data → ${data}`, { "event.action": "createNewFolder", "logrite.request": request });
        return data;
    }

    public async getFolderItem(folderId: number){
		logger.error(`>>> Entering getFolderItem(folderId=${folderId})`, { "event.action": "getFolderItem", "logrite.folderId": folderId });
        const url = `${this.requestUrl}GetFolderItem?folderId=${folderId}`;
        const headers = this.sf.getModuleHeaders();
		const _start_getFolderItem_247 = Date.now();
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            headers,
        });
        if (response.status !== 200){
			logger.error(`getFolderItem(folderId=${folderId}): headers → ${headers}`, { "event.action": "getFolderItem", "logrite.folderId": folderId });
			logger.error(`getFolderItem(folderId=${folderId}): response → ${response}`, { "event.action": "getFolderItem", "logrite.folderId": folderId });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getFolderItem(folderId=${folderId}): error → ${error}`, { "event.action": "getFolderItem", "logrite.folderId": folderId });
            throw new Error(error);
        }
        const data = await response.json() as Item;
		logger.error(`getFolderItem(folderId=${folderId}): API call took ${Date.now() - _start_getFolderItem_247} ms`, { "event.action": "getFolderItem", "event.category": "network", "logrite.folderId": folderId });
		logger.error(`getFolderItem(folderId=${folderId}): data → ${data}`, { "event.action": "getFolderItem", "logrite.folderId": folderId });
        return data;
    }

    public async getFolderDetails(folderId: number){
		logger.error(`>>> Entering getFolderDetails(folderId=${folderId})`, { "event.action": "getFolderDetails", "logrite.folderId": folderId });
        const url = `${this.requestUrl}GetFolderDetails?folderId=${folderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			const _start_getFolderDetails_262 = Date.now();
			logger.error(`getFolderDetails(folderId=${folderId}): response → ${response}`, { "event.action": "getFolderDetails", "logrite.folderId": folderId });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getFolderDetails(folderId=${folderId}): error → ${error}`, { "event.action": "getFolderDetails", "logrite.folderId": folderId });
            throw new Error(error);
        }
        const data = await response.json() as FolderDetails;
		logger.error(`getFolderDetails(folderId=${folderId}): API call took ${Date.now() - _start_getFolderDetails_262} ms`, { "event.action": "getFolderDetails", "event.category": "network", "logrite.folderId": folderId });
		logger.error(`getFolderDetails(folderId=${folderId}): data → ${data}`, { "event.action": "getFolderDetails", "logrite.folderId": folderId });
        return data;
    }

    public async getFileDetails(fileId: number){
		logger.error(`>>> Entering getFileDetails(fileId=${fileId})`, { "event.action": "getFileDetails", "logrite.fileId": fileId });
        const url = `${this.requestUrl}GetFileDetails?fileId=${fileId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
		const _start_getFileDetails_275 = Date.now();
        if (response.status === 404)
        {
			logger.error(`getFileDetails(fileId=${fileId}): response → ${response}`, { "event.action": "getFileDetails", "logrite.fileId": fileId });
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getFileDetails(fileId=${fileId}): error → ${error}`, { "event.action": "getFileDetails", "logrite.fileId": fileId });
            throw new Error(error);
        }
        const data = await response.json() as FileDetails;
		logger.error(`getFileDetails(fileId=${fileId}): API call took ${Date.now() - _start_getFileDetails_275} ms`, { "event.action": "getFileDetails", "event.category": "network", "logrite.fileId": fileId });
		logger.error(`getFileDetails(fileId=${fileId}): data → ${data}`, { "event.action": "getFileDetails", "logrite.fileId": fileId });
        return data;
    }

    public async getRoleGroups(){
		logger.error('>>> Entering getRoleGroups()', { "event.action": "getRoleGroups" });
        const url = `${this.requestUrl}GetRoleGroups`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status === 401){
			const _start_getRoleGroups_292 = Date.now();
			logger.error(`getRoleGroups(): response → ${response}`, { "event.action": "getRoleGroups" });
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getRoleGroups(): error → ${error}`, { "event.action": "getRoleGroups" });
            throw new Error(error);
        }
        const data = await response.json() as IRoleGroup[];
		logger.error(`getRoleGroups(): API call took ${Date.now() - _start_getRoleGroups_292} ms`, { "event.action": "getRoleGroups", "event.category": "network" });
		logger.error(`getRoleGroups(): data → ${data}`, { "event.action": "getRoleGroups" });
        return data;
    }

    public async getRoles(){
		logger.error('>>> Entering getRoles()', { "event.action": "getRoles" });
        const url = `${this.requestUrl}GetRoles`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			const _start_getRoles_308 = Date.now();
			logger.error(`getRoles(): response → ${response}`, { "event.action": "getRoles" });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`getRoles(): error → ${error}`, { "event.action": "getRoles" });
            throw new Error(error);
        }
        const data = await response.json() as IRole[];
		logger.error(`getRoles(): API call took ${Date.now() - _start_getRoles_308} ms`, { "event.action": "getRoles", "event.category": "network" });
		logger.error(`getRoles(): data → ${data}`, { "event.action": "getRoles" });
        return data;
    }

    public async searchUsers(query: string) {
		logger.error(`>>> Entering searchUsers(query=${query})`, { "event.action": "searchUsers", "logrite.query": query });
        const url = `${this.requestUrl}GetSuggestionUsers?keyword=${query}&count=50`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
			const _start_searchUsers_321 = Date.now();
			logger.error(`searchUsers(query=${query}): response → ${response}`, { "event.action": "searchUsers", "logrite.query": query });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`searchUsers(query=${query}): error → ${error}`, { "event.action": "searchUsers", "logrite.query": query });
            throw new Error(error);
        }
        const data = await response.json() as ISearchedUser[];
		logger.error(`searchUsers(query=${query}): API call took ${Date.now() - _start_searchUsers_321} ms`, { "event.action": "searchUsers", "event.category": "network", "logrite.query": query });
		logger.error(`searchUsers(query=${query}): data → ${data}`, { "event.action": "searchUsers", "logrite.query": query });
        return data;
    }

    public async saveFolderDetails(request: SaveFolderDetailsRequest){
		logger.error(`>>> Entering saveFolderDetails(request=${request})`, { "event.action": "saveFolderDetails", "logrite.request": request });
        const url = `${this.requestUrl}SaveFolderDetails`;
        const headers = this.sf.getModuleHeaders();
		const _start_saveFolderDetails_334 = Date.now();
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status === 404){
			logger.error(`saveFolderDetails(request=${request}): API call took ${Date.now() - _start_saveFolderDetails_334} ms`, { "event.action": "saveFolderDetails", "event.category": "network", "logrite.request": request });
			logger.error(`saveFolderDetails(request=${request}): headers → ${headers}`, { "event.action": "saveFolderDetails", "logrite.request": request });
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
			logger.error(`saveFolderDetails(request=${request}): response → ${response}`, { "event.action": "saveFolderDetails", "logrite.request": request });
			logger.warn(`<<< Exiting saveFolderDetails(request=${request})`, { "event.action": "saveFolderDetails", "logrite.request": request });
            throw new Error(response.statusText);
        }
    }

    public async saveFileDetails(request: SaveFileDetailsRequest){
		logger.error(`>>> Entering saveFileDetails(request=${request})`, { "event.action": "saveFileDetails", "logrite.request": request });
        const url = `${this.requestUrl}SaveFileDetails`;
        const headers = this.sf.getModuleHeaders();
		const _start_saveFileDetails_351 = Date.now();
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.error(`saveFileDetails(request=${request}): API call took ${Date.now() - _start_saveFileDetails_351} ms`, { "event.action": "saveFileDetails", "event.category": "network", "logrite.request": request });
			logger.warn(`<<< Exiting saveFileDetails(request=${request})`, { "event.action": "saveFileDetails", "logrite.request": request });
            throw new Error(response.statusText);
        }
    }

    public async moveFile(request: MoveFileRequest){
		logger.error(`>>> Entering moveFile(request=${request})`, { "event.action": "moveFile", "logrite.request": request });
        const url = `${this.requestUrl}MoveFile`;
        const headers = this.sf.getModuleHeaders();
		const _start_moveFile_365 = Date.now();
		logger.error(`moveFile(request=${request}): headers → ${headers}`, { "event.action": "moveFile", "logrite.request": request });
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.error(`moveFile(request=${request}): API call took ${Date.now() - _start_moveFile_365} ms`, { "event.action": "moveFile", "event.category": "network", "logrite.request": request });
			logger.error(`moveFile(request=${request}): response → ${response}`, { "event.action": "moveFile", "logrite.request": request });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`moveFile(request=${request}): error → ${error}`, { "event.action": "moveFile", "logrite.request": request });
            throw new Error(error);
        }
    }

    public async moveFolder(request: MoveFolderRequest){
		logger.error(`>>> Entering moveFolder(request=${request})`, { "event.action": "moveFolder", "logrite.request": request });
        const url = `${this.requestUrl}MoveFolder`;
        const headers = this.sf.getModuleHeaders();
		const _start_moveFolder_381 = Date.now();
		logger.error(`moveFolder(request=${request}): headers → ${headers}`, { "event.action": "moveFolder", "logrite.request": request });
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.error(`moveFolder(request=${request}): API call took ${Date.now() - _start_moveFolder_381} ms`, { "event.action": "moveFolder", "event.category": "network", "logrite.request": request });
			logger.error(`moveFolder(request=${request}): response → ${response}`, { "event.action": "moveFolder", "logrite.request": request });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`moveFolder(request=${request}): error → ${error}`, { "event.action": "moveFolder", "logrite.request": request });
            throw new Error(error);
        }
    }

    public async deleteFolder(request: DeleteFolderRequest){
		logger.error(`>>> Entering deleteFolder(request=${request})`, { "event.action": "deleteFolder", "logrite.request": request });
        const url = `${this.requestUrl}DeleteFolder`;
        const headers = this.sf.getModuleHeaders();
		const _start_deleteFolder_397 = Date.now();
		logger.error(`deleteFolder(request=${request}): headers → ${headers}`, { "event.action": "deleteFolder", "logrite.request": request });
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.error(`deleteFolder(request=${request}): API call took ${Date.now() - _start_deleteFolder_397} ms`, { "event.action": "deleteFolder", "event.category": "network", "logrite.request": request });
			logger.error(`deleteFolder(request=${request}): response → ${response}`, { "event.action": "deleteFolder", "logrite.request": request });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`deleteFolder(request=${request}): error → ${error}`, { "event.action": "deleteFolder", "logrite.request": request });
            throw new Error(error);
        }
    }

    public async deleteFile(request: DeleteFileRequest){
		logger.error(`>>> Entering deleteFile(request=${request})`, { "event.action": "deleteFile", "logrite.request": request });
        const url = `${this.requestUrl}DeleteFile`;
        const headers = this.sf.getModuleHeaders();
		const _start_deleteFile_413 = Date.now();
		logger.error(`deleteFile(request=${request}): headers → ${headers}`, { "event.action": "deleteFile", "logrite.request": request });
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
			logger.error(`deleteFile(request=${request}): API call took ${Date.now() - _start_deleteFile_413} ms`, { "event.action": "deleteFile", "event.category": "network", "logrite.request": request });
			logger.error(`deleteFile(request=${request}): response → ${response}`, { "event.action": "deleteFile", "logrite.request": request });
            const error = await this.getGenericErrorMessage(response);
			logger.error(`deleteFile(request=${request}): error → ${error}`, { "event.action": "deleteFile", "logrite.request": request });
            throw new Error(error);
        }
    }

    private decodeRFC5987ContentDisposition(filename: string): string {
		logger.error(`>>> Entering decodeRFC5987ContentDisposition(filename=${filename})`, { "event.action": "decodeRFC5987ContentDisposition", "logrite.filename": filename });
        filename = filename.replace(/(^")|("$)/g, '');

		logger.error(`decodeRFC5987ContentDisposition(filename=${filename}): filename → ${filename}`, { "event.action": "decodeRFC5987ContentDisposition", "logrite.filename": filename });
        if (filename.startsWith("=?utf-8?B?") && filename.endsWith("?=")) {
            const encoded = filename.slice(10, -2);
            return decodeURIComponent(
              atob(encoded)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
            );
          }
		logger.warn(`<<< Exiting decodeRFC5987ContentDisposition(filename=${filename})`, { "event.action": "decodeRFC5987ContentDisposition", "logrite.filename": filename });
          return filename;
    }

    private getGroupId(): string {
		logger.error('>>> Entering getGroupId()', { "event.action": "getGroupId" });
        let result = "-1";
        if (window.location.pathname.indexOf("/groupid/") > -1){
            const parts = window.location.pathname.split("/");
			logger.error(`getGroupId(): parts → ${parts}`, { "event.action": "getGroupId" });
            const groupIdIndex = parts.indexOf("groupid");
			logger.error(`getGroupId(): groupIdIndex → ${groupIdIndex}`, { "event.action": "getGroupId" });
            result = parts[groupIdIndex + 1];
            return result;
        }

        const searchParams = new URLSearchParams(window.location.search);
		logger.error(`getGroupId(): searchParams → ${searchParams}`, { "event.action": "getGroupId" });
        const groupId = searchParams.get("groupid");
        if (groupId != null){
            result = groupId;
        }

		logger.error(`getGroupId(): result → ${result}`, { "event.action": "getGroupId" });
		logger.error(`getGroupId(): groupId → ${groupId}`, { "event.action": "getGroupId" });
        return result;
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

