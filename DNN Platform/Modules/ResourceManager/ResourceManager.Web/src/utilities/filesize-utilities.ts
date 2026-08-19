const logger = console;
export function getFileSize(fileSize: number): string {
    if (fileSize == undefined || fileSize == undefined){
		logger.debug(`>>> Entering getFileSize(fileSize=${fileSize})`);
		logger.debug(`<<< Exiting getFileSize(fileSize=${fileSize})`);
        return "";
    }

    if (fileSize < 1024){
        return fileSize.toString() + " B";
    }

    if (fileSize < 1048576 ){
        return Math.round(fileSize / 1024).toString() + " KB";
    }

    return Math.round(fileSize / 1048576).toString() + " MB";
}