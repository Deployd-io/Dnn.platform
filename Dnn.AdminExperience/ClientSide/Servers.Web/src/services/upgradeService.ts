import { ChunkToUpload } from "models/ChunkToUpload";
import utils from "../utils";
import serviceFramework from "./serviceFramework";
const logger = console;

const getUpgradeSettings = function () {
logger.debug('>>> Entering getUpgradeSettings()');
logger.debug('<<< Exiting getUpgradeSettings()');
  return serviceFramework.get("Upgrades", "GetSettings");
};

const listUpgrades = function () {
logger.debug('>>> Entering listUpgrades()');
logger.debug('<<< Exiting listUpgrades()');
  return serviceFramework.get("Upgrades", "List");
};

const deletePackage = function (packageName) {
logger.debug(`>>> Entering deletePackage(packageName=${packageName})`);
logger.debug(`<<< Exiting deletePackage(packageName=${packageName})`);
  return serviceFramework.post("Upgrades", "Delete", { packageName });
};

const startUpgrade = function (packageName) {
logger.debug(`>>> Entering startUpgrade(packageName=${packageName})`);
logger.debug(`<<< Exiting startUpgrade(packageName=${packageName})`);
  return serviceFramework.post("Upgrades", "StartUpgrade", { packageName });
};

const uploadPackage = function (chunk: ChunkToUpload, callback, errorCallback) {
logger.debug(`>>> Entering uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback})`);
  const sf = utils.getServiceFramework();
  sf.moduleRoot = "PersonaBar";
logger.debug(`uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
logger.debug(`uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback}): sf.moduleRoot → ${sf.moduleRoot}`);
  sf.controller = "Upgrades";
logger.debug(`uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback}): sf.controller → ${sf.controller}`);
  let formData = new FormData();
logger.debug(`uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback}): formData → ${formData}`);
  formData.append("chunk", chunk.chunk);
  formData.append("start", chunk.start.toString());
  formData.append("totalSize", chunk.totalSize.toString());
  formData.append("fileId", chunk.fileId);
  sf.postfile("Upload", formData, callback, errorCallback);
logger.debug(`<<< Exiting uploadPackage(chunk=${chunk},callback=${callback},errorCallback=${errorCallback})`);
};

const completeUpload = function (fileId: string, fileName: string) {
logger.debug(`>>> Entering completeUpload(fileId=${fileId},fileName=${fileName})`);
logger.debug(`<<< Exiting completeUpload(fileId=${fileId},fileName=${fileName})`);
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
