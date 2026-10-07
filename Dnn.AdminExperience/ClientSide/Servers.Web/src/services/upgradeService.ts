import { ChunkToUpload } from "models/ChunkToUpload";
import utils from "../utils";
import serviceFramework from "./serviceFramework";
const logger = console;

const getUpgradeSettings = function () {
logger.error('>>> Entering getUpgradeSettings()', { "event.action": "getUpgradeSettings" });
logger.warn('<<< Exiting getUpgradeSettings()', { "event.action": "getUpgradeSettings" });
  return serviceFramework.get("Upgrades", "GetSettings");
};

const listUpgrades = function () {
logger.error('>>> Entering listUpgrades()', { "event.action": "listUpgrades" });
logger.warn('<<< Exiting listUpgrades()', { "event.action": "listUpgrades" });
  return serviceFramework.get("Upgrades", "List");
};

const deletePackage = function (packageName) {
logger.error(`>>> Entering deletePackage(packageName=${packageName})`, { "event.action": "deletePackage", "logrite.packageName": packageName });
logger.warn(`<<< Exiting deletePackage(packageName=${packageName})`, { "event.action": "deletePackage", "logrite.packageName": packageName });
  return serviceFramework.post("Upgrades", "Delete", { packageName });
};

const startUpgrade = function (packageName) {
logger.error(`>>> Entering startUpgrade(packageName=${packageName})`, { "event.action": "startUpgrade", "logrite.packageName": packageName });
logger.warn(`<<< Exiting startUpgrade(packageName=${packageName})`, { "event.action": "startUpgrade", "logrite.packageName": packageName });
  return serviceFramework.post("Upgrades", "StartUpgrade", { packageName });
};

const uploadPackage = function (chunk: ChunkToUpload, callback, errorCallback) {
logger.error(`>>> Entering uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "uploadPackage", "logrite.chunk": chunk, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
  const sf = utils.getServiceFramework();
  sf.moduleRoot = "PersonaBar";
logger.error(`uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "uploadPackage", "logrite.chunk": chunk, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
logger.debug(`uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "uploadPackage", "logrite.chunk": chunk, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
  sf.controller = "Upgrades";
logger.debug(`uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback}): sf.controller → ${sf.controller}`, { "event.action": "uploadPackage", "logrite.chunk": chunk, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
  let formData = new FormData();
logger.error(`uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback}): formData → ${formData}`, { "event.action": "uploadPackage", "logrite.chunk": chunk, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
  formData.append("chunk", chunk.chunk);
  formData.append("start", chunk.start.toString());
  formData.append("totalSize", chunk.totalSize.toString());
  formData.append("fileId", chunk.fileId);
  sf.postfile("Upload", formData, callback, errorCallback);
logger.warn(`<<< Exiting uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "uploadPackage", "logrite.chunk": chunk, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
};

const completeUpload = function (fileId: string, fileName: string) {
logger.error(`>>> Entering completeUpload(fileId=${fileId},fileName=${fileName})`, { "event.action": "completeUpload", "logrite.fileId": fileId, "logrite.fileName": fileName });
logger.warn(`<<< Exiting completeUpload(fileId=${fileId},fileName=${fileName})`, { "event.action": "completeUpload", "logrite.fileId": fileId, "logrite.fileName": fileName });
  return serviceFramework.post("Upgrades", "UploadComplete", { fileId, fileName });
};

const upgradeService = {
  getUpgradeSettings,
  listUpgrades,
  uploadPackage,
  completeUpload,
  deletePackage,
  startUpgrade,
};

export default upgradeService;
