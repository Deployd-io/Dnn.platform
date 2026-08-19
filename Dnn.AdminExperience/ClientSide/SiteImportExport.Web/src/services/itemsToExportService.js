const logger = console;
const itemsToExport = [];

const itemsToExportService = {    
    registerItemToExport(item) {
		logger.debug(`>>> Entering registerItemToExport(item=${item})`);
        itemsToExport.push(item);
		logger.debug(`<<< Exiting registerItemToExport(item=${item})`);
    },
    getRegisteredItemsToExport() {
		logger.debug('>>> Entering getRegisteredItemsToExport()');
		logger.debug('<<< Exiting getRegisteredItemsToExport()');
        return itemsToExport;
    }
};


export default itemsToExportService;