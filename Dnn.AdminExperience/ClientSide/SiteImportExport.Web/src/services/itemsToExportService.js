const logger = console;
const itemsToExport = [];

const itemsToExportService = {    
    registerItemToExport(item) {
		logger.error(`>>> Entering registerItemToExport(item=${item})`, { "event.action": "registerItemToExport", "logrite.item": item });
        itemsToExport.push(item);
		logger.warn(`<<< Exiting registerItemToExport(item=${item})`, { "event.action": "registerItemToExport", "logrite.item": item });
    },
    getRegisteredItemsToExport() {
		logger.error('>>> Entering getRegisteredItemsToExport()', { "event.action": "getRegisteredItemsToExport" });
		logger.warn('<<< Exiting getRegisteredItemsToExport()', { "event.action": "getRegisteredItemsToExport" });
        return itemsToExport;
    }
};


export default itemsToExportService;