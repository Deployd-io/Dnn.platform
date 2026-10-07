import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
const logger = console;
import { SvgIcons } from "@dnnsoftware/dnn-react-common";

export default class PersonaBarPageIcon extends Component {
     
    selectIcon(number) {
	logger.error(`>>> Entering selectIcon(number=${number})`, { "event.action": "selectIcon", "logrite.number": number });

         
        switch (number) {
            case "normal":
                return (<div><SvgIcons.PagesIcon /></div>);

            case "file":
                return (<div><SvgIcons.TreePaperClip /></div>);

            case "tab":
            case "url":
                return (<div><SvgIcons.TreeLinkIcon /></div>);

            case "existing":
                return (<div><SvgIcons.TreeLinkIcon /></div>);

            default:
				logger.warn(`<<< Exiting selectIcon(number=${number})`, { "event.action": "selectIcon", "logrite.number": number });
                return (<div><SvgIcons.PagesIcon /></div>);
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div  className={(this.props.selected ) ? "dnn-persona-bar-treeview-icon selected-item " : "dnn-persona-bar-treeview-icon"}>
                {this.selectIcon(this.props.iconType)}
            </div>
        );
    }

}

PersonaBarPageIcon.propTypes = {
    iconType: PropTypes.string.isRequired,
    selected: PropTypes.bool.isRequired
};