import * as dayjs from "dayjs";
import serviceFramework from "./serviceFramework";
const logger = console;

const getLogs = function () {
	logger.debug('>>> Entering getLogs()');
	logger.debug('<<< Exiting getLogs()');
    return serviceFramework
        .get("ServerSettingsLogs", "GetLogs")
        .then(response => {
            const logList = response.Results.LogList.map(
                ({ Name, LastWriteTimeUtc, Size }) => {
                    return {
                        name: Name,
                        lastWriteTimeUtc: LastWriteTimeUtc,
                        size: Size,
                        upgradeLog: false
                    };
                });
            
            const upgradeLogList = response.Results.UpgradeLogList.map(
                ({ Name, LastWriteTimeUtc, Size }) => {
                    return {
                        name: Name,
                        lastWriteTimeUtc: LastWriteTimeUtc,
                        size: Size,
                        upgradeLog: true
                    };
                });
            
            return logList
                .concat(upgradeLogList)
                .sort((a, b) => dayjs(b.lastWriteTimeUtc) - dayjs(a.lastWriteTimeUtc));
        });
};

const getLog = function (fileName, upgradeLog) {
	logger.debug(`>>> Entering getLog(fileName=${fileName},upgradeLog=${upgradeLog})`);
    if (upgradeLog) {
	logger.debug(`<<< Exiting getLog(fileName=${fileName},upgradeLog=${upgradeLog})`);
        return serviceFramework.get("ServerSettingsLogs", "GetUpgradeLogFile", {logName: fileName});    
    }
    
    return serviceFramework.get("ServerSettingsLogs", "GetLogFile", {fileName: fileName});
};

const databaseTabService = {
    getLogs: getLogs,
    getLog: getLog
};

export default databaseTabService; 