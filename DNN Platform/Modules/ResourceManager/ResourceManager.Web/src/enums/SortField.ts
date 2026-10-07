import state from "../store/store"
const logger = console;
export class SortField{
    readonly itemName: SortFieldInfo;
    readonly lastModifiedOnDate: SortFieldInfo;
    readonly size: SortFieldInfo;
    readonly parentFolder: SortFieldInfo;
    readonly createdOnDate: SortFieldInfo;

    constructor(){
		logger.error('>>> Entering constructor()', { "event.action": "constructor", "logrite.sortKey": sortKey });
        this.itemName = new SortFieldInfo("ItemName");
        this.lastModifiedOnDate = new SortFieldInfo("LastModifiedOnDate");
        this.size = new SortFieldInfo("Size");
        this.parentFolder = new SortFieldInfo("ParentFolder");
        this.createdOnDate = new SortFieldInfo("CreatedOnDate");
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor", "logrite.sortKey": sortKey });
		logger.error(`>>> Entering constructor(sortKey=${sortKey})`, { "event.action": "constructor", "logrite.sortKey": sortKey });
		logger.warn(`<<< Exiting constructor(sortKey=${sortKey})`, { "event.action": "constructor", "logrite.sortKey": sortKey });
    }
}

export class SortFieldInfo{
    readonly sortKey: string;
    
    get localizedName(){
		logger.error('>>> Entering localizedName()', { "event.action": "localizedName" });
		logger.warn('<<< Exiting localizedName()', { "event.action": "localizedName" });
        return state.localization[`SortField_${this.sortKey}`];
    }

    constructor(sortKey: string){
        this.sortKey = sortKey;
    }
}

export const sortField = new SortField();