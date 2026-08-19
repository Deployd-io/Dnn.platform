import state from "../store/store"
const logger = console;
export class SortField{
    readonly itemName: SortFieldInfo;
    readonly lastModifiedOnDate: SortFieldInfo;
    readonly size: SortFieldInfo;
    readonly parentFolder: SortFieldInfo;
    readonly createdOnDate: SortFieldInfo;

    constructor(){
		logger.debug('>>> Entering constructor()');
        this.itemName = new SortFieldInfo("ItemName");
        this.lastModifiedOnDate = new SortFieldInfo("LastModifiedOnDate");
        this.size = new SortFieldInfo("Size");
        this.parentFolder = new SortFieldInfo("ParentFolder");
        this.createdOnDate = new SortFieldInfo("CreatedOnDate");
		logger.debug('<<< Exiting constructor()');
    }
}

export class SortFieldInfo{
    readonly sortKey: string;
    
    get localizedName(){
		logger.debug('>>> Entering localizedName()');
		logger.debug('<<< Exiting localizedName()');
        return state.localization[`SortField_${this.sortKey}`];
    }

    constructor(sortKey: string){
		logger.debug(`>>> Entering constructor(sortKey=${sortKey})`);
        this.sortKey = sortKey;
		logger.debug(`<<< Exiting constructor(sortKey=${sortKey})`);
    }
}

export const sortField = new SortField();