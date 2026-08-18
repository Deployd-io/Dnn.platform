import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
const logger = console;
import { SvgIcons } from "@dnnsoftware/dnn-react-common";

export default class PersonaBarPageIcon extends Component {

    render_icon(hasUnpublishedChanges) {
         

        switch (true) {
            case hasUnpublishedChanges === true:
                return ( <div><SvgIcons.TreeDraftIcon /></div> );
            case hasUnpublishedChanges === false:
                return ( <div /> );
            default:
				logger.debug(`<<< Exiting render_icon(hasUnpublishedChanges=${hasUnpublishedChanges})`);
                return ( <div /> );
        }
    }

    render() {
	logger.debug('>>> Entering render()');

		logger.debug('<<< Exiting render()');
        return (
            <div className="dnn-persona-bar-draft-icon">
                {this.render_icon(this.props.display)}
            </div>
        );
    }

}

PersonaBarPageIcon.propTypes = {
    display: PropTypes.bool
};