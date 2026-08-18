import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SvgIcons, Checkbox, RadioButtons } from "@dnnsoftware/dnn-react-common";
import styles from "./style.module.less";
import Localization from "../../../localization";
const logger = console;

class ModuleRow extends Component {
    getCopyOptions(isPortable) {
		logger.debug(`>>> Entering getCopyOptions(isPortable=${isPortable})`);
        let options = [
            { label: Localization.get("ModuleCopyType.New"), value: "0" },
            { label: Localization.get("ModuleCopyType.Reference"), value: "2" }
        ];
        if (isPortable) {
            options.splice(1, 0, { label: Localization.get("ModuleCopyType.Copy"), value: "1" });
        }
		logger.debug(`<<< Exiting getCopyOptions(isPortable=${isPortable})`);
        return options;
    }

    render() {
		logger.debug('>>> Entering render()');
        const { module, onDelete, onEditing, onSetting, isEditingModule, showCopySettings, onCopyChange } = this.props;
        const editClassName = "extension-action" + (isEditingModule ? " selected" : "");
		logger.debug('<<< Exiting render()');
        return (
             
            <div className={styles.moduleRow} >
                {showCopySettings &&
                    <GridCell columnSize={10}>
                        <Checkbox value={module.includedInCopy !== null ? module.includedInCopy : true} 
                            onChange={onCopyChange.bind(this, module.id, "includedInCopy")} />
                    </GridCell>
                }
                <GridCell columnSize={showCopySettings ? 25 : 45} >
                    {module.title}
                </GridCell>
                <GridCell columnSize={showCopySettings ? 25 : 45} >
                    {module.friendlyName}
                </GridCell>
                {!showCopySettings &&
                    <GridCell columnSize={10} >
                        <div className="extension-action" onClick={onDelete.bind(this, module)}><SvgIcons.TrashIcon /></div>
                        <div 
                            className={editClassName} 
                            onClick={onSetting.bind(this, module)} 
                        ><SvgIcons.SettingsIcon /></div>
                        {module.allTabs === false && module.editContentUrl &&
                            <div 
                                className={editClassName} 
                                onClick={onEditing.bind(this, module)} 
                            ><SvgIcons.EditIcon /></div>}
                    </GridCell>
                }
                {showCopySettings &&
                    <GridCell columnSize={40}>
                        <RadioButtons
                            id={module.id}
                            onChange={onCopyChange.bind(this, module.id, "copyType")}
                            options={this.getCopyOptions(module.isPortable)}
                            value={module.copyType !== null ? module.copyType.toString() : (module.isPortable ? "1" : "0")} />
                    </GridCell>
                }
            </div>
             
        );
    }
}

ModuleRow.propTypes = {
    module: PropTypes.object.isRequired,
    isEditingModule: PropTypes.bool.isRequired,
    onDelete: PropTypes.func.isRequired,
    onEditing: PropTypes.func.isRequired,
    onSetting: PropTypes.func.isRequired,
    onCopyChange: PropTypes.func,
    showCopySettings: PropTypes.bool
};

export default ModuleRow;