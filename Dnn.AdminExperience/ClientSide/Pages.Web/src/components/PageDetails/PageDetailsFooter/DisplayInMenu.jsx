import React, {Component} from "react";
import PropTypes from "prop-types";
import { Switch, Label } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
const logger = console;

class DisplayInMenu extends Component {

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div>
            <Label
                labelType="inline"
                tooltipMessage={Localization.get("DisplayInMenuTooltip") }
                label={Localization.get("DisplayInMenu") }
            />
            <Switch
                labelHidden={false}
                onText={Localization.get("On") }
                offText={Localization.get("Off") }
                value={props.includeInMenu}
                onChange={props.onChangeIncludeInMenu} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

DisplayInMenu.propTypes = {
    includeInMenu: PropTypes.bool.isRequired,
    onChangeIncludeInMenu: PropTypes.func.isRequired
};

export default DisplayInMenu;