import * as React from "react";
import utils from "../../../utils";
import Localization from "../../../localization";
import "./style.less";
const logger = console;

interface IUpgradingProps {}

const Upgrading: React.FC<IUpgradingProps> = (props) => {
logger.debug(`>>> Entering Upgrading(props=${props})`);
logger.debug(`<<< Exiting Upgrading(props=${props})`);
  return (
    <div className="dnn-servers-grid-panel-upgrading">
      <p className="upgrading-title">{Localization.get("Upgrading")}</p>
      <p className="upgrading-help">{Localization.get("Upgrading.Help")}</p>
    </div>
  );
};

export default Upgrading;
