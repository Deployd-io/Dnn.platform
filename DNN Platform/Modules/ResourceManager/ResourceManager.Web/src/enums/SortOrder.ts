import state from "../store/store"
const logger = console;
 
 export class SortOrder{
     readonly ascending: SortOrderInfo;
     readonly descending: SortOrderInfo;
 
     constructor(){
		logger.error('>>> Entering constructor()', { "event.action": "constructor", "logrite.sortOrderKey": sortOrderKey });
         this.ascending = new SortOrderInfo("Ascending");
         this.descending = new SortOrderInfo("Descending");
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor", "logrite.sortOrderKey": sortOrderKey });
		logger.error(`>>> Entering constructor(sortOrderKey=${sortOrderKey})`, { "event.action": "constructor", "logrite.sortOrderKey": sortOrderKey });
		logger.warn(`<<< Exiting constructor(sortOrderKey=${sortOrderKey})`, { "event.action": "constructor", "logrite.sortOrderKey": sortOrderKey });
     }
 }
 
 export class SortOrderInfo{
     readonly sortOrderKey: string;
 
     get localizedName(){
		logger.error('>>> Entering localizedName()', { "event.action": "localizedName" });
		logger.warn('<<< Exiting localizedName()', { "event.action": "localizedName" });
         return state.localization[`SortOrder_${this.sortOrderKey}`];
     }
 
     constructor(sortOrderKey: string = "Ascending"){
         this.sortOrderKey = sortOrderKey;
     }
 }
 
 export const sortOrder = new SortOrder();