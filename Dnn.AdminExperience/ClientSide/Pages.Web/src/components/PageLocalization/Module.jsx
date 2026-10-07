import React, {Component} from "react";
import PropTypes from "prop-types";
import Localization from "localization";
const logger = console;
import { Tooltip, Checkbox, SvgIcons } from "@dnnsoftware/dnn-react-common";

class Module extends Component {

    onUpdateModules(key, e) {
		logger.error(`>>> Entering onUpdateModules(key=${key},e=${e})`, { "event.action": "onUpdateModules", "logrite.key": key, "logrite.e": e });
        const value = e.target ? e.target.value : e;
        const {index} = this.props;
        const {cultureCode} = this.props;
        this.props.onUpdateModules(cultureCode, index, value, key);
		logger.warn(`<<< Exiting onUpdateModules(key=${key},e=${e})`, { "event.action": "onUpdateModules", "logrite.key": key, "logrite.e": e });
    }

    onDeleteModule(TabModuleId) {
		logger.error(`>>> Entering onDeleteModule(TabModuleId=${TabModuleId})`, { "event.action": "onDeleteModule", "logrite.TabModuleId": TabModuleId });
        this.props.onDeleteModule(TabModuleId);
		logger.warn(`<<< Exiting onDeleteModule(TabModuleId=${TabModuleId})`, { "event.action": "onDeleteModule", "logrite.TabModuleId": TabModuleId });
    }

    onRestoreModule(TabModuleId) {
		logger.error(`>>> Entering onRestoreModule(TabModuleId=${TabModuleId})`, { "event.action": "onRestoreModule", "logrite.TabModuleId": TabModuleId });
        this.props.onRestoreModule(TabModuleId);
		logger.warn(`<<< Exiting onRestoreModule(TabModuleId=${TabModuleId})`, { "event.action": "onRestoreModule", "logrite.TabModuleId": TabModuleId });
    }

    toggleLink() {
		logger.error('>>> Entering toggleLink()', { "event.action": "toggleLink" });
        const {module} = this.props;
        const value = !module.IsLocalized;
        this.onUpdateModules("IsLocalized", value);
		logger.warn('<<< Exiting toggleLink()', { "event.action": "toggleLink" });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {module} = this.props;
        const {isDefault} = this.props;

        if (module.Exist) {
            const className = "module-row" + (module.IsDeleted ? " deleted" : "");
            const toolTip = [module.IsShared ? Localization.get("SharedModule") : Localization.get("IsNotSharedModule"), module.ModuleInfoHelp].join("<br>");
            return <div className={className}>
                <Tooltip
                    messages={[toolTip]}
                    style={{ float: "left", position: "static" }}
                />
                <input type="text" value={module.ModuleTitle} onChange={this.onUpdateModules.bind(this, "ModuleTitle") } aria-label="Title"/>
                {module.IsDeleted && <div className="icons-container">
                    <span className="icon" onClick={this.onDeleteModule.bind(this, module.TabModuleId) }><SvgIcons.TrashIcon /></span>
                    <span className="icon" onClick={this.onRestoreModule.bind(this, module.TabModuleId) }><SvgIcons.CycleIcon /></span>
                </div>}
                {!module.IsDeleted && !isDefault && <div className="icons-container">
                    <span
                        className={`icon float-left ${(module.IsShared ? " disabled" : (module.IsLocalized ? " blue" : ""))}`}
                        title={module.IsShared ? Localization.get("LocalizedSharedModule_tooltip") : 
                            (module.IsLocalized ? Localization.get("ClickToDeLocalizeModule_tooltip") : Localization.get("ClickToLocalizeModule_tooltip"))}
                        onClick={this.toggleLink.bind(this) }
                    ><SvgIcons.LinkIcon /></span>
                    {module.TranslatedVisible && 
                        <div title={module.IsTranslated ? Localization.get("UnCheckTranslatedModule_tooltip") : Localization.get("CheckToTranslateModule_tooltip")} style={{ float: "left", display: "inline-block" }}>
                            <Checkbox
                                value={module.IsTranslated}
                                onChange={this.onUpdateModules.bind(this, "IsTranslated") } />
                        </div>}
                </div>}
            </div>;
        }
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="module-row copy">
            <Checkbox
                style={{ float: "left" }}
                value={module.CopyModule}
                onChange={this.onUpdateModules.bind(this, "CopyModule") } />
            <div>{Localization.get("CopyModule") }</div>
        </div>;
    }
}

Module.propTypes = {
    isDefault: PropTypes.bool.isRequired,
    module: PropTypes.object.isRequired,
    onUpdateModules: PropTypes.func,
    onDeleteModule: PropTypes.func,
    onRestoreModule: PropTypes.func,
    cultureCode: PropTypes.string,
    index: PropTypes.number
};

export default Module;