import state from "../store/store"
const logger = console;
 
 export class SortOrder{
     readonly ascending: SortOrderInfo;
     readonly descending: SortOrderInfo;
 
     constructor(){
		logger.debug('>>> Entering constructor()');
         this.ascending = new SortOrderInfo("Ascending");
         this.descending = new SortOrderInfo("Descending");
		logger.debug('<<< Exiting constructor()');
     }
 }
 
 export class SortOrderInfo{
     readonly sortOrderKey: string;
 
     get localizedName(){
		logger.debug('>>> Entering localizedName()');
		logger.debug('<<< Exiting localizedName()');
         return state.localization[`SortOrder_${this.sortOrderKey}`];
     }
 
     constructor(sortOrderKey: string = "Ascending"){
		logger.debug(`>>> Entering constructor(sortOrderKey=${sortOrderKey})`);
         this.sortOrderKey = sortOrderKey;
		logger.debug(`<<< Exiting constructor(sortOrderKey=${sortOrderKey})`);
     }
 }
 
 export const sortOrder = new SortOrder();