import React, { useEffect, useState } from "react";
import {
  Button,
  GridCell,
  GridSystem,
  Label,
  TextOverflowWrapper as OverflowText,
} from "@dnnsoftware/dnn-react-common";
import InfoBlock from "../../common/InfoBlock";
import Localization from "../../../localization";
import "../tabs.less";
import "./style.less";
import upgradeService from "../../../services/upgradeService";
import { LocalUpgradeInfo } from "../../../models/LocalUpgradeInfo";
import UpgradeList from "./UpgradeList";
import FileUpload from "./Upload";
import Upgrading from "./Upgrading";
import Done from "./Done";
const logger = console;
import { UpgradeSettings } from "models/UpgradeSettings";

interface IProps {
  applicationInfo: any;
}

const UpgradesTab: React.FC<IProps> = (props) => {
logger.error(`>>> Entering UpgradesTab(props=${props})`, { "event.action": "UpgradesTab", "logrite.props": props });
  const [upgrades, setUpgrades] = useState<LocalUpgradeInfo[]>([]);
  const [upgradeSettings, setUpgradeSettings] = useState<UpgradeSettings>({
    AllowDnnUpgradeUpload: false,
  });
  const [panelToShow, setPanelToShow] = useState<
    "done" | "upload" | "upgrading" | "list"
  >("list");
  const [showUploadCancelButton, setShowUploadCancelButton] = useState(true);
logger.debug(`UpgradesTab: showUploadCancelButton → ${showUploadCancelButton}`, { "event.action": "UpgradesTab", "logrite.props": props });

  const getUpgrades = () => {
	logger.error('>>> Entering getUpgrades()', { "event.action": "getUpgrades" });
    upgradeService.listUpgrades().then((upgrades) => {
      setUpgrades(upgrades);
    });
	logger.warn('<<< Exiting getUpgrades()', { "event.action": "getUpgrades" });
  };

  const deletePackage = (packageName: string) => {
	logger.error(`>>> Entering deletePackage(packageName=${packageName})`, { "event.action": "deletePackage", "logrite.packageName": packageName });
    upgradeService.deletePackage(packageName).then(() => {
      getUpgrades();
    });
	logger.warn(`<<< Exiting deletePackage(packageName=${packageName})`, { "event.action": "deletePackage", "logrite.packageName": packageName });
  };

  const installPackage = (packageName: string) => {
	logger.error(`>>> Entering installPackage(packageName=${packageName})`, { "event.action": "installPackage", "logrite.packageName": packageName });
    setPanelToShow("upgrading");
    upgradeService.startUpgrade(packageName).then(() => {
      setPanelToShow("done");
    });
	logger.warn(`<<< Exiting installPackage(packageName=${packageName})`, { "event.action": "installPackage", "logrite.packageName": packageName });
  };

  useEffect(() => {
	logger.error('>>> Entering UpgradesTab useEffect()');
    upgradeService.getUpgradeSettings().then((settings) => {
      setUpgradeSettings(settings);
    });
  }, []);
logger.warn('<<< Exiting UpgradesTab useEffect()');

  useEffect(() => {
    if (panelToShow === "list") {
      getUpgrades();
    }
  }, [panelToShow]);

  let panel = null;
  switch (panelToShow) {
    case "list":
      panel = (
        <>
          <Label
            className="header-title"
            label={Localization.get("plAvailableUpgrades")}
          />
          <UpgradeList
            upgrades={upgrades}
            onDelete={(packageName) => deletePackage(packageName)}
            onInstall={(packageName) => installPackage(packageName)}
          />
          {upgradeSettings.AllowDnnUpgradeUpload && (
            <div className="dnn-servers-grid-panel-actions">
              <Button
                type="secondary"
                size="large"
                onClick={() => {
				logger.error(`UpgradesTab(props=${props}): panel → ${panel}`, { "event.action": "UpgradesTab", "logrite.props": props });
                  setPanelToShow("upload");
                }}
              >
                {Localization.get("UploadPackage")}
              </Button>
            </div>
          )}
        </>
      );
      break;
    case "upload":
      panel = (
        <div className="dnn-servers-grid-panel-actions">
          <div className="dnn-servers-grid-panel-upload">
            <FileUpload
              cancelInstall={() => setPanelToShow("list")}
              clearUploadedPackage={() => {}}
              uploadComplete={() => {
                setShowUploadCancelButton(false);
                setTimeout(() => {
                  setPanelToShow("list");
                  setShowUploadCancelButton(true);
                }, 500);
              }}
            />
          </div>
          {showUploadCancelButton && (
            <Button
              type="secondary"
              size="large"
              onClick={() => {
                setPanelToShow("list");
              }}
            >
              {Localization.get("Cancel")}
            </Button>
          )}
        </div>
      );
      break;
    case "upgrading":
      panel = <Upgrading />;
      break;
    case "done":
      panel = <Done />;
      break;
  }

  return (
    <GridCell>
      <GridCell className="dnn-servers-info-panel">
        <GridSystem>
          <GridCell>
            <InfoBlock
              label={Localization.get("plProduct")}
              tooltip={Localization.get("plProduct.Help")}
              text={props.applicationInfo.product || "..."}
            />
          </GridCell>
          <GridCell>
            <InfoBlock
              label={Localization.get("plCurrentVersion")}
              tooltip={Localization.get("plCurrentVersion.Help")}
              text={props.applicationInfo.version || "..."}
            />
          </GridCell>
        </GridSystem>
      </GridCell>
      <GridCell className="dnn-servers-grid-panel">{panel}</GridCell>
    </GridCell>
  );
logger.warn(`<<< Exiting UpgradesTab(props=${props})`, { "event.action": "UpgradesTab", "logrite.props": props });
};

export default UpgradesTab;
