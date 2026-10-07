import React, {Component} from "react";
import PropTypes from "prop-types";
import ModulesRow from "./ModuleRow/ModuleRow";
import styles from "./style.module.less";
import { GridCell, SvgIcons, Modal } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import utils from "../../utils";
import ModuleEdit from "./ModuleEdit/ModuleEdit";
const logger = console;

class Modules extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });

        this.state = {
            editType: ""
        };
    }

    onDeleteModule(module) {
		logger.error(`>>> Entering onDeleteModule(module=${module})`, { "event.action": "onDeleteModule", "logrite.module": module });
        const {onDeleteModule} = this.props;
        utils.confirm(
            Localization.get("DeleteModuleConfirm").replace("[MODULETITLE]", module.title),
            Localization.get("Delete"),
            Localization.get("Cancel"),
            function onConfirmDeletion() {
				logger.error('>>> Entering onConfirmDeletion()', { "event.action": "onConfirmDeletion" });
                onDeleteModule(module);
            });
    }

    onEditingModule(editType, module) {
		logger.error(`>>> Entering onEditingModule(editType=${editType},module=${module})`, { "event.action": "onEditingModule", "logrite.editType": editType, "logrite.module": module });
        const { onEditingModule } = this.props;
        this.setState({editType: editType}, () => {
            onEditingModule(module);
        });
		logger.warn(`<<< Exiting onEditingModule(editType=${editType},module=${module})`, { "event.action": "onEditingModule", "logrite.editType": editType, "logrite.module": module });
    }

    getModules() {
		logger.error('>>> Entering getModules()', { "event.action": "getModules" });
        const {modules, editingSettingModuleId, showCopySettings, onModuleCopyChange} = this.props;

		logger.debug(`getModules(): showCopySettings → ${showCopySettings}`, { "event.action": "getModules" });
        if (modules.length === 0) {
            return <GridCell className="no-modules" columnSize={100} >
                {Localization.get("NoModules")}
            </GridCell>;
        }

        return modules.map((module, index) => {
            const isEditingModule = module.id === editingSettingModuleId;
            return (
                <ModulesRow 
                    key={index}
                    module={module} 
                    onDelete={this.onDeleteModule.bind(this)}
                    onEditing={this.onEditingModule.bind(this, "content")}
                    onSetting={this.onEditingModule.bind(this, "settings")}
                    isEditingModule={isEditingModule}
                    onCopyChange={onModuleCopyChange}
                    showCopySettings={showCopySettings} />
            );
        });
		logger.warn('<<< Exiting getModules()', { "event.action": "getModules" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { state } = this;
        const {modules, onCancelEditingModule, editingSettingModuleId, showCopySettings, selectedPage} = this.props;
		logger.debug(`render(): selectedPage → ${selectedPage}`, { "event.action": "render" });
        const moduleRows = this.getModules();
		logger.error(`render(): moduleRows → ${moduleRows}`, { "event.action": "render" });
        const editingModule = modules.find(m => m.id === editingSettingModuleId);
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
             
            <div className={styles.moduleContainer}>
                <div className="module-title">
                    <div className="module-icon"><SvgIcons.ModuleIcon /></div>
                    <div className="sectionTitle">{Localization.get("ModulesOnThisPage")}</div>
                </div>
                <div className="module-table">    
                    <div className="header-row">
                        {showCopySettings &&
                            <GridCell columnSize={10} />
                        }
                        <GridCell columnSize={showCopySettings ? 25: 45} >
                            {Localization.get("Title")}
                        </GridCell>
                        <GridCell  columnSize={showCopySettings ? 25: 45} >
                            {Localization.get("Module")}
                        </GridCell>
                    </div>
                    {moduleRows}
                </div>
                <Modal 
                    isOpen={editingModule && state.editType !== ""} 
                    header={state.editType === "content" ? Localization.get("EditContent") : Localization.get("ModuleSettings")} 
                    onRequestClose={onCancelEditingModule}>
                    {editingModule && 
                        <ModuleEdit 
                            module={editingModule}
                            editType={state.editType}
                            onUpdatedModuleSettings={onCancelEditingModule}
                            selectedPage={selectedPage}
                        /> }
                </Modal>      
            </div>      
             
        );
    }
}

Modules.propTypes = {
    modules: PropTypes.array.isRequired,
    onDeleteModule: PropTypes.func.isRequired,
    onEditingModule: PropTypes.func.isRequired,
    onCancelEditingModule: PropTypes.func.isRequired,
    editingSettingModuleId: PropTypes.number,
    onModuleCopyChange: PropTypes.func,
    showCopySettings: PropTypes.bool,
    selectedPage: PropTypes.object.isRequired
};

export default Modules;
