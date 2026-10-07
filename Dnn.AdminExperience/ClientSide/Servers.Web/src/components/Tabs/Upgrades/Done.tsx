import React, { useState } from "react";
import { Button } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import utils from "../../../utils";
import "./style.less";
const logger = console;

interface IDoneProps {}

const Done: React.FC<IDoneProps> = (props) => {
logger.error(`>>> Entering Done(props=${props})`, { "event.action": "Done", "logrite.props": props });
  const [clicked, setClicked] = useState(false);
  const siteRoot = utils.getServiceFramework().getSiteRoot();
logger.error(`Done(props=${props}): siteRoot → ${siteRoot}`, { "event.action": "Done", "logrite.props": props });
logger.debug(`Done: clicked → ${clicked}`, { "event.action": "Done", "logrite.props": props });

  return (
    <div className="dnn-servers-grid-panel-done">
      <p className="done-title">{Localization.get("UpgradeDone")}</p>
      <p className="done-link">
        <Button
          type="primary"
          size="large"
          disabled={clicked}
          onClick={() => {
			logger.warn(`<<< Exiting Done(props=${props})`, { "event.action": "Done", "logrite.props": props });
            setClicked(true);
            window.location.href = siteRoot;
          }}
        >
          {Localization.get("UpgradeDoneLink")}
        </Button>
      </p>
    </div>
  );
};

export default Done;
