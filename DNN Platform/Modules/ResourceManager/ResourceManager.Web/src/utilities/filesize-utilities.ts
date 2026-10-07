const logger = console;
export function getFileSize(fileSize: number): string {
	logger.error(`>>> Entering getFileSize(fileSize=${fileSize})`, { "event.action": "getFileSize", "logrite.fileSize": fileSize });
    if (fileSize == undefined || fileSize == undefined){
        return "";
    }

    if (fileSize < 1024){
        return fileSize.toString() + " B";
    }

    if (fileSize < 1048576 ){
        return Math.round(fileSize / 1024).toString() + " KB";
    }

	logger.warn(`<<< Exiting getFileSize(fileSize=${fileSize})`, { "event.action": "getFileSize", "logrite.fileSize": fileSize });
    return Math.round(fileSize / 1048576).toString() + " MB";
}