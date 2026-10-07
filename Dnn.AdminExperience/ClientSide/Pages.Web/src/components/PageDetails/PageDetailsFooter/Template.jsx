import React, { Component } from "react";
import PropTypes from "prop-types";
import { Label, Dropdown } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
const logger = console;


const styles = {
    label: {
        width: "100%",
        paddingBottom: "10px"
    },
    dropdown: {
        width: "95%"
    }
};

class Template extends Component {
    getTemplatesOptions() {
		logger.error('>>> Entering getTemplatesOptions()', { "event.action": "getTemplatesOptions" });
        const {props} = this;
        const {templates} = props;
		logger.warn('<<< Exiting getTemplatesOptions()', { "event.action": "getTemplatesOptions" });
        return templates.map(template => {
            return {value: template.Value, label: template.Id};
        });
    }
    
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {selectedTemplateId, templates, onSelect} = this.props;
        
        if (!templates || templates.length === 0) {
		logger.debug(`render(): onSelect → ${onSelect}`, { "event.action": "render" });
            return null;
        }
        
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div>
            <Label style={styles.label}
                labelType="inline"
                tooltipMessage={Localization.get("TemplateTooltip")}
                label={Localization.get("Template")}
            />
            <Dropdown style={styles.dropdown} 
                options={this.getTemplatesOptions()}
                value={selectedTemplateId}
                onSelect={onSelect.bind(this)} 
                withBorder={true} />
        </div>;
    }
}

Template.propTypes = {
    templates: PropTypes.arrayOf(PropTypes.object).isRequired,
    selectedTemplateId: PropTypes.number.isRequired,
    onSelect: PropTypes.func.isRequired
};

export default Template;